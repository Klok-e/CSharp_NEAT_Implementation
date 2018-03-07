using CSharpNEAT.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpNEAT.Core
{
    public interface IGenomeFactory<T> where T : IGenome
    {
        IList<T> CreateGenomeList(int population, Random random);

        void Mutate(Random generator, T genome, bool end);

        T Crossover(Random generator, T parent1, T parent2);

        T Clone(T toClone);

        void HandleComplexity(IList<T> genomes, GeneticAlgorithm.AlgorithmConfig algConfig);
    }
}
