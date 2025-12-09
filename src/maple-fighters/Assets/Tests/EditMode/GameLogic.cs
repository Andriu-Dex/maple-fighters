namespace Scripts.Core.Domain.Logic
{
    /// <summary>
    /// Interface for damage calculation logic.
    /// </summary>
    public interface IDamageCalculator
    {
        int CalculateDamage(int baseDamage, int defense);
        int CalculateCriticalDamage(int damage, float critMultiplier);
        bool IsCriticalHit(float critChance);
    }

    /// <summary>
    /// Pure logic class for damage calculations - no Unity dependencies.
    /// </summary>
    public class DamageCalculator : IDamageCalculator
    {
        private readonly System.Random random;

        public DamageCalculator() : this(new System.Random()) { }

        public DamageCalculator(System.Random random)
        {
            this.random = random;
        }

        public int CalculateDamage(int baseDamage, int defense)
        {
            var damage = baseDamage - defense;
            return System.Math.Max(1, damage);
        }

        public int CalculateCriticalDamage(int damage, float critMultiplier)
        {
            return (int)(damage * critMultiplier);
        }

        public bool IsCriticalHit(float critChance)
        {
            return random.NextDouble() < critChance;
        }
    }

    /// <summary>
    /// Interface for health management logic.
    /// </summary>
    public interface IHealthManager
    {
        int CurrentHealth { get; }
        int MaxHealth { get; }
        bool IsDead { get; }
        void TakeDamage(int amount);
        void Heal(int amount);
        void Reset();
    }

    /// <summary>
    /// Pure logic class for health management - no Unity dependencies.
    /// </summary>
    public class HealthManager : IHealthManager
    {
        public int CurrentHealth { get; private set; }
        public int MaxHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0;

        public HealthManager(int maxHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (IsDead) return;
            CurrentHealth -= amount;
            if (CurrentHealth < 0) CurrentHealth = 0;
        }

        public void Heal(int amount)
        {
            if (IsDead) return;
            CurrentHealth += amount;
            if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;
        }

        public void Reset()
        {
            CurrentHealth = MaxHealth;
        }
    }

    /// <summary>
    /// Interface for movement validation logic.
    /// </summary>
    public interface IMovementValidator
    {
        bool IsValidPosition(float x, float y, float minX, float maxX, float minY, float maxY);
        bool CanMove(float currentX, float currentY, float targetX, float targetY, float maxDistance);
        (float x, float y) ClampPosition(float x, float y, float minX, float maxX, float minY, float maxY);
        float CalculateDistance(float x1, float y1, float x2, float y2);
    }

    /// <summary>
    /// Pure logic class for movement validation - no Unity dependencies.
    /// </summary>
    public class MovementValidator : IMovementValidator
    {
        public bool IsValidPosition(float x, float y, float minX, float maxX, float minY, float maxY)
        {
            return x >= minX && x <= maxX && y >= minY && y <= maxY;
        }

        public bool CanMove(float currentX, float currentY, float targetX, float targetY, float maxDistance)
        {
            var distance = CalculateDistance(currentX, currentY, targetX, targetY);
            return distance <= maxDistance;
        }

        public (float x, float y) ClampPosition(float x, float y, float minX, float maxX, float minY, float maxY)
        {
            var clampedX = System.Math.Max(minX, System.Math.Min(maxX, x));
            var clampedY = System.Math.Max(minY, System.Math.Min(maxY, y));
            return (clampedX, clampedY);
        }

        public float CalculateDistance(float x1, float y1, float x2, float y2)
        {
            var dx = x2 - x1;
            var dy = y2 - y1;
            return (float)System.Math.Sqrt(dx * dx + dy * dy);
        }
    }

    /// <summary>
    /// Interface for score calculation logic.
    /// </summary>
    public interface IScoreCalculator
    {
        int CalculateKillScore(int enemyLevel, int playerLevel);
        int CalculateComboBonus(int comboCount);
        int CalculateTotalScore(int baseScore, int comboBonus, float timeMultiplier);
    }

    /// <summary>
    /// Pure logic class for score calculations - no Unity dependencies.
    /// </summary>
    public class ScoreCalculator : IScoreCalculator
    {
        private const int BaseScorePerKill = 100;
        private const float ComboMultiplierIncrement = 0.1f;
        private const float LevelDifferenceMultiplier = 0.05f;

        public int CalculateKillScore(int enemyLevel, int playerLevel)
        {
            var levelDifference = enemyLevel - playerLevel;
            var multiplier = 1.0f + (levelDifference * LevelDifferenceMultiplier);
            multiplier = System.Math.Max(0.5f, multiplier);
            return (int)(BaseScorePerKill * multiplier);
        }

        public int CalculateComboBonus(int comboCount)
        {
            if (comboCount <= 1) return 0;
            return (int)(BaseScorePerKill * comboCount * ComboMultiplierIncrement);
        }

        public int CalculateTotalScore(int baseScore, int comboBonus, float timeMultiplier)
        {
            timeMultiplier = System.Math.Max(0.1f, timeMultiplier);
            return (int)((baseScore + comboBonus) * timeMultiplier);
        }
    }
}
