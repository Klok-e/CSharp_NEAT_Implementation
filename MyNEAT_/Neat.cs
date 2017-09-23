using System;
using System.Collections.Generic;
using System.Linq;

namespace MyNEAT
{
    public class Config
    {
        public ActivationFunctions.ActivationFunction activationOutp;
        public ActivationFunctions.ActivationFunction activationNormal;

        private const double defaultConnWeightRange = 5d;
        private const double defaultWeightChangeRange = 0.5;
        private const double defaultProbabilityOfMutation = 0.9;
        private const double defaultProbabilityOfResetWeight = 0.2;
        private const double defaultProbabilityOfChangeWeight = 0.6;
        private const double defaultProbabilityAddNeuron = 0.15;
        private const double defaultProbabilityRemoveNeuron = 0.2;
        private const double defaultProbabilityAddConnection = 0.3;
        private const double defaultProbabilityRemoveConnection = 0.35;

        public double connWeightRange;
        public double probabilityAddConnection;
        public double probabilityAddNeuron;
        public double probabilityOfChangeWeight;
        public double probabilityOfMutation;
        public double probabilityOfResetWeight;
        public double probabilityRemoveConnection;
        public double probabilityRemoveNeuron;
        public double weightChangeRange;

        public Config()
        {
            connWeightRange = defaultConnWeightRange;
            weightChangeRange = defaultWeightChangeRange;
            probabilityOfMutation = defaultProbabilityOfMutation;
            probabilityOfResetWeight = defaultProbabilityOfResetWeight;
            probabilityOfChangeWeight = defaultProbabilityOfChangeWeight;
            probabilityAddNeuron = defaultProbabilityAddNeuron;
            probabilityRemoveNeuron = defaultProbabilityRemoveNeuron;
            probabilityAddConnection = defaultProbabilityAddConnection;
            probabilityRemoveConnection = defaultProbabilityRemoveConnection;

            activationOutp = ActivationFunctions.Linear;
            activationNormal = ActivationFunctions.Linear;
        }
    }

    internal abstract class G
    {
        public int id;
    }

    internal class GNeuron:G
    {
        public bool isBias;
        public bool isHidden;

        public bool isInput;

        //public List<int> inConnections;
        //public List<int> outConnections;
        public bool isOutput;

        public GNeuron(int id)
        {
            //inConnections = new List<int>();
            //outConnections = new List<int>();
            isBias = false;
            isHidden = false;
            isOutput = false;
            isInput = false;
            this.id = id;
        }

        public override string ToString()
        {
            var str = "This id: ";
            str += id;
            str += ", " + "Is input: " + isInput + ", " + "Is bias: " + isBias + ", " + "Is Output: " + isOutput;

            return str;
        }
    }

    internal class GConnection : G
    {
        public readonly int fromNeuron;
        public readonly int toNeuron;
        public readonly double weight;

        public GConnection(GNeuron fromneuron, GNeuron toneuron, double wei, int idForThis)
        {
            fromNeuron = fromneuron.id;
            toNeuron = toneuron.id;

            id = idForThis;

            weight = wei;
        }

        public GConnection(int fromneuron, int toneuron, double wei, int idForThis)
        {
            fromNeuron = fromneuron;
            toNeuron = toneuron;

            id = idForThis;

            weight = wei;
        }

        public override string ToString()
        {
            var str = "This id: " + id + ", ";
            str += "This weight: " + Math.Round(weight, 2) + ", |||";
            str += "From: " + fromNeuron + ", ";
            str += "To: " + toNeuron + ", ";
            return str;
        }
    }

    public class Genome
    {
        public static Config conf;

        private static int geneIndex;
        internal List<GConnection> connections;
        public float fitness;

        internal List<GNeuron> neurons;

        public override string ToString()
        {
            var str = "";
            for (var i = 0; i < neurons.Count; i++)
                str += neurons[i] + "\n";
            str += "\n";
            for (var i = 0; i < connections.Count; i++)
                str += connections[i] + "\n";

            return str;
        }

        public int GetComplexity()
        {
            var sum = 0;
            sum += neurons.Count + connections.Count;
            return sum;
        }

        #region Constructors

        public Genome(int inputs, int outputs)
        {
            if (conf == null) throw new Exception("conf = null");

            geneIndex = 0;
            neurons = new List<GNeuron>();
            connections = new List<GConnection>();

            for (var i = 0; i < inputs; i++) //only inputs
            {
                var inpNeuron = new GNeuron(geneIndex);
                inpNeuron.isInput = true;
                geneIndex++;

                neurons.Add(inpNeuron);
            }

            var biasNeuron = new GNeuron(geneIndex);
            biasNeuron.isBias = true;
            geneIndex++;
            neurons.Add(biasNeuron);

            for (var i = 0; i < outputs; i++) //only output neurons
            {
                var outNeuron = new GNeuron(geneIndex);
                outNeuron.isOutput = true;
                geneIndex++;

                neurons.Add(outNeuron);
            }

            var randGenerator = new Random();
            foreach (var neuron in neurons)
                if (neuron.isOutput)
                    foreach (var neuron1 in neurons)
                        if (neuron1.isInput || neuron1.isBias)
                        {
                            var conn = new GConnection(neuron1, neuron,
                                randGenerator.NextDouble() * (conf.connWeightRange - -conf.connWeightRange) +
                                -conf.connWeightRange, geneIndex);
                            geneIndex++;

                            connections.Add(conn);
                        }
        }


        public Genome()
        {
            if (conf == null) throw new Exception("conf = null");
        }

        #endregion

        #region Mutators

        public static Genome MutationChangeWeight(Random generator, Genome genome)
        {
            if (genome.connections.Count != 0)
            {
                var num = generator.Next(genome.connections.Count);
                var conn = genome.connections[num];
                if (generator.NextDouble() < conf.probabilityOfResetWeight)
                    genome.connections[num] = new GConnection(conn.fromNeuron, conn.toNeuron,
                        generator.NextDouble() * (conf.connWeightRange - -conf.connWeightRange) + -conf.connWeightRange,
                        conn.id);
                else
                    genome.connections[num] = new GConnection(conn.fromNeuron, conn.toNeuron,
                        genome.connections[num].weight +
                        generator.NextDouble() * (conf.weightChangeRange - -conf.weightChangeRange) +
                        -conf.weightChangeRange, conn.id);
            }
            return genome;
        }

        public static Genome MutationAddNeuron(Random generator, Genome genome)
        {
            if (genome.connections.Count != 0)
            {
                var ind = generator.Next(genome.connections.Count);
                var conn = genome.connections[ind];
                genome.connections.RemoveAt(ind);

                var newNeuron = new GNeuron(geneIndex);
                genome.neurons.Add(newNeuron);
                geneIndex++;

                var newConnIn = new GConnection(conn.fromNeuron, newNeuron.id, 1, geneIndex);
                genome.connections.Add(newConnIn);
                geneIndex++;

                var newConnOut = new GConnection(newNeuron.id, conn.toNeuron, conn.weight, geneIndex);
                genome.connections.Add(newConnOut);
                geneIndex++;
            }

            return genome;
        }

        public static Genome MutationRemoveNeuron(Random generator, Genome genome)
        {
            var availableNeurons = new List<GNeuron>();
            for (var i = 0; i < genome.neurons.Count; i++)
            {
                var inOut = FindAmountOfInAndOutConnectionsForNeuronWithId(genome.connections, genome.neurons[i].id);
                if (inOut[0] == 0 && inOut[1] == 0 && genome.neurons[i].isInput != true &&
                    genome.neurons[i].isOutput != true && genome.neurons[i].isBias != true)
                    availableNeurons.Add(genome.neurons[i]);
            }
            if (availableNeurons.Count != 0)
            {
                var toDelete = availableNeurons[generator.Next(availableNeurons.Count)];
                genome.neurons.Remove(toDelete);
            }

            return genome;
        }

        public static Genome MutationAddConnection(Random generator, Genome genome)
        {
            var neuron1 = genome.neurons[generator.Next(genome.neurons.Count)];
            var neuron2 = genome.neurons[generator.Next(genome.neurons.Count)];

            var n1InOut = GetListOfInAndOutConnections(genome.connections, neuron1.id);
            var n2InOut = GetListOfInAndOutConnections(genome.connections, neuron2.id);
            if (n1InOut[1].Intersect(n2InOut[0]).Count() == 0 && n1InOut[0].Intersect(n2InOut[1]).Count() == 0)
            {
                genome.connections.Add(new GConnection(neuron1.id, neuron2.id,
                    generator.NextDouble() * (conf.connWeightRange - -conf.connWeightRange) + -conf.connWeightRange,
                    geneIndex));
                geneIndex++;
            }
            return genome;
        }

        public static Genome MutationRemoveConnection(Random generator, Genome genome)
        {
            if (genome.connections.Count != 0)
            {
                var connToRemove = genome.connections[generator.Next(genome.connections.Count)];
                genome.connections.Remove(connToRemove);
            }

            return genome;
        }

        #endregion

        #region Reproduction

        /// <summary>
        ///     Asexual reproduction
        /// </summary>
        /// <returns></returns>
        public Genome CreateOffSpring(Random generator)
        {
            var offspring = new Genome();
            offspring.neurons = new List<GNeuron>(neurons);
            offspring.connections = new List<GConnection>(connections);

            if (generator.NextDouble() < conf.probabilityOfMutation)
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

            if (generator.NextDouble() < conf.probabilityOfMutation)
                offspring = Mutate(generator, offspring);
            return offspring;
        }

        #endregion

        #region Static methods

        public static Genome Crossover(Random generator, Genome parent1, Genome parent2)
        {
            var neurons = new List<GNeuron>();

            #region Build neurons

            for (var i = 0; i < Math.Min(parent1.neurons.Count, parent2.neurons.Count); i++)
                if (parent1.neurons[i].id == parent2.neurons[i].id)
                {
                    neurons.Add(parent1.neurons[i]);
                }
                else
                {
                    GNeuron[] arr = { parent1.neurons[i], parent2.neurons[i] };
                    var toAdd = arr[generator.Next(arr.Length)];
                    neurons.Add(toAdd);
                }
            if (neurons.Count != Math.Max(parent1.neurons.Count, parent2.neurons.Count))
                for (var i = Math.Min(parent1.neurons.Count, parent2.neurons.Count);
                    i < Math.Max(parent1.neurons.Count, parent2.neurons.Count);
                    i++)
                    if (Math.Max(parent1.neurons.Count, parent2.neurons.Count) == parent1.neurons.Count)
                        neurons.Add(parent1.neurons[i]);
                    else
                        neurons.Add(parent2.neurons[i]);

            #endregion

            var connections = new List<GConnection>(Math.Max(parent1.connections.Count, parent2.connections.Count));

            #region Build connections

            var sortedParents = new List<List<GConnection>>();
            sortedParents.Add(parent1.connections);
            sortedParents.Add(parent2.connections);
            sortedParents.Sort((x, y) => x.Count.CompareTo(y.Count)); //now i know what has low count and what high

            for (var i = 0; i < sortedParents[1].Count; i++)
                if (i < sortedParents[0].Count)
                    if (sortedParents[0][i].id == sortedParents[1][i].id)
                    {
                        if (IsGWithIdExistsInList(neurons, sortedParents[0][i].fromNeuron) &&
                            IsGWithIdExistsInList(neurons, sortedParents[0][i].toNeuron))
                            connections.Add(sortedParents[0][i]);
                    }
                    else
                    {
                        GConnection[] arr = { sortedParents[0][i], sortedParents[1][i] };
                        var num = generator.Next(arr.Length);

                        if (IsGWithIdExistsInList(neurons, arr[num].fromNeuron) &&
                            IsGWithIdExistsInList(neurons, arr[num].toNeuron))
                            connections.Add(arr[num]);
                        else if (IsGWithIdExistsInList(neurons, arr[Math.Abs(num - 1)].fromNeuron) &&
                                 IsGWithIdExistsInList(neurons, arr[Math.Abs(num - 1)].toNeuron))
                            connections.Add(arr[Math.Abs(num - 1)]);
                    }
                else if (i < sortedParents[1].Count)
                    if (IsGWithIdExistsInList(neurons, sortedParents[1][i].fromNeuron) &&
                        IsGWithIdExistsInList(neurons, sortedParents[1][i].toNeuron))
                        connections.Add(sortedParents[1][i]);

            #endregion

            void FindDuplicates<T>(List<T> neus) where T:G
            {
                foreach (var n in neus)
                {
                    foreach (var n2 in neus)
                    {
                        if (n == n2) continue;
                        if (n.id == n2.id)
                        {
                            throw new Exception("shi~");
                        }
                    }
                }
            }
            //TODO: duplicates in neurons and conns
            FindDuplicates(neurons);
            FindDuplicates(connections);

            var child = new Genome();
            child.neurons = new List<GNeuron>(neurons);
            child.connections = new List<GConnection>(connections);
            return child;
        }

        public static Genome Mutate(Random generator, Genome toMutate)
        {
            if (generator.NextDouble() < conf.probabilityOfChangeWeight)
                toMutate = MutationChangeWeight(generator, toMutate);
            if (generator.NextDouble() < conf.probabilityAddNeuron)
                toMutate = MutationAddNeuron(generator, toMutate);
            if (generator.NextDouble() < conf.probabilityRemoveNeuron)
                toMutate = MutationRemoveNeuron(generator, toMutate);
            if (generator.NextDouble() < conf.probabilityAddConnection)
                toMutate = MutationAddConnection(generator, toMutate);
            if (generator.NextDouble() < conf.probabilityRemoveConnection)
                toMutate = MutationRemoveConnection(generator, toMutate);

            return toMutate;
        }

        internal static bool IsGWithIdExistsInList(List<GNeuron> neurons, int id)
        {
            foreach (var neuron in neurons)
                if (neuron.id == id)
                    return true;
            return false;
        }

        internal static bool IsGWithIdExistsInList(List<GConnection> conns, int id)
        {
            foreach (var conn in conns)
                if (conn.id == id)
                    return true;
            return false;
        }

        internal static int[] FindAmountOfInAndOutConnectionsForNeuronWithId(List<GConnection> connectionList, int id)
        {
            var sumIn = 0;
            var sumOut = 0;
            foreach (var conn in connectionList)
            {
                if (conn.toNeuron == id)
                    sumIn++;
                if (conn.fromNeuron == id)
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
                if (conn.toNeuron == id)
                    inConn.Add(conn);
                if (conn.fromNeuron == id)
                    outConn.Add(conn);
            }
            return new[] { inConn, outConn };
        }

        #endregion
    }


    internal class DNeuron
    {
        public static ActivationFunctions.ActivationFunction normalActivation;
        public static ActivationFunctions.ActivationFunction outpActivation;

        public DNeuron(int id)
        {
            depths = new List<int>();

            outConnections = new List<DConnection>();

            this.id = id;
            inputsAdded = new List<double>();
            depth = 0;
            output = 0;
        }

        public void Activate()
        {
            double sum = 0;
            for (var i = 0; i < inputsAdded.Count; i++)
                sum += inputsAdded[i];
            inputsAdded.Clear();

            if (isInput)
                output = sum; //linear
            else if (isBias)
                output = 1; //bias' output is always 1
            else if (isOutput)
                output = outpActivation(sum);
            else
                output = normalActivation(sum); //TODO: currently linear

            TransferOutput();
        }

        private void TransferOutput()
        {
            foreach (var conn in outConnections)
                conn.toNeuron.inputsAdded.Add(output * conn.weight);
        }

        public static DNeuron FindNeuronWithId(List<DNeuron> neuronslist, int id)
        {
            for (var i = 0; i < neuronslist.Count; i++)
                if (neuronslist[i].id == id)
                    return neuronslist[i];
            throw new Exception();
        }

        #region Depth things

        public void SetDepth()
        {
            if (depths.Count != 0)
                depth = depths.Min();
        }

        #endregion

        public override string ToString()
        {
            var str = "This id: " + id;
            str += ", this n depth: " + depth;
            str += ", " + "Is input: " + isInput + ", " + "Is bias: " + isBias + ", " + "Is Output: " + isOutput +
                   " ||| ";
            str += "Out: ";
            for (var i = 0; i < outConnections.Count; i++)
            {
                str += " Id = " + outConnections[i].toNeuron.id;
                str += " Weight = " + Math.Round(outConnections[i].weight, 2);
                str += " Depth = " + outConnections[i].toNeuron.depth;
            }

            return str;
        }

        #region Public

        public List<DConnection> outConnections;

        public bool isBias;
        public bool isInput;
        public bool isOutput;

        public int amountOfInConnections;

        public int id;

        public List<double> inputsAdded;
        public double output;

        public List<int> depths;
        public int depth;

        #endregion
    }

    internal class DConnection
    {
        public DNeuron toNeuron;
        public double weight;
    }

    public class Network
    {
        private readonly List<DNeuron> dneurons = new List<DNeuron>();
        private readonly List<DNeuron> hidden = new List<DNeuron>();
        private readonly List<DNeuron> inputs = new List<DNeuron>();
        private readonly List<DNeuron> outputs = new List<DNeuron>();

        public Network(Genome genome)
        {
            DNeuron.normalActivation = Genome.conf.activationNormal;
            DNeuron.outpActivation = Genome.conf.activationOutp;

            for (var i = 0; i < genome.neurons.Count; i++)
                dneurons.Add(new DNeuron(genome.neurons[i].id));

            //iterate through all neurons
            DNeuron currneu;
            foreach (var neuron in genome.neurons)
            {
                currneu = DNeuron.FindNeuronWithId(dneurons, neuron.id);

                currneu.amountOfInConnections =
                    Genome.FindAmountOfInAndOutConnectionsForNeuronWithId(genome.connections, neuron.id)[0];
                if (neuron.isInput || neuron.isBias)
                {
                    //currneu.isInput = true;
                    inputs.Add(currneu);
                    //currneu.amountOfInConnections = neuron.inConnections.Count;
                    if (neuron.isBias)
                        currneu.isBias = true;
                    else if (neuron.isInput)
                        currneu.isInput = true;
                }
                else if (neuron.isOutput)
                {
                    currneu.isOutput = true;
                    outputs.Add(currneu);
                }
                else
                {
                    hidden.Add(currneu);
                }
            }

            foreach (var conn in genome.connections)
            {
                var connout = new DConnection();
                connout.toNeuron = DNeuron.FindNeuronWithId(dneurons, conn.toNeuron);
                connout.weight = conn.weight;
                DNeuron.FindNeuronWithId(dneurons, conn.fromNeuron).outConnections.Add(connout);
            }

            //another check
            if (inputs.Last().isBias != true)
                throw new Exception();

            var depthCalculator = new DepthCalculator();
            depthCalculator.SetDepthsToNetwork(inputs);
            foreach (var n in dneurons)
                n.SetDepth();

            //sort list of neurons
            dneurons.Sort(new Comparer());
        }

        public double[] Predict(double[] state)
        {
            var prediction = new double[outputs.Count];

            for (var i = 0; i < inputs.Count - 1; i++)
            {
                inputs[i].inputsAdded.Add(state[i]);
                inputs[i].Activate();
            }
            inputs[inputs.Count - 1].Activate(); //bias

            foreach (var neuron in hidden)
                neuron.Activate();

            foreach (var neuron in outputs)
                neuron.Activate();

            for (var i = 0; i < outputs.Count; i++)
                prediction[i] = outputs[i].output;
            return prediction;
        }

        public override string ToString()
        {
            var str = "\n";
            //write structure
            for (var i = 0; i < dneurons.Count; i++)
                str += dneurons[i] + "\n";
            str += "\n";
            return str;
        }
    }

    internal class DepthCalculator
    {
        public void SetDepthsToNetwork(List<DNeuron> inputs)
        {
            for (var i = 0; i < inputs.Count; i++)
            {
                var depth = 0;
                var visited = new List<int>();
                SetDepthStartingFromThisNeuron(inputs[i], depth, visited);
            }
        }

        private void SetDepthStartingFromThisNeuron(DNeuron neuron, int depth, List<int> visited)
        {
            neuron.depths.Add(depth);
            visited.Add(neuron.id);

            foreach (var conn in neuron.outConnections)
                if (visited.Contains(conn.toNeuron.id) == false)
                    SetDepthStartingFromThisNeuron(conn.toNeuron, depth + 1, visited);
        }
    }

    internal class Comparer : IComparer<DNeuron>
    {
        int IComparer<DNeuron>.Compare(DNeuron x, DNeuron y)
        {
            var compareDate = x.depth.CompareTo(y.depth);
            return compareDate;
        }
    }
}