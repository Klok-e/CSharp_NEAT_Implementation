using System;

namespace CSharpNEAT.Core
{
    public interface IGenome
    {
        float Fitness { get; set; }

        int Complexity { get; }

        void Mutate(Random generator, AlgorithmConfig config, bool end);

        IGenome Crossover(Random generator, IGenome other);

        IGenome Clone();
    }
}
