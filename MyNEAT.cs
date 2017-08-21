using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyNEAT;

class Program
{

    static void Main()
    {
        Genome genome = new Genome(3, 2);
        Random randGener = new Random();

        for (int i = 0; i < 1000; i++)
        {
            genome = genome.CreateOffSpring(randGener);
        }
        




        Console.Write(genome + "\n\n");

        Network network = new Network(genome);

        double[] pr = network.Predict(new double[] { -0.3, 0.2, 2 });

        string str = "";
        for (int i = 0; i < pr.Length; i++)
        {
            str += pr[i] + ", ";
        }
        Console.Write(str);

        Console.WriteLine(network);

        Console.ReadKey();
    }
}

namespace MyNEAT
{
    class GNeuron
    {
        public List<GConnection> inConnections;
        public List<GConnection> outConnections;
        public bool isOutput;
        public bool isInput;
        public bool isBias;
        public bool isHidden;

        public int Id;

        public GNeuron(int id)
        {
            inConnections = new List<GConnection>();
            outConnections = new List<GConnection>();
            isBias = false;
            isHidden = false;
            isOutput = false;
            isInput = false;
            Id = id;
        }

        public override string ToString()
        {
            string str = "This id: ";
            str += Id;
            str += ", " + "Is input: " + isInput + ", " + "Is bias: " + isBias + ", " + "Is Output: " + isOutput + " ||| ";
            str += "Out: ";
            for (int i = 0; i < outConnections.Count; i++)
            {
                str += " Id = " + outConnections[i].toNeuron.Id;
                str += " Weight = " + outConnections[i].weight;
            }

            return str;
        }
    }

    class GConnection
    {
        public double weight;
        public GNeuron fromNeuron;
        public GNeuron toNeuron;
        public GConnection(GNeuron fromneuron, GNeuron toneuron)
        {
            fromNeuron = fromneuron;
            toNeuron = toneuron;

            toNeuron.inConnections.Add(this);
            fromNeuron.outConnections.Add(this);

            weight = 0;
        }

        public void DeleteThisFromRelatedNeurons()
        {
            toNeuron.inConnections.Remove(this);
            fromNeuron.outConnections.Remove(this);
        }
    }

    class Genome
    {
        public List<GNeuron> neurons;
        public List<GConnection> connections;

        public static double connWeightRange = 5d;
        public static double weightChangeRange = 0.5;

        public static double probabilityOfMutation = 0.4;
        public static double probabilityOfResetWeight = 0.1;
        public static double probabilityOfChangeWeight = 0.6;
        public static double probabilityAddNeuron = 0.1;
        public static double probabilityRemoveNeuron = 0.01;
        public static double probabilityAddConnection = 0.3;
        public static double probabilityRemoveConnection = 0.05;

        public int neuronIndex;

        #region Constructors
        public Genome(int inputs, int outputs)
        {
            neuronIndex = 0;
            neurons = new List<GNeuron>();
            connections = new List<GConnection>();

            for (int i = 0; i < inputs; i++)//only inputs
            {
                GNeuron inpNeuron = new GNeuron(neuronIndex);
                inpNeuron.isInput = true;
                neuronIndex++;

                neurons.Add(inpNeuron);
            }

            GNeuron biasNeuron = new GNeuron(neuronIndex);
            biasNeuron.isBias = true;
            neuronIndex++;
            neurons.Add(biasNeuron);

            for (int i = 0; i < outputs; i++)//only output neurons
            {
                GNeuron outNeuron = new GNeuron(neuronIndex);
                outNeuron.isOutput = true;
                neuronIndex++;

                neurons.Add(outNeuron);
            }

            Random randGenerator = new Random();
            foreach (GNeuron neuron in neurons)
            {
                if (neuron.isOutput)
                {

                    foreach (GNeuron neuron1 in neurons)
                    {
                        if (neuron1.isInput || neuron1.isBias)
                        {
                            GConnection conn = new GConnection(neuron1, neuron);
                            conn.weight = randGenerator.NextDouble() * (connWeightRange - (-connWeightRange)) + (-connWeightRange);//Random.NextDouble() * (maxValue – minValue) + minValue

                            connections.Add(conn);
                        }
                    }
                }
            }


        }


        public Genome()
        {
            neurons = new List<GNeuron>();
            connections = new List<GConnection>();
        }
        #endregion

        #region Mutators
        public static Genome MutationChangeWeight(Random generator, Genome genome)
        {
            if (genome.connections.Count != 0)
            {
                GConnection conn = genome.connections[generator.Next(genome.connections.Count)];
                if (generator.NextDouble() < probabilityOfResetWeight)
                {
                    conn.weight = generator.NextDouble() * (connWeightRange - (-connWeightRange)) + (-connWeightRange);
                }
                else
                {
                    conn.weight += generator.NextDouble() * (2 * weightChangeRange) - weightChangeRange;
                }
            }
            return genome;
        }

        public static Genome MutationAddNeuron(Random generator, Genome genome)
        {
            if (genome.connections.Count != 0)
            {
                GConnection conn = genome.connections[generator.Next(genome.connections.Count)];
                conn.DeleteThisFromRelatedNeurons();
                genome.connections.Remove(conn);

                GNeuron newNeuron = new GNeuron(genome.neuronIndex += 1);

                GConnection newConnIn = new GConnection(conn.fromNeuron, newNeuron);
                newConnIn.weight = 1;
                genome.connections.Add(newConnIn);

                GConnection newConnOut = new GConnection(newNeuron, conn.toNeuron);
                newConnOut.weight = conn.weight;
                genome.connections.Add(newConnOut);

                genome.neurons.Add(newNeuron);
            }

            return genome;
        }

        public static Genome MutationRemoveNeuron(Random generator, Genome genome)
        {
            List<GNeuron> availableNeurons = new List<GNeuron>();
            for (int i = 0; i < genome.neurons.Count; i++)
            {
                if (genome.neurons[i].inConnections.Count == 0 && genome.neurons[i].outConnections.Count == 0 && genome.neurons[i].isInput != true && genome.neurons[i].isOutput != true && genome.neurons[i].isBias != true)
                {
                    availableNeurons.Add(genome.neurons[i]);
                }
            }
            if (availableNeurons.Count != 0)
            {
                GNeuron toDelete = availableNeurons[generator.Next(availableNeurons.Count)];
                genome.neurons.Remove(toDelete);
            }

            return genome;
        }

        public static Genome MutationAddConnection(Random generator, Genome genome)
        {
            GNeuron neuron1 = genome.neurons[generator.Next(genome.neurons.Count)];
            GNeuron neuron2 = genome.neurons[generator.Next(genome.neurons.Count)];
            if (neuron1.outConnections.Intersect(neuron2.inConnections).Count() == 0 && neuron2.outConnections.Intersect(neuron1.inConnections).Count() == 0)
            {
                genome.connections.Add(new GConnection(neuron1, neuron2));
            }
            return genome;
        }

        public static Genome MutationRemoveConnection(Random generator, Genome genome)
        {
            if (genome.connections.Count != 0)
            {
                GConnection connToRemove = genome.connections[generator.Next(genome.connections.Count)];
                connToRemove.DeleteThisFromRelatedNeurons();
                genome.connections.Remove(connToRemove);
            }

            return genome;
        }
        #endregion

        #region Reproduction
        /// <summary>
        /// Asexual reproduction
        /// </summary>
        /// <returns></returns>
        public Genome CreateOffSpring(Random generator)
        {
            Genome offspring = new Genome();
            offspring.neurons = neurons;
            offspring.connections = connections;
            offspring.neuronIndex = neuronIndex;
            if (generator.NextDouble() < probabilityOfMutation)
            {
                if (generator.NextDouble() < probabilityOfChangeWeight)
                {
                    offspring = Genome.MutationChangeWeight(generator, offspring);
                }
                if (generator.NextDouble() < probabilityAddNeuron)
                {
                    offspring = Genome.MutationAddNeuron(generator, offspring);
                }
                if (generator.NextDouble() < probabilityRemoveNeuron)
                {
                    offspring = Genome.MutationRemoveNeuron(generator, offspring);
                }
                if (generator.NextDouble() < probabilityAddConnection)
                {
                    offspring = Genome.MutationAddConnection(generator, offspring);
                }
                if (generator.NextDouble() < probabilityRemoveConnection)
                {
                    offspring = Genome.MutationRemoveConnection(generator, offspring);
                }
            }
            return offspring;
        }

        /// <summary>
        /// Sexual reproduction
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="otherParent"></param>
        /// <returns></returns>
        public Genome CreateOffSpring(Random generator, Genome otherParent)
        {
            Genome offspring = new Genome();
            offspring.neurons = neurons;
            offspring.connections = connections;
            offspring.neuronIndex = neuronIndex;

            if (generator.NextDouble() < probabilityOfMutation)
            {

            }
            return offspring;
        }
        #endregion

        public override string ToString()
        {
            string str = "";
            for (int i = 0; i < neurons.Count; i++)
            {
                str += neurons[i].ToString() + "\n";
            }

            return str;
        }
    }


    class DNeuron
    {
        #region Private
        bool isSetDepthCalled;
        int setDepthCalls = 0;
        #endregion

        #region Public
        public List<DConnection> outConnections;

        public bool isBias;
        public bool isInput;
        public bool isOutput;

        public int amountOfInConnections;

        public int Id;

        public List<double> inputsAdded;
        public double output;

        public List<int> depths;
        public int depth;
        #endregion

        public DNeuron(int id)
        {
            isSetDepthCalled = false;

            depths = new List<int>();

            outConnections = new List<DConnection>();

            Id = id;
            inputsAdded = new List<double>();
            depth = 0;
            output = 0;
        }
        public void Activate()
        {
            double sum = 0;
            for (int i = 0; i < inputsAdded.Count; i++)
            {
                sum += inputsAdded[i];
            }
            inputsAdded.Clear();

            if (isInput)
            {
                output = sum;//linear
            }
            else if (isBias)
            {
                output = 1;//bias' output is always 1
            }
            else if (isOutput)
            {
                output = sum;//TODO: currently linear
            }
            else
            {
                output = sum;//TODO: currently linear
            }

            TransferOutput();
        }

        void TransferOutput()
        {
            foreach (DConnection conn in outConnections)
            {
                conn.toNeuron.inputsAdded.Add(output * conn.weight);
            }
        }

        static public DNeuron FindNeuronWithId(List<DNeuron> neuronslist, int id)
        {
            for (int i = 0; i < neuronslist.Count; i++)
            {
                if (neuronslist[i].Id == id)
                {
                    return neuronslist[i];
                }
            }
            throw new Exception();
        }

        #region Depth things
        public void SetDepth()
        {
            if (depths.Count != 0)
            {
                depth = depths.Min();
                //Console.Write("");
            }

        }
        public void SetDepthToOutNeurons()
        {

            if (setDepthCalls == amountOfInConnections && isInput != true && isBias != true)
            {
                isSetDepthCalled = true;
            }
            setDepthCalls += 1;
            if (outConnections.Count != 0 && isSetDepthCalled == false)
            {
                for (int i = 0; i < outConnections.Count; i++)
                {
                    if (outConnections[i].toNeuron.Equals(this) != true)
                    {
                        if (outConnections[i].toNeuron.depths.Contains(depth + 1) == false)
                        {
                            outConnections[i].toNeuron.depth = depth + 1;
                            outConnections[i].toNeuron.depths.Add(depth + 1);
                        }

                        outConnections[i].toNeuron.SetDepthToOutNeurons();
                        if (isInput || isBias)
                        {
                            isSetDepthCalled = true;
                        }
                    }
                }
            }

        }
        #endregion

        public override string ToString()
        {
            string str = "This id: " + Id;
            str += ", this n depth: " + depth;
            str += ", " + "Is input: " + isInput + ", " + "Is bias: " + isBias + ", " + "Is Output: " + isOutput + " ||| ";
            str += "Out: ";
            for (int i = 0; i < outConnections.Count; i++)
            {
                str += " Id = " + outConnections[i].toNeuron.Id;
                str += " Weight = " + outConnections[i].weight;
                str += " Depth = " + outConnections[i].toNeuron.depth;
            }

            return str;
        }

    }

    class DConnection
    {
        public double weight;
        public DNeuron toNeuron;
    }

    class Network
    {
        List<DNeuron> dneurons = new List<DNeuron>();
        List<DNeuron> inputs = new List<DNeuron>();
        List<DNeuron> outputs = new List<DNeuron>();
        List<DNeuron> hidden = new List<DNeuron>();

        public Network(Genome genome)
        {
            for (int i = 0; i < genome.neurons.Count; i++)
            {
                dneurons.Add(new DNeuron(genome.neurons[i].Id));
            }

            //iterate through all neurons
            DNeuron currneu;
            foreach (GNeuron neuron in genome.neurons)
            {
                currneu = DNeuron.FindNeuronWithId(dneurons, neuron.Id);

                currneu.amountOfInConnections = neuron.inConnections.Count;
                if (neuron.isInput || neuron.isBias)
                {
                    //currneu.isInput = true;
                    inputs.Add(currneu);
                    currneu.depths.Add(0);
                    //currneu.amountOfInConnections = neuron.inConnections.Count;
                    if (neuron.isBias)
                    {
                        currneu.isBias = true;
                    }
                    else if (neuron.isInput)
                    {
                        currneu.isInput = true;
                    }
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
            foreach (GConnection conn in genome.connections)
            {
                DConnection connout = new DConnection();
                connout.toNeuron = DNeuron.FindNeuronWithId(dneurons, conn.toNeuron.Id);
                connout.weight = conn.weight;
                DNeuron.FindNeuronWithId(dneurons, conn.fromNeuron.Id).outConnections.Add(connout);

            }

            //another check
            if (inputs.Last<DNeuron>().isBias != true)
            {
                throw new Exception();
            }

            //measure depth of all neurons
            for (int i = 0; i < inputs.Count; i++)
            {
                inputs[i].SetDepthToOutNeurons();
            }

            for (int i = 0; i < dneurons.Count; i++)
            {
                dneurons[i].SetDepth();
            }

            //sort list of neurons
            dneurons.Sort(new Comparer());
        }
        public double[] Predict(double[] state)
        {
            double[] prediction = new double[outputs.Count];

            for (int i = 0; i < inputs.Count - 1; i++)
            {
                inputs[i].inputsAdded.Add(state[i]);
                inputs[i].Activate();
            }
            inputs[inputs.Count - 1].Activate();//bias

            foreach (DNeuron neuron in hidden)
            {
                neuron.Activate();
            }

            foreach (DNeuron neuron in outputs)
            {
                neuron.Activate();
            }

            for (int i = 0; i < outputs.Count; i++)
            {
                prediction[i] = outputs[i].output;
            }
            return prediction;
        }

        public override string ToString()
        {
            string str = "\n";
            //write structure
            for (int i = 0; i < dneurons.Count; i++)
            {
                str += dneurons[i].ToString() + "\n";
            }
            str += "\n";
            return str;
        }
    }

    class Comparer : IComparer<DNeuron>
    {
        int IComparer<DNeuron>.Compare(DNeuron x, DNeuron y)
        {
            int compareDate = x.depth.CompareTo(y.depth);
            return compareDate;
        }
    }
}
