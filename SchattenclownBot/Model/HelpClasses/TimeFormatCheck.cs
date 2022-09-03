using DisCatSharp.Common;
using System;

namespace SchattenclownBot.Model.HelpClasses
{
   public static class TimeFormatCheck
   {
      /// <summary>
      ///     Checks if the given hour and minute are usable to make a datetime object out of them.
      ///     Returns true if the given arguments are usable.
      ///     Returns false if the hour or the minute are not usable.
      /// </summary>
      /// <param name="hour">The hour.</param>
      /// <param name="minute">The minute.</param>
      /// <returns>A bool.</returns>
      public static bool TimeFormat(int hour, int minute)
      {
         if (hour.IsInRange(0, 24) && minute.IsInRange(0, 59))
            return true;
         else
            return false;
      }
   }
}
