namespace CSharpNEAT.NEAT.Decoder
{
    internal class DecodedLayer
    {
        public uint Depth { get; }
        private DConnection[] _connections;

        public DecodedLayer(DConnection[] connectionsToNext, uint depth)
        {
            _connections = connectionsToNext;
            Depth = depth;
        }

        public void Activate(DNeuron[] allNeurons)
        {
            for (int i = 0; i < _connections.Length; i++)
            {
                var from = allNeurons[_connections[i].From];
                allNeurons[_connections[i].To]._sum += from.Activation.Eval(from._sum) * _connections[i].Weight;
            }
        }
    }
}
