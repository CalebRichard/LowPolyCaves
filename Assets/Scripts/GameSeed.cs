using UnityEngine;
using System.Collections.Generic;

public static class GameSeed {

    #region Methods

    public static int GetSeed(string seedStr) {

        // There is a chance to get the same hash code for different strings
        // so it might be better to use something else in the future
        return seedStr.GetHashCode();
    }

    /// <summary>
    /// Generates string of 8-16 random characters
    /// </summary>
    /// <returns>Random string</returns>
    public static string RandomString() {
        
        var str = string.Empty;

        // Get random integer between 8 and 16 for length of string
        var num = Random.value;
        num *= 9;
        num += 8;
        num -= num % 1;

        // Build random string
        for (int i = (int)num; i > 0; i--) {

            var chr = NumToChar(Random.value);
                str = string.Format("{0}{1}", str, chr);
        }

        return str;
    }

    /// <summary>
    /// Converts number to character
    /// </summary>
    /// <param name="num">Value between 0 and 1</param>
    /// <returns>Character: a-z, A-Z</returns>
    public static char NumToChar(float num) {

        num *= 52;
        num -= num % 1;
        return (num <= 25) ? (char)(num + 65) : (char)(num + 71);
    }

    /// <summary>
    /// Returns array of digits
    /// </summary>
    /// <param name="seed">Integer</param>
    /// <returns>Array of integers</returns>
    public static int[] GetDigits(int seed) {

        var num = Mathf.Abs(seed);
        var nums = new Stack<int>();

        do {
            nums.Push(num % 10);
            num /= 10;
        } while (num > 0);

        return nums.ToArray();
    }

    #endregion // Methods
}
