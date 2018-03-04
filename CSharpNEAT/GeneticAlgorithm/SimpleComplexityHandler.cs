using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNEAT.Genome;

namespace MyNEAT.GeneticAlgorithm
{
    public class SimpleComplexityHandler : IComplexityHandler
    {
        public SimpleComplexityHandler()
        {
        }

        public void HandleComplexity(IList<IGenome> genomes, AlgorithmConfig conf)
        {
            if (conf.IsAdaptive)
            {
                int meanComplexity = 0;
                foreach (var item in genomes)
                    meanComplexity += item.Complexity;
                meanComplexity /= genomes.Count;

                if (meanComplexity > conf.maxComplexity)
                {
                    conf.crossoverChance = 0.1f;
                    conf.probabilityAddConnection = 0.1f;
                    conf.probabilityAddNeuron = 0.01f;
                    conf.probabilityRemoveConnection = 0.9f;
                }
                else if (meanComplexity < conf.maxComplexity)
                {
                    conf.crossoverChance = 0.5f;
                    conf.probabilityAddConnection = 0.8f;
                    conf.probabilityAddNeuron = 0.5f;
                    conf.probabilityRemoveConnection = 0.1f;
                }
            }
        }
    }
}
