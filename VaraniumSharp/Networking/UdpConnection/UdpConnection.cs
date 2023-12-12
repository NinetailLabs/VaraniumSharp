#if NETSTANDARD2_1_OR_GREATER
#nullable enable
#endif

using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using VaraniumSharp.Interfaces.Networking;
using VaraniumSharp.Logging;

namespace VaraniumSharp.Networking
{
    /// <inheritdoc />
    public class UdpConnection : IUdpConnection
    {
        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public UdpConnection()
        {
            _logger = StaticLogger.GetLogger<UdpConnection>();
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool IsConnected { get; set; }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public bool Connect(string server, int localPort, int remotePort)
        {
            try
            {
                if (_client != null)
                {
                    _logger.LogWarning("Connection already established but a new connection has been requested. Old connection will be terminated before establishing the new connection.");
                    _client.Dispose();
                }

                _client = new UdpClient(localPort);
                _localEndPoint = new IPEndPoint(IPAddress.Any, localPort);
                _client.Connect(server, remotePort);

                IsConnected = true;
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error occurred while trying to connect to {ServerUrl} on {RemotePort}", server, remotePort);
                IsConnected = false;
                return false;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _client?.Dispose();
        }

        /// <inheritdoc />
        public byte[] Receive()
        {
            return Receive(TimeSpan.FromSeconds(10));
        }

        /// <inheritdoc />
        public byte[] Receive(TimeSpan timeout)
        {
            if (_client == null)
            {
                throw new InvalidOperationException("Not connected to a remote endpoint. Call the Connect method before trying to received data");
            }

            byte[] receivedData;
            try
            {
                _client.Client.ReceiveTimeout = (int)timeout.TotalMilliseconds;
                receivedData = _client.Receive(ref _localEndPoint);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error occurred while receiving data from the remote endpoint");
                throw;
            }

            return receivedData;
        }

        /// <inheritdoc />
        public async Task<byte[]> ReceiveAsync()
        {
            if (_client == null)
            {
                throw new InvalidOperationException("Not connected to a remote endpoint. Call the Connect method before trying to received data");
            }

            UdpReceiveResult receivedData;
            try
            {
                receivedData = await _client
                    .ReceiveAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error occurred while receiving data from the remote endpoint");
                throw;
            }

            return receivedData.Buffer;
        }

        /// <inheritdoc />
        public async Task<int> SendAsync(byte[] data)
        {
            if (_client == null)
            {
                throw new InvalidOperationException("Not connected to a remote endpoint. Call the Connect method before trying to send data");
            }

            var retData = 0;
            try
            {
                retData = await _client
                    .SendAsync(data, data.Length)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error occurred while sending data to remote endpoint.");
            }

            return retData;
        }

        #endregion

        #region Variables

        /// <summary>
        /// Logger instance
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// UdpClient instance
        /// </summary>
#if NETSTANDARD2_1_OR_GREATER
        private UdpClient? _client;
#else
        private UdpClient _client;
#endif

        /// <summary>
        /// IPEndPoint instance
        /// </summary>
#if NETSTANDARD2_1_OR_GREATER
        private IPEndPoint? _localEndPoint;
#else
        private IPEndPoint _localEndPoint;
#endif

#endregion
    }
}