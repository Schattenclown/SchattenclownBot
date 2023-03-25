using System;

namespace SchattenclownBot.Model.HelpClasses
{
   /// <summary>
   ///    Cuts a string until the keyWord given with variation given with an integer
   /// </summary>
   public class StringCutter
   {
      /// <summary>
      ///    Removes until keyWord.
      /// </summary>
      /// <param name="inputString">The string.</param>
      /// <param name="keyWord">The keyWord.</param>
      /// <param name="removeWordInt">The integer +/- from the keyWord.</param>
      /// <returns>A string.</returns>
      public static string RmUntil(string inputString, string keyWord, int removeWordInt)
      {
         if (inputString == null)
         {
            return null;
         }

         return !inputString.Contains(keyWord) ? inputString : inputString[(inputString.IndexOf(keyWord, StringComparison.Ordinal) + removeWordInt)..];
      }

      /// <summary>
      ///    Removes the after keyWord.
      /// </summary>
      /// <param name="inputString">The string.</param>
      /// <param name="keyWord">The keyWord.</param>
      /// <param name="keepWordInt">The integer +/- from the keyWord.</param>
      /// <returns>A string.</returns>
      public static string RmAfter(string inputString, string keyWord, int keepWordInt)
      {
         if (!inputString.Contains(keyWord))
         {
            return inputString;
         }

         int index = inputString.LastIndexOf(keyWord, StringComparison.Ordinal);
         if (index > 0)
         {
            inputString = inputString[..(index + keepWordInt)];
         }

         return inputString;
      }
   }
}