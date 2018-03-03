using MyNEAT.Genome;
using MyNEAT.Genome.NEAT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNEAT.Decoder.NEAT
{
    internal class Network : IBlackBox
    {
        #region IBlackBox

        public float[] Inputs { get; }

        public float[] Outputs { get; }

        public void Activate()
        {
            for (int i = 0; i < Inputs.Length; i++)
            {
                _neurons[_inputIndices[i]]._sum = Inputs[i];
            }

            //do activation stuff
            for (int i = 0; i < _connections.Length; i++)
            {
                _neurons[_connections[i].To]._sum += _neurons[_connections[i].From].Activate() * _connections[i].Weight;

                _neurons[_connections[i].From]._connectionsOutgoingTransferred += 1;
                if (_neurons[_connections[i].From]._connectionsOutgoingTransferred == _neurons[_connections[i].From].ConnectionsOutgoing)
                {
                    _neurons[_connections[i].From]._sum = 0f;
                    _neurons[_connections[i].From]._connectionsOutgoingTransferred = 0;
                }
            }

            for (int i = 0; i < Outputs.Length; i++)
            {
                Outputs[i] = _neurons[_outputIndices[i]].Activate();
            }
        }

        public void Reset()
        {
            for (int i = 0; i < _neurons.Length; i++)
            {
                if (_neurons[i].Type != NeuronType.bias)
                    _neurons[i]._sum = 0f;
            }
        }

        #endregion IBlackBox

        private DConnection[] _connections { get; }
        private DNeuron[] _neurons { get; }

        private int[] _inputIndices;
        private int[] _outputIndices;

        public Network(DConnection[] connections, DNeuron[] neurons, int[] inputIndices, int[] outputIndices)
        {
            _connections = connections;
            _neurons = neurons;

            Inputs = new float[inputIndices.Length];
            Outputs = new float[outputIndices.Length];

            _inputIndices = inputIndices;
            _outputIndices = outputIndices;
        }
    }
}
