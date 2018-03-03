using MyNEAT.ActivationFunctions;
using MyNEAT.Genome;
using MyNEAT.Genome.NEAT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNEAT.Decoder.NEAT
{
    internal struct DNeuron
    {
        public IActivationFunction Activation { get; }
        public NeuronType Type { get; }
        public int ConnectionsOutgoing { get; }

        public float _sum;
        public int _connectionsOutgoingTransferred;

        public float Activate()
        {
            return Activation.Eval(_sum);
        }

        public DNeuron(IActivationFunction activation, NeuronType type, int connectionsOutgoing)
        {
            Activation = activation;
            _sum = type == NeuronType.bias ? 1 : 0;
            Type = type;
            _connectionsOutgoingTransferred = 0;
            ConnectionsOutgoing = connectionsOutgoing;
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
