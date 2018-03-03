using System;
using MyNEAT.ActivationFunctions;

namespace MyNEAT.Genome.NEAT
{
    public enum NeuronType : byte
    {
        input = 0,
        bias = 1,
        hidden = 2,
        output = 3
    }

    public interface IGNode
    {
        ulong Id { get; }
    }

    public class GNeuron : IGNode
    {
        public IActivationFunction Activation { get; }
        public NeuronType Type { get; }
        public ulong Id { get; }

        public GNeuron(ulong id, NeuronType type, IActivationFunction activation)
        {
            Type = type;
            Id = id;
            Activation = activation;
        }

        public override string ToString()
        {
            return $"Id: {Id}, Type: {Type}";
        }
    }

    public class GConnection : IGNode
    {
        public ulong FromNeuron { get; }
        public ulong ToNeuron { get; }
        public float Weight { get; }
        public ulong Id { get; }

        public GConnection(ulong fromneuron, ulong toneuron, float wei, ulong idForThis)
        {
            FromNeuron = fromneuron;
            ToNeuron = toneuron;

            Id = idForThis;

            Weight = wei;
        }

        public override string ToString()
        {
            return $"This id: {Id}, This weight: {Math.Round(Weight, 2)}, ||| From: {FromNeuron}, To: {ToNeuron}";
        }
    }
}
