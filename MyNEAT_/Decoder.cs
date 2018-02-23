using MyNEAT.ActivationFunctions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyNEAT
{
    internal class DNeuron
    {
        #region Public

        public static IActivationFunction normalActivation;
        public static IActivationFunction outpActivation;

        public List<DConnection> outConnections;

        public bool isBias;
        public bool isInput;
        public bool isOutput;

        public int amountOfInConnections;

        public int id;

        public List<float> inputsAdded;
        public float output;

        public List<int> depths;
        public int depth;

        #endregion Public

        public DNeuron(int id)
        {
            depths = new List<int>();

            outConnections = new List<DConnection>();

            this.id = id;
            inputsAdded = new List<float>();
            depth = 0;
            output = 0;
        }

        public void Activate()
        {
            float sum = 0;
            for (var i = 0; i < inputsAdded.Count; i++)
                sum += inputsAdded[i];
            inputsAdded.Clear();

            if (isInput)
                output = sum; //linear
            else if (isBias)
                output = 1; //bias' output is always 1
            else if (isOutput)
                output = outpActivation.Eval(sum);
            else
                output = normalActivation.Eval(sum);

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

        #endregion Depth things

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
    }

    internal class DConnection
    {
        public DNeuron toNeuron;
        public float weight;
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
                if (neuron.IsInput || neuron.IsBias)
                {
                    //currneu.isInput = true;
                    inputs.Add(currneu);
                    //currneu.amountOfInConnections = neuron.inConnections.Count;
                    if (neuron.IsBias)
                        currneu.isBias = true;
                    else if (neuron.IsInput)
                        currneu.isInput = true;
                }
                else if (neuron.IsOutput)
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
            dneurons.Sort((x, y) => x.depth.CompareTo(y.depth));
        }

        public float[] Predict(float[] state)
        {
            var prediction = new float[outputs.Count];

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
}
