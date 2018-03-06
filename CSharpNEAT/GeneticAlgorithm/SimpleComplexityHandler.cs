using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpNEAT.Core;

namespace CSharpNEAT.GeneticAlgorithm
{
    public class SimpleComplexityHandler : IComplexityHandler
    {
        public SimpleComplexityHandler()
        {
        }

        public void HandleComplexity(IList<IGenome> genomes, AlgorithmConfig config)
        {
            if (config.IsAdaptive)
            {
                int meanComplexity = 0;
                foreach (var item in genomes)
                    meanComplexity += item.Complexity;
                meanComplexity /= genomes.Count;

                if (meanComplexity > config.maxComplexity)
                {
                    config.crossoverChance = 0.01f;
                    config.probabilityAddConnection = 0.1f;
                    config.probabilityAddNeuron = 0.01f;
                    config.probabilityRemoveConnection = 0.9f;
                }
                else if (meanComplexity < config.maxComplexity)
                {
                    config.crossoverChance = 0.4f;
                    config.probabilityAddConnection = 0.8f;
                    config.probabilityAddNeuron = 0.5f;
                    config.probabilityRemoveConnection = 0.1f;
                }
            }
        }
    }
}
