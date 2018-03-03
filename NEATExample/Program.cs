using System;
using System.Collections.Generic;
using System.Linq;
using MyNEAT;
using MyNEAT.Decoder;
using MyNEAT.Decoder.NEAT;
using MyNEAT.Domains.SinglePole;
using MyNEAT.Domains.XOR;
using MyNEAT.EvolutionAlgorithm;
using MyNEAT.Genome;
using MyNEAT.Genome.NEAT;

namespace NEATExample
{
    internal static class Program
    {
        private static void Main()
        {
            //SolveCartPole();
            Test();
            //SolveXor();

            Console.ReadKey();
        }

        private static void Test()
        {
            var gen = new Random();

            NEATGenome._conf = new Config()
            {
                inputs = 3,
                outputs = 2,
                probabilityAddConnection = 0.9f,
            };

            var genome = new NEATGenome(gen);

            Console.Write(genome + "\n\n");

            for (var i = 0; i < 500; i++)
            {
                var offspr = (NEATGenome)genome.Clone();
                offspr.Mutate(gen);
                genome = offspr;
            }
            Console.Write(genome + "\n\n");

            var decoder = new NEATDecoder();
            var network = decoder.Decode(genome);

            //new[] { -0.3f, 0.2f, 2f }

            network.Activate();
            var pr = network.Outputs;
            var str = "";
            for (var i = 0; i < pr.Length; i++)
                str += pr[i] + ", ";
            Console.Write(str);

            //Console.WriteLine(network);
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

            public void Evaluate(List<IGenome> genomes)
            {
                var gen = genomes.Cast<NEATGenome>().ToList();
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
                        genome.Fitness += (float)(fit - (genome.GetComplexity() * 0.0001));
                    }
                }
                gen.Sort((x, y) => x.Fitness.CompareTo(y.Fitness));

                float sum = 0;
                float comp_sum = 0;
                var mx = genomes[0].Fitness;
                foreach (var genome in gen)
                {
                    comp_sum += genome.GetComplexity();
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

            NEATGenome._conf = new Config()
            {
                inputs = 2,
                outputs = 1
            };

            var population = new List<NEATGenome>();

            //create initial pop
            for (var i = 0; i < pop; i++)
                population.Add(new NEATGenome(generator));

            var algor = new EvolutionaryAlgorithm(generator, new XorEval(), population.Cast<IGenome>().ToList());

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

            public void Evaluate(List<IGenome> genomes)
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
                            genome.Fitness = (s._reward - (float)genome.GetComplexity() * 0.0001f);
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
                    comp_sum += genome.GetComplexity();
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

            NEATGenome._conf = new Config()
            {
                inputs = 4,
                outputs = 1
            };

            var population = new List<NEATGenome>();

            //create initial pop
            for (var i = 0; i < pop; i++)
                population.Add(new NEATGenome(generator));

            var algor = new EvolutionaryAlgorithm(generator, new CartPoleEval(), population.Cast<IGenome>().ToList());

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
