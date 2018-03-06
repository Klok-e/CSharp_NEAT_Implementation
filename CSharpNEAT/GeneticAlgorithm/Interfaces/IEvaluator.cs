using CSharpNEAT.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpNEAT.GeneticAlgorithm
{
    public interface IEvaluator
    {
        void Evaluate(IList<IGenome> genomes);
    }
}
