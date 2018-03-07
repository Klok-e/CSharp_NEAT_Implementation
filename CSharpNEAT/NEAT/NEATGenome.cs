using CSharpNEAT.ActivationFunctions;
using CSharpNEAT.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpNEAT.NEAT
{
    public class NEATGenome : IGenome
    {
        public List<GConnection> Сonnections { get; }
        public List<GNeuron> Neurons { get; }

        public override string ToString()
        {
            var str = "";
            for (var i = 0; i < Neurons.Count; i++)
                str += Neurons[i] + "\n";
            str += "\n";
            for (var i = 0; i < Сonnections.Count; i++)
                str += Сonnections[i] + "\n";

            return str;
        }

        #region Constructors

        public NEATGenome(IList<GConnection> conns, IList<GNeuron> neurons)
        {
            Neurons = new List<GNeuron>(neurons);
            Сonnections = new List<GConnection>(conns);
        }

        #endregion Constructors

        #region IGenome

        public int Complexity
        {
            get
            {
                return Сonnections.Count;
            }
        }

        public float Fitness { get; set; }

        #endregion IGenome
    }
}
