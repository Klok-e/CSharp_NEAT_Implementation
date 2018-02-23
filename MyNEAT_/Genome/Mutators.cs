using System;
using System.Collections.Generic;
using System.Linq;

namespace MyNEAT.Genome
{
    public static class Mutators
    {
        #region Mutators

        public static void MutationChangeWeight(this Genome genome, Random generator)
        {
            var conn = genome._connections;
            if (conn.Count > 0)
            {
                var randConn = generator.RandChoice(conn, out int num);
                if (generator.NextDouble() < genome._conf.probabilityOfResetWeight)
                    conn[num] = new GConnection(randConn.FromNeuron, randConn.ToNeuron,
                        generator.RandRange(-genome._conf.connWeightRange, genome._conf.connWeightRange),
                        randConn.Id);
                else
                    conn[num] = new GConnection(randConn.FromNeuron, randConn.ToNeuron,
                        generator.RandRange(-genome._conf.weightChangeRange, genome._conf.weightChangeRange) + randConn.Weight,
                        randConn.Id);
            }
        }

        public static void MutationAddNeuron(this Genome genome, Random generator)
        {
            ref var geneInd = ref Genome._geneIndex;

            var conn = genome._connections;
            if (conn.Count > 0)
            {
                var randConn = generator.RandChoice(conn, out var ind);
                conn.RemoveAt(ind);

                var newNeuron = new GNeuron(geneInd++, NeuronType.hidden);
                genome._neurons.Add(newNeuron);

                var newConnIn = new GConnection(randConn.FromNeuron, newNeuron.Id, 1, geneInd++);
                conn.Add(newConnIn);

                var newConnOut = new GConnection(newNeuron.Id, randConn.ToNeuron, randConn.Weight, geneInd++);
                conn.Add(newConnOut);
            }
        }

        public static void RemoveDisconnectedNeurons(this Genome genome, Random generator)
        {
            var toDelIndices = new List<int>();
            for (var i = 0; i < genome._neurons.Count; i++)
            {
                FindAmountOfInAndOutConnectionsForNeuronWithId(genome._connections, genome._neurons[i].Id,
                    out var neuIn, out var neuOut);
                if (neuIn == 0 || neuOut == 0)
                {
                    toDelIndices.Add(i);
                }
            }
            for (int i = 0; i < toDelIndices.Count; i++)
            {
                genome._neurons.RemoveAt(toDelIndices[i]);
            }
        }

        public static void MutationAddConnection(this Genome genome, Random generator)
        {
            ref var geneIndex = ref Genome._geneIndex;

            var neuron1 = generator.RandChoice(genome._neurons);
            var neuron2 = generator.RandChoice(genome._neurons);

            GetListOfInAndOutConnections(genome._connections, neuron1.Id, out var inConn1, out var outConn1);
            GetListOfInAndOutConnections(genome._connections, neuron2.Id, out var inConn2, out var outConn2);
            if (inConn1.Intersect(outConn2).Count() == 0 && inConn2.Intersect(outConn1).Count() == 0)
            {
                genome._connections.Add(new GConnection(neuron1.Id, neuron2.Id,
                    generator.RandRange(-genome._conf.connWeightRange, genome._conf.connWeightRange),
                    geneIndex++));
            }
        }

        public static Genome MutationRemoveConnection(Random generator, Genome genome)
        {
            if (genome._connections.Count > 0)
            {
                generator.RandChoice(genome._connections, out var ind);
                genome._connections.RemoveAt(ind);
            }
            return genome;
        }

        #endregion Mutators

        #region Reproduction

        /// <summary>
        ///     Asexual reproduction
        /// </summary>
        /// <returns></returns>
        public static Genome CreateOffSpring(this Genome genome, Random generator)
        {
            var offspring = genome.Clone();
            offspring = Mutate(generator, offspring);
            return offspring;
        }

        /// <summary>
        ///     Sexual reproduction
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="otherParent"></param>
        /// <returns></returns>
        public static Genome CreateOffSpring(this Genome parent, Genome otherParent, Random generator)
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

        #endregion Static methods
    }
}
