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
    }

}
