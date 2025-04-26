using LibVLCSharp.Shared;
using SkiaSharp;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace MLApp1.Data;

/// <summary>
/// Source: https://code.videolan.org/mfkl/libvlcsharp-samples/-/blob/master/PreviewThumbnailExtractor.Skia/Program.cs
/// </summary>
public class DatParserV2 : IDatParser
{
    private readonly string _path;
    private static readonly LibVLC _vlc;
    private readonly MediaPlayer _player;
    private readonly CancellationTokenSource _processingCancellationTokenSource;
    private readonly Media _media;
    private readonly List<string> FilesNames = new List<string>();

    public DatParserV2(string path)
    {
        _path = path;
        _player = new MediaPlayer(_vlc);

        // Listen to events
        _processingCancellationTokenSource = new CancellationTokenSource();
        _player.Stopped += (s, e) => _processingCancellationTokenSource.CancelAfter(1);

        // Create new media
        var media = new Media(_vlc, _path);

        media.AddOption(":no-audio");
        media.AddOption(":no-sub-autodetect-file");
        media.AddOption(":no-osd");
        media.AddOption(":avcodec-hw=none"); // Disable hardware decoding if not required
        // Set the size and format of the video here.
        _player.SetVideoFormat("RV32", Width, Height, Pitch);
        _player.SetVideoCallbacks(Lock, null, Display);

        _media = new Media(_vlc, _path);
    }

    public void Dispose()
    {
        _player.Dispose();
        _media.Dispose();
        _processingCancellationTokenSource.Dispose();
    }

    public async Task<IEnumerable<string>> EnumeratesImages(int nbImage = int.MaxValue)
    {
        var temp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");

        Directory.CreateDirectory(temp);

        if (_player.Play(_media))
        {
            try
            {
                await ProcessThumbnailsAsync(temp, nbImage, _processingCancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            { }
        }

        return FilesNames.ToArray();
    }

    private int JpegQuality = 50; // Allow dynamic configuration

    private async Task ProcessThumbnailsAsync(string destination, int thumbnailPerClip, CancellationToken token)
    {
        var frameNumber = 0;
        var tasks = new List<Task>();

        while (!token.IsCancellationRequested)
        {
            if (FilesToProcess.TryDequeue(out var bitmap))
            {
                tasks.Add(Task.Run(() =>
                {
                    var fileName = Path.Combine(destination, Path.GetFileNameWithoutExtension(_path) + $"{frameNumber:0000}.jpg");
                    using (var surface = SKSurface.Create(new SKImageInfo((int)Width, (int)Height)))
                    using (var canvas = surface.Canvas)
                    {
                        canvas.DrawBitmap(bitmap, 0, 0);
                        using (var outputImage = surface.Snapshot())
                        using (var data = outputImage.Encode(SKEncodedImageFormat.Jpeg, JpegQuality))
                        using (var outputFile = File.Open(fileName, FileMode.Create))
                        {
                            data.SaveTo(outputFile);
                            bitmap.Dispose();
                            FilesNames.Add(fileName);
                        }
                    }
                }));

                frameNumber++;

                if (thumbnailPerClip < frameNumber)
                {
                    await _processingCancellationTokenSource.CancelAsync();
                }
            }
            else
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), token);
            }
        }

        await Task.WhenAll(tasks); // Wait for all tasks to complete
    }

    private const uint Width = 1280;
    private const uint Height = 720;

    /// <summary>
    /// RGBA is used, so 4 byte per pixel, or 32 bits.
    /// </summary>
    private const uint BytePerPixel = 4;

    /// <summary>
    /// the number of bytes per "line"
    /// For performance reasons inside the core of VLC, it must be aligned to multiples of 32.
    /// </summary>
    private static readonly uint Pitch;

    /// <summary>
    /// The number of lines in the buffer.
    /// For performance reasons inside the core of VLC, it must be aligned to multiples of 32.
    /// </summary>
    private static readonly uint Lines;

    static DatParserV2()
    {
        Pitch = Align(Width * BytePerPixel);
        Lines = Align(Height);

        uint Align(uint size)
        {
            if (size % 32 == 0)
            {
                return size;
            }

            return ((size / 32) + 1) * 32;// Align on the next multiple of 32
        }

        Core.Initialize();
        _vlc = new LibVLC("--quiet");
    }

    private static SKBitmap? CurrentBitmap;
    private static readonly ConcurrentQueue<SKBitmap> FilesToProcess = new ConcurrentQueue<SKBitmap>();
    private static long FrameCounter = 0;
    private static readonly ConcurrentBag<SKBitmap> BitmapPool = new ConcurrentBag<SKBitmap>();


    private static IntPtr Lock(IntPtr opaque, IntPtr planes)
    {
        if (!BitmapPool.TryTake(out CurrentBitmap))
        {
            CurrentBitmap = new SKBitmap(new SKImageInfo((int)(Pitch / BytePerPixel), (int)Lines, SKColorType.Bgra8888));
        }
        Marshal.WriteIntPtr(planes, CurrentBitmap.GetPixels());
        return IntPtr.Zero;
    }

    private static int FrameInterval = 100; // Allow dynamic configuration


    private static void Display(IntPtr opaque, IntPtr picture)
    {
        if (CurrentBitmap == null)
            return;

        if (FrameCounter % FrameInterval == 0)
        {
            FilesToProcess.Enqueue(CurrentBitmap);
            CurrentBitmap = null;
        }
        else
        {
            BitmapPool.Add(CurrentBitmap); // Reuse the bitmap
            CurrentBitmap = null;
        }
        FrameCounter++;
    }
}