using System.Collections.Generic;

namespace CSharpNEAT.NEAT
{
    public class MutationBuffer
    {
        private Dictionary<ConnectionKey, GConnection> _connectionCreatedBuffer;
        private Dictionary<ConnectionKey, (GConnection, GNeuron, GConnection)> _neuronCreatedBuffer;

        public MutationBuffer()
        {
            _connectionCreatedBuffer = new Dictionary<ConnectionKey, GConnection>();
            _neuronCreatedBuffer = new Dictionary<ConnectionKey, (GConnection, GNeuron, GConnection)>();
        }

        public void Clear()
        {
            _connectionCreatedBuffer.Clear();
            _neuronCreatedBuffer.Clear();
        }

        /// <summary>
        /// Add connection
        /// </summary>
        /// <param name="conn"></param>
        public void AddToBuffer(GConnection conn)
        {
            var key = new ConnectionKey(conn.FromNeuron, conn.ToNeuron);
            _connectionCreatedBuffer.Add(key, conn);
        }

        /// <summary>
        /// Add neuron
        /// </summary>
        /// <param name="conn1"></param>
        /// <param name="neuron"></param>
        /// <param name="conn2"></param>
        public void AddToBuffer(GConnection conn1, GNeuron neuron, GConnection conn2)
        {
            var key = new ConnectionKey(conn1.FromNeuron, conn2.ToNeuron);
            _neuronCreatedBuffer.Add(key, (conn1, neuron, conn2));
        }

        /// <summary>
        /// Connection id
        /// </summary>
        /// <param name="id1"></param>
        /// <param name="id2"></param>
        /// <param name="existingIdConn"></param>
        /// <returns></returns>
        public bool IsInsideBuffer(ulong id1, ulong id2, out ulong existingIdConn)
        {
            existingIdConn = 0;
            var conn = new ConnectionKey(id1, id2);

            if (_connectionCreatedBuffer.ContainsKey(conn))
            {
                existingIdConn = _connectionCreatedBuffer[conn].Id;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Neuron and connections ids
        /// </summary>
        /// <param name="id1"></param>
        /// <param name="id2"></param>
        /// <param name="existingIdConn1"></param>
        /// <param name="existingIdNeuron"></param>
        /// <param name="existingIdConn2"></param>
        /// <returns></returns>
        public bool IsInsideBuffer(ulong id1, ulong id2, out ulong existingIdConn1, out ulong existingIdNeuron, out ulong existingIdConn2)
        {
            existingIdConn1 = 0;
            existingIdNeuron = 0;
            existingIdConn2 = 0;
            var conn = new ConnectionKey(id1, id2);

            if (_neuronCreatedBuffer.ContainsKey(conn))
            {
                var val = _neuronCreatedBuffer[conn];
                existingIdConn1 = val.Item1.Id;
                existingIdNeuron = val.Item2.Id;
                existingIdConn2 = val.Item3.Id;
                return true;
            }
            return false;
        }

        private class ConnectionKey
        {
            public readonly ulong _id1;
            public readonly ulong _id2;

            public ConnectionKey(ulong id1, ulong id2)
            {
                _id1 = id1;
                _id2 = id2;
            }

            public override bool Equals(object obj)
            {
                var key = obj as ConnectionKey;
                return key != null &&
                       _id1 == key._id1 &&
                       _id2 == key._id2;
            }

            public override int GetHashCode()
            {
                var hashCode = 1465752489;
                hashCode = hashCode * -1521134295 + _id1.GetHashCode();
                hashCode = hashCode * -1521134295 + _id2.GetHashCode();
                return hashCode;
            }
        }
    }
}
