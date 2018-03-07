using System;
using System.Collections.Generic;
using System.Linq;
using CSharpNEAT.ActivationFunctions;
using CSharpNEAT.Core;
using CSharpNEAT.GeneticAlgorithm;

namespace CSharpNEAT.NEAT
{
    public class NEATGenomeFactory<T> : IGenomeFactory<T> where T : NEATGenome
    {
        private NEATFactoryConfig _config;
        private readonly MutationBuffer _mutationBuffer;
        private ulong _geneIndex;
        private static readonly IActivationFunction ActivInput = new Linear();
        private static readonly IActivationFunction ActivHidden = new Tanh();
        private static readonly IActivationFunction ActivOutput = new Tanh();

        public NEATGenomeFactory(NEATFactoryConfig config)
        {
            _config = config;
            _mutationBuffer = new MutationBuffer();
        }

        #region IGenomeFactory

        public void HandleComplexity(IList<T> genomes, AlgorithmConfig algConfig)
        {
            if (_config.isAdaptive)
            {
                int meanComplexity = 0;
                foreach (var item in genomes)
                    meanComplexity += item.Complexity;
                meanComplexity /= genomes.Count;

                if (meanComplexity > _config.maxComplexity)
                {
                    algConfig.crossoverChance = 0.01f;
                    _config.probabilityAddConnection = 0.1f;
                    _config.probabilityAddNeuron = 0.01f;
                    _config.probabilityRemoveConnection = 0.9f;
                }
                else if (meanComplexity < _config.maxComplexity)
                {
                    algConfig.crossoverChance = 0.4f;
                    _config.probabilityAddConnection = 0.8f;
                    _config.probabilityAddNeuron = 0.5f;
                    _config.probabilityRemoveConnection = 0.1f;
                }
            }
        }

        public IList<T> CreateGenomeList(int population, Random random)
        {
            var pop = new List<T>(population);
            for (int i = 0; i < population; i++)
            {
                var (connections, neurons) = CreateInitialArrangement(random, _config);
                pop.Add((T)new NEATGenome(connections, neurons));
            }
            if (pop.Any())
                _geneIndex = (ulong)pop[0].Neurons.Count + (ulong)pop[0].Сonnections.Count + 1;
            return pop;
        }

        public T Clone(T toClone)
        {
            return (T)new NEATGenome(toClone.Сonnections, toClone.Neurons);
        }

        public T Crossover(Random generator, T parent1, T parent2)
        {
            var neurons = new List<GNeuron>();

            #region Build neurons

            neurons.AddRange(parent1.Neurons.Concat(parent2.Neurons));
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

            connections.AddRange(parent1.Сonnections.Concat(parent2.Сonnections));
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
            return (T)new NEATGenome(connections, neurons);
        }

        public void Mutate(Random generator, T genome, bool end)
        {
#if DEBUG
            FindDuplicates(genome.Сonnections.Cast<IGNode>().ToList(), genome.Neurons.Cast<IGNode>().ToList());
            if (!IsGWithIdExistsInList(genome.Neurons, 1))
                throw new Exception();
#endif
            if (generator.NextDouble() < _config.probabilityOfChangeWeight)
                MutationChangeWeight(generator, genome);
            if (generator.NextDouble() < _config.probabilityAddNeuron)
                MutationAddNeuron(generator, genome);
            if (generator.NextDouble() < _config.probabilityAddConnection)
                MutationAddConnection(generator, genome);
            if (generator.NextDouble() < _config.probabilityRemoveConnection)
                MutationRemoveConnection(generator, genome);
            if (end)
            {
                _mutationBuffer.Clear();
            }
#if DEBUG
            FindDuplicates(genome.Сonnections.Cast<IGNode>().ToList(), genome.Neurons.Cast<IGNode>().ToList());
            if (!IsGWithIdExistsInList(genome.Neurons, 1))
                throw new Exception();
#endif
        }

        #endregion IGenomeFactory

        #region Static

        #region Mutators

        private void MutationChangeWeight(Random generator, T genome)
        {
            if (genome.Сonnections.Count > 0)
            {
                var randConn = generator.RandChoice(genome.Сonnections, out int num);
                if (generator.NextDouble() < _config.probabilityOfResetWeight)
                    genome.Сonnections[num] = new GConnection(randConn.FromNeuron, randConn.ToNeuron,
                        generator.RandRange(-_config.connWeightRange, _config.connWeightRange),
                        randConn.Id);
                else
                    genome.Сonnections[num] = new GConnection(randConn.FromNeuron, randConn.ToNeuron,
                        generator.RandRange(-_config.weightChangeRange, _config.weightChangeRange) + randConn.Weight,
                        randConn.Id);
            }
        }

        private void MutationAddNeuron(Random generator, T genome)
        {
            if (genome.Сonnections.Count > 0)
            {
                var randConn = generator.RandChoice(genome.Сonnections, out var ind);
                genome.Сonnections.RemoveAt(ind);

                if (_mutationBuffer.IsInsideBuffer(randConn.FromNeuron, randConn.ToNeuron, out var conn1, out var neu, out var conn2))
                {
                    var newNeuron = new GNeuron(neu, NeuronType.hidden, ActivHidden);
                    genome.Neurons.Add(newNeuron);

                    var newConnIn = new GConnection(randConn.FromNeuron, newNeuron.Id, 1, conn1);
                    genome.Сonnections.Add(newConnIn);

                    var newConnOut = new GConnection(newNeuron.Id, randConn.ToNeuron, randConn.Weight, conn2);
                    genome.Сonnections.Add(newConnOut);
                }
                else
                {
                    var newNeuron = new GNeuron(_geneIndex++, NeuronType.hidden, ActivHidden);
                    genome.Neurons.Add(newNeuron);

                    var newConnIn = new GConnection(randConn.FromNeuron, newNeuron.Id, 1, _geneIndex++);
                    genome.Сonnections.Add(newConnIn);

                    var newConnOut = new GConnection(newNeuron.Id, randConn.ToNeuron, randConn.Weight, _geneIndex++);
                    genome.Сonnections.Add(newConnOut);

                    _mutationBuffer.AddToBuffer(newConnIn, newNeuron, newConnOut);
                }
            }
        }

        private void RemoveDisconnectedNeurons(T genome)
        {
            var toDel = new List<GNeuron>();
            for (var i = 0; i < genome.Neurons.Count; i++)
            {
                GetListOfInAndOutConnections(genome.Сonnections, genome.Neurons[i].Id,
                    out var neuIn, out var neuOut);
                if ((!neuIn.Any() && !neuOut.Any()) && genome.Neurons[i].Type == NeuronType.hidden)
                {
                    toDel.Add(genome.Neurons[i]);
                }
            }
            foreach (var item in toDel)
            {
                genome.Neurons.Remove(item);
            }
        }

        private void MutationAddConnection(Random generator, T genome)
        {
#if DEBUG
            FindDuplicates(genome.Сonnections.Cast<IGNode>().ToList(), genome.Neurons.Cast<IGNode>().ToList());
#endif
            var neuron1 = generator.RandChoice(genome.Neurons.Where((x) => x.Type != NeuronType.output).ToList());
            var neuron2 = generator.RandChoice(genome.Neurons.Where((x) => x.Type != NeuronType.input && x.Type != NeuronType.bias).ToList());

            GetListOfInAndOutConnections(genome.Сonnections, neuron1.Id, out var inConn1, out var outConn1);
            GetListOfInAndOutConnections(genome.Сonnections, neuron2.Id, out var inConn2, out var outConn2);
            if (!inConn1.Intersect(outConn2).Any() && !inConn2.Intersect(outConn1).Any())
            {
                if (_mutationBuffer.IsInsideBuffer(neuron1.Id, neuron2.Id, out var existingIdConn))
                {
                    genome.Сonnections.Add(new GConnection(neuron1.Id, neuron2.Id,
                       generator.RandRange(-_config.connWeightRange, _config.connWeightRange),
                       existingIdConn));
                }
                else
                {
                    var conn = new GConnection(neuron1.Id, neuron2.Id,
                        generator.RandRange(-_config.connWeightRange, _config.connWeightRange),
                        _geneIndex++);
                    genome.Сonnections.Add(conn);
                    _mutationBuffer.AddToBuffer(conn);
                }
            }
#if DEBUG
            FindDuplicates(genome.Сonnections.Cast<IGNode>().ToList(), genome.Neurons.Cast<IGNode>().ToList());
#endif
        }

        private void MutationRemoveConnection(Random generator, T genome)
        {
            if (genome.Сonnections.Count > 0)
            {
                generator.RandChoice(genome.Сonnections, out var ind);
                genome.Сonnections.RemoveAt(ind);

                //TODO: fix this fast fix
                RemoveDisconnectedNeurons(genome);
            }
        }

        #endregion Mutators

        private static (List<GConnection> connections, List<GNeuron> neurons) CreateInitialArrangement(Random random, NEATFactoryConfig config)
        {
            if (config.inputs == 0 || config.outputs == 0) throw new Exception("fuck you");

            var inputs = config.inputs;
            var outputs = config.outputs;

            var neurons = new List<GNeuron>(inputs + outputs);
            var connections = new List<GConnection>(inputs * outputs);

            ulong index = 1;
            for (var i = 0; i < inputs; i++) //only inputs
            {
                neurons.Add(new GNeuron(index++, NeuronType.input, ActivInput));
            }

            var biasNeuron = new GNeuron(index++, NeuronType.bias, ActivInput);
            neurons.Add(biasNeuron);

            for (var i = 0; i < outputs; i++) //only output neurons
            {
                neurons.Add(new GNeuron(index++, NeuronType.output, ActivOutput));
            }

            foreach (var neuron in neurons)
            {
                if (neuron.Type == NeuronType.output)
                {
                    foreach (var neuron1 in neurons)
                    {
                        if (neuron1.Type == NeuronType.input || neuron1.Type == NeuronType.bias)
                        {
                            connections.Add(new GConnection(neuron1.Id, neuron.Id,
                                random.RandRange(-5, 5),
                                index++));
                        }
                    }
                }
            }
            return (connections, neurons);
        }

        private static void FindDuplicates<K>(List<K> x, List<K> y) where K : IGNode
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

        public static bool IsGWithIdExistsInList(IEnumerable<IGNode> lst, ulong id)
        {
            foreach (var neuron in lst)
            {
                if (neuron.Id == id)
                    return true;
            }

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

        public static class DepthCalculator
        {
            private static void RecursiveDepthSet(NEATGenome gnm, GNeuron neuron, DepthInfo depthInfo, uint depth)
            {
                if (depthInfo.Neurons.ContainsKey(neuron))
                {
                    //if current depth is smaller than previous then stop traversing this path
                    if (depthInfo.Neurons[neuron] > depth)
                        depthInfo.Neurons[neuron] = depth;
                    else
                        return;
                }
                else
                {
                    depthInfo.Neurons.Add(neuron, depth);
                }

                GetListOfInAndOutConnections(gnm.Сonnections, neuron.Id, out var inConn, out var outConn);
                foreach (var outgoing in outConn)
                {
                    var conn = (GConnection)GetNodeById(gnm.Сonnections.Cast<IGNode>().ToList(), outgoing);
                    var toNeuron = (GNeuron)GetNodeById(gnm.Neurons.Cast<IGNode>().ToList(), conn.ToNeuron);
                    RecursiveDepthSet(gnm, toNeuron, depthInfo, depth + 1);

                    if (depthInfo.Connections.ContainsKey(conn))
                        depthInfo.Connections[conn] = depth + 1;
                    else
                        depthInfo.Connections.Add(conn, depth + 1);
                }
            }

            private static void LinearDepthSet(NEATGenome gnm, IEnumerable<GNeuron> startingNeurons, out DepthInfo depthInfo)
            {
                depthInfo = new DepthInfo();
                var currLayer = new List<GNeuron>(startingNeurons);
                var nextLayer = new List<GNeuron>();
                uint depthOfCurrLayer = 0;
                do
                {
                    currLayer.AddRange(nextLayer);
                    nextLayer.Clear();
                    foreach (var item in currLayer)
                    {
                        GetListOfInAndOutConnections(gnm.Сonnections, item.Id, out var inConn, out var outConn);
                        foreach (var outgoing in outConn)
                        {
                            var conn = (GConnection)GetNodeById(gnm.Сonnections.Cast<IGNode>().ToList(), outgoing);
                            var toNeuron = (GNeuron)GetNodeById(gnm.Neurons.Cast<IGNode>().ToList(), conn.ToNeuron);

                            if (!depthInfo.Neurons.ContainsKey(toNeuron))
                            {
                                depthInfo.Neurons.Add(toNeuron, depthOfCurrLayer + 1);
                                nextLayer.Add(toNeuron);
                            }

                            if (!depthInfo.Connections.ContainsKey(conn))
                                depthInfo.Connections.Add(conn, depthOfCurrLayer + 1);
                        }
                    }
                    currLayer.Clear();
                    depthOfCurrLayer++;
                }
                while (nextLayer.Count > 0);

                //if nothing connects to outputs add output neurons to dict with depth 0
                foreach (var item in gnm.Neurons.Where((x) => x.Type != NeuronType.hidden))
                {
                    if (!depthInfo.Neurons.ContainsKey(item))
                    {
                        depthInfo.Neurons.Add(item, 0);
                    }
                }
            }

            public static DepthInfo GetDepthsOfNetwork(NEATGenome genome)
            {
                var inputs = genome.Neurons.Where((x) => x.Type == NeuronType.input || x.Type == NeuronType.bias);
                LinearDepthSet(genome, inputs, out DepthInfo info);

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
        }

        #endregion Static
    }
}
