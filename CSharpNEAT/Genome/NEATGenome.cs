using MyNEAT.ActivationFunctions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyNEAT.Genome
{
    public class NEATGenome : IGenome
    {
        private static readonly IActivationFunction ActivInput = new Linear();
        private static readonly IActivationFunction ActivHidden = new Tanh();
        private static readonly IActivationFunction ActivOutput = new Tanh();

        private static ulong _geneIndex = 0;
        private ulong GeneIndex { get { return ++_geneIndex; } }

        public float Fitness { get; set; }

        public List<GConnection> Сonnections { get; private set; }
        public List<GNeuron> Neurons { get; private set; }

        public override string ToString()
        {
            var str = "";
            for (var i = 0; i < Neurons.Count; i++)
                str += Neurons[i] + "\n";
            str += "\n";
            for (var i = 0; i < Сonnections.Count; i++)
                str += Сonnections[i] + "\n";

            return str;
        }

        #region Constructors

        public NEATGenome(Random generator, GenomeConfig config)
        {
            if (config.inputs == 0 || config.outputs == 0) throw new Exception("fuck you");

            var inputs = config.inputs;
            var outputs = config.outputs;

            Neurons = new List<GNeuron>(inputs + outputs);
            Сonnections = new List<GConnection>(inputs * outputs);

            for (var i = 0; i < inputs; i++) //only inputs
            {
                Neurons.Add(new GNeuron(GeneIndex, NeuronType.input, ActivInput));
            }

            var biasNeuron = new GNeuron(GeneIndex, NeuronType.bias, ActivInput);
            Neurons.Add(biasNeuron);

            for (var i = 0; i < outputs; i++) //only output neurons
            {
                Neurons.Add(new GNeuron(GeneIndex, NeuronType.output, ActivOutput));
            }

            foreach (var neuron in Neurons)
            {
                if (neuron.Type == NeuronType.output)
                {
                    foreach (var neuron1 in Neurons)
                    {
                        if (neuron1.Type == NeuronType.input || neuron1.Type == NeuronType.bias)
                        {
                            var conn = new GConnection(neuron1.Id, neuron.Id,
                                generator.RandRange(-5, 5),
                                GeneIndex);
                            Сonnections.Add(conn);
                        }
                    }
                }
            }
        }

        private NEATGenome()
        {
        }

        #endregion Constructors

        #region IGenome

        public int Complexity
        {
            get
            {
                return Сonnections.Count;
            }
        }

        public IGenome Clone()
        {
            var clone = new NEATGenome()
            {
                Сonnections = new List<GConnection>(Сonnections),
                Neurons = new List<GNeuron>(Neurons),
            };

            return clone;
        }

        public IGenome Crossover(Random generator, IGenome other)
        {
            var parent2 = (NEATGenome)other;

            var neurons = new List<GNeuron>();

            #region Build neurons

            neurons.AddRange(Neurons.Concat(parent2.Neurons));
            neurons.Sort((x, y) => x.Id.CompareTo(y.Id));
            for (int i = neurons.Count - 1; i > 0; i--)
            {
                if (neurons[i].Id == neurons[i - 1].Id)
                {
                    neurons.RemoveAt(i - generator.Next(2));
                }
            }

            #endregion Build neurons

            var connections = new List<GConnection>();

            #region Build connections

            connections.AddRange(Сonnections.Concat(parent2.Сonnections));
            connections.Sort((x, y) => x.Id.CompareTo(y.Id));
            for (int i = connections.Count - 1; i > 0; i--)
            {
                if (connections[i].Id == connections[i - 1].Id)
                {
                    connections.RemoveAt(i - generator.Next(2));
                }
            }

            #endregion Build connections

#if DEBUG
            FindDuplicates(neurons, neurons);
            FindDuplicates(connections, connections);
            FindDuplicates(neurons.Cast<IGNode>().ToList(), connections.Cast<IGNode>().ToList());
#endif
            var child = new NEATGenome()
            {
                Neurons = new List<GNeuron>(neurons),
                Сonnections = new List<GConnection>(connections)
            };
            return child;
        }

        public void Mutate(Random generator, AlgorithmConfig conf)
        {
#if DEBUG
            FindDuplicates(Сonnections.Cast<IGNode>().ToList(), Neurons.Cast<IGNode>().ToList());
#endif
            if (generator.NextDouble() < conf.probabilityOfChangeWeight)
                MutationChangeWeight(generator, conf);
            if (generator.NextDouble() < conf.probabilityAddNeuron)
                MutationAddNeuron(generator, conf);
            if (generator.NextDouble() < conf.probabilityAddConnection)
                MutationAddConnection(generator, conf);
            if (generator.NextDouble() < conf.probabilityRemoveConnection)
                MutationRemoveConnection(generator, conf);
#if DEBUG
            FindDuplicates(Сonnections.Cast<IGNode>().ToList(), Neurons.Cast<IGNode>().ToList());
#endif
        }

        #endregion IGenome

        #region Mutators

        private void MutationChangeWeight(Random generator, AlgorithmConfig conf)
        {
            if (Сonnections.Count > 0)
            {
                var randConn = generator.RandChoice(Сonnections, out int num);
                if (generator.NextDouble() < conf.probabilityOfResetWeight)
                    Сonnections[num] = new GConnection(randConn.FromNeuron, randConn.ToNeuron,
                        generator.RandRange(-conf.connWeightRange, conf.connWeightRange),
                        randConn.Id);
                else
                    Сonnections[num] = new GConnection(randConn.FromNeuron, randConn.ToNeuron,
                        generator.RandRange(-conf.weightChangeRange, conf.weightChangeRange) + randConn.Weight,
                        randConn.Id);
            }
        }

        private void MutationAddNeuron(Random generator, AlgorithmConfig conf)
        {
            if (Сonnections.Count > 0)
            {
                var randConn = generator.RandChoice(Сonnections, out var ind);
                Сonnections.RemoveAt(ind);

                var newNeuron = new GNeuron(GeneIndex, NeuronType.hidden, ActivHidden);
                Neurons.Add(newNeuron);

                var newConnIn = new GConnection(randConn.FromNeuron, newNeuron.Id, 1, GeneIndex);
                Сonnections.Add(newConnIn);

                var newConnOut = new GConnection(newNeuron.Id, randConn.ToNeuron, randConn.Weight, GeneIndex);
                Сonnections.Add(newConnOut);
            }
        }

        private void RemoveDisconnectedNeurons()
        {
            var toDel = new List<GNeuron>();
            for (var i = 0; i < Neurons.Count; i++)
            {
                GetListOfInAndOutConnections(Сonnections, Neurons[i].Id,
                    out var neuIn, out var neuOut);
                if ((neuIn.Count == 0 && neuOut.Count == 0) && Neurons[i].Type == NeuronType.hidden)
                {
                    toDel.Add(Neurons[i]);
                }
            }
            foreach (var item in toDel)
            {
                Neurons.Remove(item);
            }
        }

        private void MutationAddConnection(Random generator, AlgorithmConfig conf)
        {
#if DEBUG
            FindDuplicates(Сonnections.Cast<IGNode>().ToList(), Neurons.Cast<IGNode>().ToList());
#endif
            var neuron1 = generator.RandChoice(Neurons.Where((x) => { return x.Type != NeuronType.output; }).ToList());
            var neuron2 = generator.RandChoice(Neurons.Where((x) => { return x.Type != NeuronType.input && x.Type != NeuronType.bias; }).ToList());

            GetListOfInAndOutConnections(Сonnections, neuron1.Id, out var inConn1, out var outConn1);
            GetListOfInAndOutConnections(Сonnections, neuron2.Id, out var inConn2, out var outConn2);
            if (inConn1.Intersect(outConn2).Count() == 0 && inConn2.Intersect(outConn1).Count() == 0)
            {
                Сonnections.Add(new GConnection(neuron1.Id, neuron2.Id,
                    generator.RandRange(-conf.connWeightRange, conf.connWeightRange),
                    GeneIndex));
            }
#if DEBUG
            FindDuplicates(Сonnections.Cast<IGNode>().ToList(), Neurons.Cast<IGNode>().ToList());
#endif
        }

        private void MutationRemoveConnection(Random generator, AlgorithmConfig conf)
        {
            if (Сonnections.Count > 0)
            {
                generator.RandChoice(Сonnections, out var ind);
                Сonnections.RemoveAt(ind);

                //TODO: fix this fast fix
                RemoveDisconnectedNeurons();
            }
        }

        #endregion Mutators

        #region Static

        private static void FindDuplicates<T>(List<T> x, List<T> y) where T : IGNode
        {
            foreach (var n in x)
            {
                foreach (var n2 in y)
                {
                    if (n.Equals(n2)) continue;
                    if (n.Id == n2.Id)
                    {
                        throw new Exception("shi~");
                    }
                }
            }
        }

        public static bool IsGWithIdExistsInList(List<GNeuron> neurons, ulong id)
        {
            foreach (var neuron in neurons)
                if (neuron.Id == id)
                    return true;
            return false;
        }

        public static bool IsGWithIdExistsInList(List<GConnection> conns, ulong id)
        {
            foreach (var conn in conns)
                if (conn.Id == id)
                    return true;
            return false;
        }

        public static void FindAmountOfInAndOutConnectionsForNeuronWithId(List<GConnection> connectionList, ulong id, out int sumIn, out int sumOut)
        {
            sumIn = 0;
            sumOut = 0;
            foreach (var conn in connectionList)
            {
                if (conn.ToNeuron == id)
                    sumIn++;
                if (conn.FromNeuron == id)
                    sumOut++;
            }
        }

        public static void GetListOfInAndOutConnections(List<GConnection> connectionList, ulong id, out List<ulong> inConn, out List<ulong> outConn)
        {
            inConn = new List<ulong>();
            outConn = new List<ulong>();
            foreach (var conn in connectionList)
            {
                if (conn.ToNeuron == id)
                    inConn.Add(conn.Id);
                if (conn.FromNeuron == id)
                    outConn.Add(conn.Id);
            }
        }

        public static IGNode GetNodeById(List<IGNode> nodeList, ulong id)
        {
            foreach (var item in nodeList)
            {
                if (item.Id == id)
                {
                    return item;
                }
            }
            throw new Exception("not found");
        }

        public static DepthInfo GetDepthsOfNetwork(NEATGenome genome)
        {
            void RecursiveDepthSet(NEATGenome gnm, GNeuron neuron, DepthInfo depthInfo, uint depth)
            {
                if (depthInfo.Neurons.ContainsKey(neuron))
                {
                    //if current depth is larger than previous then stop traversing this path
                    if (depthInfo.Neurons[neuron] < depth)
                        return;
                    depthInfo.Neurons[neuron] = depth;
                }
                else
                {
                    depthInfo.Neurons.Add(neuron, depth);
                }

                NEATGenome.GetListOfInAndOutConnections(gnm.Сonnections, neuron.Id, out var inConn, out var outConn);
                foreach (var outgoing in outConn)
                {
                    var conn = (GConnection)NEATGenome.GetNodeById(gnm.Сonnections.Cast<IGNode>().ToList(), outgoing);
                    var toNeuron = (GNeuron)NEATGenome.GetNodeById(gnm.Neurons.Cast<IGNode>().ToList(), conn.ToNeuron);
                    RecursiveDepthSet(gnm, toNeuron, depthInfo, depth + 1);

                    if (depthInfo.Connections.ContainsKey(conn))
                        depthInfo.Connections[conn] = depth + 1;
                    else
                        depthInfo.Connections.Add(conn, depth + 1);
                }
            }

            var inputs = genome.Neurons.Where((x) => x.Type == NeuronType.input || x.Type == NeuronType.bias).ToList();
            var info = new DepthInfo();
            foreach (var item in inputs)
            {
                RecursiveDepthSet(genome, item, info, 0);
            }

            //if nothing connects to outputs add output neurons to dict with depth 0
            var outps = genome.Neurons.Where((x) => x.Type == NeuronType.output).ToList();
            foreach (var item in outps)
            {
                if (!info.Neurons.ContainsKey(item))
                {
                    info.Neurons.Add(item, 0);
                }
            }
            return info;
        }

        public class DepthInfo
        {
            public Dictionary<GNeuron, uint> Neurons { get; }
            public Dictionary<GConnection, uint> Connections { get; }

            public DepthInfo()
            {
                Neurons = new Dictionary<GNeuron, uint>();
                Connections = new Dictionary<GConnection, uint>();
            }
        }

        #endregion Static
    }
}
