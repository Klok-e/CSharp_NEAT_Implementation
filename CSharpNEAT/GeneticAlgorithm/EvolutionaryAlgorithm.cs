using CSharpNEAT.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpNEAT.GeneticAlgorithm
{
    public class NEATEvolAlgorithm
    {
        private AlgorithmConfig _conf;
        private IList<IGenome> _population;
        private IEvaluator _evaluator;
        private IGenomeFactory _genomeFactory;
        private Random _generator;

        public NEATEvolAlgorithm(Random generator, IEvaluator evaluator, IGenomeFactory genomeFactory, AlgorithmConfig config, int popSize)
        {
            _evaluator = evaluator;
            _population = genomeFactory.CreateGenomeList(popSize, generator);
            _generator = generator;
            _conf = config;
            _genomeFactory = genomeFactory;
        }

        public void PassGeneration()
        {
            _conf.ComplexityHandler.HandleComplexity(_population, _conf);

            _evaluator.Evaluate(_population);

            var toSelect = (int)(_population.Count - _conf.elitism * _population.Count);
            var addToPop = new List<IGenome>();
            for (; toSelect > 0; toSelect--)
            {
                IGenome tmp;
                if (_generator.NextDouble() < _conf.crossoverChance)
                {
                    var g1 = _population[_generator.Next(_population.Count)];
                    _population.Remove(g1);
                    var g2 = _population[_generator.Next(_population.Count)];
                    _population.Remove(g2);
                    var g3 = _population[_generator.Next(_population.Count)];
                    _population.Add(g1);
                    _population.Add(g2);

                    var genomes = new List<IGenome>(new[] { g1, g2, g3 });
                    genomes.Sort((x, y) => x.Fitness.CompareTo(y.Fitness));
                    _population.Remove(genomes[0]);

                    tmp = genomes[1].Crossover(_generator, genomes[2]);
                    tmp.Fitness = ((genomes[1].Fitness + genomes[2].Fitness) / 2) * 0.9f;
                }
                else
                {
                    var g1 = _population[_generator.Next(_population.Count)];
                    _population.Remove(g1);
                    var g2 = _population[_generator.Next(_population.Count)];
                    _population.Add(g1);

                    var genomes = new List<IGenome>(new[] { g1, g2 });
                    genomes.Sort((x, y) => x.Fitness.CompareTo(y.Fitness));
                    _population.Remove(genomes[0]);

                    tmp = genomes[1].Clone();
                    tmp.Fitness = genomes[1].Fitness * 0.9f;
                }
                for (int i = 0; i < _conf.mutationAmount; i++)
                {
                    tmp.Mutate(_generator, _conf, toSelect == 1 ? true : false);
                }
                addToPop.Add(tmp);
            }
            foreach (var item in addToPop)
            {
                _population.Add(item);
            }
        }
    }
}
