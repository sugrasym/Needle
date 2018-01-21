using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using IMS.Common.Needle.Extensions;

namespace IMS.Common.Needle
{
    public static class Needle
    {
        /// <summary>
        ///     Generates the haystack string for the provided object using properties
        ///     marked with Haystack attributes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string HaystackFactory<T>(T item) where T : new()
        {
            var set = new List<T>
            {
                item
            };

            var sb = new StringBuilder();
            var attrs = Utility.GetHaystackAttributes<T>();

            foreach (var property in attrs)
            {
                if (!string.IsNullOrWhiteSpace(property.IsVisible))
                {
                    if (!set.AsQueryable().Where(property.IsVisible).Any())
                    {
                        continue;
                    }
                }

                var value = Utility.GetCompositePropertyValue(item, property.Path);
                sb.Append(value);
                sb.Append(NeedleSearchExtensions.Delimiter);
            }

            return sb.ToString().TrimEnd(NeedleSearchExtensions.Delimiter);
        }

        /// <summary>
        ///     Returns a queryable for performing a haystack search on the provided
        ///     base query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="haystack"></param>
        /// <param name="baseQuery"></param>
        /// <returns></returns>
        public static IQueryable<T> HaystackQueryFactory<T>(string haystack, IQueryable<T> baseQuery)
            where T : new()
        {
            if (string.IsNullOrWhiteSpace(haystack))
            {
                return baseQuery;
            }

            var instance = new T();
            //split the haystack search query into an array of words
            var searchTerms = haystack.Trim().Split(' ')
                .Select(Utility.ToLiteral)
                .Select(x => x.ToLower())
                .Distinct()
                .ToList();

            var clauses = new List<string>();

            //build a dynamic query
            var attrs = Utility.GetHaystackAttributes<T>();
            foreach (var term in searchTerms)
            {
                foreach (var haystackProperty in attrs)
                {
                    var path = haystackProperty.Path;
                    var type = Utility.GetPropertyType(instance, path);
                    var route = Utility.TokenizeRoute(path.Split('.'), 0, 11, instance);

                    var accessor = Utility.BuildAccessor(haystackProperty, type, route);
                    var subQueryBuilder = new StringBuilder();

                    //if provided, add the visibility clause
                    if (!string.IsNullOrWhiteSpace(haystackProperty.IsVisible))
                    {
                        subQueryBuilder.Append($"{haystackProperty.IsVisible}&&");
                    }

                    subQueryBuilder.Append("(");

                    //add a clause for this search term and this property
                    subQueryBuilder.Append($"{accessor.Replace("***", $".Contains({term})")}");

                    //store clause
                    clauses.Add($"{subQueryBuilder})");
                }
            }

            //union the clauses producing a new query
            var filteredQuery = baseQuery.Where(x => false);

            return clauses.Aggregate(filteredQuery,
                (current, clause) => current.Union(baseQuery.Where(clause)));
        }

        /// <summary>
        ///     Returns the paths matched by a haystack search and their values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public static List<Match> GetMatches<T>(T item, string search) where T : new()
        {
            var matches = new List<Match>();
            var terms = search.ToLower().Split(' ');

            var set = new List<T>
            {
                item
            };

            var attrs = Utility.GetHaystackAttributes<T>();
            foreach (var property in attrs)
            {
                if (!string.IsNullOrWhiteSpace(property.IsVisible))
                {
                    if (!set.AsQueryable().Where(property.IsVisible).Any())
                    {
                        continue;
                    }
                }

                var value = Utility.GetCompositePropertyValue(item, property.Path);
                matches.AddRange(terms.Where(term => value.ToLower().Contains(term))
                    .Select(term => new Match
                    {
                        Label = property.DisplayName,
                        Path = property.Path,
                        Value = value
                    }));
            }

            return matches;
        }

        /// <summary>
        ///     Returns the items in a collection matched by a haystack matches and their property values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public static IQueryable<T> FilterHaystack<T>(IQueryable<T> items, List<Match> matches) where T : new()
        {
            var attrs = Utility.GetHaystackAttributes<T>()
                .Where(p => !string.IsNullOrWhiteSpace(p.IsVisible) &&
                            matches.Any(m => m.Path == p.Path));
            return items.Where(i => matches.Any(m => m.Value == Utility.GetCompositePropertyValue(i, m.Path)));
        }
    }
}