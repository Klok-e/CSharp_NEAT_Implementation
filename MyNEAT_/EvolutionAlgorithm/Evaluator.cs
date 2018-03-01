using MyNEAT.Genome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNEAT.EvolutionAlgorithm
{
    public interface IEvaluator
    {
        void Evaluate(IList<IGenome> genomes);
    }
}
