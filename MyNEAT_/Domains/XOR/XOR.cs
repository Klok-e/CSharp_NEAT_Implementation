using System;
using System.Collections.Generic;

namespace MyNEAT.Domains.XOR
{
    public class Xor
    {
        private readonly int numInp;

        public Xor(int numOfInp)
        {
            if (numOfInp % 2 == 1) throw new Exception("Wrong parameter!");
            numInp = numOfInp;
        }

        public List<double[]> GetNums(Random gen)
        {
            var ans = new List<double[]>();
            var inps = new double[numInp];
            for (var i = 0; i < numInp; i++)
                inps[i] = gen.Next(0, 2);
            ans.Add(inps);

            var expOutps = new double[numInp / 2];
            for (var i = 0; i < numInp / 2; i++)
                expOutps[i] = (int)Math.Round(inps[i]) ^ (int)Math.Round(inps[i + numInp / 2]);
            ans.Add(expOutps);
            return ans;
        }

        public double GetError(double[] networkOutput, double[] expctdOut)
        {
            if (networkOutput.Length != expctdOut.Length) throw new Exception("Wrong array length");

            double error = 0;
            for (var i = 0; i < networkOutput.Length; i++)
                error += Math.Abs(networkOutput[i] - expctdOut[i]);
            return error;
        }
    }
}