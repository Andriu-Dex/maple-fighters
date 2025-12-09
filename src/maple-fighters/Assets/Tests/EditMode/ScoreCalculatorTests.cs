using NUnit.Framework;
using Scripts.Core.Domain.Logic;

namespace MapleFighters.Tests.EditMode
{
    [TestFixture]
    public class ScoreCalculatorTests
    {
        private ScoreCalculator calculator;

        [SetUp]
        public void SetUp()
        {
            calculator = new ScoreCalculator();
        }

        #region CalculateKillScore Tests

        [Test]
        public void CalculateKillScore_WithEqualLevels_ReturnsBaseScore()
        {
            // Arrange
            int enemyLevel = 10;
            int playerLevel = 10;

            // Act
            int result = calculator.CalculateKillScore(enemyLevel, playerLevel);

            // Assert
            Assert.AreEqual(100, result); // Base score
        }

        [Test]
        public void CalculateKillScore_WithHigherEnemyLevel_ReturnsHigherScore()
        {
            // Arrange
            int enemyLevel = 15;
            int playerLevel = 10;

            // Act
            int result = calculator.CalculateKillScore(enemyLevel, playerLevel);

            // Assert
            Assert.Greater(result, 100); // More than base score
        }

        [Test]
        public void CalculateKillScore_WithLowerEnemyLevel_ReturnsLowerScore()
        {
            // Arrange
            int enemyLevel = 5;
            int playerLevel = 10;

            // Act
            int result = calculator.CalculateKillScore(enemyLevel, playerLevel);

            // Assert
            Assert.Less(result, 100); // Less than base score
        }

        [Test]
        public void CalculateKillScore_WithVeryLowEnemyLevel_ReturnsMinimumScore()
        {
            // Arrange
            int enemyLevel = 1;
            int playerLevel = 100;

            // Act
            int result = calculator.CalculateKillScore(enemyLevel, playerLevel);

            // Assert
            Assert.AreEqual(50, result); // Minimum 50% = 50 points
        }

        #endregion

        #region CalculateComboBonus Tests

        [Test]
        public void CalculateComboBonus_WithNoCombo_ReturnsZero()
        {
            // Act
            int result = calculator.CalculateComboBonus(0);

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void CalculateComboBonus_WithSingleKill_ReturnsZero()
        {
            // Act
            int result = calculator.CalculateComboBonus(1);

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void CalculateComboBonus_WithCombo_ReturnsBonus()
        {
            // Act
            int result = calculator.CalculateComboBonus(5);

            // Assert
            Assert.Greater(result, 0);
        }

        [Test]
        public void CalculateComboBonus_HigherCombo_ReturnsHigherBonus()
        {
            // Act
            int lowCombo = calculator.CalculateComboBonus(3);
            int highCombo = calculator.CalculateComboBonus(10);

            // Assert
            Assert.Greater(highCombo, lowCombo);
        }

        #endregion

        #region CalculateTotalScore Tests

        [Test]
        public void CalculateTotalScore_WithNormalMultiplier_ReturnsCorrectTotal()
        {
            // Arrange
            int baseScore = 100;
            int comboBonus = 50;
            float timeMultiplier = 1.0f;

            // Act
            int result = calculator.CalculateTotalScore(baseScore, comboBonus, timeMultiplier);

            // Assert
            Assert.AreEqual(150, result);
        }

        [Test]
        public void CalculateTotalScore_WithDoubleMultiplier_ReturnsDoubledTotal()
        {
            // Arrange
            int baseScore = 100;
            int comboBonus = 50;
            float timeMultiplier = 2.0f;

            // Act
            int result = calculator.CalculateTotalScore(baseScore, comboBonus, timeMultiplier);

            // Assert
            Assert.AreEqual(300, result);
        }

        [Test]
        public void CalculateTotalScore_WithZeroMultiplier_ReturnsMinimumTotal()
        {
            // Arrange
            int baseScore = 100;
            int comboBonus = 0;
            float timeMultiplier = 0.0f;

            // Act
            int result = calculator.CalculateTotalScore(baseScore, comboBonus, timeMultiplier);

            // Assert
            Assert.AreEqual(10, result); // Minimum 10% multiplier
        }

        [TestCase(100, 0, 1.0f, ExpectedResult = 100)]
        [TestCase(100, 50, 1.0f, ExpectedResult = 150)]
        [TestCase(100, 50, 2.0f, ExpectedResult = 300)]
        [TestCase(200, 100, 1.5f, ExpectedResult = 450)]
        public int CalculateTotalScore_WithVariousInputs_ReturnsExpected(
            int baseScore, int comboBonus, float timeMultiplier)
        {
            return calculator.CalculateTotalScore(baseScore, comboBonus, timeMultiplier);
        }

        #endregion
    }
}
