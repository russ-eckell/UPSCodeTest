using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace UPSCodeTest
{
    class Program
    {
        public static List<DateTime> HolidayDates { get; set; }

        static void Main(string[] args)
        {
            DateTime startDate = DateTime.Now; double numberOfMinutes;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-= END DATE GENERATOR 3000 =-");
            Console.WriteLine("-----------------------------");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Enter the Assignment Start Date: ");
            Console.ForegroundColor = ConsoleColor.White;
            
            // Validate the start date to make sure it's valid. Also validate it's a proper type / not blank.
            if (DateTime.TryParse(Console.ReadLine(), out startDate)) { }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid Date. Please run again and enter a valid date.");
                Environment.Exit(-1);
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Enter the Estimated Number of Minutes: ");
            Console.ForegroundColor = ConsoleColor.White;

            if (Double.TryParse(Console.ReadLine(), out numberOfMinutes)) { }
            else {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please enter a valid number. Please run again and enter a valid number.");
                Environment.Exit(-1);
            }

            // Create a list of possible holidays based on the startt date entered.
            HolidayDates = GetHolidayList(startDate);

            // Depending on the time given or whether the date entered lands on a weekend or holiday, adjust the start time.
            startDate = AdjustDate(startDate);

            Console.WriteLine("-----------------------");
            Console.WriteLine("The project will start at: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(startDate.ToString("dddd, MMMM dd, yyyy hh:mm:ss tt"));
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("The project will be completed at: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(GetEndDate(startDate, numberOfMinutes).ToString("dddd, MMMM dd, yyyy hh:mm:ss tt"));
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("-----------------------");
            
            Console.Read();
        }

        /// <summary>
        /// Returns an end date based on a given start date and number of minutes and considers weekends and holidays.
        /// </summary>
        /// <param name="startDate">The start date</param>
        /// <param name="NumberOfMinutes">The number of minutes a project will take</param>
        /// <returns></returns>
        static DateTime GetEndDate(DateTime start, double minutes)
        {
            const double minutesInADay = 480;
            double minutesLeft = minutes;
            DateTime currentDate;

            // Get the number of minutes available to work in the first day given the start time.
            DateTime endOfFirstDay = new DateTime(start.Year, start.Month, start.Day, 17, 0, 0);
            TimeSpan timeSpanFirstDay = endOfFirstDay - start;

            // if it's more than half a day, we can assume lunch was taken during the day and we need to take out 60 minutes to accomodate that.
            double minutesAvailableFirstDay = timeSpanFirstDay.TotalMinutes > (minutesInADay / 2) ? timeSpanFirstDay.TotalMinutes - 60: timeSpanFirstDay.TotalMinutes;

            if (minutesAvailableFirstDay >= minutes)
            {
                // this is an assignment that will be completed in the first day, so we just need to add the minutes.
                if (minutes > (minutesInADay/2)) { 
                    //adding an hour to account for lunch.
                    return start.AddMinutes(minutes + 60);
                } else
                {
                    // No lunch to consider.
                    return start.AddMinutes(minutes);
                }
            } else
            {
                // update the amount of remaining minutes and push the day forward one day.
                minutesLeft = minutesLeft - minutesAvailableFirstDay;
                currentDate = start.AddDays(1);
            }

            // Loop through the full days until there's a partial day.
            for (DateTime date = currentDate; date.Date <= start.AddYears(1); date = date.AddDays(1)) {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday || HolidayDates.Contains(date.Date))
                {
                    //if it's an invalid day, skip to the next day without taking any minutes away.
                    currentDate = date;
                    continue;
                }
                else
                {
                    if (minutesLeft > minutesInADay) { 
                        currentDate = date;
                        minutesLeft = minutesLeft - minutesInADay;
                    }
                    else { 
                        break;
                    }
                }
            }
            

            // We have the date and can assume it's starting at 8am. Now we need to figure out the time based on the number of minutes left.
            if (minutesLeft > (minutesInADay / 2)) {
                // adding an hour to account for lunch because it's more than half a day.
                return currentDate.AddMinutes(minutesLeft + 60);
            } else
            {
                // No lunch to consider.
                return currentDate.AddMinutes(minutesLeft);
            }
        }

        #region Adjust Start Date

        /// <summary>
        /// Adjusts start date based on Time, Weekends, and Observed Holidays
        /// </summary>
        /// <param name="date">start date</param>
        /// <returns></returns>
        static DateTime AdjustDate(DateTime date)
        {
            // Adjusts the start date depending on the time entered.
            date = AdjustDateBasedOnTime(date);

            // Adjusts the start date if the one entered/calculated is on a weekend.
            date = AdjustDateForWeekend(date);

            if (HolidayDates.Contains(date.Date))
                date = AdjustDateForHoliday(date);

            return date;
        }

        /// <summary>
        /// Adjusts start date / time based on time passed in. Enforces rules about beginning and ending of day and lunchtime.
        /// </summary>
        /// <param name="date">date</param>
        /// <returns></returns>
        static DateTime AdjustDateBasedOnTime (DateTime date)
        {
            // Account for Time
            if (date.TimeOfDay < new TimeSpan(8, 0, 0)) 
            {
                // if the supplied time is before 8am, make it 8am.
                date = new DateTime(date.Year, date.Month, date.Day, 8, 0, 0);
            }
            else if (date.TimeOfDay >= new TimeSpan(12, 0, 0) && date.TimeOfDay < new TimeSpan(13, 0, 0))
            {
                // If the time is between 12 and 1, we'll push it to 1pm.
                date = date.Date;
                date = new DateTime(date.Year, date.Month, date.Day, 13, 0, 0);
            }
            else if (date.TimeOfDay >= new TimeSpan(17, 0, 0))
            {
                //If the time is 5pm or later, push it to the next day at 8am.
                date = date.Date.AddDays(1);
                date = new DateTime(date.Year, date.Month, date.Day, 8, 0, 0);
            }

            return date;
        }

        /// <summary>
        /// Returns the following Monday at 8am.
        /// </summary>
        /// <param name="date">date</param>
        /// <returns></returns>
        static DateTime AdjustDateForWeekend (DateTime date)
        {
            // If it's a weekend, push it to the following Monday at 8am.
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) { 
                int daysUntilMonday = ((int)DayOfWeek.Monday - (int)date.DayOfWeek + 7) % 7;
                date = date.AddDays(daysUntilMonday);
                date = new DateTime(date.Year, date.Month, date.Day, 8, 0, 0);
            }
            return date;
        }

        /// <summary>
        /// Returns the following Monday at 8am.
        /// </summary>
        /// <param name="date">date</param>
        /// <returns></returns>
        static DateTime AdjustDateForHoliday(DateTime date)
        {
            //If it's a holiday, push it to the following day at 8am.
            date = date.AddDays(1);
            date = new DateTime(date.Year, date.Month, date.Day, 8, 0, 0);
            // Adjust for the weekend in case the original date was a Friday holiday. In that case, it gets pushed to Monday at 8am.
            return AdjustDateForWeekend(date);
            
        }

        #endregion AdjustDate

        #region Build Holidays List

        /// <summary>
        /// Builds a known list of holidays to check against.
        /// </summary>
        /// <param name="startDate">date </param>
        /// <returns></returns>
        static List<DateTime> GetHolidayList(DateTime startDate)
        {
            List<DateTime> holidayList = new List<DateTime>();
            DateTime holidayDate;
            DayOfWeek dayOfWeek;

            // Since we're going to be getting this data prior to making adjustments based on weekends and holidays, we need to get the year initially entered and the next year. There 
            // is a small chance that we will need the New Year's after that as well. For example, if the user enters December 31, 2020 and it's a Sunday, it'll get pushed to Monday, Jan 1,2021
            // which will get pushed to Jan 2, 2021, making Jan 1, 2022 a possible holiday to be considered.
            int [] years = new int[] { startDate.Year, startDate.Year + 1};

            // Get the holiday dates for this year and next year.
            foreach (int year in years) {
                
                /* Holidays that are on the same calendar date every year */
                
                DateTime[] staticHolidays = new DateTime[] {
                    new DateTime(year, 1,1), // New Year's Day
                    new DateTime(year, 7,4), // Independence Day
                    new DateTime(year, 12,25), // Christmas
                };

                // These may land on a weekend, but according to the instructions, employees still get a day off, so I'm going to say "OBSERVED" would be Friday if it lands on a Saturday.
                // Monday if it lands on a Sunday. In a real-world scenario, I would probably need to clarify this rule.
                foreach (DateTime holiday in staticHolidays) { 
                   holidayList.Add(GetObservedHolidayDate(holiday));
                }

                /* Holidays that land on different dates, but are based on particular days in particular weeks in a given month. */

                // Memorial Day (Last Monday in May)
                holidayDate = new DateTime(year, 5, 31); // last day of the month
                dayOfWeek = holidayDate.DayOfWeek;
                
                // Start at the last day of the month and loop back through the days until we get to the most recent Monday (last monday in May).
                while (dayOfWeek != DayOfWeek.Monday)
                {
                    holidayDate = holidayDate.AddDays(-1);
                    dayOfWeek = holidayDate.DayOfWeek;
                }

                holidayList.Add(holidayDate);

                // Labor Day (First Monday in Sept)
                holidayDate = new DateTime(year, 9, 1); // first day of the month
                dayOfWeek = holidayDate.DayOfWeek;

                // Start at the first day of the month and loop forward through the days until we get to the first Monday.
                while (dayOfWeek != DayOfWeek.Monday)
                {
                    holidayDate = holidayDate.AddDays(1);
                    dayOfWeek = holidayDate.DayOfWeek;
                }

                holidayList.Add(holidayDate);

                // Thanksgiving. This one is the trickiest. What a dumb way to schedule a holiday :). 4th Thursday in Novemeber
                int fourthThursday = (from day in Enumerable.Range(1, 30)
                                   where new DateTime(year, 11, day).DayOfWeek == DayOfWeek.Thursday
                                   select day).ElementAt(3);

                holidayList.Add(new DateTime(year, 11, fourthThursday));
            }
            
            
            // Covering a very specific and probably rare case of a start date happening on a weekend in Dec that gets bumped to the New Year holiday, that gets bumped into the next year.
            holidayList.Add(new DateTime(startDate.Year + 2, 1, 1));

            return holidayList;
        }

        /// <summary>
        /// Returns observed holiday date. Saturday will return preceding Friday. Sunday will return following Monday.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        static DateTime GetObservedHolidayDate(DateTime date)
        {
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                    return date.AddDays(-1);
                case DayOfWeek.Sunday:
                    return date.AddDays(1);
                default:
                    return date;
            }
        }

        #endregion Holidays
    }
}
