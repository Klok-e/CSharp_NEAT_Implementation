using CSharpNEAT.ActivationFunctions;
using CSharpNEAT.NEAT;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpNEAT
{
    public class GenomeConfig
    {
        public readonly IActivationFunction activationOutp;
        public readonly IActivationFunction activationNormal;

        public ulong _geneIndex;
        public readonly MutationBuffer _mutationBuffer;

        public int inputs;
        public int outputs;

        public GenomeConfig()
        {
            activationOutp = new Linear();
            activationNormal = new Tanh();
            _mutationBuffer = new MutationBuffer();
        }
    }
}
