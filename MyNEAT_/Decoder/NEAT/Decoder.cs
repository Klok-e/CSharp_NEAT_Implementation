using MyNEAT.ActivationFunctions;
using MyNEAT.Genome;
using MyNEAT.Genome.NEAT;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyNEAT.Decoder.NEAT
{
    public class NEATDecoder : IDecoder
    {
        public IBlackBox Decode(IGenome genome)
        {
            var neatGenome = (NEATGenome)genome;
            var depthInfo = DepthCalculator.GetDepthsToNetwork(neatGenome);

            var neuronsSorted = new List<GNeuron>();
            neuronsSorted.Sort((x, y) => depthInfo.Neurons[x].CompareTo(depthInfo.Neurons[y]));

            var connsSorted = new List<GConnection>();
            connsSorted.Sort((x, y) => depthInfo.Connections[x].CompareTo(depthInfo.Connections[y]));

            //neurons
            var decodedNeurons = new DNeuron[neuronsSorted.Count];
            for (int i = 0; i < decodedNeurons.Length; i++)
            {
                NEATGenome.FindAmountOfInAndOutConnectionsForNeuronWithId(connsSorted, neuronsSorted[i].Id, out var sumIn, out var sumOut);
                decodedNeurons[i] = new DNeuron(neuronsSorted[i].Activation, neuronsSorted[i].Type, sumOut);
            }

            //connections
            var decodedConns = new DConnection[connsSorted.Count];
            for (int i = 0; i < decodedConns.Length; i++)
            {
                //DConnection holds indices from decodedNeurons array
                decodedConns[i] = new DConnection(connsSorted[i].Weight,
                    GetIndex(neuronsSorted, connsSorted[i].FromNeuron),
                    GetIndex(neuronsSorted, connsSorted[i].ToNeuron));
            }

            //obtain inp/outp indices
            var inputs = neuronsSorted.Where((x) => x.Type == NeuronType.input).ToArray();
            var inpIndices = new int[inputs.Length];
            for (int i = 0; i < inputs.Length; i++)
            {
                inpIndices[i] = GetIndex(neuronsSorted, inputs[i].Id);
            }

            var outputs = neuronsSorted.Where((x) => x.Type == NeuronType.output).ToArray();
            var outpIndices = new int[outputs.Length];
            for (int i = 0; i < inputs.Length; i++)
            {
                outpIndices[i] = GetIndex(neuronsSorted, outputs[i].Id);
            }

            return new Network(decodedConns, decodedNeurons, inpIndices, outpIndices);
        }

        private static int GetIndex(IList<GNeuron> nodes, ulong toFind)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Id == toFind)
                    return i;
            }
            throw new Exception("could not find");
        }

        private static class DepthCalculator
        {
            public static DepthInfo GetDepthsToNetwork(NEATGenome genome)
            {
                var inputs = genome._neurons.Where((x) => x.Type == NeuronType.input).ToList();
                var info = new DepthInfo();
                foreach (var item in inputs)
                {
                    RecursiveDepthSet(genome, item, info, 0);
                }

                return info;
            }

            private static void RecursiveDepthSet(NEATGenome genome, GNeuron neuron, DepthInfo depthInfo, int depth)
            {
                NEATGenome.GetListOfInAndOutConnections(genome._connections, neuron.Id, out var inConn, out var outConn);
                foreach (var outgoing in outConn)
                {
                    var conn = (GConnection)NEATGenome.GetNodeById(genome._connections.Cast<IGNode>().ToList(), outgoing);
                    var toNeuron = (GNeuron)NEATGenome.GetNodeById(genome._neurons.Cast<IGNode>().ToList(), conn.ToNeuron);
                    RecursiveDepthSet(genome, toNeuron, depthInfo, depth + 1);

                    if (depthInfo.Connections.ContainsKey(conn))
                    {
                        depthInfo.Connections[conn] = Math.Max(depthInfo.Connections[conn], depth);
                    }
                    else
                    {
                        depthInfo.Connections.Add(conn, depth);
                    }
                }
                if (depthInfo.Neurons.ContainsKey(neuron))
                {
                    depthInfo.Neurons[neuron] = Math.Max(depthInfo.Neurons[neuron], depth);
                }
                else
                {
                    depthInfo.Neurons.Add(neuron, depth);
                }
            }

            public class DepthInfo
            {
                public Dictionary<GNeuron, int> Neurons { get; }
                public Dictionary<GConnection, int> Connections { get; }

                public DepthInfo()
                {
                    Neurons = new Dictionary<GNeuron, int>();
                    Connections = new Dictionary<GConnection, int>();
                }
            }
        }
    }
}
