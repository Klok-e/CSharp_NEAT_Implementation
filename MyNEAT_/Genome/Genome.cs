using MyNEAT.Decoder;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyNEAT.Genome
{
    public class NEATGenome : IGenome
    {
        public static Config _conf { get; set; }

        internal static ulong _geneIndex;

        public float Fitness { get; set; }

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

        public NEATGenome(Random generator)
        {
            if (_conf.inputs == 0 || _conf.outputs == 0) throw new Exception("fuck you");

            var inputs = _conf.inputs;
            var outputs = _conf.outputs;

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

        private NEATGenome()
        {
        }

        #endregion Constructors

        #region IGenome

        public IGenome Clone()
        {
            var clone = new NEATGenome()
            {
                _connections = new List<GConnection>(_connections),
                _neurons = new List<GNeuron>(_neurons),
            };

            return clone;
        }

        public IBlackBox Decode()
        {
            throw new Exception();
        }

        public IGenome Crossover(Random generator, IGenome interfacked)
        {
            void FindDuplicates<T>(List<T> neus) where T : IGNode
            {
                foreach (var n in neus)
                {
                    foreach (var n2 in neus)
                    {
                        if (n.Equals(n2)) continue;
                        if (n.Id == n2.Id)
                        {
                            throw new Exception("shi~");
                        }
                    }
                }
            }
            var other = (NEATGenome)interfacked;

            var neurons = new List<GNeuron>();

            #region Build neurons

            neurons.AddRange(_neurons.Concat(other._neurons));
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

            connections.AddRange(_connections.Concat(other._connections));
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
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            FindDuplicates(neurons);
            FindDuplicates(connections);

            watch.Stop();
#endif
            var child = new NEATGenome(generator)
            {
                _neurons = new List<GNeuron>(neurons),
                _connections = new List<GConnection>(connections)
            };
            return child;
        }

        public void Mutate(Random generator)
        {
            if (generator.NextDouble() < _conf.probabilityOfChangeWeight)
                MutationChangeWeight(generator);
            if (generator.NextDouble() < _conf.probabilityAddNeuron)
                MutationAddNeuron(generator);
            if (generator.NextDouble() < _conf.probabilityAddConnection)
                MutationAddConnection(generator);
            if (generator.NextDouble() < _conf.probabilityRemoveConnection)
                MutationRemoveConnection(generator);

            if (_geneIndex % 10 == 1)//magic
                RemoveDisconnectedNeurons();
        }

        #endregion IGenome

        #region Mutators

        private void MutationChangeWeight(Random generator)
        {
            if (_connections.Count > 0)
            {
                var randConn = generator.RandChoice(_connections, out int num);
                if (generator.NextDouble() < _conf.probabilityOfResetWeight)
                    _connections[num] = new GConnection(randConn.FromNeuron, randConn.ToNeuron,
                        generator.RandRange(-_conf.connWeightRange, _conf.connWeightRange),
                        randConn.Id);
                else
                    _connections[num] = new GConnection(randConn.FromNeuron, randConn.ToNeuron,
                        generator.RandRange(-_conf.weightChangeRange, _conf.weightChangeRange) + randConn.Weight,
                        randConn.Id);
            }
        }

        private void MutationAddNeuron(Random generator)
        {
            if (_connections.Count > 0)
            {
                var randConn = generator.RandChoice(_connections, out var ind);
                _connections.RemoveAt(ind);

                var newNeuron = new GNeuron(_geneIndex++, NeuronType.hidden);
                _neurons.Add(newNeuron);

                var newConnIn = new GConnection(randConn.FromNeuron, newNeuron.Id, 1, _geneIndex++);
                _connections.Add(newConnIn);

                var newConnOut = new GConnection(newNeuron.Id, randConn.ToNeuron, randConn.Weight, _geneIndex++);
                _connections.Add(newConnOut);
            }
        }

        private void RemoveDisconnectedNeurons()
        {
            var toDel = new List<GNeuron>();
            for (var i = 0; i < _neurons.Count; i++)
            {
                GetListOfInAndOutConnections(_connections, _neurons[i].Id,
                    out var neuIn, out var neuOut);
                if ((neuIn.Count == 0 || neuOut.Count == 0) && _neurons[i].Type == NeuronType.hidden)
                {
                    toDel.Add(_neurons[i]);
                    foreach (var item in neuIn.Concat(neuOut))
                    {
                        _connections.Remove(GetConnection(_connections, item));
                    }
                }
            }
            foreach (var item in toDel)
            {
                _neurons.Remove(item);
            }
        }

        private void MutationAddConnection(Random generator)
        {
            var neuron1 = generator.RandChoice(_neurons);
            var neuron2 = generator.RandChoice(_neurons);

            GetListOfInAndOutConnections(_connections, neuron1.Id, out var inConn1, out var outConn1);
            GetListOfInAndOutConnections(_connections, neuron2.Id, out var inConn2, out var outConn2);
            if (inConn1.Intersect(outConn2).Count() == 0 && inConn2.Intersect(outConn1).Count() == 0)
            {
                _connections.Add(new GConnection(neuron1.Id, neuron2.Id,
                    generator.RandRange(-_conf.connWeightRange, _conf.connWeightRange),
                    _geneIndex++));
            }
        }

        private void MutationRemoveConnection(Random generator)
        {
            if (_connections.Count > 0)
            {
                generator.RandChoice(_connections, out var ind);
                _connections.RemoveAt(ind);
            }
        }

        #endregion Mutators

        #region Static methods

        public static void SetConf(Config conf)
        {
            _conf = conf;
        }

        internal static bool IsGWithIdExistsInList(List<GNeuron> neurons, ulong id)
        {
            foreach (var neuron in neurons)
                if (neuron.Id == id)
                    return true;
            return false;
        }

        internal static bool IsGWithIdExistsInList(List<GConnection> conns, ulong id)
        {
            foreach (var conn in conns)
                if (conn.Id == id)
                    return true;
            return false;
        }

        internal static void FindAmountOfInAndOutConnectionsForNeuronWithId(List<GConnection> connectionList, ulong id, out int sumIn, out int sumOut)
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

        internal static void GetListOfInAndOutConnections(List<GConnection> connectionList, ulong id, out List<ulong> inConn, out List<ulong> outConn)
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

        private static GConnection GetConnection(List<GConnection> connectionList, ulong id)
        {
            foreach (var item in connectionList)
            {
                if (item.Id == id)
                {
                    return item;
                }
            }
            throw new Exception();
        }

        #endregion Static methods
    }
}
