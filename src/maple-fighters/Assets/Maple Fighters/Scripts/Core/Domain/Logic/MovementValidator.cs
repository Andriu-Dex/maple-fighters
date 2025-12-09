namespace Scripts.Core.Domain.Logic
{
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
}
