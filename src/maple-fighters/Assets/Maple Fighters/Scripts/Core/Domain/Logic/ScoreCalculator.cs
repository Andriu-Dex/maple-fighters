namespace Scripts.Core.Domain.Logic
{
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
            multiplier = System.Math.Max(0.5f, multiplier); // Minimum 50% score
            return (int)(BaseScorePerKill * multiplier);
        }

        public int CalculateComboBonus(int comboCount)
        {
            if (comboCount <= 1) return 0;
            return (int)(BaseScorePerKill * comboCount * ComboMultiplierIncrement);
        }

        public int CalculateTotalScore(int baseScore, int comboBonus, float timeMultiplier)
        {
            timeMultiplier = System.Math.Max(0.1f, timeMultiplier); // Minimum 10%
            return (int)((baseScore + comboBonus) * timeMultiplier);
        }
    }
}
