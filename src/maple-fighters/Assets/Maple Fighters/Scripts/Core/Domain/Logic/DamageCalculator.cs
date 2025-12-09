namespace Scripts.Core.Domain.Logic
{
    /// <summary>
    /// Interface for damage calculation logic.
    /// </summary>
    public interface IDamageCalculator
    {
        /// <summary>
        /// Calculates the final damage based on base damage and defense.
        /// </summary>
        int CalculateDamage(int baseDamage, int defense);

        /// <summary>
        /// Calculates critical damage with a multiplier.
        /// </summary>
        int CalculateCriticalDamage(int damage, float critMultiplier);

        /// <summary>
        /// Determines if an attack is a critical hit based on chance.
        /// </summary>
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
            return System.Math.Max(1, damage); // Minimum 1 damage
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
}
