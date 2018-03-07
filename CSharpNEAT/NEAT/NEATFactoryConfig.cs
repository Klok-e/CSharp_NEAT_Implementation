using CSharpNEAT.ActivationFunctions;
using CSharpNEAT.NEAT;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpNEAT.NEAT
{
    public class NEATFactoryConfig
    {
        public readonly IActivationFunction activationOutp;
        public readonly IActivationFunction activationNormal;

        public float connWeightRange;
        public float probabilityAddConnection;
        public float probabilityAddNeuron;
        public float probabilityOfChangeWeight;
        public float probabilityOfResetWeight;
        public float probabilityRemoveConnection;
        public float weightChangeRange;

        public bool isAdaptive;
        public int maxComplexity;

        public int inputs;
        public int outputs;

        public NEATFactoryConfig()
        {
            isAdaptive = true;

            maxComplexity = 50;

            connWeightRange = 5;
            weightChangeRange = 0.5f;
            probabilityOfResetWeight = 0.05f;
            probabilityOfChangeWeight = 0.95f;
            probabilityAddNeuron = 0.01f;
            probabilityAddConnection = 0.5f;
            probabilityRemoveConnection = 0.4f;

            activationOutp = new Linear();
            activationNormal = new Tanh();
        }
    }
}
