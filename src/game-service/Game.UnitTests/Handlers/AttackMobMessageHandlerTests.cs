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
    public class AttackMobMessageHandlerTests
    {
        private readonly IGameObject mockPlayer;
        private readonly IProximityChecker mockProximityChecker;
        private readonly AttackMobMessageHandler handler;

        public AttackMobMessageHandlerTests()
        {
            mockPlayer = MockGameObject.CreatePlayer();
            mockProximityChecker = mockPlayer.Components.Get<IProximityChecker>();
            handler = new AttackMobMessageHandler(mockPlayer);
        }

        [Fact]
        public void Handle_ShouldDamageMob_WhenMobIsNearby()
        {
            // Arrange
            var mobId = 123;
            var damageAmount = 50;
            var message = new AttackMobMessage { MobId = mobId, DamageAmount = damageAmount };

            var mockMob = MockGameObject.CreateMob(mobId);
            mockProximityChecker.GetNearbyGameObjects().Returns(new[] { mockMob });

            // Act
            handler.Handle(message);

            // Assert
            mockMob.Components.Get<IMobHealthController>()
                .Received(1).Damage(damageAmount);
        }

        [Fact]
        public void Handle_ShouldChangeBehaviourToAttacked_WhenMobIsDamaged()
        {
            // Arrange
            var mobId = 123;
            var message = new AttackMobMessage { MobId = mobId, DamageAmount = 10 };

            var mockMob = MockGameObject.CreateMob(mobId);
            mockProximityChecker.GetNearbyGameObjects().Returns(new[] { mockMob });

            // Act
            handler.Handle(message);

            // Assert
            mockMob.Components.Get<IMobBehaviourManager>()
                .Received(1).ChangeBehaviour(MobBehaviourType.Attacked);
        }

        [Fact]
        public void Handle_ShouldNotDamage_WhenMobIdDoesNotMatch()
        {
            // Arrange
            var targetMobId = 999;
            var actualMobId = 123;
            var message = new AttackMobMessage { MobId = targetMobId, DamageAmount = 50 };

            var mockMob = MockGameObject.CreateMob(actualMobId);
            mockProximityChecker.GetNearbyGameObjects().Returns(new[] { mockMob });

            // Act
            handler.Handle(message);

            // Assert
            mockMob.Components.Get<IMobHealthController>()
                .DidNotReceive().Damage(Arg.Any<int>());
        }

        [Fact]
        public void Handle_ShouldNotChangeBehaviour_WhenMobIdDoesNotMatch()
        {
            // Arrange
            var targetMobId = 999;
            var actualMobId = 123;
            var message = new AttackMobMessage { MobId = targetMobId, DamageAmount = 50 };

            var mockMob = MockGameObject.CreateMob(actualMobId);
            mockProximityChecker.GetNearbyGameObjects().Returns(new[] { mockMob });

            // Act
            handler.Handle(message);

            // Assert
            mockMob.Components.Get<IMobBehaviourManager>()
                .DidNotReceive().ChangeBehaviour(Arg.Any<MobBehaviourType>());
        }

        [Fact]
        public void Handle_ShouldHandleEmptyNearbyObjects_Gracefully()
        {
            // Arrange
            var message = new AttackMobMessage { MobId = 123, DamageAmount = 50 };
            mockProximityChecker.GetNearbyGameObjects().Returns(new List<IGameObject>());

            // Act & Assert - Should not throw exception
            Should.NotThrow(() => handler.Handle(message));
        }

        [Fact]
        public void Handle_ShouldOnlyDamageTargetMob_WhenMultipleMobsAreNearby()
        {
            // Arrange
            var targetMobId = 123;
            var otherMobId = 456;
            var damageAmount = 50;
            var message = new AttackMobMessage { MobId = targetMobId, DamageAmount = damageAmount };

            var targetMob = MockGameObject.CreateMob(targetMobId);
            var otherMob = MockGameObject.CreateMob(otherMobId);
            mockProximityChecker.GetNearbyGameObjects().Returns(new[] { targetMob, otherMob });

            // Act
            handler.Handle(message);

            // Assert
            targetMob.Components.Get<IMobHealthController>()
                .Received(1).Damage(damageAmount);
            otherMob.Components.Get<IMobHealthController>()
                .DidNotReceive().Damage(Arg.Any<int>());
        }

        [Fact]
        public void Handle_ShouldNotThrow_WhenBehaviourManagerIsNull()
        {
            // Arrange
            var mobId = 123;
            var message = new AttackMobMessage { MobId = mobId, DamageAmount = 50 };

            var mockMob = Substitute.For<IGameObject>();
            var mockComponents = Substitute.For<IComponents>();
            mockMob.Id.Returns(mobId);
            mockMob.Components.Returns(mockComponents);
            // BehaviourManager returns null
            mockComponents.Get<IMobBehaviourManager>().Returns((IMobBehaviourManager)null);
            mockComponents.Get<IMobHealthController>().Returns(Substitute.For<IMobHealthController>());

            mockProximityChecker.GetNearbyGameObjects().Returns(new[] { mockMob });

            // Act & Assert - Should not throw exception
            Should.NotThrow(() => handler.Handle(message));
        }

        [Fact]
        public void Handle_ShouldNotThrow_WhenHealthControllerIsNull()
        {
            // Arrange
            var mobId = 123;
            var message = new AttackMobMessage { MobId = mobId, DamageAmount = 50 };

            var mockMob = Substitute.For<IGameObject>();
            var mockComponents = Substitute.For<IComponents>();
            mockMob.Id.Returns(mobId);
            mockMob.Components.Returns(mockComponents);
            mockComponents.Get<IMobBehaviourManager>().Returns(Substitute.For<IMobBehaviourManager>());
            // HealthController returns null
            mockComponents.Get<IMobHealthController>().Returns((IMobHealthController)null);

            mockProximityChecker.GetNearbyGameObjects().Returns(new[] { mockMob });

            // Act & Assert - Should not throw exception
            Should.NotThrow(() => handler.Handle(message));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(9999)]
        public void Handle_ShouldApplyCorrectDamageAmount(int damageAmount)
        {
            // Arrange
            var mobId = 123;
            var message = new AttackMobMessage { MobId = mobId, DamageAmount = damageAmount };

            var mockMob = MockGameObject.CreateMob(mobId);
            mockProximityChecker.GetNearbyGameObjects().Returns(new[] { mockMob });

            // Act
            handler.Handle(message);

            // Assert
            mockMob.Components.Get<IMobHealthController>()
                .Received(1).Damage(damageAmount);
        }
    }
}
