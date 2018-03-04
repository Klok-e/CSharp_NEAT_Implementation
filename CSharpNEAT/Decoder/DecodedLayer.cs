namespace MyNEAT.Decoder
{
    internal class DecodedLayer
    {
        private DConnection[] _connections;

        public DecodedLayer(DConnection[] connectionsToNext)
        {
            _connections = connectionsToNext;
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
