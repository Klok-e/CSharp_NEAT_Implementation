using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyNEAT;
using MyNEAT.Domains;

class Program
{

    static void Main()
    {
        SolveCartPole();
        //Test();
        Console.ReadKey();
    }

    static void Test()
    {
        Random gen = new Random();
        Genome genome = new Genome(3, 2);

        Console.Write(genome + "\n\n");

        genome = genome.CreateOffSpring(gen);

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
    }

    static void SolveCartPole()
    {
        float elitism = 0.4f;
        int generations = 5000;
        int pop = 100;
        Random generator = new Random();

        List<Genome> population = new List<Genome>();

        //create initial pop
        for (int i = 0; i < pop; i++)
        {
            population.Add(new Genome(4, 1));
        }

        for (int i = 0; i < generations; i++)
        {
            foreach (Genome genome in population)
            {
                SinglePoleBalancingEnvironment env = new SinglePoleBalancingEnvironment();
                Network network = new Network(genome);
                SinglePoleStateData s = env.SimulateTimestep(true);
                while (true)
                {
                    if (s._done == true)
                    {
                        genome.fitness = (float)(s._reward - Math.Sqrt(Math.Sqrt(genome.GetComplexity())));
                        break;
                    }

                    bool a = network.Predict(new double[] { s._cartPosX / env._trackLengthHalf,
                        s._cartVelocityX / 0.75,
                        s._poleAngle / SinglePoleBalancingEnvironment.TwelveDegrees,
                        s._poleAngularVelocity})[0] > 0;

                    env.SimulateTimestep(a);
                }
            }
            float sum = 0;
            float mx = -10000;
            foreach (Genome genome in population)
            {
                sum += genome.fitness;
                if (genome.fitness > mx)
                {
                    mx = genome.fitness;
                }
            }
            Console.Write("Generation: " + i + ", " + "Average fitness: " + sum / population.Count + ", " + "Max Fitness: " + mx + "\n");

            //breed
            int toSelect = (int)(population.Count - elitism * population.Count);
            List<Genome> addToPop = new List<Genome>();
            for (; toSelect > 0; toSelect--)
            {
                var g1 = population[generator.Next(population.Count)];
                population.Remove(g1);
                var g2 = population[generator.Next(population.Count)];
                population.Remove(g2);
                var g3 = population[generator.Next(population.Count)];
                population.Add(g1);
                population.Add(g2);

                List<Genome> genomes = new List<Genome>(new Genome[] { g1, g2, g3 });
                genomes.Sort(new Comparer2());

                population.Remove(genomes[0]);

                addToPop.Add(genomes[1].CreateOffSpring(generator, genomes[2]));

            }
            population.AddRange(addToPop);
        } 
    }
}
class Comparer2 : IComparer<Genome>
{
    public int Compare(Genome x, Genome y)
    {
        int compareDate = x.fitness.CompareTo(y.fitness);
        return compareDate;
    }
}


namespace MyNEAT.Domains
{
    /// <summary>
    /// Model state variables for single pole balancing task.
    /// </summary>
    public class SinglePoleStateData
    {
        /// <summary>
        /// Cart position (meters from origin).
        /// </summary>
		public double _cartPosX;
        /// <summary>
        /// Cart velocity (m/s).
        /// </summary>
		public double _cartVelocityX;
        /// <summary>
        /// Pole angle (radians). Straight up = 0.
        /// </summary>
		public double _poleAngle;
        /// <summary>
        /// Pole angular velocity (radians/sec).
        /// </summary>
		public double _poleAngularVelocity;
        /// <summary>
        /// Action applied during most recent timestep.
        /// </summary>
		public bool _action;

        public float _reward;

        public bool _done;
    }

    public class SinglePoleBalancingEnvironment
    {
        #region Constants

        // Some physical model constants.
        public const double Gravity = 9.8;
        public const double MassCart = 1.0;
        public const double MassPole = 0.1;
        public const double TotalMass = (MassPole + MassCart);
        public const double Length = 0.5;    // actually half the pole's length.
        public const double PoleMassLength = (MassPole * Length);
        public const double ForceMag = 10.0;
        /// <summary>Time increment interval in seconds.</summary>
		public const double TimeDelta = 0.02;
        public const double FourThirds = 4.0 / 3.0;

        // Some precalced angle constants.
        public const double OneDegree = Math.PI / 180.0;   //= 0.0174532;
        public const double SixDegrees = Math.PI / 30.0;   //= 0.1047192;
        public const double TwelveDegrees = Math.PI / 15.0;    //= 0.2094384;
        public const double TwentyFourDegrees = Math.PI / 7.5; //= 0.2094384;
        public const double ThirtySixDegrees = Math.PI / 5.0;  //= 0.628329;
        public const double FiftyDegrees = Math.PI / 3.6;  //= 0.87266;

        #endregion

        #region Class Variables

        // Domain parameters.
        public SinglePoleStateData currState;
        int stepsPassed;

        public double _trackLength;
        public double _trackLengthHalf;
        public int _maxTimesteps;
        public double _poleAngleThreshold;

        #endregion

        #region Constructors

        /// <summary>
        /// Construct evaluator with default task arguments/variables.
        /// </summary>
		public SinglePoleBalancingEnvironment() : this(4.8, 300, TwelveDegrees)
        { }

        /// <summary>
        /// Construct evaluator with the provided task arguments/variables.
        /// </summary>
		public SinglePoleBalancingEnvironment(double trackLength, int maxTimesteps, double poleAngleThreshold)
        {
            _trackLength = trackLength;
            _trackLengthHalf = trackLength / 2.0;
            _maxTimesteps = maxTimesteps;
            _poleAngleThreshold = poleAngleThreshold;
            currState = new SinglePoleStateData();
            currState._poleAngle = SixDegrees;
            stepsPassed = 0;
        }

        #endregion



        /// <summary>
        /// Calculates a state update for the next timestep using current model state and a single 'action' from the
        /// controller. The action specifies if the controller is pushing the cart left or right. Note that this is a binary 
        /// action and therefore full force is always applied to the cart in some direction. This is the standard model for
        /// the single pole balancing task.
        /// </summary>
        /// <param name="action">push direction, left(false) or right(true). Force magnitude is fixed.</param>
        public SinglePoleStateData SimulateTimestep(bool action)
        {
            stepsPassed += 1;
            //float xacc,thetaacc,force,costheta,sintheta,temp;
            double force = action ? ForceMag : -ForceMag;
            double cosTheta = Math.Cos(currState._poleAngle);
            double sinTheta = Math.Sin(currState._poleAngle);
            double tmp = (force + (PoleMassLength * currState._poleAngularVelocity * currState._poleAngularVelocity * sinTheta)) / TotalMass;

            double thetaAcc = ((Gravity * sinTheta) - (cosTheta * tmp))
                            / (Length * (FourThirds - ((MassPole * cosTheta * cosTheta) / TotalMass)));

            double xAcc = tmp - ((PoleMassLength * thetaAcc * cosTheta) / TotalMass);


            // Update the four state variables, using Euler's method.
            currState._cartPosX += TimeDelta * currState._cartVelocityX;
            currState._cartVelocityX += TimeDelta * xAcc;
            currState._poleAngle += TimeDelta * currState._poleAngularVelocity;
            currState._poleAngularVelocity += TimeDelta * thetaAcc;
            currState._action = action;
            currState._reward = stepsPassed;
            currState._done = (currState._cartPosX < -_trackLengthHalf) ||
                (currState._cartPosX > _trackLengthHalf) ||
                (currState._poleAngle > _poleAngleThreshold) ||
                (currState._poleAngle < -_poleAngleThreshold) ||
                (stepsPassed > _maxTimesteps);

            return currState;
        }
    }
}

namespace MyNEAT
{
    class GNeuron
    {
        //public List<int> inConnections;
        //public List<int> outConnections;
        public bool isOutput;
        public bool isInput;
        public bool isBias;
        public bool isHidden;

        public readonly int Id;

        public GNeuron(int id)
        {
            //inConnections = new List<int>();
            //outConnections = new List<int>();
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
            str += ", " + "Is input: " + isInput + ", " + "Is bias: " + isBias + ", " + "Is Output: " + isOutput;

            return str;
        }
    }

    class GConnection
    {
        public readonly int id;
        public readonly double weight;
        public readonly int fromNeuron;
        public readonly int toNeuron;
        public GConnection(GNeuron fromneuron, GNeuron toneuron, double wei, int idForThis)
        {
            fromNeuron = fromneuron.Id;
            toNeuron = toneuron.Id;

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
            string str = "This id: " + id + ", ";
            str += "This weight: " + Math.Round(weight, 2) + ", |||";
            str += "From: " + fromNeuron + ", ";
            str += "To: " + toNeuron + ", ";
            return str;
        }


    }

    class Genome
    {
        public float fitness;

        public List<GNeuron> neurons;
        public List<GConnection> connections;

        public static double connWeightRange = 5d;
        public static double weightChangeRange = 0.5;

        public static double probabilityOfMutation = 0.9;
        public static double probabilityOfResetWeight = 0.2;
        public static double probabilityOfChangeWeight = 0.6;
        public static double probabilityAddNeuron = 0.1;
        public static double probabilityRemoveNeuron = 0.01;
        public static double probabilityAddConnection = 0.3;
        public static double probabilityRemoveConnection = 0.1;

        public static int geneIndex;

        #region Constructors
        public Genome(int inputs, int outputs)
        {
            geneIndex = 0;
            neurons = new List<GNeuron>();
            connections = new List<GConnection>();

            for (int i = 0; i < inputs; i++)//only inputs
            {
                GNeuron inpNeuron = new GNeuron(geneIndex);
                inpNeuron.isInput = true;
                geneIndex++;

                neurons.Add(inpNeuron);
            }

            GNeuron biasNeuron = new GNeuron(geneIndex);
            biasNeuron.isBias = true;
            geneIndex++;
            neurons.Add(biasNeuron);

            for (int i = 0; i < outputs; i++)//only output neurons
            {
                GNeuron outNeuron = new GNeuron(geneIndex);
                outNeuron.isOutput = true;
                geneIndex++;

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
                            GConnection conn = new GConnection(neuron1, neuron, randGenerator.NextDouble() * (connWeightRange - (-connWeightRange)) + (-connWeightRange), geneIndex);
                            geneIndex++;

                            connections.Add(conn);
                        }
                    }
                }
            }


        }


        public Genome()
        {
        }
        #endregion

        #region Mutators
        public static Genome MutationChangeWeight(Random generator, Genome genome)
        {
            if (genome.connections.Count != 0)
            {
                int num = generator.Next(genome.connections.Count);
                GConnection conn = genome.connections[num];
                if (generator.NextDouble() < probabilityOfResetWeight)
                {
                    genome.connections[num] = new GConnection(conn.fromNeuron, conn.toNeuron, generator.NextDouble() * (connWeightRange - (-connWeightRange)) + (-connWeightRange), conn.id);

                }
                else
                {
                    genome.connections[num] = new GConnection(conn.fromNeuron, conn.toNeuron, genome.connections[num].weight + generator.NextDouble() * (weightChangeRange - (-weightChangeRange)) + (-weightChangeRange), conn.id);
                }
            }
            return genome;
        }

        public static Genome MutationAddNeuron(Random generator, Genome genome)
        {
            if (genome.connections.Count != 0)
            {
                int ind = generator.Next(genome.connections.Count);
                GConnection conn = genome.connections[ind];
                genome.connections.RemoveAt(ind);

                GNeuron newNeuron = new GNeuron(Genome.geneIndex);
                genome.neurons.Add(newNeuron);
                Genome.geneIndex++;

                GConnection newConnIn = new GConnection(conn.fromNeuron, newNeuron.Id, 1, Genome.geneIndex);
                genome.connections.Add(newConnIn);
                Genome.geneIndex++;

                GConnection newConnOut = new GConnection(newNeuron.Id, conn.toNeuron, conn.weight, Genome.geneIndex);
                genome.connections.Add(newConnOut);
                Genome.geneIndex++;
            }

            return genome;
        }

        public static Genome MutationRemoveNeuron(Random generator, Genome genome)
        {
            List<GNeuron> availableNeurons = new List<GNeuron>();
            for (int i = 0; i < genome.neurons.Count; i++)
            {
                int[] inOut = Genome.FindAmountOfInAndOutConnectionsForNeuronWithId(genome.connections, genome.neurons[i].Id);
                if (inOut[0] == 0 && inOut[1] == 0 && genome.neurons[i].isInput != true && genome.neurons[i].isOutput != true && genome.neurons[i].isBias != true)
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

            var n1InOut = GetListOfInAndOutConnections(genome.connections, neuron1.Id);
            var n2InOut = GetListOfInAndOutConnections(genome.connections, neuron2.Id);
            if (n1InOut[1].Intersect(n2InOut[0]).Count() == 0 && n1InOut[0].Intersect(n2InOut[1]).Count() == 0)
            {
                genome.connections.Add(new GConnection(neuron1.Id, neuron2.Id,
                    generator.NextDouble() * (connWeightRange - (-connWeightRange)) + (-connWeightRange),
                    Genome.geneIndex));
                Genome.geneIndex++;
            }
            return genome;
        }

        public static Genome MutationRemoveConnection(Random generator, Genome genome)
        {
            if (genome.connections.Count != 0)
            {
                GConnection connToRemove = genome.connections[generator.Next(genome.connections.Count)];
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
            offspring.neurons = new List<GNeuron>(neurons);
            offspring.connections = new List<GConnection>(connections);

            if (generator.NextDouble() < probabilityOfMutation)
            {
                offspring = Mutate(generator, offspring);
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
            Genome offspring = Crossover(generator, this, otherParent);

            if (generator.NextDouble() < probabilityOfMutation)
            {
                offspring = Mutate(generator, offspring);
            }
            return offspring;
        }
        #endregion

        #region Static methods
        public static Genome Crossover(Random generator, Genome parent1, Genome parent2)
        {
            List<GNeuron> neurons = new List<GNeuron>();

            #region Build neurons
            for (int i = 0; i < Math.Min(parent1.neurons.Count, parent2.neurons.Count); i++)
            {
                if (parent1.neurons[i].Id == parent2.neurons[i].Id)
                {
                    neurons.Add(parent1.neurons[i]);
                }
                else
                {
                    GNeuron[] arr = new GNeuron[] { parent1.neurons[i], parent2.neurons[i] };
                    GNeuron toAdd = arr[generator.Next(arr.Length)];
                    neurons.Add(toAdd);
                    break;
                }
            }
            if (neurons.Count != Math.Max(parent1.neurons.Count, parent2.neurons.Count))
            {
                for (int i = Math.Min(parent1.neurons.Count, parent2.neurons.Count) - 1; i < Math.Max(parent1.neurons.Count, parent2.neurons.Count); i++)
                {
                    if (Math.Max(parent1.neurons.Count, parent2.neurons.Count) == parent1.neurons.Count)
                    {
                        neurons.Add(parent1.neurons[i]);
                    }
                    else
                    {
                        neurons.Add(parent2.neurons[i]);
                    }
                }
            }
            #endregion

            List<GConnection> connections = new List<GConnection>();

            List<GConnection> tempconnections = new List<GConnection>();
            tempconnections.AddRange(parent1.connections);
            tempconnections.AddRange(parent2.connections);

            #region Build connections
            for (int i = 0; i < Math.Min(parent1.connections.Count, parent2.connections.Count); i++)
            {
                if (parent1.connections[i].id == parent2.connections[i].id)
                {
                    if (IsGWithIdExistsInList(neurons, parent1.connections[i].fromNeuron) && IsGWithIdExistsInList(neurons, parent1.connections[i].toNeuron))
                    {
                        connections.Add(parent1.connections[i]);
                    }
                }
                else
                {
                    GConnection[] arr = new GConnection[] { parent1.connections[i], parent2.connections[i] };
                    int num = generator.Next(arr.Length);
                    if (IsGWithIdExistsInList(neurons, arr[num].fromNeuron) && IsGWithIdExistsInList(neurons, arr[num].toNeuron))
                    {
                        connections.Add(arr[num]);
                    }
                    else if (IsGWithIdExistsInList(neurons, arr[Math.Abs(num - 1)].fromNeuron) && IsGWithIdExistsInList(neurons, arr[Math.Abs(num - 1)].toNeuron))
                    {
                        connections.Add(arr[Math.Abs(num - 1)]);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            }
            if (connections.Count != Math.Max(parent1.connections.Count, parent2.connections.Count))
            {
                for (int i = Math.Min(parent1.connections.Count, parent2.connections.Count) - 1; i < Math.Max(parent1.connections.Count, parent2.connections.Count); i++)
                {

                    if (Math.Max(parent1.connections.Count, parent2.connections.Count) == parent1.connections.Count)
                    {
                        if (IsGWithIdExistsInList(neurons, parent1.connections[i].fromNeuron) && IsGWithIdExistsInList(neurons, parent1.connections[i].toNeuron))
                        {
                            connections.Add(parent1.connections[i]);
                        }
                    }
                    else
                    {
                        if (IsGWithIdExistsInList(neurons, parent2.connections[i].fromNeuron) && IsGWithIdExistsInList(neurons, parent2.connections[i].toNeuron))
                        {
                            connections.Add(parent2.connections[i]);
                        }
                    }
                }
            }
            #endregion

            Genome child = new Genome();
            child.neurons = new List<GNeuron>(neurons);
            child.connections = new List<GConnection>(connections);
            return child;
        }

        public static Genome Mutate(Random generator, Genome toMutate)
        {

            if (generator.NextDouble() < probabilityOfChangeWeight)
            {
                toMutate = Genome.MutationChangeWeight(generator, toMutate);
            }
            if (generator.NextDouble() < probabilityAddNeuron)
            {
                toMutate = Genome.MutationAddNeuron(generator, toMutate);
            }
            if (generator.NextDouble() < probabilityRemoveNeuron)
            {
                toMutate = Genome.MutationRemoveNeuron(generator, toMutate);
            }
            if (generator.NextDouble() < probabilityAddConnection)
            {
                toMutate = Genome.MutationAddConnection(generator, toMutate);
            }
            if (generator.NextDouble() < probabilityRemoveConnection)
            {
                toMutate = Genome.MutationRemoveConnection(generator, toMutate);
            }

            return toMutate;
        }

        public static bool IsGWithIdExistsInList(List<GNeuron> neurons, int id)
        {
            foreach (GNeuron neuron in neurons)
            {
                if (neuron.Id == id)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsGWithIdExistsInList(List<GConnection> conns, int id)
        {
            foreach (GConnection conn in conns)
            {
                if (conn.id == id)
                {
                    return true;
                }
            }
            return false;
        }

        public static int[] FindAmountOfInAndOutConnectionsForNeuronWithId(List<GConnection> connectionList, int id)
        {
            int sumIn = 0;
            int sumOut = 0;
            foreach (GConnection conn in connectionList)
            {
                if (conn.toNeuron == id)
                {
                    sumIn++;
                }
                if (conn.fromNeuron == id)
                {
                    sumOut++;
                }
            }
            return new int[] { sumIn, sumOut };
        }
        public static List<GConnection>[] GetListOfInAndOutConnections(List<GConnection> connectionList, int id)
        {
            List<GConnection> inConn = new List<GConnection>();
            List<GConnection> outConn = new List<GConnection>();
            foreach (GConnection conn in connectionList)
            {
                if (conn.toNeuron == id)
                {
                    inConn.Add(conn);
                }
                if (conn.fromNeuron == id)
                {
                    outConn.Add(conn);
                }
            }
            return new List<GConnection>[] { inConn, outConn };

        }
        #endregion

        public override string ToString()
        {
            string str = "";
            for (int i = 0; i < neurons.Count; i++)
            {
                str += neurons[i].ToString() + "\n";
            }
            str += "\n";
            for (int i = 0; i < connections.Count; i++)
            {
                str += connections[i].ToString() + "\n";
            }

            return str;
        }

        public float GetComplexity()
        {
            float sum = 0;
            sum += neurons.Count + connections.Count;
            return sum;
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
                output = Math.Tanh(sum);//TODO: currently linear
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
                            outConnections[i].toNeuron.depths.Add(depth + 1);
                            outConnections[i].toNeuron.SetDepth();
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
                str += " Weight = " + Math.Round(outConnections[i].weight, 2);
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

                currneu.amountOfInConnections = Genome.FindAmountOfInAndOutConnectionsForNeuronWithId(genome.connections, neuron.Id)[0];
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
                connout.toNeuron = DNeuron.FindNeuronWithId(dneurons, conn.toNeuron);
                connout.weight = conn.weight;
                DNeuron.FindNeuronWithId(dneurons, conn.fromNeuron).outConnections.Add(connout);

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
