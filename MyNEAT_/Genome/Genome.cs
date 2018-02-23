using System;
using System.Collections.Generic;
using System.Linq;

namespace MyNEAT.Genome
{
    public class Genome
    {
        public Config _conf { get; }

        internal static ulong _geneIndex;

        public float _fitness;

        public List<GConnection> _connections { get; private set; }
        public List<GNeuron> _neurons { get; private set; }

        public override string ToString()
        {
            var str = "";
            for (var i = 0; i < _neurons.Count; i++)
                str += _neurons[i] + "\n";
            str += "\n";
            for (var i = 0; i < _connections.Count; i++)
                str += _connections[i] + "\n";

            return str;
        }

        public int GetComplexity()
        {
            return _connections.Count;
        }

        #region Constructors

        public Genome(Config conf, Random generator)
        {
            if (conf.inputs == 0 || conf.outputs == 0) throw new Exception("fuck you");

            _conf = conf ?? throw new Exception("conf = null");

            var inputs = conf.inputs;
            var outputs = conf.outputs;

            _geneIndex = 1;
            _neurons = new List<GNeuron>(inputs + outputs);
            _connections = new List<GConnection>(inputs * outputs);

            for (var i = 0; i < inputs; i++) //only inputs
            {
                _neurons.Add(new GNeuron(_geneIndex++, NeuronType.input));
            }

            var biasNeuron = new GNeuron(_geneIndex++, NeuronType.bias);
            _neurons.Add(biasNeuron);

            for (var i = 0; i < outputs; i++) //only output neurons
            {
                _neurons.Add(new GNeuron(_geneIndex++, NeuronType.output));
            }

            foreach (var neuron in _neurons)
                if (neuron.Type == NeuronType.output)
                    foreach (var neuron1 in _neurons)
                        if (neuron1.Type == NeuronType.input || neuron1.Type == NeuronType.bias)
                        {
                            var conn = new GConnection(neuron1.Id, neuron.Id,
                                generator.RandRange(-_conf.connWeightRange, _conf.connWeightRange),
                                _geneIndex++);
                            _connections.Add(conn);
                        }
        }

        private Genome(Config conf)
        {
            _conf = conf ?? throw new Exception("conf = null");
        }

        #endregion Constructors

        public Genome Clone()
        {
            var clone = new Genome(_conf)
            {
                _connections = new List<GConnection>(_connections),
                _neurons = new List<GNeuron>(_neurons),
                _fitness = _fitness * 0.9f,
            };

            return clone;
        }

        #region Reproduction

        /// <summary>
        ///     Asexual reproduction
        /// </summary>
        /// <returns></returns>
        public Genome CreateOffSpring(Random generator)
        {
            var offspring = new Genome
            {
                _neurons = new List<GNeuron>(_neurons),
                _connections = new List<GConnection>(_connections)
            };
            offspring = Mutate(generator, offspring);
            return offspring;
        }

        /// <summary>
        ///     Sexual reproduction
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="otherParent"></param>
        /// <returns></returns>
        public Genome CreateOffSpring(Random generator, Genome otherParent)
        {
            var offspring = Crossover(generator, this, otherParent);

            offspring = Mutate(generator, offspring);
            return offspring;
        }

        #endregion Reproduction

        #region Static methods

        public static Genome Crossover(Random generator, Genome parent1, Genome parent2)
        {
            void FindDuplicates<T>(List<T> neus) where T : G
            {
                foreach (var n in neus)
                {
                    foreach (var n2 in neus)
                    {
                        if (n == n2) continue;
                        if (n.Id == n2.Id)
                        {
                            throw new Exception("shi~");
                        }
                    }
                }
            }

            var neurons = new List<GNeuron>();

            #region Build neurons

            var neuronsSortedByCount = new List<List<GNeuron>>
            {
                parent1._neurons,
                parent2._neurons
            };
            neuronsSortedByCount.Sort((x, y) => x.Count.CompareTo(y.Count));//sorted by increasing

            for (int i = 0; i < neuronsSortedByCount[1].Count; i++)
            {
                if (i < neuronsSortedByCount[0].Count)
                {
                    int ind = generator.Next(0, 2);
                    if (!IsGWithIdExistsInList(neurons, neuronsSortedByCount[ind][i].Id))
                        neurons.Add(neuronsSortedByCount[ind][i]);
                }
                else
                {
                    if (!IsGWithIdExistsInList(neurons, neuronsSortedByCount[1][i].Id))
                        neurons.Add(neuronsSortedByCount[1][i]);//take from the longest list
                }
            }

            #endregion Build neurons

            var connections = new List<GConnection>(Math.Max(parent1._connections.Count, parent2._connections.Count));

            #region Build connections

            var connsSortedByCount = new List<List<GConnection>>
            {
                parent1._connections,
                parent2._connections
            };
            connsSortedByCount.Sort((x, y) => x.Count.CompareTo(y.Count));

            for (var i = 0; i < connsSortedByCount[1].Count; i++)
            {
                if (i < connsSortedByCount[0].Count)
                {
                    int ind = generator.Next(0, 2);
                    if (!IsGWithIdExistsInList(connections, connsSortedByCount[ind][i].Id))
                        connections.Add(connsSortedByCount[ind][i]);
                }
                else
                {
                    if (!IsGWithIdExistsInList(connections, connsSortedByCount[1][i].Id))
                        connections.Add(connsSortedByCount[1][i]);//take from the longest list
                }
            }

            //delete all impossible connections
            var toDelete = new List<GConnection>();
            foreach (var conn in connections)
            {
                if (!IsGWithIdExistsInList(neurons, conn.FromNeuron) || !IsGWithIdExistsInList(neurons, conn.ToNeuron))
                {
                    toDelete.Add(conn);
                }
            }
            foreach (var del in toDelete)
            {
                connections.Remove(del);
            }

            #endregion Build connections

            FindDuplicates(neurons);
            FindDuplicates(connections);

            var child = new Genome();
            child._neurons = new List<GNeuron>(neurons);
            child._connections = new List<GConnection>(connections);
            return child;
        }

        public static Genome Mutate(Random generator, Genome toMutate)
        {
            if (generator.NextDouble() < _conf.probabilityOfChangeWeight)
                toMutate = MutationChangeWeight(generator, toMutate);
            if (generator.NextDouble() < _conf.probabilityAddNeuron)
                toMutate = MutationAddNeuron(generator, toMutate);
            if (generator.NextDouble() < _conf.probabilityAddConnection)
                toMutate = MutationAddConnection(generator, toMutate);
            if (generator.NextDouble() < _conf.probabilityRemoveConnection)
                toMutate = MutationRemoveConnection(generator, toMutate);

            return toMutate;
        }

        internal static bool IsGWithIdExistsInList(List<GNeuron> neurons, int id)
        {
            foreach (var neuron in neurons)
                if (neuron.Id == id)
                    return true;
            return false;
        }

        internal static bool IsGWithIdExistsInList(List<GConnection> conns, int id)
        {
            foreach (var conn in conns)
                if (conn.Id == id)
                    return true;
            return false;
        }

        internal static int[] FindAmountOfInAndOutConnectionsForNeuronWithId(List<GConnection> connectionList, int id)
        {
            var sumIn = 0;
            var sumOut = 0;
            foreach (var conn in connectionList)
            {
                if (conn.ToNeuron == id)
                    sumIn++;
                if (conn.FromNeuron == id)
                    sumOut++;
            }
            return new[] { sumIn, sumOut };
        }

        internal static List<GConnection>[] GetListOfInAndOutConnections(List<GConnection> connectionList, int id)
        {
            var inConn = new List<GConnection>();
            var outConn = new List<GConnection>();
            foreach (var conn in connectionList)
            {
                if (conn.ToNeuron == id)
                    inConn.Add(conn);
                if (conn.FromNeuron == id)
                    outConn.Add(conn);
            }
            return new[] { inConn, outConn };
        }

        #endregion Static methods
    }
}
