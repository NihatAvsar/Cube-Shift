using System.Collections;
using CubeShift.Core;
using CubeShift.Tiles;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CubeShift.Player
{
    /// <summary>
    /// Moves the player cube one grid cell at a time with a 90 degree rolling animation.
    /// Tile checks are intentionally done only after each move for mobile-friendly performance.
    /// </summary>
    [RequireComponent(typeof(CubeFaceTracker))]
    public sealed class PlayerCubeController : MonoBehaviour
    {
        private const int MaxGroundHits = 8;
        private const float RotationStepDegrees = 90f;

        [Header("Movement")]
        [SerializeField, Min(0.01f)] private float tileSize = 1f;
        [SerializeField, Min(0.01f)] private float cubeSize = 1f;
        [SerializeField, Min(0.01f)] private float moveDuration = 0.22f;
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;

        [Header("Tile Detection")]
        [SerializeField] private LayerMask groundLayer = Physics.DefaultRaycastLayers;
        [SerializeField, Min(0.01f)] private float raycastStartHeight = 0.75f;
        [SerializeField, Min(0.01f)] private float raycastDistance = 2f;

        [Header("Fall Restart")]
        [SerializeField, Min(0f)] private float restartDelayAfterFall = 0.55f;
        [SerializeField, Min(0.1f)] private float fallDistance = 3f;

        [Header("References")]
        [SerializeField] private CubeFaceTracker faceTracker;
        [SerializeField] private LevelManager levelManager;

        private bool isMoving;
        private bool isFalling;
        private readonly RaycastHit[] groundHits = new RaycastHit[MaxGroundHits];

        public bool IsMoving => isMoving;
        public bool IsFalling => isFalling;
        public bool CanReceiveInput => !isMoving && !isFalling && (levelManager == null || !levelManager.IsTransitioning);
        public CubeFace BottomFace => faceTracker != null ? faceTracker.CurrentBottomFace : CubeFace.White;
        public CubeFaceTracker FaceTracker => faceTracker;

        private void Awake()
        {
            if (faceTracker == null)
            {
                faceTracker = GetComponent<CubeFaceTracker>();
            }

            if (levelManager == null)
            {
                levelManager = LevelManager.Instance != null
                    ? LevelManager.Instance
                    : FindAnyObjectByType<LevelManager>();
            }

            WarnIfRollScaleIsInconsistent();
        }

        public bool TryMove(Vector2Int gridDirection)
        {
            if (!CanReceiveInput || !GridDirectionUtility.TryNormalize(gridDirection, out Vector2Int direction))
            {
                return false;
            }

            isMoving = true;
            StartCoroutine(RollRoutine(direction));
            return true;
        }

        public void CheckCurrentTile()
        {
            if (isMoving || isFalling)
            {
                return;
            }

            EvaluateLandingTile();
        }

        private IEnumerator RollRoutine(Vector2Int gridDirection)
        {
            Vector3 startPosition = transform.position;
            Vector3 worldDirection = GridDirectionUtility.ToWorldDirection(gridDirection);
            Vector3 targetPosition = startPosition + worldDirection * tileSize;
            Vector3 pivot = CalculateRollPivot(startPosition, worldDirection);
            Vector3 rotationAxis = Vector3.Cross(Vector3.up, worldDirection).normalized;

            float elapsed = 0f;
            float rotatedAngle = 0f;

            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / moveDuration);
                float targetAngle = Mathf.SmoothStep(0f, RotationStepDegrees, progress);
                float deltaAngle = targetAngle - rotatedAngle;

                transform.RotateAround(pivot, rotationAxis, deltaAngle);
                rotatedAngle = targetAngle;

                yield return null;
            }

            float remainingAngle = RotationStepDegrees - rotatedAngle;
            if (remainingAngle > 0f)
            {
                transform.RotateAround(pivot, rotationAxis, remainingAngle);
            }

            transform.position = SnapPosition(targetPosition, startPosition.y);
            transform.rotation = SnapRotation(transform.rotation);

            if (faceTracker != null)
            {
                faceTracker.Roll(gridDirection);
            }

            isMoving = false;
            EvaluateLandingTile();
        }

        private void EvaluateLandingTile()
        {
            TileBase tile = FindTileBelow();

            if (tile == null)
            {
                StartFallAndRestart();
                return;
            }

            tile.OnPlayerLanded(this);
        }

        private TileBase FindTileBelow()
        {
            Vector3 rayOrigin = transform.position + Vector3.up * raycastStartHeight;
            int hitCount = Physics.RaycastNonAlloc(
                rayOrigin,
                Vector3.down,
                groundHits,
                raycastDistance,
                groundLayer,
                QueryTriggerInteraction.Collide);

            if (hitCount == 0)
            {
                return null;
            }

            TileBase closestTile = null;
            float closestDistance = float.PositiveInfinity;

            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit hit = groundHits[index];
                if (hit.transform == null || hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                TileBase tile = hit.collider.GetComponentInParent<TileBase>();
                if (tile != null && hit.distance < closestDistance)
                {
                    closestTile = tile;
                    closestDistance = hit.distance;
                }
            }

            return closestTile;
        }

        private void StartFallAndRestart()
        {
            if (isFalling)
            {
                return;
            }

            StartCoroutine(FallAndRestartRoutine());
        }

        private IEnumerator FallAndRestartRoutine()
        {
            isFalling = true;

            Vector3 startPosition = transform.position;
            Quaternion startRotation = transform.rotation;
            Quaternion fallRotation = startRotation * Quaternion.Euler(25f, 0f, 25f);
            float elapsed = 0f;

            while (elapsed < restartDelayAfterFall)
            {
                elapsed += Time.deltaTime;
                float progress = restartDelayAfterFall <= 0f ? 1f : Mathf.Clamp01(elapsed / restartDelayAfterFall);
                float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

                transform.position = Vector3.Lerp(startPosition, startPosition + Vector3.down * fallDistance, easedProgress);
                transform.rotation = Quaternion.Slerp(startRotation, fallRotation, easedProgress);

                yield return null;
            }

            if (levelManager != null)
            {
                levelManager.RestartLevel();
            }
            else
            {
                ReloadActiveSceneFallback();
            }
        }

        private Vector3 CalculateRollPivot(Vector3 cubeCenter, Vector3 worldDirection)
        {
            return cubeCenter + (worldDirection + Vector3.down) * (cubeSize * 0.5f);
        }

        private Vector3 SnapPosition(Vector3 position, float y)
        {
            return new Vector3(
                SnapToGrid(position.x, gridOrigin.x),
                y,
                SnapToGrid(position.z, gridOrigin.z));
        }

        private float SnapToGrid(float value, float origin)
        {
            return Mathf.Round((value - origin) / tileSize) * tileSize + origin;
        }

        private static Quaternion SnapRotation(Quaternion rotation)
        {
            Vector3 euler = rotation.eulerAngles;
            return Quaternion.Euler(
                SnapAngle(euler.x),
                SnapAngle(euler.y),
                SnapAngle(euler.z));
        }

        private static float SnapAngle(float angle)
        {
            return Mathf.Repeat(Mathf.Round(angle / RotationStepDegrees) * RotationStepDegrees, 360f);
        }

        private void ReloadActiveSceneFallback()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.buildIndex >= 0)
            {
                SceneManager.LoadScene(activeScene.buildIndex);
                return;
            }

            if (!string.IsNullOrEmpty(activeScene.name))
            {
                SceneManager.LoadScene(activeScene.name);
                return;
            }

            Debug.LogWarning("Could not reload the active scene because it has no build index or scene name.", this);
        }

        private void WarnIfRollScaleIsInconsistent()
        {
            if (Mathf.Approximately(tileSize, cubeSize))
            {
                return;
            }

            Debug.LogWarning(
                "PlayerCubeController rolls most accurately when tileSize and cubeSize match. " +
                "The final position will still snap to the grid, but the visual pivot may look offset.",
                this);
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 rayOrigin = transform.position + Vector3.up * raycastStartHeight;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * raycastDistance);
        }
    }
}
