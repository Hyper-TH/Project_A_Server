using System.Collections.Concurrent;

namespace Project_A_Server.Utils
{
    public class AvailabilityMapper
    {
        public static List<object> MapAvailabilities(List<dynamic> availabilities)
        {
            var groupedByDate = availabilities
                .GroupBy(x => x.Date.ToString("yyyy-MM-dd"));

            var result = new List<object>();

            /*
             * TODO: Follow this JSON
             * {
             *  id,
             *  title,
             *  start,
             *  end,
             *  timezone, 
             *  color (modify group model)
             * }
             */

            foreach (var group in groupedByDate)
            {
                var times = new List<object>();

                foreach (var item in group)
                {
                    times.Add(new
                    {
                        item.StartTime,
                        item.EndTime,
                        item.Timezone,
                    });
                }

                result.Add(new
                {
                    Date = group.Key,
                    Times = times
                });
            }

            return result;
        }
        private static bool IsValidDate(string date)
        {
            return DateTime.TryParse(date, out _);
        }
    }
}
