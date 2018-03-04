using MyNEAT.Genome;

namespace MyNEAT.Decoder
{
    internal class NEATNetwork : IBlackBox
    {
        #region IBlackBox

        public float[] Inputs { get; }

        public float[] Outputs { get; }

        public void Activate()
        {
            for (int i = 0; i < Inputs.Length; i++)
            {
                Neurons[_inputIndices[i]]._sum = Inputs[i];
            }

            //do activation stuff
            foreach (var layer in Layers)
            {
                layer.Activate(Neurons);
            }

            for (int i = 0; i < Outputs.Length; i++)
            {
                Outputs[i] = Neurons[_outputIndices[i]].Activation.Eval(Neurons[_outputIndices[i]]._sum);
            }
        }

        public void Reset()
        {
            for (int i = 0; i < Neurons.Length; i++)
            {
                if (Neurons[i].Type != NeuronType.bias)
                    Neurons[i]._sum = 0f;
            }
        }

        #endregion IBlackBox

        private DecodedLayer[] Layers { get; }
        private DNeuron[] Neurons { get; }

        private int[] _inputIndices;
        private int[] _outputIndices;

        public NEATNetwork(DecodedLayer[] layers, DNeuron[] neurons, int[] inputIndices, int[] outputIndices)
        {
            Layers = layers;
            Neurons = neurons;

            Inputs = new float[inputIndices.Length];
            Outputs = new float[outputIndices.Length];

            _inputIndices = inputIndices;
            _outputIndices = outputIndices;
        }
    }
}
