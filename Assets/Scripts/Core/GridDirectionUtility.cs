using UnityEngine;

namespace CubeShift.Core
{
    /// <summary>
    /// Shared helpers for converting raw input into the four supported grid directions.
    /// </summary>
    public static class GridDirectionUtility
    {
        public static bool TryNormalize(Vector2Int input, out Vector2Int direction)
        {
            direction = Vector2Int.zero;

            if (input == Vector2Int.zero)
            {
                return false;
            }

            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                direction = input.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                direction = input.y > 0 ? Vector2Int.up : Vector2Int.down;
            }

            return true;
        }

        public static bool TryFromSwipeDelta(Vector2 swipeDelta, float threshold, out Vector2Int direction)
        {
            direction = Vector2Int.zero;

            float safeThreshold = Mathf.Max(0f, threshold);
            if (swipeDelta.sqrMagnitude < safeThreshold * safeThreshold)
            {
                return false;
            }

            if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
            {
                direction = swipeDelta.x > 0f ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                direction = swipeDelta.y > 0f ? Vector2Int.up : Vector2Int.down;
            }

            return true;
        }

        public static Vector3 ToWorldDirection(Vector2Int gridDirection)
        {
            return new Vector3(gridDirection.x, 0f, gridDirection.y);
        }
    }
}
