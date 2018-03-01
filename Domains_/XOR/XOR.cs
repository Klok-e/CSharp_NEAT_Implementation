using System;
using System.Collections.Generic;

namespace MyNEAT.Domains.XOR
{
    public class Xor
    {
        public Xor()
        {
        }

        public (float[] x, float y) GetNums(int ind)
        {
            var x = new float[2];
            var y = new float();

            switch (ind)
            {
                case 0:
                    x = new float[] { 0f, 0f };
                    y = 0f;
                    break;

                case 1:
                    x = (new float[] { 1f, 0f });
                    y = 1f;
                    break;

                case 2:
                    x = (new float[] { 0f, 1f });
                    y = 1f;
                    break;

                case 3:
                    x = (new float[] { 1f, 1f });
                    y = 0f;
                    break;
            }
            return (x, y);
        }

        public float GetError(float networkOutput, float expctdOut)
        {
            return Math.Abs(networkOutput - expctdOut);
        }
    }
}
