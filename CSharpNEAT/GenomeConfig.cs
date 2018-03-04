using MyNEAT.ActivationFunctions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyNEAT
{
    public class GenomeConfig
    {
        public IActivationFunction activationOutp;
        public IActivationFunction activationNormal;

        public int inputs;
        public int outputs;

        public GenomeConfig()
        {
            activationOutp = new Linear();
            activationNormal = new Tanh();
        }
    }
}
