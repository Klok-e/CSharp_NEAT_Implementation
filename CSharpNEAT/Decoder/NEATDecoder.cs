using MyNEAT.ActivationFunctions;
using MyNEAT.Genome;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyNEAT.Decoder
{
    public class NEATDecoder : IDecoder
    {
        public IBlackBox Decode(IGenome genome)
        {
            var neatGenome = (NEATGenome)genome;
            var depthInfo = NEATGenome.GetDepthsOfNetwork(neatGenome);

            //remove all isolated neurons and sort them by depth
            var neuronsSorted = new List<GNeuron>(neatGenome.Neurons);
            neuronsSorted.RemoveAll((x) =>
            {
                var r = !depthInfo.Neurons.TryGetValue(x, out uint v) && x.Type == NeuronType.hidden;
                return r;
            });
            neuronsSorted.Sort((x, y) => depthInfo.Neurons[x].CompareTo(depthInfo.Neurons[y]));

            //remove all isolated connections and sort them by depth
            var connsSorted = new List<GConnection>(neatGenome.Сonnections);
            connsSorted.RemoveAll((x) =>
            {
                return !depthInfo.Connections.TryGetValue(x, out uint v);
            });
            connsSorted.Sort((x, y) => { return depthInfo.Connections[x].CompareTo(depthInfo.Connections[y]); });

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
            for (int i = 0; i < outputs.Length; i++)
            {
                outpIndices[i] = GetIndex(neuronsSorted, outputs[i].Id);
            }

            return new NEATNetwork(decodedConns, decodedNeurons, inpIndices, outpIndices);
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
    }
}
