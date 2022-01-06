using System;
using System.Collections.Generic;
using System.Text;

namespace SchattenclownBot.HelpClasses
{
    /// <summary>
    /// Cuts a string until the word given with variation given with an integer
    /// </summary>
    public class WordCutter
    {
        /// <summary>
        /// Removes until word.
        /// </summary>
        /// <param name="inputstring">The string.</param>
        /// <param name="word">The word.</param>
        /// <param name="removewordint">The integer +/- from the word.</param>
        /// <returns>A string.</returns>
        public static string RemoveUntilWord(string inputstring, string word, int removewordint)
        {
            return inputstring.Substring(inputstring.IndexOf(word) + removewordint);
        }
        /// <summary>
        /// Removes the after word.
        /// </summary>
        /// <param name="inputstring">The string.</param>
        /// <param name="word">The word.</param>
        /// <param name="keepwordint">The integer +/- from the word.</param>
        /// <returns>A string.</returns>
        public static string RemoveAfterWord(string inputstring, string word, int keepwordint)
        {
            int index = inputstring.LastIndexOf(word);
            if (index > 0)
                inputstring = inputstring.Substring(0, index + keepwordint);

            return inputstring;
        }
    }
}
