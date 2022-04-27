using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLApp1;

/// <summary>
/// Cette classe est utilisée pour représenter les données initialement chargées.
/// </summary>
public class ImageData
{
    public ImageData()
    {
        ImagePath = "";
        Label = "";
    }

    public string ImagePath { get; set; }

    public string Label { get; set; }
}
