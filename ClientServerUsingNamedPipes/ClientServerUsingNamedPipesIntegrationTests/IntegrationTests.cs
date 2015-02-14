using System.Threading;
using ClientServerUsingNamedPipes.Client;
using ClientServerUsingNamedPipes.Server;
using NUnit.Framework;

namespace ClientServerUsingNamedPipesIntegrationTests
{
    public class IntegrationTests
    {
        private PipeServer _server;
        private PipeClient _client;

        [TearDown]
        public void TearDown()
        {
            if (_client != null)
            {
                _client.Stop();
                _client = null;
            }

            if (_server != null)
            {
                _server.Stop();
                _server = null;
            }
        }

        [Test]
        public void Instantiate_PipeServer_ShouldHaveValidServerId()
        {
            // Act
            _server = new PipeServer();

            // Verify
            Assert.IsNotNull(_server.ServerId);
            Assert.IsNotEmpty(_server.ServerId);
        }

        [Test]
        public void Client_Connect_ServerShouldFireClientConnectedEvent()
        {
            // Prepare
            var isConnected = false;
            _server = new PipeServer();

            _server.ClientConnectedEvent += (sender, args) =>
            {
                isConnected = true;
            };

            _server.Start();
            Assert.IsFalse(isConnected);

            // Act
            _client = new PipeClient(_server.ServerId);
            _client.Start();
            Thread.Sleep(100);

            // Verify
            Assert.IsTrue(isConnected);
        }

        [Test]
        public void Client_Disconnect_ServerShouldFireClientDisconnectedEvent()
        {
            // Prepare
            var isDisconnected = false;
            _server = new PipeServer();
            
            _server.ClientDisconnectedEvent += (sender, args) =>
            {
                isDisconnected = true;
            };

            _server.Start();
            Assert.IsFalse(isDisconnected);

            _client = new PipeClient(_server.ServerId);
            _client.Start();

            // Act
            _client.Stop();
            _client = null;
            Thread.Sleep(100);

            // Verify
            Assert.IsTrue(isDisconnected);
        }

        [Test]
        public void Client_SendMessage_ServerShouldFireMessageReceivedEvent()
        {
            // Prepare
            _server = new PipeServer();
            _client = new PipeClient(_server.ServerId);

            string message = null;
            var autoEvent = new AutoResetEvent(false);

            _server.MessageReceivedEvent += (sender, args) =>
            {
                message = args.Message;
                autoEvent.Set();
            };

            _server.Start();
            _client.Start();

            // Act
            _client.SendMessage("Client's message");

            // Verify
            autoEvent.WaitOne();

            Assert.AreEqual("Client's message", message);
        }
    }
}
