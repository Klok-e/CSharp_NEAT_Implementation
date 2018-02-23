using MyNEAT.ActivationFunctions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyNEAT
{
    public class Config
    {
        public IActivationFunction activationOutp;
        public IActivationFunction activationNormal;

        private const float defaultConnWeightRange = 5;
        private const float defaultWeightChangeRange = 0.5f;
        private const float defaultProbabilityOfResetWeight = 0.05f;
        private const float defaultProbabilityOfChangeWeight = 0.95f;
        private const float defaultProbabilityAddNeuron = 0.01f;
        private const float defaultProbabilityAddConnection = 0.5f;
        private const float defaultProbabilityRemoveConnection = 0.4f;

        public int inputs;
        public int outputs;

        public float connWeightRange;
        public float probabilityAddConnection;
        public float probabilityAddNeuron;
        public float probabilityOfChangeWeight;
        public float probabilityOfResetWeight;
        public float probabilityRemoveConnection;
        public float weightChangeRange;

        public Config()
        {
            connWeightRange = defaultConnWeightRange;
            weightChangeRange = defaultWeightChangeRange;
            probabilityOfResetWeight = defaultProbabilityOfResetWeight;
            probabilityOfChangeWeight = defaultProbabilityOfChangeWeight;
            probabilityAddNeuron = defaultProbabilityAddNeuron;
            probabilityAddConnection = defaultProbabilityAddConnection;
            probabilityRemoveConnection = defaultProbabilityRemoveConnection;

            activationOutp = new Linear();
            activationNormal = new Tanh();
        }
    }
}
