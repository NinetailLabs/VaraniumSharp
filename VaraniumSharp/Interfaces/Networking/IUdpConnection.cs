using System.Threading.Tasks;
using System;

namespace VaraniumSharp.Interfaces.Networking
{
    /// <summary>
    /// Connection to a UDP endpoint
    /// </summary>
    public interface IUdpConnection : IDisposable
    {
        #region Properties

        /// <summary>
        /// Indicate if a connection to a remote endpoint has been established
        /// </summary>
        bool IsConnected { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Connect to the remote endpoint.
        /// This method should be called before any of the other methods on this class are called
        /// </summary>
        /// <param name="server">URL where the server is located</param>
        /// <param name="localPort">Local port to use for the connection</param>
        /// <param name="remotePort">Remote port to send requests to</param>
        /// <returns>Indicates if the connection could be established</returns>
        bool Connect(string server, int localPort, int remotePort);

        /// <summary>
        /// Receive data from the remote endpoint with a default timeout of 10 seconds
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the <see cref="Connect"/> method was not called yet</exception>
        /// <returns>Byte array containing the data that was received</returns>
        byte[] Receive();

        /// <summary>
        /// Receive data from the remote endpoint
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the <see cref="Connect"/> method was not called yet</exception>
        /// <param name="timeout">Timespan to wait before timing out</param>
        /// <returns>Byte array containing the data that was received</returns>
        byte[] Receive(TimeSpan timeout);

        /// <summary>
        /// Receive data from the remote endpoint.
        /// Note that this method does not have a timeout.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the <see cref="Connect"/> method was not called yet</exception>
        /// <returns>Byte array containing the data that was received</returns>
        Task<byte[]> ReceiveAsync();

        /// <summary>
        /// Send data to the remote endpoint
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the <see cref="Connect"/> method was not called yet</exception>
        /// <param name="data">Byte array to send</param>
        /// <returns>Response code received from the remote endpoint</returns>
        Task<int> SendAsync(byte[] data);

        #endregion
    }
}