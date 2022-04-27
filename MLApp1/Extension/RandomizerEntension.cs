using System.Security.Cryptography;

namespace MLApp1.Extension;
public static class RandomizerEntension
{
    public static T[] Suffle<T>(this T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            byte[] randomInt = RandomNumberGenerator.GetBytes(4);
            int k = Convert.ToInt32(randomInt[0]) % n;
            n--;
            T value = array[n];
            array[n] = array[k];
            array[k] = value;
        }

        return array;
    }
}
