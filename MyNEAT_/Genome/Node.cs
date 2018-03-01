using System;

namespace MyNEAT.Genome
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
        ulong Id { get; set; }
    }

    public class GNeuron : IGNode
    {
        public NeuronType Type { get; }
        public ulong Id { get; set; }

        public GNeuron(ulong id, NeuronType type)
        {
            Type = type;
            Id = id;
        }

        public override string ToString()
        {
            var str = "This id: ";
            str += Id;
            //str += ", " + "Is input: " + IsInput + ", " + "Is bias: " + IsBias + ", " + "Is Output: " + IsOutput;

            return str;
        }
    }

    public class GConnection : IGNode
    {
        public ulong FromNeuron { get; }
        public ulong ToNeuron { get; }
        public float Weight { get; }
        public ulong Id { get; set; }

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
