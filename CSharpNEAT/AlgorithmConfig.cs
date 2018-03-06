using CSharpNEAT.GeneticAlgorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpNEAT
{
    public class AlgorithmConfig
    {
        public float connWeightRange;
        public float probabilityAddConnection;
        public float probabilityAddNeuron;
        public float probabilityOfChangeWeight;
        public float probabilityOfResetWeight;
        public float probabilityRemoveConnection;
        public float weightChangeRange;

        public int maxComplexity;
        public float elitism;
        public float crossoverChance;
        public int mutationAmount;

        public bool IsAdaptive { get; }
        public IComplexityHandler ComplexityHandler { get; }

        public AlgorithmConfig()
        {
            connWeightRange = 5;
            weightChangeRange = 0.5f;
            probabilityOfResetWeight = 0.05f;
            probabilityOfChangeWeight = 0.95f;
            probabilityAddNeuron = 0.01f;
            probabilityAddConnection = 0.5f;
            probabilityRemoveConnection = 0.4f;
            IsAdaptive = true;
            maxComplexity = 30;
            elitism = 0.5f;
            crossoverChance = 0.5f;
            mutationAmount = 1;
            ComplexityHandler = new SimpleComplexityHandler();
        }
    }
}
