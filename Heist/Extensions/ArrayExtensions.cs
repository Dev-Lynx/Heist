using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heist.Extensions
{
    public static class ArrayExtensions
    {
        public static bool ContainsAny(this string[] source, out int index, params string[] values)
        {
            foreach (var value in values)
            {
                index = Array.FindIndex(source, x => x == value); ;
                if (index < 0) continue;

                return true;
            }

            index = -1;
            return false;
        }
    }
}
