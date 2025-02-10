using System.Globalization;
using System.Text.RegularExpressions;

namespace SvgPathProperties
{
    public static class Parser
    {
        private static readonly Dictionary<char, int> _length = new Dictionary<char, int>
        {
            { 'a', 7 },
            { 'c', 6 },
            { 'h', 1 },
            { 'l', 2 },
            { 'm', 2 },
            { 'q', 4 },
            { 's', 4 },
            { 't', 2 },
            { 'v', 1 },
            { 'z', 0 },
        };

        public static List<(char, List<double>)> Parse(string path)
        {
            path = string.IsNullOrEmpty(path) ? "M0,0" : path;
            var segments = Regex.Matches(path, "([astvzqmhlc])([^astvzqmhlc]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            if (segments.Count == 0)
                throw new Exception($"No path elements found in string {path}");

            var result = new List<(char, List<double>)>(segments.Count);
            foreach (Match match in segments)
            {
                var command = match.Value[0];
                var type = char.ToLowerInvariant(command);
                var args = ParseValues(match.Value.Substring(1));

                // overloaded moveTo
                if (type == 'm' && args.Count > 2)
                {
                    result.Add((command, args.Splice(0, 2)));
                    type = 'l';
                    command = command == 'm' ? 'l' : 'L';
                }

                while (args.Count >= 0)
                {
                    if (args.Count == _length[type])
                    {
                        result.Add((command, args.Splice(0, _length[type])));
                        break;
                    }

                    if (args.Count < _length[type])
                        throw new Exception($"Malformed path data: \"{command}\" must have {_length[type]} elements and has {args.Count}: {match.Value}");

                    result.Add((command, args.Splice(0, _length[type])));
                }
            }

            return result;
        }

        private static List<double> ParseValues(string args)
        {
            var numbers = Regex.Matches(args, @"-?[0-9]*\.?[0-9]+(?:e[-+]?\d+)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            return numbers.OfType<Match>().Select(m => double.Parse(m.Value, CultureInfo.InvariantCulture)).ToList();
        }

        public static List<T> Splice<T>(this List<T> source, int start, int size)
        {
            var items = source.Skip(start).Take(size).ToList();
            if (source.Count >= size)
                source.RemoveRange(start, size);
            else
                source.Clear();
            return items;
        }
    }
}
