using System.Collections.Generic;
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
    public class ChangeSceneMessageHandlerTests
    {
        private readonly IGameObject mockPlayer;
        private readonly IProximityChecker mockProximityChecker;
        private readonly IMessageSender mockMessageSender;
        private readonly IPresenceSceneProvider mockPresenceSceneProvider;
        private readonly IGameSceneCollection mockGameSceneCollection;
        private readonly ChangeSceneMessageHandler handler;

        public ChangeSceneMessageHandlerTests()
        {
            mockPlayer = MockGameObject.CreatePlayer();
            mockProximityChecker = mockPlayer.Components.Get<IProximityChecker>();
            mockMessageSender = mockPlayer.Components.Get<IMessageSender>();
            mockPresenceSceneProvider = mockPlayer.Components.Get<IPresenceSceneProvider>();
            mockGameSceneCollection = Substitute.For<IGameSceneCollection>();

            handler = new ChangeSceneMessageHandler(mockPlayer, mockGameSceneCollection);
        }

        [Fact]
        public void Handle_ShouldChangeScene_WhenPortalIsNearbyAndSceneExists()
        {
            // Arrange
            var portalId = 123;
            var destinationMap = Map.TheDarkForest;
            var message = new ChangeSceneMessage { PortalId = portalId };

            var mockPortal = CreateMockPortal(portalId, destinationMap);
            var mockGameScene = Substitute.For<IGameScene>();

            mockProximityChecker.GetNearbyGameObjects().Returns(new[] { mockPortal });
            mockGameSceneCollection.TryGet("thedarkforest", out Arg.Any<IGameScene>())
                .Returns(x =>
                {
                    x[1] = mockGameScene;
                    return true;
                });

            // Act
            handler.Handle(message);

            // Assert
            mockPresenceSceneProvider.Received(1).SetScene(mockGameScene);
        }

        [Fact]
        public void Handle_ShouldSendSceneChangedMessage_WhenSceneChanges()
        {
            // Arrange
            var portalId = 123;
            var destinationMap = Map.TheDarkForest;
            var message = new ChangeSceneMessage { PortalId = portalId };

            var mockPortal = CreateMockPortal(portalId, destinationMap);
            var mockGameScene = Substitute.For<IGameScene>();

            mockProximityChecker.GetNearbyGameObjects().Returns(new[] { mockPortal });
            mockGameSceneCollection.TryGet("thedarkforest", out Arg.Any<IGameScene>())
                .Returns(x =>
                {
                    x[1] = mockGameScene;
                    return true;
                });

            // Act
            handler.Handle(message);

            // Assert
            mockMessageSender.Received(1).SendMessage(
                (byte)MessageCodes.SceneChanged,
                Arg.Is<SceneChangedMessage>(m => m.Map == (byte)destinationMap));
        }

        [Fact]
        public void Handle_ShouldNotChangeScene_WhenPortalNotFound()
        {
            // Arrange
            var portalId = 999;
            var message = new ChangeSceneMessage { PortalId = portalId };

            // Different portal ID
            var mockPortal = CreateMockPortal(123, Map.Lobby);
            mockProximityChecker.GetNearbyGameObjects().Returns(new[] { mockPortal });

            // Act
            handler.Handle(message);

            // Assert
            mockPresenceSceneProvider.DidNotReceive().SetScene(Arg.Any<IGameScene>());
            mockMessageSender.DidNotReceive().SendMessage(Arg.Any<byte>(), Arg.Any<SceneChangedMessage>());
        }

        [Fact]
        public void Handle_ShouldNotChangeScene_WhenNoNearbyObjects()
        {
            // Arrange
            var message = new ChangeSceneMessage { PortalId = 123 };
            mockProximityChecker.GetNearbyGameObjects().Returns(new List<IGameObject>());

            // Act
            handler.Handle(message);

            // Assert
            mockPresenceSceneProvider.DidNotReceive().SetScene(Arg.Any<IGameScene>());
        }

        [Fact]
        public void Handle_ShouldNotChangeScene_WhenDestinationSceneNotFound()
        {
            // Arrange
            var portalId = 123;
            var message = new ChangeSceneMessage { PortalId = portalId };

            var mockPortal = CreateMockPortal(portalId, Map.TheDarkForest);
            mockProximityChecker.GetNearbyGameObjects().Returns(new[] { mockPortal });
            mockGameSceneCollection.TryGet(Arg.Any<string>(), out Arg.Any<IGameScene>())
                .Returns(false);

            // Act
            handler.Handle(message);

            // Assert
            mockPresenceSceneProvider.DidNotReceive().SetScene(Arg.Any<IGameScene>());
        }

        [Fact]
        public void Handle_ShouldHandleEmptyNearbyObjects_Gracefully()
        {
            // Arrange
            var message = new ChangeSceneMessage { PortalId = 123 };
            mockProximityChecker.GetNearbyGameObjects().Returns(new List<IGameObject>());

            // Act & Assert - Should not throw
            Should.NotThrow(() => handler.Handle(message));
        }

        [Fact]
        public void Handle_ShouldFindCorrectPortal_WhenMultipleObjectsNearby()
        {
            // Arrange
            var targetPortalId = 123;
            var destinationMap = Map.TheDarkForest;
            var message = new ChangeSceneMessage { PortalId = targetPortalId };

            var mockMob = MockGameObject.CreateMob(456);
            var mockPortal = CreateMockPortal(targetPortalId, destinationMap);
            var mockOtherPortal = CreateMockPortal(789, Map.Lobby);
            var mockGameScene = Substitute.For<IGameScene>();

            mockProximityChecker.GetNearbyGameObjects().Returns(new[] { mockMob, mockPortal, mockOtherPortal });
            mockGameSceneCollection.TryGet("thedarkforest", out Arg.Any<IGameScene>())
                .Returns(x =>
                {
                    x[1] = mockGameScene;
                    return true;
                });

            // Act
            handler.Handle(message);

            // Assert
            mockPresenceSceneProvider.Received(1).SetScene(mockGameScene);
        }

        private IGameObject CreateMockPortal(int id, Map destinationMap)
        {
            var portal = Substitute.For<IGameObject>();
            var components = Substitute.For<IComponents>();
            var teleportationData = Substitute.For<IPortalTeleportationData>();

            portal.Id.Returns(id);
            portal.Components.Returns(components);
            teleportationData.GetDestinationMap().Returns((byte)destinationMap);
            components.Get<IPortalTeleportationData>().Returns(teleportationData);

            return portal;
        }
    }
}
