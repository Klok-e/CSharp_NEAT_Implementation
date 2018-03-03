using MyNEAT.Genome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNEAT.Decoder
{
    public interface IDecoder
    {
        IBlackBox Decode(IGenome genome);
    }
}
