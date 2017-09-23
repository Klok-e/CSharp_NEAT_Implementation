using System;

namespace MyNEAT
{
    public static class ActivationFunctions
    {
        public delegate double ActivationFunction(double n);

        public static double Tanh(double n)
        {
            return Math.Tanh(n);
        }

        public static double Linear(double n)
        {
            return n;
        }
    }
}