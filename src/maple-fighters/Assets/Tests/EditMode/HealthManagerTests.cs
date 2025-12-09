using NUnit.Framework;
using Scripts.Core.Domain.Logic;

namespace MapleFighters.Tests.EditMode
{
    [TestFixture]
    public class HealthManagerTests
    {
        private HealthManager healthManager;
        private const int DefaultMaxHealth = 100;

        [SetUp]
        public void SetUp()
        {
            healthManager = new HealthManager(DefaultMaxHealth);
        }

        [Test]
        public void Constructor_InitializesWithMaxHealth()
        {
            // Assert
            Assert.AreEqual(DefaultMaxHealth, healthManager.CurrentHealth);
            Assert.AreEqual(DefaultMaxHealth, healthManager.MaxHealth);
        }

        [Test]
        public void TakeDamage_ReducesCurrentHealth()
        {
            // Arrange
            int damage = 30;

            // Act
            healthManager.TakeDamage(damage);

            // Assert
            Assert.AreEqual(70, healthManager.CurrentHealth);
        }

        [Test]
        public void TakeDamage_WithExcessiveDamage_SetsHealthToZero()
        {
            // Arrange
            int damage = 150;

            // Act
            healthManager.TakeDamage(damage);

            // Assert
            Assert.AreEqual(0, healthManager.CurrentHealth);
        }

        [Test]
        public void TakeDamage_WhenDead_DoesNothing()
        {
            // Arrange
            healthManager.TakeDamage(100); // Kill
            
            // Act
            healthManager.TakeDamage(50); // Try to damage when dead

            // Assert
            Assert.AreEqual(0, healthManager.CurrentHealth);
        }

        [Test]
        public void IsDead_WhenHealthIsZero_ReturnsTrue()
        {
            // Arrange
            healthManager.TakeDamage(100);

            // Assert
            Assert.IsTrue(healthManager.IsDead);
        }

        [Test]
        public void IsDead_WhenHealthIsPositive_ReturnsFalse()
        {
            // Assert
            Assert.IsFalse(healthManager.IsDead);
        }

        [Test]
        public void Heal_IncreasesCurrentHealth()
        {
            // Arrange
            healthManager.TakeDamage(50);
            
            // Act
            healthManager.Heal(30);

            // Assert
            Assert.AreEqual(80, healthManager.CurrentHealth);
        }

        [Test]
        public void Heal_DoesNotExceedMaxHealth()
        {
            // Arrange
            healthManager.TakeDamage(20);
            
            // Act
            healthManager.Heal(50); // Try to heal more than missing

            // Assert
            Assert.AreEqual(DefaultMaxHealth, healthManager.CurrentHealth);
        }

        [Test]
        public void Heal_WhenDead_DoesNothing()
        {
            // Arrange
            healthManager.TakeDamage(100); // Kill
            
            // Act
            healthManager.Heal(50);

            // Assert
            Assert.AreEqual(0, healthManager.CurrentHealth);
            Assert.IsTrue(healthManager.IsDead);
        }

        [Test]
        public void Reset_RestoresHealthToMax()
        {
            // Arrange
            healthManager.TakeDamage(70);
            
            // Act
            healthManager.Reset();

            // Assert
            Assert.AreEqual(DefaultMaxHealth, healthManager.CurrentHealth);
        }

        [TestCase(100, 50, ExpectedResult = 50)]
        [TestCase(100, 100, ExpectedResult = 0)]
        [TestCase(100, 0, ExpectedResult = 100)]
        [TestCase(100, 150, ExpectedResult = 0)]
        public int TakeDamage_WithVariousDamages_ReturnsExpectedHealth(int maxHealth, int damage)
        {
            var manager = new HealthManager(maxHealth);
            manager.TakeDamage(damage);
            return manager.CurrentHealth;
        }
    }
}
