using CSharpNEAT.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpNEAT.GeneticAlgorithm
{
    public interface IGenomeFactory
    {
        IList<IGenome> CreateGenomeList(int population, Random random);
    }
}
