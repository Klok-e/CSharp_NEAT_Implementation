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

        int Complexity { get; }

        void Mutate(Random generator, AlgorithmConfig config, bool end);

        IGenome Crossover(Random generator, IGenome other);

        IGenome Clone();
    }
}
