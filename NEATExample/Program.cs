using System;
using System.Collections.Generic;
using MyNEAT;
using MyNEAT.Decoder;
using MyNEAT.Domains.SinglePole;
using MyNEAT.Domains.XOR;
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

        private static void Test()
        {
            var gen = new Random();
            /*
            var env = new Xor(4);

            var pr = env.GetNums(gen);

            var str = "";
            for (var i = 0; i < pr[0].Length; i++)
                str += pr[0][i] + ", ";
            str += "\n";
            for (var i = 0; i < pr[1].Length; i++)
                str += pr[1][i] + ", ";

            Console.WriteLine(str);

            int err = env.GetError(new int[]{ 1,0 }, pr[1]);
            Console.Write(err);*/
            NEATGenome._conf = new Config()
            {
                inputs = 3,
                outputs = 2,
            };

            var genome = new NEATGenome(gen);

            Console.Write(genome + "\n\n");

            //for (var i = 0; i < 500; i++) genome = genome.CreateOffSpring(gen);

            //Console.Write(genome + "\n\n");

            var network = new Network(genome);

            var pr = network.Predict(new[] { -0.3f, 0.2f, 2f });

            var str = "";
            for (var i = 0; i < pr.Length; i++)
                str += pr[i] + ", ";
            Console.Write(str);

            //Console.WriteLine(network);
        }

        private static void SolveXor()
        {
            bool isCrossover = true;

            var elitism = 0.4f;
            var generations = 100000;
            var pop = 500;
            var generator = new Random();
            var env = new Xor();

            NEATGenome._conf = new Config()
            {
                inputs = 2,
                outputs = 1
            };

            var population = new List<NEATGenome>();

            //create initial pop
            for (var i = 0; i < pop; i++)
                population.Add(new NEATGenome(generator));

            for (var i = 0; i < generations; i++)
            {
                foreach (var genome in population)
                {
                    var network = new Network(genome);

                    genome.Fitness = 0;
                    for (int j = 0; j < 4; j++)
                    {
                        var (x, y) = env.GetNums(j);
                        var prediction = network.Predict(x);
                        float fit = 1 / (Math.Abs(env.GetError(prediction[0], y)) + 1);
                        genome.Fitness += (float)(fit - (genome.GetComplexity() * 0.0001));
                    }
                }
                population.Sort((x, y) => x.Fitness.CompareTo(y.Fitness));

                float sum = 0;
                float comp_sum = 0;
                var mx = population[0].Fitness;
                foreach (var genome in population)
                {
                    comp_sum += genome.GetComplexity();
                    sum += genome.Fitness;
                    if (genome.Fitness > mx)
                        mx = genome.Fitness;
                }
                Console.Write("Generation: " + i + ", " + "Average fitness: " + sum / population.Count + ", " +
                              "Max Fitness: " + mx + ", " + "Average complexity " + comp_sum / population.Count + "\n");

                //breed
                var toSelect = (int)(population.Count - elitism * population.Count);
                var addToPop = new List<NEATGenome>();
                for (; toSelect > 0; toSelect--)
                {
                    if (isCrossover)
                    {
                        var g1 = population[generator.Next(population.Count)];
                        population.Remove(g1);
                        var g2 = population[generator.Next(population.Count)];
                        population.Remove(g2);
                        var g3 = population[generator.Next(population.Count)];
                        population.Add(g1);
                        population.Add(g2);

                        var genomes = new List<NEATGenome>(new[] { g1, g2, g3 });
                        genomes.Sort((x, y) => x.Fitness.CompareTo(y.Fitness));

                        population.Remove(genomes[0]);

                        var tmp = (NEATGenome)genomes[1].Crossover(generator, genomes[2]);
                        tmp.Mutate(generator);
                        addToPop.Add(tmp);
                    }
                    else
                    {
                        var g1 = population[generator.Next(population.Count)];
                        population.Remove(g1);
                        var g2 = population[generator.Next(population.Count)];
                        population.Add(g1);

                        var genomes = new List<NEATGenome>(new[] { g1, g2 });
                        genomes.Sort((x, y) => x.Fitness.CompareTo(y.Fitness));
                        population.Remove(genomes[0]);

                        var tmp = (NEATGenome)genomes[1].Clone();
                        tmp.Mutate(generator);
                        addToPop.Add(tmp);
                    }
                }
                population.AddRange(addToPop);
            }
        }

        private static void SolveCartPole()
        {
            var elitism = 0.4f;
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

            for (var i = 0; i < generations; i++)
            {
                foreach (var genome in population)
                {
                    //evaluation
                    var env = new SinglePoleBalancingEnvironment();
                    var network = new Network(genome);
                    var s = env.SimulateTimestep(true);
                    while (true)
                    {
                        if (s._done)
                        {
                            genome.Fitness = (float)(s._reward - Math.Sqrt(Math.Sqrt(genome.GetComplexity())));
                            //genome.fitness = s._reward;
                            break;
                        }

                        var a = network.Predict(new[]
                        {
                            (float)(s._cartPosX / env._trackLengthHalf),
                            (float)(s._cartVelocityX / 0.75),
                            (float)(s._poleAngle / SinglePoleBalancingEnvironment.TwelveDegrees),
                            (float)s._poleAngularVelocity
                        })[0] > 0;

                        env.SimulateTimestep(a);
                    }
                }
                float sum = 0;
                float comp_sum = 0;
                var mx = population[0].Fitness;
                foreach (var genome in population)
                {
                    comp_sum += genome.GetComplexity();
                    sum += genome.Fitness;
                    if (genome.Fitness > mx)
                        mx = genome.Fitness;
                }
                Console.Write("Generation: " + i + ", " + "Average fitness: " + sum / population.Count + ", " +
                              "Max Fitness: " + mx + ", " + "Average complexity " + comp_sum / population.Count + "\n");

                //breed
                var toSelect = (int)(population.Count - elitism * population.Count);
                var addToPop = new List<NEATGenome>();
                for (; toSelect > 0; toSelect--)
                {
                    var g1 = population[generator.Next(population.Count)];
                    population.Remove(g1);
                    var g2 = population[generator.Next(population.Count)];
                    population.Remove(g2);
                    var g3 = population[generator.Next(population.Count)];
                    population.Add(g1);
                    population.Add(g2);

                    var genomes = new List<NEATGenome>(new[] { g1, g2, g3 });
                    genomes.Sort((x, y) => x.Fitness.CompareTo(y.Fitness));

                    population.Remove(genomes[0]);

                    var tmp = (NEATGenome)genomes[1].Crossover(generator, genomes[2]);
                    tmp.Mutate(generator);
                    addToPop.Add(tmp);
                }
                population.AddRange(addToPop);
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
