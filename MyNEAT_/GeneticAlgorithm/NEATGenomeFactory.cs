using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNEAT.Genome;

namespace MyNEAT.GeneticAlgorithm
{
    public class NEATGenomeFactory : IGenomeFactory
    {
        private GenomeConfig _config;

        public NEATGenomeFactory(GenomeConfig config)
        {
            _config = config;
        }

        #region IGenomeFactory

        public IList<IGenome> CreateGenomeList(int population, Random random)
        {
            var pop = new List<IGenome>(population);
            for (int i = 0; i < population; i++)
            {
                pop.Add(new NEATGenome(random, _config));
            }
            return pop;
        }

        #endregion IGenomeFactory
    }
}
