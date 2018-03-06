using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpNEAT
{
    internal static class Help
    {
        public static float RandRange(this Random generator, float min, float max)
        {
            return (float)generator.NextDouble() * (max - min) + min;
        }

        public static T RandChoice<T>(this Random generator, IList<T> list, out int ind)
        {
            ind = generator.Next(list.Count);
            return list[ind];
        }

        public static T RandChoice<T>(this Random generator, IList<T> list)
        {
            return list[generator.Next(list.Count)];
        }
    }
}
