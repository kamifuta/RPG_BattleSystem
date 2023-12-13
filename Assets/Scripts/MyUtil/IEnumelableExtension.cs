using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyUtil
{
    public static class IEnumelableExtension
    {
        public static T RandomGet<T>(this IEnumerable<T> list)
        {
            var rand = new Random();
            return list.ElementAt(rand.Next(list.Count()));
        }

        public static string Enumerate<T>(this IEnumerable<T> list)
        {
            StringBuilder ret = new StringBuilder();
            foreach(var e in list)
            {
                ret.Append($"{e}/");
            }
            return ret.ToString();
        }

        public static IEnumerable<IEnumerable<T>> Combination<T>(this IEnumerable<T> list, int k)
        {
            if (k == 0)
            {
                yield return Enumerable.Empty<T>();
            }
            else
            {
                int i = 1;
                foreach (var element in list)
                {
                    var skippedList = list.Skip(i);
                    foreach (var combinationList in Combination(skippedList, k - 1))
                        yield return combinationList.Prepend(element);

                    i++;
                }
            }
        }

        public static IEnumerable<IEnumerable<T>> Combination<T>(this IEnumerable<T> list, T containElement, int k)
        {
            IEnumerable<T> operatedList = list.Where(x => !x.Equals(containElement));
            foreach (var combinationList in Combination(operatedList, k - 1))
            {
                yield return combinationList.Prepend(containElement);
            }
        }

        public static IEnumerable<IEnumerable<T>> Combination<T>(this IEnumerable<T> list, T containElement, IEnumerable<T> exceptElements, int k)
        {
            IEnumerable<T> operatedList = list.Where(x => !x.Equals(containElement)).Except(exceptElements);
            foreach (var combinationList in Combination(operatedList, k - 1))
            {
                yield return combinationList.Prepend(containElement);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach(var e in list)
            {
                action.Invoke(e);
            }
        }
    }

}
