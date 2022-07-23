namespace SchattenclownBot.Model.HelpClasses
{
    /// <summary>
    /// Cuts a string until the keyWord given with variation given with an integer
    /// </summary>
    public class StringCutter
    {
        /// <summary>
        /// Removes until keyWord.
        /// </summary>
        /// <param name="inputString">The string.</param>
        /// <param name="keyWord">The keyWord.</param>
        /// <param name="removeWordInt">The integer +/- from the keyWord.</param>
        /// <returns>A string.</returns>
        public static string RemoveUntilWord(string inputString, string keyWord, int removeWordInt)
        {
            return inputString.Substring(inputString.IndexOf(keyWord) + removeWordInt);
        }
        /// <summary>
        /// Removes the after keyWord.
        /// </summary>
        /// <param name="inputString">The string.</param>
        /// <param name="keyWord">The keyWord.</param>
        /// <param name="keepWordInt">The integer +/- from the keyWord.</param>
        /// <returns>A string.</returns>
        public static string RemoveAfterWord(string inputString, string keyWord, int keepWordInt)
        {
            int index = inputString.LastIndexOf(keyWord);
            if (index > 0)
                inputString = inputString.Substring(0, index + keepWordInt);

            return inputString;
        }
    }
}
