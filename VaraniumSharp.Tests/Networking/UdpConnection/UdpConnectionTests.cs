using FluentAssertions;
using System.Text;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
// ReSharper disable AccessToDisposedClosure - These won't be reused in the test so it's fine

namespace VaraniumSharp.Tests.Networking.UdpConnection
{
    public class UdpConnectionTests
    {
        [Fact]
        public void AFailureDuringTheUdpConnectionSetsIsConnectedToFalse()
        {
            // arrange
            const int remotePort = -10;
            const int localPort = 4558;

            var sut = new VaraniumSharp.Networking.UdpConnection();

            // act
            var result = sut.Connect("localhost", remotePort, localPort);

            // assert
            result.Should().BeFalse();
            sut.IsConnected.Should().BeFalse();

        }

        [Fact]
        public async Task AttemptingToReceiveDataAsyncWhenNotConnectedThrowsAnInvalidOperationException()
        {
            // arrange
            var sut = new VaraniumSharp.Networking.UdpConnection();
            _ = Encoding.UTF8.GetBytes("Hello World!");
            var act = new Func<Task<byte[]>>(() => sut.ReceiveAsync());

            // act
            // assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public void AttemptingToReceiveDataWhenNotConnectedThrowsAnInvalidOperationException()
        {
            // arrange
            var sut = new VaraniumSharp.Networking.UdpConnection();
            _ = Encoding.UTF8.GetBytes("Hello World!");
            var act = new Action(() => sut.Receive());

            // act
            // assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void AttemptingToSendDataBeforeConnectingThrowsAnInvalidOperationException()
        {
            // arrange
            var sut = new VaraniumSharp.Networking.UdpConnection();
            _ = Encoding.UTF8.GetBytes("Hello World!");
            var dataToSend = Encoding.UTF8.GetBytes("Hello Server");
            var act = new Action(() => sut.SendAsync(dataToSend).Wait());

            // act
            // assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public async Task ReceivingDataAsyncWorksCorrectly()
        {
            // arrange
            const int remotePort = 9658;
            const int localPort = 4558;

            var sut = new VaraniumSharp.Networking.UdpConnection();
            var udpServer = new UdpClient(localPort);
            sut.Connect("localhost", remotePort, localPort);
            byte[]? result = null;
            _ = Task.Run(async () => result = await sut.ReceiveAsync());
            await Task.Delay(1000);
            var data = Encoding.UTF8.GetBytes("Hello World!");

            // act
            await udpServer.SendAsync(data, data.Length, "localhost", remotePort);
            await Task.Delay(1000);

            // assert
            result.Should().NotBeNull();
            Encoding.UTF8.GetString(result!).Should().Be("Hello World!");

            udpServer.Dispose();
            sut.Dispose();
        }

        [Fact]
        public async Task ReceivingDataWorksCorrectly()
        {
            // arrange
            const int remotePort = 9658;
            const int localPort = 4558;

            var sut = new VaraniumSharp.Networking.UdpConnection();
            var udpServer = new UdpClient(localPort);
            sut.Connect("localhost", remotePort, localPort);
            byte[]? result = null;
            _ = Task.Run(() => result = sut.Receive());
            await Task.Delay(1000);
            var data = Encoding.UTF8.GetBytes("Hello World!");

            // act
            await udpServer.SendAsync(data, data.Length, "localhost", remotePort);
            await Task.Delay(1000);

            // assert
            result.Should().NotBeNull();
            Encoding.UTF8.GetString(result!).Should().Be("Hello World!");

            udpServer.Dispose();
            sut.Dispose();
        }

        [Fact]
        public async Task SendingDataReturnsZeroOnError()
        {
            // arrange
            const int remotePort = 9658;
            const int localPort = 4558;

            var sut = new VaraniumSharp.Networking.UdpConnection();
            sut.Connect("localhost", remotePort, localPort);
            await Task.Delay(1000);
            _ = Encoding.UTF8.GetBytes("Hello World!");
            var dataToSend = Encoding.UTF8.GetBytes("Hello Server");
            sut.Dispose();

            // act
            var response = await sut.SendAsync(dataToSend);
            Thread.Sleep(1000);

            // assert
            response.Should().Be(0);
        }

        [Fact]
        public async Task SendingDatWorksCorrectly()
        {
            // arrange
            const int remotePort = 9658;
            const int localPort = 4558;

            var sut = new VaraniumSharp.Networking.UdpConnection();
            var udpServer = new UdpClient(localPort);
            var remoteEndpoint = new IPEndPoint(IPAddress.Any, remotePort);
            sut.Connect("localhost", remotePort, localPort);
            _ = Task.Run(() => sut.Receive());
            await Task.Delay(1000);
            _ = Encoding.UTF8.GetBytes("Hello World!");
            byte[]? receiveResult = null;
            _ = Task.Run(() => receiveResult = udpServer.Receive(ref remoteEndpoint));
            await Task.Delay(1000);
            var dataToSend = Encoding.UTF8.GetBytes("Hello Server");

            // act
            sut.SendAsync(dataToSend).Wait();
            await Task.Delay(1000);

            // assert
            receiveResult.Should().NotBeNull();
            Encoding.UTF8.GetString(receiveResult!).Should().Be("Hello Server");

            udpServer.Dispose();
            sut.Dispose();
        }

        [Fact]
        public void TimeOutWorksCorrectly()
        {
            // arrange
            const int remotePort = 9658;
            const int localPort = 4558;

            var sut = new VaraniumSharp.Networking.UdpConnection();
            sut.Connect("localhost", remotePort, localPort);

            var act = new Action(() => sut.Receive());

            // act
            // assert
            act.Should().Throw<SocketException>();

            sut.Dispose();
        }
    }
}