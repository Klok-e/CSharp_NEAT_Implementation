using CSharpNEAT.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpNEAT.Core
{
    public interface IEvaluator<T> where T : IGenome
    {
        void Evaluate(IList<T> genomes);
    }
}
