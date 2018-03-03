using MyNEAT.Decoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNEAT.Genome
{
    public interface IGenome
    {
        float Fitness { get; set; }

        void Mutate(Random generator);

        IGenome Crossover(Random generator, IGenome other);

        IGenome Clone();
    }
}
