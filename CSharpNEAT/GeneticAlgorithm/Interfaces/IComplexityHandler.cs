using CSharpNEAT.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpNEAT.GeneticAlgorithm
{
    public interface IComplexityHandler
    {
        void HandleComplexity(IList<IGenome> genomes, AlgorithmConfig config);
    }
}
