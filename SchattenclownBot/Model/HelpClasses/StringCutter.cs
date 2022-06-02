namespace SchattenclownBot.Model.HelpClasses
{
    /// <summary>
    /// Cuts a string until the word given with variation given with an integer
    /// </summary>
    public class StringCutter
    {
        /// <summary>
        /// Removes until word.
        /// </summary>
        /// <param name="inputString">The string.</param>
        /// <param name="word">The word.</param>
        /// <param name="removeWordInt">The integer +/- from the word.</param>
        /// <returns>A string.</returns>
        public static string RemoveUntilWord(string inputString, string word, int removeWordInt)
        {
            return inputString.Substring(inputString.IndexOf(word) + removeWordInt);
        }
        /// <summary>
        /// Removes the after word.
        /// </summary>
        /// <param name="inputString">The string.</param>
        /// <param name="word">The word.</param>
        /// <param name="keepWordInt">The integer +/- from the word.</param>
        /// <returns>A string.</returns>
        public static string RemoveAfterWord(string inputString, string word, int keepWordInt)
        {
            var index = inputString.LastIndexOf(word);
            if (index > 0)
                inputString = inputString.Substring(0, index + keepWordInt);

            return inputString;
        }
    }
}
