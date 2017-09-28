using System;
using System.Collections.Generic;

namespace MyNEAT.Domains.XOR
{
    public class Xor
    {
        private readonly int numInp;
        int count = 0;

        public Xor(int numOfInp)
        {
            if (numOfInp % 2 == 1) throw new Exception("Wrong parameter!");
            numInp = numOfInp;
        }

        public List<double[]> GetNums()
        {
            var ans = new List<double[]>();

            if (count == 4) count = 0;
            switch (count)
            {
                case 0:
                    ans.Add(new double[] { 0, 0 });
                    ans.Add(new double[] { 0 });
                    break;
                case 1:
                    ans.Add(new double[] { 1, 0 });
                    ans.Add(new double[] { 1 });
                    break;
                case 2:
                    ans.Add(new double[] { 0, 1 });
                    ans.Add(new double[] { 1 });
                    break;
                case 3:
                    ans.Add(new double[] { 1, 1 });
                    ans.Add(new double[] { 0 });
                    break;
            }
            count++;
            /*
            var inps = new double[numInp];
            for (var i = 0; i < numInp; i++)
                inps[i] = gen.Next(0, 2);
            ans.Add(inps);

            var expOutps = new double[numInp / 2];
            for (var i = 0; i < numInp / 2; i++)
                expOutps[i] = (int)Math.Round(inps[i]) ^ (int)Math.Round(inps[i + numInp / 2]);
            ans.Add(expOutps);*/
            return ans;
        }

        public double GetError(double[] networkOutput, double[] expctdOut)
        {
            if (networkOutput.Length != expctdOut.Length) throw new Exception("Wrong array length");

            double error = 0;
            for (var i = 0; i < networkOutput.Length; i++)
                error += Math.Abs(networkOutput[i] - expctdOut[i]);
            return -error;
        }
    }
}