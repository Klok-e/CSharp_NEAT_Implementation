using MyNEAT.ActivationFunctions;
using MyNEAT.Genome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNEAT.Decoder
{
    internal struct DNeuron
    {
        public IActivationFunction Activation { get; }
        public NeuronType Type { get; }

        public float _sum;

        public DNeuron(IActivationFunction activation, NeuronType type)
        {
            Activation = activation;
            _sum = type == NeuronType.bias ? 1 : 0;
            Type = type;
        }
    }

    internal struct DConnection
    {
        public int From { get; }
        public int To { get; }
        public float Weight { get; }

        public DConnection(float weight, int from, int to)
        {
            From = from;
            To = to;
            Weight = weight;
        }
    }
}
