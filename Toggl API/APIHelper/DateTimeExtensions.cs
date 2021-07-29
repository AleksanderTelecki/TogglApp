using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toggl_API.APIHelper
{
    public static partial class DateTimeExtensions
    {
        /// <summary>
        /// Extension for getting the first day of current week
        /// </summary>
        /// <param name="dt">Date</param>
        /// <returns>First Day of current week</returns>
        public static DateTime FirstDayOfWeek(this DateTime dt)
        {
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            var diff = dt.DayOfWeek - culture.DateTimeFormat.FirstDayOfWeek;

            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-diff).Date;
        }

        /// <summary>
        /// Extension for getting the last day of current week
        /// </summary>
        /// <param name="dt">Date</param>
        /// <returns>Last Day of current week</returns>
        public static DateTime LastDayOfWeek(this DateTime dt) => dt.FirstDayOfWeek().AddDays(6);

    }
}
