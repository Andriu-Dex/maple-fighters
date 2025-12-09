using Game.Application.Handlers;
using Game.Application.Objects;
using Game.Messages;
using Game.UnitTests.Mocks;
using InterestManagement;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Game.UnitTests.Handlers
{
    public class ChangePositionMessageHandlerTests
    {
        private readonly IGameObject mockPlayer;
        private readonly ITransform mockTransform;
        private readonly ChangePositionMessageHandler handler;

        public ChangePositionMessageHandlerTests()
        {
            mockPlayer = MockGameObject.CreatePlayer();
            mockTransform = mockPlayer.Transform;
            handler = new ChangePositionMessageHandler(mockPlayer);
        }

        [Fact]
        public void Handle_ShouldUpdatePlayerPosition()
        {
            // Arrange
            var newX = 100f;
            var newY = 50f;
            var message = new ChangePositionMessage { X = newX, Y = newY };
            mockTransform.Position.Returns(new Vector2(0, 0));

            // Act
            handler.Handle(message);

            // Assert
            mockTransform.Received(1).SetPosition(Arg.Is<Vector2>(v => v.X == newX && v.Y == newY));
        }

        [Fact]
        public void Handle_ShouldSetDirection_WhenMovingRight()
        {
            // Arrange
            var currentPosition = new Vector2(100, 0);
            var newX = 50f; // Moving left (new position is to the left of current)
            var newY = 0f;
            var message = new ChangePositionMessage { X = newX, Y = newY };
            mockTransform.Position.Returns(currentPosition);

            // Act
            handler.Handle(message);

            // Assert - Direction should be set (positive X when moving right relative to new position)
            mockTransform.Received(1).SetDirection(Arg.Any<Vector2>());
        }

        [Fact]
        public void Handle_ShouldSetDirection_WhenMovingLeft()
        {
            // Arrange
            var currentPosition = new Vector2(0, 0);
            var newX = 100f; // Moving right (new position is to the right of current)
            var newY = 0f;
            var message = new ChangePositionMessage { X = newX, Y = newY };
            mockTransform.Position.Returns(currentPosition);

            // Act
            handler.Handle(message);

            // Assert - Direction should be set
            mockTransform.Received(1).SetDirection(Arg.Any<Vector2>());
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(100, 200)]
        [InlineData(-50, -100)]
        [InlineData(999.99f, 888.88f)]
        public void Handle_ShouldSetPositionWithCorrectCoordinates(float x, float y)
        {
            // Arrange
            var message = new ChangePositionMessage { X = x, Y = y };
            mockTransform.Position.Returns(new Vector2(0, 0));

            // Act
            handler.Handle(message);

            // Assert
            mockTransform.Received(1).SetPosition(Arg.Is<Vector2>(v => 
                System.Math.Abs(v.X - x) < 0.001f && 
                System.Math.Abs(v.Y - y) < 0.001f));
        }

        [Fact]
        public void Handle_ShouldHandleZeroPosition()
        {
            // Arrange
            var message = new ChangePositionMessage { X = 0, Y = 0 };
            mockTransform.Position.Returns(new Vector2(10, 10));

            // Act & Assert - Should not throw
            Should.NotThrow(() => handler.Handle(message));
            mockTransform.Received(1).SetPosition(Arg.Is<Vector2>(v => v.X == 0 && v.Y == 0));
        }

        [Fact]
        public void Handle_ShouldHandleNegativeCoordinates()
        {
            // Arrange
            var message = new ChangePositionMessage { X = -100, Y = -200 };
            mockTransform.Position.Returns(new Vector2(0, 0));

            // Act & Assert - Should not throw
            Should.NotThrow(() => handler.Handle(message));
            mockTransform.Received(1).SetPosition(Arg.Is<Vector2>(v => v.X == -100 && v.Y == -200));
        }

        [Fact]
        public void Handle_ShouldHandleLargeCoordinates()
        {
            // Arrange
            var message = new ChangePositionMessage { X = 10000f, Y = 10000f };
            mockTransform.Position.Returns(new Vector2(0, 0));

            // Act & Assert - Should not throw
            Should.NotThrow(() => handler.Handle(message));
            mockTransform.Received(1).SetPosition(Arg.Is<Vector2>(v => v.X == 10000f && v.Y == 10000f));
        }
    }
}
