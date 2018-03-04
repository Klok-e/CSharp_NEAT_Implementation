using System;
using System.Collections.Generic;
using System.Linq;
using MyNEAT;
using MyNEAT.Decoder;
using MyNEAT.Domains.SinglePole;
using MyNEAT.Domains.XOR;
using MyNEAT.GeneticAlgorithm;
using MyNEAT.Genome;

namespace NEATExample
{
    internal static class Program
    {
        private static void Main()
        {
            //SolveCartPole();
            //Test();
            SolveXor();

            Console.ReadKey();
        }

        private class XorEval : IEvaluator
        {
            private Xor env;
            private NEATDecoder decoder;

            public XorEval()
            {
                env = new Xor();
                decoder = new NEATDecoder();
            }

            public void Evaluate(IList<IGenome> genomes)
            {
                var gen = genomes.Cast<NEATGenome>().ToList();

#if DEBUG
                foreach (var genome in gen)
                {
                    bool biasExists = false;
                    foreach (var item in genome.Neurons)
                    {
                        if (item.Type == NeuronType.bias)
                            biasExists = true;
                    }
                    if (!biasExists) throw new Exception();
                }
#endif

                foreach (var genome in gen)
                {
                    var network = decoder.Decode(genome);

                    genome.Fitness = 0;
                    for (int j = 0; j < 4; j++)
                    {
                        var (x, y) = env.GetNums(j);

                        for (int i = 0; i < x.Length; i++)
                        {
                            network.Inputs[i] = x[i];
                        }
                        network.Activate();
                        var prediction = network.Outputs;
                        network.Reset();

                        float fit = 1 / (Math.Abs(env.GetError(prediction[0], y)) + 1);
                        genome.Fitness += (float)(fit - (genome.Complexity * 0.0001));
                    }
                }
                gen.Sort((x, y) => x.Fitness.CompareTo(y.Fitness));

                float sum = 0;
                float comp_sum = 0;
                var mx = genomes[0].Fitness;
                foreach (var genome in gen)
                {
                    comp_sum += genome.Complexity;
                    sum += genome.Fitness;
                    if (genome.Fitness > mx)
                        mx = genome.Fitness;
                }
                Console.Write("Generation: " + 0 + ", " + "Average fitness: " + sum / gen.Count + ", " +
                              "Max Fitness: " + mx + ", " + "Average complexity " + comp_sum / gen.Count + "\n");
            }
        }

        private static void SolveXor()
        {
            var generations = 100000;
            var pop = 500;
            var generator = new Random();

            var fabric = new NEATGenomeFactory(new GenomeConfig()
            {
                inputs = 2,
                outputs = 1,
            });
            var algCofig = new AlgorithmConfig()
            {
                probabilityAddConnection = 0.4f,
                probabilityAddNeuron = 0.1f,
                probabilityRemoveConnection = 0.3f,
            };

            var algor = new NEATEvolAlgorithm(generator, new XorEval(), fabric, algCofig, pop);

            for (var i = 0; i < generations; i++)
            {
                algor.PassGeneration();
            }
        }

        private class CartPoleEval : IEvaluator
        {
            private NEATDecoder decoder;

            public CartPoleEval()
            {
                decoder = new NEATDecoder();
            }

            public void Evaluate(IList<IGenome> genomes)
            {
                var pop = genomes.Cast<NEATGenome>().ToList();
                foreach (var genome in pop)
                {
                    //evaluation
                    var env = new SinglePoleBalancingEnvironment();
                    var network = decoder.Decode(genome);
                    var s = env.SimulateTimestep(true);
                    while (true)
                    {
                        if (s._done)
                        {
                            genome.Fitness = (s._reward - (float)genome.Complexity * 0.0001f);
                            //genome.fitness = s._reward;
                            break;
                        }

                        network.Inputs[0] = (float)(s._cartPosX / env._trackLengthHalf);
                        network.Inputs[1] = (float)(s._cartVelocityX / 0.75);
                        network.Inputs[2] = (float)(s._poleAngle / SinglePoleBalancingEnvironment.TwelveDegrees);
                        network.Inputs[3] = (float)s._poleAngularVelocity;

                        network.Activate();
                        var a = network.Outputs[0] > 0;

                        env.SimulateTimestep(a);
                    }
                }
                float sum = 0;
                float comp_sum = 0;
                var mx = pop[0].Fitness;
                foreach (var genome in pop)
                {
                    comp_sum += genome.Complexity;
                    sum += genome.Fitness;
                    if (genome.Fitness > mx)
                        mx = genome.Fitness;
                }
                Console.Write("Generation: " + 0 + ", " + "Average fitness: " + sum / pop.Count + ", " +
                              "Max Fitness: " + mx + ", " + "Average complexity " + comp_sum / pop.Count + "\n");
            }
        }

        private static void SolveCartPole()
        {
            var generations = 500;
            var pop = 100;
            var generator = new Random();

            var factory = new NEATGenomeFactory(new GenomeConfig()
            {
                inputs = 4,
                outputs = 1
            });
            var algConf = new AlgorithmConfig()
            {
            };

            var algor = new NEATEvolAlgorithm(generator, new CartPoleEval(), factory, algConf, pop);

            for (var i = 0; i < generations; i++)
            {
                algor.PassGeneration();
            }
        }

        private static int[] DoubleArrToIntArr(double[] arr)
        {
            var newArr = new int[arr.Length];
            for (var i = 0; i < arr.Length; i++)
                newArr[i] = (int)Math.Round(arr[i]);
            return newArr;
        }

        private static double[] IntArrToDoubleArr(int[] arr)
        {
            var newArr = new double[arr.Length];
            for (var i = 0; i < arr.Length; i++)
                newArr[i] = arr[i];
            return newArr;
        }
    }
}
