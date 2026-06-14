using CubeShift.Core;
using CubeShift.LevelDesign;
using CubeShift.Player;
using UnityEngine;

namespace CubeShift.Tiles
{
    /// <summary>
    /// Completes the current level when the cube lands on this tile.
    /// </summary>
    public sealed class GoalTile : TileBase
    {
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private bool logDebugMessages;
        [SerializeField] private bool showRuntimeMarker = true;
        [SerializeField] private Color markerColor = new Color(0.15f, 0.95f, 1f, 1f);
        [SerializeField, Min(0.1f)] private float standingCompletionRadius = 0.62f;
        [SerializeField, Min(0.1f)] private float standingHeightTolerance = 1.35f;

        private Transform marker;
        private Material markerMaterial;
        private PlayerCubeController cachedPlayer;

        public override TileEffectType EffectType => TileEffectType.Goal;

        private void Awake()
        {
            if (levelManager == null && LevelManager.Instance != null)
            {
                levelManager = LevelManager.Instance;
            }

            EnsureRuntimeMarker();
        }

        private void Update()
        {
            CompleteIfPlayerIsStandingHere();

            if (marker == null)
            {
                return;
            }

            marker.Rotate(0f, 42f * Time.deltaTime, 0f, Space.Self);
            float pulse = 1f + Mathf.Sin(Time.time * 3.25f) * 0.08f;
            marker.localScale = new Vector3(0.78f * pulse, 0.018f, 0.78f * pulse);
        }

        protected override void HandlePlayerLanded(PlayerCubeController player)
        {
            LevelManager manager = LevelManager.Instance != null
                ? LevelManager.Instance
                : levelManager != null
                    ? levelManager
                    : FindAnyObjectByType<LevelManager>();

            if (manager == null)
            {
                Debug.LogWarning("GoalTile could not find a LevelManager to complete the level.", this);
                return;
            }

            levelManager = manager;

            bool canComplete = manager.CanCompleteActiveLevel();
            if (logDebugMessages)
            {
                if (canComplete)
                {
                    Debug.Log("[GoalTile] CanCompleteLevel=True -> CompleteLevel()", this);
                }
                else
                {
                    var activeLevel = manager.ActiveLevel;
                    var completed = manager.CompletedObjectives;
                    var required = activeLevel != null ? activeLevel.RequiredObjectives : LevelObjective.None;
                    var missing = required & ~completed;
                    Debug.Log($"[GoalTile] CanCompleteLevel=False | Required={required} | Completed={completed} | Missing={missing}", this);
                }
            }

            manager.CompleteLevel();
        }

        private void CompleteIfPlayerIsStandingHere()
        {
            LevelManager manager = LevelManager.Instance != null ? LevelManager.Instance : levelManager;
            if (manager == null || manager.IsLevelComplete || manager.IsTransitioning)
            {
                return;
            }

            if (cachedPlayer == null)
            {
                cachedPlayer = FindAnyObjectByType<PlayerCubeController>();
            }

            if (cachedPlayer == null || cachedPlayer.IsMoving || cachedPlayer.IsFalling)
            {
                return;
            }

            Vector3 playerPosition = cachedPlayer.transform.position;
            Vector3 goalPosition = transform.position;
            Vector2 playerXZ = new Vector2(playerPosition.x, playerPosition.z);
            Vector2 goalXZ = new Vector2(goalPosition.x, goalPosition.z);

            if (Vector2.Distance(playerXZ, goalXZ) > standingCompletionRadius)
            {
                return;
            }

            if (Mathf.Abs(playerPosition.y - (goalPosition.y + 0.6f)) > standingHeightTolerance)
            {
                return;
            }

            if (logDebugMessages)
            {
                Debug.Log("[GoalTile] Player standing fallback completed the level.", this);
            }

            levelManager = manager;
            manager.CompleteLevel();
        }

        private void EnsureRuntimeMarker()
        {
            if (!showRuntimeMarker || marker != null)
            {
                return;
            }

            TintGoalSurface();

            GameObject halo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            halo.name = "GoalMarker_Halo";
            halo.transform.SetParent(transform, false);
            halo.transform.localPosition = new Vector3(0f, 0.085f, 0f);
            halo.transform.localScale = new Vector3(0.78f, 0.018f, 0.78f);
            marker = halo.transform;

            Collider markerCollider = halo.GetComponent<Collider>();
            if (markerCollider != null)
            {
                Destroy(markerCollider);
            }

            Renderer markerRenderer = halo.GetComponent<Renderer>();
            if (markerRenderer != null)
            {
                markerMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                markerMaterial.color = markerColor;
                markerMaterial.SetColor("_BaseColor", markerColor);
                markerMaterial.EnableKeyword("_EMISSION");
                markerMaterial.SetColor("_EmissionColor", markerColor * 1.25f);
                markerRenderer.sharedMaterial = markerMaterial;
            }
        }

        private void TintGoalSurface()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer targetRenderer in renderers)
            {
                if (targetRenderer == null)
                {
                    continue;
                }

                Material material = targetRenderer.material;
                material.color = markerColor;
                material.SetColor("_BaseColor", markerColor);
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", markerColor * 0.75f);
            }
        }
    }
}
