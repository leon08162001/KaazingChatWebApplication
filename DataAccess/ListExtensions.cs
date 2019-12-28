using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Text;

namespace DataAccess
{
    public static class ListExtensions
    {
        public static void SortBy<T>(this List<T> list, params Expression<Func<T, IComparable>>[] keys)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (keys == null || !keys.Any())
                throw new ArgumentException("Must have at least one key!");
            list.Sort((x, y) => (from selector in keys
                                 let xVal = selector.Compile()(x)
                                 let yVal = selector.Compile()(y)
                                 select xVal.CompareTo(yVal))
                                 .FirstOrDefault(compared => compared != 0));
        }
        public static List<T> ApplySort<T>(this List<T> source, string strSort)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "Data source is empty.");
            }

            if (strSort == null)
            {
                return source;
            }

            var lstSort = strSort.Split(',');

            StringBuilder sortExpression = new StringBuilder("");

            foreach (var sortOption in lstSort)
            {
                if (sortOption.Trim().StartsWith("-"))
                {
                    sortExpression.Append(sortOption.Trim().Remove(0, 1) + " descending,");
                }
                else if (sortOption.Trim().StartsWith("+"))
                {
                    sortExpression.Append(sortOption.Trim().Remove(0, 1) + ",");
                }
                else
                {
                    sortExpression.Append(sortOption.Trim() + ",");
                }
            }
            string StringortExpression = sortExpression.ToString();
            if (!string.IsNullOrWhiteSpace(StringortExpression))
            {
                // Note: system.linq.dynamic NuGet package is required here to operate OrderBy on string
                source = source.OrderBy(StringortExpression.Remove(StringortExpression.Length - 1)).ToList();
            }
            return source;
        }
    }
}
