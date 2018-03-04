using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNEAT.Decoder
{
    public interface IBlackBox
    {
        float[] Inputs { get; }
        float[] Outputs { get; }

        void Activate();

        void Reset();
    }
}
