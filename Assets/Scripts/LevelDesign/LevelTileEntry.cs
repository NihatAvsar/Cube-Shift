using System;
using UnityEngine;

namespace CubeShift.LevelDesign
{
    [Serializable]
    public sealed class LevelTileEntry
    {
        [SerializeField] private Vector2Int coordinate;
        [SerializeField] private LevelTileType tileType;
        [SerializeField] private string linkId;
        [SerializeField, Min(0)] private int heightLevel;

        public Vector2Int Coordinate => coordinate;
        public LevelTileType TileType => tileType;
        public string LinkId => linkId;
        public int HeightLevel => heightLevel;

        public LevelTileEntry(Vector2Int coordinate, LevelTileType tileType, string linkId = "", int heightLevel = 0)
        {
            this.coordinate = coordinate;
            this.tileType = tileType;
            this.linkId = linkId;
            this.heightLevel = Mathf.Max(0, heightLevel);
        }
    }
}
