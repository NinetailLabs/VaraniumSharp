using VaraniumSharp.Attributes;
using VaraniumSharp.Enumerations;
using VaraniumSharp.Interfaces.Networking;

namespace VaraniumSharp.Networking
{
    /// <inheritdoc />
    [AutomaticContainerRegistration(typeof(IUdpConnectionFactory), ServiceReuse.Singleton)]
    public class UdpConnectionFactory : IUdpConnectionFactory
    {
        #region Public Methods

        /// <inheritdoc />
        public IUdpConnection Create()
        {
            return new UdpConnection();
        }

        /// <inheritdoc />
        public (IUdpConnection connection, bool isConnected) CreateAndConnect(string server, int localPort, int remotePort)
        {
            var connection = new UdpConnection();
            var connected = connection.Connect(server, localPort, remotePort);
            return (connection,  connected);
        }

        #endregion
    }
}