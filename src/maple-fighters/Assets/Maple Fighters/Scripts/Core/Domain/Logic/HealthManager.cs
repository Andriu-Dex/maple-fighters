namespace Scripts.Core.Domain.Logic
{
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
            if (CurrentHealth < 0)
            {
                CurrentHealth = 0;
            }
        }

        public void Heal(int amount)
        {
            if (IsDead) return;

            CurrentHealth += amount;
            if (CurrentHealth > MaxHealth)
            {
                CurrentHealth = MaxHealth;
            }
        }

        public void Reset()
        {
            CurrentHealth = MaxHealth;
        }
    }
}
