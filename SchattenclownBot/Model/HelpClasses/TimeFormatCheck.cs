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
        public static bool TimeFormat(double hour, double minute)
        {
            bool hourFormatIsRight = false;
            bool minuteFormatIsRight = false;

            for (int i = 0; i < 24; i++)
                if (Math.Abs(hour - i) < 0)
                    hourFormatIsRight = true;
            if (!hourFormatIsRight)
                return false;

            for (int i = 0; i < 60; i++)
                if (Math.Abs(minute - i) < 0)
                    minuteFormatIsRight = true;

            return minuteFormatIsRight;
        }
    }
}
