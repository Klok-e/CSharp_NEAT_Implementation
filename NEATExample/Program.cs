using System;
using System.Collections.Generic;
using MyNEAT;
using MyNEAT.Domains.SinglePole;
using MyNEAT.Domains.XOR;

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
            Genome.conf = new Config();

            var genome = new Genome(3, 2);


            Console.Write(genome + "\n\n");

            //for (var i = 0; i < 500; i++) genome = genome.CreateOffSpring(gen);


            //Console.Write(genome + "\n\n");


            var network = new Network(genome);

            var pr = network.Predict(new[] { -0.3, 0.2, 2 });

            var str = "";
            for (var i = 0; i < pr.Length; i++)
                str += pr[i] + ", ";
            Console.Write(str);

            //Console.WriteLine(network);
        }

        private static void SolveXor()
        {
            var elitism = 0.4f;
            var generations = 10000;
            var pop = 50;
            var generator = new Random();

            Genome.conf = new Config();

            var population = new List<Genome>();

            var num = 2;

            //create initial pop
            for (var i = 0; i < pop; i++)
                population.Add(new Genome(num, num / 2));

            for (var i = 0; i < generations; i++)
            {
                foreach (var genome in population)
                {
                    var network = new Network(genome);
                    var env = new Xor(num);

                    var nums = env.GetNums(generator);
                    genome.fitness =
                        (float)(env.GetError(network.Predict(nums[0]), nums[1]) - genome.GetComplexity() * 0.5);
                }


                float sum = 0;
                float comp_sum = 0;
                var mx = population[0].fitness;
                foreach (var genome in population)
                {
                    comp_sum += genome.GetComplexity();
                    sum += genome.fitness;
                    if (genome.fitness > mx)
                        mx = genome.fitness;
                }
                Console.Write("Generation: " + i + ", " + "Average fitness: " + sum / population.Count + ", " +
                              "Max Fitness: " + mx + ", " + "Average complexity " + comp_sum / population.Count + "\n");

                //breed
                var toSelect = (int)(population.Count - elitism * population.Count);
                var addToPop = new List<Genome>();
                for (; toSelect > 0; toSelect--)
                {
                    var g1 = population[generator.Next(population.Count)];
                    population.Remove(g1);
                    var g2 = population[generator.Next(population.Count)];
                    population.Remove(g2);
                    var g3 = population[generator.Next(population.Count)];
                    population.Add(g1);
                    population.Add(g2);

                    var genomes = new List<Genome>(new[] { g1, g2, g3 });
                    genomes.Sort(new Comparer2());

                    population.Remove(genomes[0]);

                    addToPop.Add(genomes[1].CreateOffSpring(generator, genomes[2]));
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

            Genome.conf = new Config();

            var population = new List<Genome>();

            //create initial pop
            for (var i = 0; i < pop; i++)
                population.Add(new Genome(4, 1));

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
                            genome.fitness = (float)(s._reward - Math.Sqrt(Math.Sqrt(genome.GetComplexity())));
                            //genome.fitness = s._reward;
                            break;
                        }

                        var a = network.Predict(new[]
                        {
                            s._cartPosX / env._trackLengthHalf,
                            s._cartVelocityX / 0.75,
                            s._poleAngle / SinglePoleBalancingEnvironment.TwelveDegrees,
                            s._poleAngularVelocity
                        })[0] > 0;

                        env.SimulateTimestep(a);
                    }
                }
                float sum = 0;
                float comp_sum = 0;
                var mx = population[0].fitness;
                foreach (var genome in population)
                {
                    comp_sum += genome.GetComplexity();
                    sum += genome.fitness;
                    if (genome.fitness > mx)
                        mx = genome.fitness;
                }
                Console.Write("Generation: " + i + ", " + "Average fitness: " + sum / population.Count + ", " +
                              "Max Fitness: " + mx + ", " + "Average complexity " + comp_sum / population.Count + "\n");

                //breed
                var toSelect = (int)(population.Count - elitism * population.Count);
                var addToPop = new List<Genome>();
                for (; toSelect > 0; toSelect--)
                {
                    var g1 = population[generator.Next(population.Count)];
                    population.Remove(g1);
                    var g2 = population[generator.Next(population.Count)];
                    population.Remove(g2);
                    var g3 = population[generator.Next(population.Count)];
                    population.Add(g1);
                    population.Add(g2);

                    var genomes = new List<Genome>(new[] { g1, g2, g3 });
                    genomes.Sort(new Comparer2());

                    population.Remove(genomes[0]);

                    addToPop.Add(genomes[1].CreateOffSpring(generator, genomes[2]));
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

    internal class Comparer2 : IComparer<Genome>
    {
        public int Compare(Genome x, Genome y)
        {
            var compareData = x.fitness.CompareTo(y.fitness);
            return compareData;
        }
    }
}