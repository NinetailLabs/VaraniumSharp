using FluentAssertions;
using System.Net.Sockets;
using VaraniumSharp.Networking;
using Xunit;

namespace VaraniumSharp.Tests.Networking.UdpConnection
{
    public class UdpConnectionFactoryTests
    {
        [Fact]
        public void CreatingAndConnectingUdpConnectionReturnsAConnectedInstance()
        {
            // arrange
            const int remotePort = 9658;
            const int localPort = 4558;

            var sut = new UdpConnectionFactory();
            var udpServer = new UdpClient(remotePort);

            // act
            var result = sut.CreateAndConnect("localhost", localPort, remotePort);

            // assert
            result.isConnected.Should().BeTrue();
            udpServer.Dispose();
            result.connection.Dispose();
        }

        [Fact]
        public void CreatingNewUdpConnectionReturnsANewConnection()
        {
            // arrange
            var sut = new UdpConnectionFactory();

            // act
            var result = sut.Create();

            // assert
            result.IsConnected.Should().BeFalse();
        }
    }
}