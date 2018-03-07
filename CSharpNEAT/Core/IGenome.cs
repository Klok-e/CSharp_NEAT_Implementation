using System;

namespace CSharpNEAT.Core
{
    public interface IGenome
    {
        float Fitness { get; set; }

        int Complexity { get; }
    }
}
