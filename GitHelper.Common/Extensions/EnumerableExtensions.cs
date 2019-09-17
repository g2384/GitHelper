using System;
using System.Collections.Generic;
using System.Linq;

namespace GitHelper.Common.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool None<T>(this IEnumerable<T> value)
        {
            return !value.Any();
        }

        public static bool None<T>(this IEnumerable<T> value, Func<T, bool> predicate)
        {
            return !value.Any(predicate);
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> value)
        {
            return value == null || value.None();
        }

        public static bool HasAtLeast<T>(this IEnumerable<T> value, int count)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            return value.Skip(count - 1).Any();
        }

        public static IEnumerable<T> FlattenHierarchy<T>(this T root, Func<T, IEnumerable<T>> childSource, bool includeRoot = true)
        {
            if (includeRoot) yield return root;
            foreach (var child in childSource(root).SelectMany(i => FlattenHierarchy(i, childSource)))
            {
                yield return child;
            }
        }

        public static T Single<T>(this IEnumerable<T> items, Func<T, bool> predicate, Action noMatchAction, Action multipleMatchAction)
        {
            predicate.ArgumentNullCheck(nameof(predicate));
            noMatchAction.ArgumentNullCheck(nameof(noMatchAction));
            multipleMatchAction.ArgumentNullCheck(nameof(multipleMatchAction));

            var matches = items.ArgumentNullCheck(nameof(items)).Where(predicate);

            var count = 0;
            var firstMatch = default(T);
            foreach (var match in matches)
            {
                if (++count == 1)
                {
                    firstMatch = match;
                }
                if (count > 1)
                {
                    multipleMatchAction();
                    break;
                }
            }
            if (count == 0)
            {
                noMatchAction();
            }
            return firstMatch;
        }
    }
}
