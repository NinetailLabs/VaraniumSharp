namespace VaraniumSharp.Interfaces.Networking
{
    /// <summary>
    /// Factory for creating new <see cref="IUdpConnection"/>s
    /// </summary>
    public interface IUdpConnectionFactory
    {
        #region Public Methods

        /// <summary>
        /// Creates a new <see cref="IUdpConnection"/> instance
        /// </summary>
        /// <returns>UdpConnection instance</returns>
        IUdpConnection Create();

        /// <summary>
        /// Create a new <see cref="IUdpConnection"/> and attempt to connect to a remote endpoint
        /// </summary>
        /// <param name="server">URL where the server is located</param>
        /// <param name="localPort">Local port to use for the connection</param>
        /// <param name="remotePort">Remote port to send requests to</param>
        /// <returns>The created connection and an indicator if the connection to the remote endpoint could be established or not</returns>
        (IUdpConnection connection, bool isConnected) CreateAndConnect(string server, int localPort, int remotePort);

        #endregion
    }
}