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

        private const string GeneratedRootName = "GeneratedTiles";

        public LevelData LevelData => levelData;

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
                tile.name = $"Tile_{entry.TileType}_{entry.Coordinate.x}_{entry.Coordinate.y}";
                tile.GetComponent<TileBase>()?.ConfigureGrid(entry.Coordinate, entry.HeightLevel);
                CreateHeightSupport(tile, entry);

                AddLinkedComponent(tile.GetComponent<ButtonTile>(), entry.LinkId, buttons);
                AddLinkedComponent(tile.GetComponent<DoorTile>(), entry.LinkId, doors);
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

            ResetPlayer();
            if (levelManager != null)
            {
                levelManager.ConfigureLevel(levelData);
            }
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

            foreach (LevelTileEntry entry in levelData.Tiles)
            {
                if (GetPrefab(entry.TileType) == null)
                {
                    Debug.LogError($"LevelBuilder is missing the {entry.TileType} prefab.", this);
                    return false;
                }
            }

            return true;
        }

        private GameObject GetPrefab(LevelTileType type)
        {
            return type switch
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
                _ => null
            };
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
