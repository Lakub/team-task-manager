using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TeamTaskManager.Helpers
{
    public static class TimeSpanExtensions
    {
        public static bool TryParse(string input, out TimeSpan result)
            => TryParse(input, out result, out _);

        public static bool TryParse(string input, out TimeSpan result, out string normalized)
        {
            try
            {
                result = Parse(input, out normalized);
                return true;
            }
            catch
            {
                result = TimeSpan.Zero;
                normalized = string.Empty;
                return false;
            }
        }

        public static TimeSpan Parse(string input) => Parse(input, out _);

        public static TimeSpan Parse(string input, out string normalized)
        {
            normalized = string.Empty;

            if (string.IsNullOrWhiteSpace(input)) return TimeSpan.Zero;

            // domyslnie minuty bo tak robi jira
            if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out var minutesOnly))
            {
                normalized = $"{minutesOnly}m";
                return TimeSpan.FromMinutes(minutesOnly);
            }

            // np. 1:30:00, 2:45, 0:15:30
            if (TimeSpan.TryParse(input, CultureInfo.InvariantCulture, out var result))
            {
                normalized = result.ToString(@"d\.hh\:mm\:ss");
                return result;
            }

            // np. 12w 3d 4h; 12h30m10s; 45m; 1.5h; 2,5w
            const string pattern = @"(\d+([.,]\d+)?)\s*([wdhms])";
            var matches = Regex.Matches(input, pattern);

            var unparsed = Regex.Replace(input, pattern, "").Trim();
            if (!string.IsNullOrWhiteSpace(unparsed))
                throw new FormatException($"Nieprawidłowy format czasu: '{input}'");

            double weeks = 0, days = 0, hours = 0, minutes = 0, seconds = 0;

            foreach (Match match in matches)
            {
                string rawValue = match.Groups[1].Value.Replace(',', '.');
                double value = double.Parse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture);
                string unit = match.Groups[3].Value;

                switch (unit)
                {
                    case "w": weeks += value; break;
                    case "d": days += value; break;
                    case "h": hours += value; break;
                    case "m": minutes += value; break;
                    case "s": seconds += value; break;
                }
            }

            var parts = new List<string>();
            if (weeks > 0) parts.Add($"{weeks}w");
            if (days > 0) parts.Add($"{days}d");
            if (hours > 0) parts.Add($"{hours}h");
            if (minutes > 0) parts.Add($"{minutes}m");
            if (seconds > 0) parts.Add($"{seconds}s");

            normalized = parts.Any() ? string.Join(" ", parts) : "0m";
            return TimeSpan.FromDays(weeks * 7 + days)
                 + TimeSpan.FromHours(hours)
                 + TimeSpan.FromMinutes(minutes)
                 + TimeSpan.FromSeconds(seconds);
        }
    }
}
