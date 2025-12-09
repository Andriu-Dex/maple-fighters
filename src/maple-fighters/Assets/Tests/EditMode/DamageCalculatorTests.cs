using NUnit.Framework;
using Scripts.Core.Domain.Logic;

namespace MapleFighters.Tests.EditMode
{
    [TestFixture]
    public class DamageCalculatorTests
    {
        private DamageCalculator calculator;

        [SetUp]
        public void SetUp()
        {
            calculator = new DamageCalculator();
        }

        [Test]
        public void CalculateDamage_WithNormalValues_ReturnsCorrectDamage()
        {
            // Arrange
            int baseDamage = 100;
            int defense = 30;

            // Act
            int result = calculator.CalculateDamage(baseDamage, defense);

            // Assert
            Assert.AreEqual(70, result);
        }

        [Test]
        public void CalculateDamage_WithHighDefense_ReturnsMinimumDamage()
        {
            // Arrange
            int baseDamage = 10;
            int defense = 100;

            // Act
            int result = calculator.CalculateDamage(baseDamage, defense);

            // Assert
            Assert.AreEqual(1, result); // Minimum damage is 1
        }

        [Test]
        public void CalculateDamage_WithZeroDefense_ReturnsFullDamage()
        {
            // Arrange
            int baseDamage = 50;
            int defense = 0;

            // Act
            int result = calculator.CalculateDamage(baseDamage, defense);

            // Assert
            Assert.AreEqual(50, result);
        }

        [Test]
        public void CalculateCriticalDamage_WithDoubleMultiplier_ReturnsDoubleDamage()
        {
            // Arrange
            int damage = 100;
            float critMultiplier = 2.0f;

            // Act
            int result = calculator.CalculateCriticalDamage(damage, critMultiplier);

            // Assert
            Assert.AreEqual(200, result);
        }

        [Test]
        public void CalculateCriticalDamage_WithHalfMultiplier_ReturnsHalfDamage()
        {
            // Arrange
            int damage = 100;
            float critMultiplier = 0.5f;

            // Act
            int result = calculator.CalculateCriticalDamage(damage, critMultiplier);

            // Assert
            Assert.AreEqual(50, result);
        }

        [Test]
        public void IsCriticalHit_With100PercentChance_ReturnsTrue()
        {
            // Arrange - 100% critical chance should always hit
            
            // Act
            bool result = calculator.IsCriticalHit(1.0f);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsCriticalHit_With0PercentChance_ReturnsFalse()
        {
            // Arrange - 0% critical chance should never hit
            
            // Act
            bool result = calculator.IsCriticalHit(0.0f);

            // Assert
            Assert.IsFalse(result);
        }

        [TestCase(100, 0, ExpectedResult = 100)]
        [TestCase(100, 50, ExpectedResult = 50)]
        [TestCase(100, 100, ExpectedResult = 1)]
        [TestCase(50, 25, ExpectedResult = 25)]
        public int CalculateDamage_WithVariousInputs_ReturnsExpected(int baseDamage, int defense)
        {
            return calculator.CalculateDamage(baseDamage, defense);
        }
    }
}
