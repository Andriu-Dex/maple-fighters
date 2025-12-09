using Game.Application.Components;
using Game.Application.Objects;
using Game.Application.Objects.Components;
using InterestManagement;
using NSubstitute;

namespace Game.UnitTests.Mocks
{
    /// <summary>
    /// Factory class for creating mock game objects for testing purposes.
    /// </summary>
    public static class MockGameObject
    {
        /// <summary>
        /// Creates a basic mock IGameObject.
        /// </summary>
        public static IGameObject Create(int id = 1, string name = "TestObject")
        {
            var gameObject = Substitute.For<IGameObject>();
            var components = Substitute.For<IComponents>();
            var transform = Substitute.For<ITransform>();

            gameObject.Id.Returns(id);
            gameObject.Name.Returns(name);
            gameObject.Components.Returns(components);
            gameObject.Transform.Returns(transform);

            return gameObject;
        }

        /// <summary>
        /// Creates a mock player with ProximityChecker and MessageSender components.
        /// </summary>
        public static IGameObject CreatePlayer(int id = 1, string name = "Player")
        {
            var player = Create(id, name);
            var proximityChecker = Substitute.For<IProximityChecker>();
            var messageSender = Substitute.For<IMessageSender>();
            var presenceSceneProvider = Substitute.For<IPresenceSceneProvider>();

            player.Components.Get<IProximityChecker>().Returns(proximityChecker);
            player.Components.Get<IMessageSender>().Returns(messageSender);
            player.Components.Get<IPresenceSceneProvider>().Returns(presenceSceneProvider);

            return player;
        }

        /// <summary>
        /// Creates a mock mob with behaviour manager and health controller.
        /// </summary>
        public static IGameObject CreateMob(int id, int health = 100)
        {
            var mob = Create(id, $"Mob_{id}");
            var behaviourManager = Substitute.For<IMobBehaviourManager>();
            var healthController = Substitute.For<IMobHealthController>();

            mob.Components.Get<IMobBehaviourManager>().Returns(behaviourManager);
            mob.Components.Get<IMobHealthController>().Returns(healthController);

            return mob;
        }

        /// <summary>
        /// Creates a mock portal with teleportation data.
        /// </summary>
        public static IGameObject CreatePortal(int id, byte destinationMap)
        {
            var portal = Create(id, $"Portal_{id}");
            var teleportationData = Substitute.For<IPortalTeleportationData>();

            teleportationData.GetDestinationMap().Returns(destinationMap);
            portal.Components.Get<IPortalTeleportationData>().Returns(teleportationData);

            return portal;
        }
    }
}
