using NUnit.Framework;
using Scripts.Core.Domain.Logic;

namespace MapleFighters.Tests.EditMode
{
    [TestFixture]
    public class MovementValidatorTests
    {
        private MovementValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new MovementValidator();
        }

        #region IsValidPosition Tests

        [Test]
        public void IsValidPosition_WithinBounds_ReturnsTrue()
        {
            // Arrange
            float x = 5, y = 5;
            float minX = 0, maxX = 10, minY = 0, maxY = 10;

            // Act
            bool result = validator.IsValidPosition(x, y, minX, maxX, minY, maxY);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsValidPosition_OnBoundary_ReturnsTrue()
        {
            // Arrange
            float x = 0, y = 10;
            float minX = 0, maxX = 10, minY = 0, maxY = 10;

            // Act
            bool result = validator.IsValidPosition(x, y, minX, maxX, minY, maxY);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsValidPosition_OutsideBoundsX_ReturnsFalse()
        {
            // Arrange
            float x = 15, y = 5;
            float minX = 0, maxX = 10, minY = 0, maxY = 10;

            // Act
            bool result = validator.IsValidPosition(x, y, minX, maxX, minY, maxY);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsValidPosition_OutsideBoundsY_ReturnsFalse()
        {
            // Arrange
            float x = 5, y = -5;
            float minX = 0, maxX = 10, minY = 0, maxY = 10;

            // Act
            bool result = validator.IsValidPosition(x, y, minX, maxX, minY, maxY);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region CanMove Tests

        [Test]
        public void CanMove_WithinMaxDistance_ReturnsTrue()
        {
            // Arrange
            float currentX = 0, currentY = 0;
            float targetX = 3, targetY = 4; // Distance = 5
            float maxDistance = 10;

            // Act
            bool result = validator.CanMove(currentX, currentY, targetX, targetY, maxDistance);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void CanMove_ExactlyAtMaxDistance_ReturnsTrue()
        {
            // Arrange
            float currentX = 0, currentY = 0;
            float targetX = 3, targetY = 4; // Distance = 5
            float maxDistance = 5;

            // Act
            bool result = validator.CanMove(currentX, currentY, targetX, targetY, maxDistance);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void CanMove_BeyondMaxDistance_ReturnsFalse()
        {
            // Arrange
            float currentX = 0, currentY = 0;
            float targetX = 30, targetY = 40; // Distance = 50
            float maxDistance = 10;

            // Act
            bool result = validator.CanMove(currentX, currentY, targetX, targetY, maxDistance);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void CanMove_SamePosition_ReturnsTrue()
        {
            // Arrange
            float currentX = 5, currentY = 5;
            float targetX = 5, targetY = 5;
            float maxDistance = 1;

            // Act
            bool result = validator.CanMove(currentX, currentY, targetX, targetY, maxDistance);

            // Assert
            Assert.IsTrue(result);
        }

        #endregion

        #region ClampPosition Tests

        [Test]
        public void ClampPosition_WithinBounds_ReturnsSamePosition()
        {
            // Arrange
            float x = 5, y = 5;
            float minX = 0, maxX = 10, minY = 0, maxY = 10;

            // Act
            var (clampedX, clampedY) = validator.ClampPosition(x, y, minX, maxX, minY, maxY);

            // Assert
            Assert.AreEqual(5, clampedX);
            Assert.AreEqual(5, clampedY);
        }

        [Test]
        public void ClampPosition_OutsideMaxBounds_ReturnsClampedPosition()
        {
            // Arrange
            float x = 15, y = 20;
            float minX = 0, maxX = 10, minY = 0, maxY = 10;

            // Act
            var (clampedX, clampedY) = validator.ClampPosition(x, y, minX, maxX, minY, maxY);

            // Assert
            Assert.AreEqual(10, clampedX);
            Assert.AreEqual(10, clampedY);
        }

        [Test]
        public void ClampPosition_OutsideMinBounds_ReturnsClampedPosition()
        {
            // Arrange
            float x = -5, y = -10;
            float minX = 0, maxX = 10, minY = 0, maxY = 10;

            // Act
            var (clampedX, clampedY) = validator.ClampPosition(x, y, minX, maxX, minY, maxY);

            // Assert
            Assert.AreEqual(0, clampedX);
            Assert.AreEqual(0, clampedY);
        }

        #endregion

        #region CalculateDistance Tests

        [Test]
        public void CalculateDistance_WithKnownValues_ReturnsCorrectDistance()
        {
            // Arrange - 3-4-5 triangle
            float x1 = 0, y1 = 0;
            float x2 = 3, y2 = 4;

            // Act
            float result = validator.CalculateDistance(x1, y1, x2, y2);

            // Assert
            Assert.AreEqual(5f, result, 0.001f);
        }

        [Test]
        public void CalculateDistance_SamePoint_ReturnsZero()
        {
            // Arrange
            float x1 = 5, y1 = 5;
            float x2 = 5, y2 = 5;

            // Act
            float result = validator.CalculateDistance(x1, y1, x2, y2);

            // Assert
            Assert.AreEqual(0f, result, 0.001f);
        }

        [Test]
        public void CalculateDistance_NegativeCoordinates_ReturnsCorrectDistance()
        {
            // Arrange
            float x1 = -3, y1 = -4;
            float x2 = 0, y2 = 0;

            // Act
            float result = validator.CalculateDistance(x1, y1, x2, y2);

            // Assert
            Assert.AreEqual(5f, result, 0.001f);
        }

        #endregion
    }
}
