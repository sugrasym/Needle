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

        public static IQueryable<T> FilterHaystack<T>(this IQueryable<T> item, List<Match> matches) where T : new()
        {
            return Needle.FilterHaystack(item, matches);
        }
    }
}