using System;
using System.Linq;

namespace SeaBattle.Server.Misc
{
    /// <summary>
    /// Generates random string
    /// </summary>
    public static class StringGenerator
    {
        internal static Random Rnd = new Random();

        /// <summary>
        /// Generates random string of length
        /// </summary>
        /// <param name="length">Length of the string</param>
        /// <returns>Random string</returns>
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Rnd.Next(s.Length)]).ToArray());
        }
    }
}
