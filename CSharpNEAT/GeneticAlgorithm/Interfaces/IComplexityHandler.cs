using MyNEAT.Genome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNEAT.GeneticAlgorithm
{
    public interface IComplexityHandler
    {
        void HandleComplexity(IList<IGenome> genomes, AlgorithmConfig config);
    }
}
