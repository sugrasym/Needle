using System.Collections.Generic;
using System.Linq;

namespace IMS.Common.Needle.Extensions
{
    public static class NeedleSearchExtensions
    {
        public const char Delimiter = 'ᐰ';
        public const char SubDelimiter = 'ꗏ';
        public const char ElementDelimiter = 'ꓩ';

        public static IQueryable<T> Search<T>(this IQueryable<T> query, string searchText)
            where T : new()
        {
            return Needle.HaystackQueryFactory(searchText, query);
        }

        public static List<Match> FindMatches<T>(this T item, string searchText) where T : new()
        {
            return Needle.GetMatches(item, searchText);
        }

        public static List<Match> FindMatches<T>(IQueryable<T> query, string searchText) where T : new()
        {
            var items = query.Search(searchText).ToList();

            var matches = new List<Match>();
            foreach (var item in items)
            {
                var itemMatches = item.FindMatches(searchText);
                foreach (var match in itemMatches)
                {
                    var existingMatch = matches.FirstOrDefault(m => m.Path == match.Path &&
                                                                    m.Value == match.Value);
                    if (existingMatch == null)
                    {
                        existingMatch = match;
                        matches.Add(existingMatch);
                    }

                    existingMatch.Count++;
                }
            }

            return matches;
        }

        public static IQueryable<T> FilterHaystack<T>(this IQueryable<T> item, List<Match> matches) where T : new()
        {
            return Needle.FilterHaystack(item, matches);
        }
    }
}