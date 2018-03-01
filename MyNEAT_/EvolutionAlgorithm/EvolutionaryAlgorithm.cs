using MyNEAT.Genome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNEAT.EvolutionAlgorithm
{
    public class EvolutionaryAlgorithm
    {
        private List<IGenome> _population;
        private IEvaluator _evaluator;

        private float _elitism;
        private Random _generator;

        public EvolutionaryAlgorithm(IEvaluator evaluator)
        {
            _evaluator = evaluator;
            _population = new List<IGenome>();
        }

        public void PassGeneration()
        {
            bool isCrossover = true;

            _evaluator.Evaluate(_population);

            var toSelect = (int)(_population.Count - _elitism * _population.Count);
            var addToPop = new List<IGenome>();
            for (; toSelect > 0; toSelect--)
            {
                if (isCrossover)
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

                    var tmp = genomes[1].Crossover(_generator, genomes[2]);
                    tmp.Mutate(_generator);
                    addToPop.Add(tmp);
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

                    var tmp = genomes[1].Clone();
                    tmp.Mutate(_generator);
                    addToPop.Add(tmp);
                }
            }
            _population.AddRange(addToPop);
        }
    }
}
