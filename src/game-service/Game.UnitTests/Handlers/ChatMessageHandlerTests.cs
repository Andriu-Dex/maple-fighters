using System.Collections.Generic;
using Game.Application;
using Game.Application.Components;
using Game.Application.Handlers;
using Game.Application.Objects;
using Game.Application.Objects.Components;
using Game.Messages;
using Game.UnitTests.Mocks;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Game.UnitTests.Handlers
{
    public class ChatMessageHandlerTests
    {
        private readonly IGameObject mockPlayer;
        private readonly IMessageSender mockMessageSender;
        private readonly IGameClientCollection mockGameClientCollection;
        private readonly ChatMessageHandler handler;

        public ChatMessageHandlerTests()
        {
            mockPlayer = MockGameObject.CreatePlayer();
            mockMessageSender = mockPlayer.Components.Get<IMessageSender>();
            mockGameClientCollection = Substitute.For<IGameClientCollection>();

            handler = new ChatMessageHandler(mockPlayer, mockGameClientCollection);
        }

        [Fact]
        public void Handle_ShouldSendBubbleNotification_ToSender()
        {
            // Arrange
            var senderId = 1;
            var name = "TestPlayer";
            var content = "Hello, World!";
            var message = new ChatMessage
            {
                SenderId = senderId,
                Name = name,
                Content = content
            };

            mockGameClientCollection.GetEnumerator()
                .Returns(new List<IGameClient>().GetEnumerator());

            // Act
            handler.Handle(message);

            // Assert
            mockMessageSender.Received(1).SendMessage(
                (byte)MessageCodes.BubbleNotification,
                Arg.Is<BubbleNotificationMessage>(m =>
                    m.NotifierId == senderId &&
                    m.Message == content));
        }

        [Fact]
        public void Handle_ShouldSendBubbleNotification_ToNearbyPlayers()
        {
            // Arrange
            var senderId = 1;
            var content = "Hello!";
            var message = new ChatMessage
            {
                SenderId = senderId,
                Name = "Player",
                Content = content
            };

            mockGameClientCollection.GetEnumerator()
                .Returns(new List<IGameClient>().GetEnumerator());

            // Act
            handler.Handle(message);

            // Assert
            mockMessageSender.Received(1).SendMessageToNearbyGameObjects(
                (byte)MessageCodes.BubbleNotification,
                Arg.Any<BubbleNotificationMessage>());
        }

        [Fact]
        public void Handle_ShouldBroadcastChatMessage_ToAllClients()
        {
            // Arrange
            var senderId = 1;
            var name = "TestPlayer";
            var content = "Hello, everyone!";
            var message = new ChatMessage
            {
                SenderId = senderId,
                Name = name,
                Content = content
            };

            var mockClient1 = CreateMockGameClient();
            var mockClient2 = CreateMockGameClient();
            var clients = new List<IGameClient> { mockClient1, mockClient2 };

            mockGameClientCollection.GetEnumerator().Returns(clients.GetEnumerator());

            // Act
            handler.Handle(message);

            // Assert - Each client should receive the chat message
            var connectionProvider1 = mockClient1.Components.Get<IWebSocketConnectionProvider>();
            var connectionProvider2 = mockClient2.Components.Get<IWebSocketConnectionProvider>();

            connectionProvider1.Received(1).SendMessage(
                (byte)MessageCodes.ChatMessage,
                Arg.Is<ChatMessage>(m =>
                    m.SenderId == senderId &&
                    m.Name == name &&
                    m.Content == content));

            connectionProvider2.Received(1).SendMessage(
                (byte)MessageCodes.ChatMessage,
                Arg.Is<ChatMessage>(m =>
                    m.SenderId == senderId &&
                    m.Name == name &&
                    m.Content == content));
        }

        [Fact]
        public void Handle_ShouldSetBubbleNotificationTime_To3Seconds()
        {
            // Arrange
            var message = new ChatMessage
            {
                SenderId = 1,
                Name = "Player",
                Content = "Test"
            };

            mockGameClientCollection.GetEnumerator()
                .Returns(new List<IGameClient>().GetEnumerator());

            // Act
            handler.Handle(message);

            // Assert
            mockMessageSender.Received(1).SendMessage(
                (byte)MessageCodes.BubbleNotification,
                Arg.Is<BubbleNotificationMessage>(m => m.Time == 3));
        }

        [Fact]
        public void Handle_ShouldHandleEmptyClientCollection_Gracefully()
        {
            // Arrange
            var message = new ChatMessage
            {
                SenderId = 1,
                Name = "Player",
                Content = "Hello"
            };

            mockGameClientCollection.GetEnumerator()
                .Returns(new List<IGameClient>().GetEnumerator());

            // Act & Assert - Should not throw
            Should.NotThrow(() => handler.Handle(message));
        }

        [Fact]
        public void Handle_ShouldIncludeFormattedContent_InChatMessage()
        {
            // Arrange
            var name = "TestPlayer";
            var content = "Hello!";
            var message = new ChatMessage
            {
                SenderId = 1,
                Name = name,
                Content = content
            };

            var mockClient = CreateMockGameClient();
            mockGameClientCollection.GetEnumerator()
                .Returns(new List<IGameClient> { mockClient }.GetEnumerator());

            // Act
            handler.Handle(message);

            // Assert - ContentFormatted should be set
            mockClient.Components.Get<IWebSocketConnectionProvider>()
                .Received(1).SendMessage(
                    (byte)MessageCodes.ChatMessage,
                    Arg.Is<ChatMessage>(m => !string.IsNullOrEmpty(m.ContentFormatted)));
        }

        [Theory]
        [InlineData("")]
        [InlineData("Hello")]
        [InlineData("This is a longer message with multiple words")]
        [InlineData("Special characters: !@#$%^&*()")]
        public void Handle_ShouldProcessVariousMessageContents(string content)
        {
            // Arrange
            var message = new ChatMessage
            {
                SenderId = 1,
                Name = "Player",
                Content = content
            };

            mockGameClientCollection.GetEnumerator()
                .Returns(new List<IGameClient>().GetEnumerator());

            // Act & Assert - Should not throw
            Should.NotThrow(() => handler.Handle(message));
        }

        private IGameClient CreateMockGameClient()
        {
            var gameClient = Substitute.For<IGameClient>();
            var components = Substitute.For<IComponents>();
            var connectionProvider = Substitute.For<IWebSocketConnectionProvider>();

            gameClient.Components.Returns(components);
            components.Get<IWebSocketConnectionProvider>().Returns(connectionProvider);

            return gameClient;
        }
    }
}
