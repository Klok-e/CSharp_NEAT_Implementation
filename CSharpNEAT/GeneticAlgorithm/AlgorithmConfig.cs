using CSharpNEAT.Core;
using CSharpNEAT.GeneticAlgorithm;

namespace CSharpNEAT.GeneticAlgorithm
{
    public class AlgorithmConfig
    {
        public float elitism;
        public float crossoverChance;
        public int mutationAmount;

        public AlgorithmConfig()
        {
            elitism = 0.5f;
            crossoverChance = 0.5f;
            mutationAmount = 1;
        }
    }
}
