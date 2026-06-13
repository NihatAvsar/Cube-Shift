using System.Collections.Generic;
using UnityEngine;

namespace CubeShift.LevelDesign
{
    [CreateAssetMenu(fileName = "Level_New", menuName = "Cube Shift/Level Data")]
    public sealed class LevelData : ScriptableObject
    {
        [SerializeField] private string levelName = "New Level";
        [SerializeField, Min(1)] private int width = 18;
        [SerializeField, Min(1)] private int height = 11;
        [SerializeField, Min(0.01f)] private float tileSize = 1f;
        [SerializeField] private Vector2Int startPosition;
        [SerializeField] private LevelObjective requiredObjectives = LevelObjective.RedBreakable
            | LevelObjective.YellowButton
            | LevelObjective.GreenJump
            | LevelObjective.IceSlide;
        [SerializeField] private List<LevelTileEntry> tiles = new();
        [SerializeField, TextArea(5, 16)] private string notes;
        [SerializeField, TextArea(3, 12)] private string verifiedSolution;

        public string LevelName => levelName;
        public int Width => width;
        public int Height => height;
        public float TileSize => tileSize;
        public Vector2Int StartPosition => startPosition;
        public LevelObjective RequiredObjectives => requiredObjectives;
        public IReadOnlyList<LevelTileEntry> Tiles => tiles;
        public string Notes => notes;
        public string VerifiedSolution => verifiedSolution;

#if UNITY_EDITOR
        public void SetEditorData(
            string newName,
            int newWidth,
            int newHeight,
            float newTileSize,
            Vector2Int newStart,
            LevelObjective objectives,
            List<LevelTileEntry> newTiles,
            string newNotes,
            string newSolution)
        {
            levelName = newName;
            width = newWidth;
            height = newHeight;
            tileSize = newTileSize;
            startPosition = newStart;
            requiredObjectives = objectives;
            tiles = newTiles;
            notes = newNotes;
            verifiedSolution = newSolution;
        }
#endif
    }
}
