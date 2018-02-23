using System;

namespace MyNEAT.ActivationFunctions
{
    public interface IActivationFunction
    {
        float Eval(float x);

        float EvalDerivative(float x);
    }

    public class Tanh : IActivationFunction
    {
        public float Eval(float x)
        {
            return (float)Math.Tanh(x);
        }

        public float EvalDerivative(float x)
        {
            throw new NotImplementedException();
        }
    }

    public class Linear : IActivationFunction
    {
        public float Eval(float x)
        {
            return x;
        }

        public float EvalDerivative(float x)
        {
            return 1;
        }
    }
}
