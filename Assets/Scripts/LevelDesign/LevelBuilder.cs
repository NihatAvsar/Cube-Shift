using System.Collections.Generic;
using CubeShift.Core;
using CubeShift.Player;
using CubeShift.Tiles;
using UnityEngine;

namespace CubeShift.LevelDesign
{
    /// <summary>Builds a LevelData asset below a dedicated generated root.</summary>
    public sealed class LevelBuilder : MonoBehaviour
    {
        [Header("Level")]
        [SerializeField] private LevelData levelData;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private LevelManager levelManager;

        [Header("Tile Prefabs")]
        [SerializeField] private GameObject normalTilePrefab;
        [SerializeField] private GameObject startTilePrefab;
        [SerializeField] private GameObject goalTilePrefab;
        [SerializeField] private GameObject redBreakableTilePrefab;
        [SerializeField] private GameObject breakableExitTilePrefab;
        [SerializeField] private GameObject yellowButtonTilePrefab;
        [SerializeField] private GameObject yellowDoorTilePrefab;
        [SerializeField] private GameObject greenJumpTilePrefab;
        [SerializeField] private GameObject jumpLandingTilePrefab;
        [SerializeField] private GameObject iceSlide1Prefab;
        [SerializeField] private GameObject iceSlide2Prefab;
        [SerializeField] private GameObject iceSlide3Prefab;
        [SerializeField] private GameObject iceLandingPrefab;
        [SerializeField] private GameObject pressurePlatePrefab;
        [SerializeField] private GameObject toggleBridgePrefab;
        [SerializeField] private GameObject oneTimeTilePrefab;

        private const string GeneratedRootName = "GeneratedTiles";

        public LevelData LevelData => levelData;

        private void Start()
        {
            if (levelData == null)
            {
                return;
            }

            if (levelManager == null)
            {
                levelManager = LevelManager.Instance != null
                    ? LevelManager.Instance
                    : FindAnyObjectByType<LevelManager>();
            }

            if (levelManager != null && levelManager.ActiveLevel != levelData)
            {
                levelManager.ConfigureLevel(levelData);
            }

            ConfigureGoalCompletionGuard();
        }

        [ContextMenu("Build Level")]
        public void BuildLevel()
        {
            if (!ValidateReferences())
            {
                return;
            }

            ClearLevel();
            Transform generatedRoot = new GameObject(GeneratedRootName).transform;
            generatedRoot.SetParent(transform, false);

            Dictionary<string, List<ButtonTile>> buttons = new();
            Dictionary<string, List<DoorTile>> doors = new();
            Dictionary<string, List<PressurePlateTile>> plates = new();
            Dictionary<string, List<ToggleBridgeTile>> bridges = new();

            foreach (LevelTileEntry entry in levelData.Tiles)
            {
                GameObject prefab = GetPrefab(entry.TileType);
                if (prefab == null)
                {
                    Debug.LogError($"No prefab assigned for {entry.TileType}.", this);
                    continue;
                }

                Vector3 position = new Vector3(
                    entry.Coordinate.x * levelData.TileSize,
                    entry.HeightLevel * levelData.TileSize - 0.1f,
                    entry.Coordinate.y * levelData.TileSize);
                GameObject tile = Instantiate(prefab, position, Quaternion.identity, generatedRoot);
                tile.name = BuildTileName(entry);
                tile.GetComponent<TileBase>()?.ConfigureGrid(entry.Coordinate, entry.HeightLevel);
                CreateHeightSupport(tile, entry);

                ButtonTile button = tile.GetComponent<ButtonTile>();
                if (button != null)
                {
                    button.ConfigureRequiredFace(entry.RequiredFace);
                    ApplyRequiredFaceTint(tile, entry.RequiredFace);
                    AddLinkedComponent(button, entry.LinkId, buttons);
                }

                DoorTile door = tile.GetComponent<DoorTile>();
                if (door != null)
                {
                    ApplyRequiredFaceTint(tile, entry.RequiredFace);
                    AddLinkedComponent(door, entry.LinkId, doors);
                }

                PressurePlateTile plate = tile.GetComponent<PressurePlateTile>();
                if (plate != null)
                {
                    plate.Configure(entry.LinkId, entry.RequiredFace, entry.PlateActivationMode, entry.SingleUse);
                    AddLinkedComponent(plate, entry.LinkId, plates);
                }

                ToggleBridgeTile bridge = tile.GetComponent<ToggleBridgeTile>();
                if (bridge != null)
                {
                    bridge.Configure(entry.LinkId, entry.StartsActive);
                    AddLinkedComponent(bridge, entry.LinkId, bridges);
                }
            }

            foreach (KeyValuePair<string, List<ButtonTile>> pair in buttons)
            {
                if (!doors.TryGetValue(pair.Key, out List<DoorTile> linkedDoors))
                {
                    Debug.LogError($"YellowButton link '{pair.Key}' has no matching YellowDoor.", this);
                    continue;
                }

                foreach (ButtonTile button in pair.Value)
                {
                    button.SetControlledDoors(linkedDoors.ToArray());
                }
            }

            foreach (KeyValuePair<string, List<PressurePlateTile>> pair in plates)
            {
                if (!bridges.TryGetValue(pair.Key, out List<ToggleBridgeTile> linkedBridges))
                {
                    Debug.LogError($"PressurePlate link '{pair.Key}' has no matching ToggleBridge.", this);
                    continue;
                }

                foreach (PressurePlateTile plate in pair.Value)
                {
                    plate.SetControlledBridges(linkedBridges.ToArray());
                }
            }

            ResetPlayer();
            if (levelManager != null)
            {
                levelManager.ConfigureLevel(levelData);
            }

            ConfigureGoalCompletionGuard();
        }

        private void CreateHeightSupport(GameObject tile, LevelTileEntry entry)
        {
            if (entry.HeightLevel <= 0 || tile == null)
            {
                return;
            }

            Renderer tileRenderer = tile.GetComponentInChildren<Renderer>();
            GameObject support = GameObject.CreatePrimitive(PrimitiveType.Cube);
            support.name = $"HeightSupport_h{entry.HeightLevel}";
            support.transform.SetParent(tile.transform.parent, false);
            support.transform.position = new Vector3(
                entry.Coordinate.x * levelData.TileSize,
                entry.HeightLevel * levelData.TileSize * 0.5f - 0.2f,
                entry.Coordinate.y * levelData.TileSize);
            support.transform.localScale = new Vector3(
                levelData.TileSize * 0.9f,
                entry.HeightLevel * levelData.TileSize,
                levelData.TileSize * 0.9f);

            Collider supportCollider = support.GetComponent<Collider>();
            if (supportCollider != null)
            {
                DestroyImmediateSafe(supportCollider);
            }

            Renderer supportRenderer = support.GetComponent<Renderer>();
            if (supportRenderer != null && tileRenderer != null)
            {
                supportRenderer.sharedMaterial = tileRenderer.sharedMaterial;
            }
        }

        private static void DestroyImmediateSafe(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        [ContextMenu("Clear Level")]
        public void ClearLevel()
        {
            Transform existing = transform.Find(GeneratedRootName);
            if (existing == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(existing.gameObject);
            }
            else
            {
                DestroyImmediate(existing.gameObject);
            }
        }

        [ContextMenu("Rebuild Level")]
        public void RebuildLevel()
        {
            ClearLevel();
            BuildLevel();
        }

        private void ResetPlayer()
        {
            if (playerTransform == null)
            {
                return;
            }

            Vector3 spawn = new Vector3(
                levelData.StartPosition.x * levelData.TileSize,
                GetStartHeight() * levelData.TileSize + 0.5f,
                levelData.StartPosition.y * levelData.TileSize);
            PlayerCubeController controller = playerTransform.GetComponent<PlayerCubeController>();
            if (controller != null)
            {
                controller.ResetForLevel(spawn);
            }
            else
            {
                playerTransform.SetPositionAndRotation(spawn, Quaternion.identity);
            }
        }

        private int GetStartHeight()
        {
            foreach (LevelTileEntry entry in levelData.Tiles)
            {
                if (entry.Coordinate == levelData.StartPosition)
                {
                    return entry.HeightLevel;
                }
            }

            return 0;
        }

        private bool ValidateReferences()
        {
            if (levelData == null)
            {
                Debug.LogError("LevelBuilder requires LevelData.", this);
                return false;
            }

            if (playerTransform == null)
            {
                Debug.LogError("LevelBuilder requires a Player Transform.", this);
                return false;
            }

            return true;
        }

        private void ConfigureGoalCompletionGuard()
        {
            LevelGoalCompletionGuard guard = GetComponent<LevelGoalCompletionGuard>();
            if (guard == null)
            {
                guard = gameObject.AddComponent<LevelGoalCompletionGuard>();
            }

            guard.Configure(levelData, playerTransform, levelManager);
        }

        private GameObject GetPrefab(LevelTileType type)
        {
            GameObject prefab = type switch
            {
                LevelTileType.Normal => normalTilePrefab,
                LevelTileType.Start => startTilePrefab,
                LevelTileType.Goal => goalTilePrefab,
                LevelTileType.RedBreakable => redBreakableTilePrefab,
                LevelTileType.BreakableExit => breakableExitTilePrefab,
                LevelTileType.YellowButton => yellowButtonTilePrefab,
                LevelTileType.YellowDoor => yellowDoorTilePrefab,
                LevelTileType.GreenJumpUp => greenJumpTilePrefab,
                LevelTileType.JumpLanding => jumpLandingTilePrefab,
                LevelTileType.IceDownStart => iceSlide1Prefab,
                LevelTileType.IceMiddle => iceSlide2Prefab != null ? iceSlide2Prefab : iceSlide3Prefab,
                LevelTileType.IceLanding => iceLandingPrefab,
                LevelTileType.PressurePlate => pressurePlatePrefab,
                LevelTileType.ToggleBridge => toggleBridgePrefab,
                LevelTileType.OneTime => oneTimeTilePrefab,
                _ => null
            };

#if UNITY_EDITOR
            if (prefab == null)
            {
                prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(GetDefaultPrefabPath(type));
            }
#endif

            return prefab;
        }

#if UNITY_EDITOR
        private static string GetDefaultPrefabPath(LevelTileType type)
        {
            return type switch
            {
                LevelTileType.PressurePlate => "Assets/Prefabs/Tiles/PressurePlateTile.prefab",
                LevelTileType.ToggleBridge => "Assets/Prefabs/Tiles/ToggleBridgeTile.prefab",
                LevelTileType.OneTime => "Assets/Prefabs/Tiles/OneTimeTile.prefab",
                _ => string.Empty
            };
        }
#endif

        private static void ApplyRequiredFaceTint(GameObject tile, RequiredCubeFace requiredFace)
        {
            if (tile == null || requiredFace == RequiredCubeFace.Any)
            {
                return;
            }

            Color color = FaceColor(requiredFace);
            Renderer[] renderers = tile.GetComponentsInChildren<Renderer>();
            foreach (Renderer tileRenderer in renderers)
            {
                if (tileRenderer == null)
                {
                    continue;
                }

#if UNITY_EDITOR
                Material material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(FaceMaterialPath(requiredFace));
                if (material != null)
                {
                    tileRenderer.sharedMaterial = material;
                    continue;
                }
#endif
                Material instance = tileRenderer.material;
                instance.color = color;
            }
        }

        private static Color FaceColor(RequiredCubeFace face)
        {
            return face switch
            {
                RequiredCubeFace.White => Color.white,
                RequiredCubeFace.Blue => new Color(0.1f, 0.35f, 1f),
                RequiredCubeFace.Red => new Color(1f, 0.12f, 0.1f),
                RequiredCubeFace.Yellow => new Color(1f, 0.82f, 0.08f),
                RequiredCubeFace.Green => new Color(0.16f, 0.82f, 0.28f),
                RequiredCubeFace.Purple => new Color(0.55f, 0.2f, 0.95f),
                _ => Color.white
            };
        }

        private static string FaceMaterialPath(RequiredCubeFace face)
        {
            return face switch
            {
                RequiredCubeFace.White => "Assets/Materials/Player/Face_White.mat",
                RequiredCubeFace.Blue => "Assets/Materials/Player/Face_Blue.mat",
                RequiredCubeFace.Red => "Assets/Materials/Player/Face_Red.mat",
                RequiredCubeFace.Yellow => "Assets/Materials/Player/Face_Yellow.mat",
                RequiredCubeFace.Green => "Assets/Materials/Player/Face_Green.mat",
                RequiredCubeFace.Purple => "Assets/Materials/Player/Face_Purple.mat",
                _ => string.Empty
            };
        }

        private static string BuildTileName(LevelTileEntry entry)
        {
            string link = string.IsNullOrWhiteSpace(entry.LinkId) ? "" : $"_{entry.LinkId}";
            return $"Tile_{entry.TileType}{link}_{entry.Coordinate.x}_{entry.Coordinate.y}_h{entry.HeightLevel}";
        }

        private static void AddLinkedComponent<T>(
            T component,
            string linkId,
            Dictionary<string, List<T>> groups) where T : Component
        {
            if (component == null || string.IsNullOrWhiteSpace(linkId))
            {
                return;
            }

            if (!groups.TryGetValue(linkId, out List<T> list))
            {
                list = new List<T>();
                groups.Add(linkId, list);
            }

            list.Add(component);
        }
    }
}
