using System;
using System.Collections.Generic;
using System.Linq;

namespace MyNEAT.Domains.XOR
{
    public class XOR
    {
        int numInp;
        public XOR(int numOfInp)
        {
            if (numOfInp % 2 == 1) throw new Exception("Wrong parameter!");
            numInp = numOfInp;
        }

        public List<int[]> GetNums(Random gen)
        {
            List<int[]> ans = new List<int[]>();
            int[] inps = new int[numInp];
            for (int i = 0; i < numInp; i++)
            {
                inps[i] = gen.Next(0, 2);
            }
            ans.Add(inps);

            int[] expOutps = new int[numInp / 2];
            for (int i = 0; i < numInp / 2; i++)
            {
                expOutps[i] = (inps[i]) ^ (inps[i + numInp / 2]);
            }
            ans.Add(expOutps);
            return ans;
        }

        public int GetError(int[] networkOutput, int[] expctdOut)
        {
            if (networkOutput.Length != expctdOut.Length) throw new Exception("Wrong array length");

            int error = 0;
            for (int i = 0; i < networkOutput.Length; i++)
            {
                error += Math.Abs(networkOutput[i] - expctdOut[i]);
            }
            return error;
        }
    }
}
