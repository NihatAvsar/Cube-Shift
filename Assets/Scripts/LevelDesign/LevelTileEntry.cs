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
        [SerializeField] private RequiredCubeFace requiredFace = RequiredCubeFace.Any;
        [SerializeField] private PlateActivationMode plateActivationMode = PlateActivationMode.ActivateOnly;
        [SerializeField] private bool startsActive = true;
        [SerializeField] private bool singleUse;
        [SerializeField, TextArea(1, 4)] private string note;

        public Vector2Int Coordinate => coordinate;
        public LevelTileType TileType => tileType;
        public string LinkId => linkId;
        public int HeightLevel => heightLevel;
        public RequiredCubeFace RequiredFace => requiredFace;
        public PlateActivationMode PlateActivationMode => plateActivationMode;
        public bool StartsActive => startsActive;
        public bool SingleUse => singleUse;
        public string Note => note;

        public LevelTileEntry(
            Vector2Int coordinate,
            LevelTileType tileType,
            string linkId = "",
            int heightLevel = 0,
            RequiredCubeFace requiredFace = RequiredCubeFace.Any,
            PlateActivationMode plateActivationMode = PlateActivationMode.ActivateOnly,
            bool startsActive = true,
            bool singleUse = false,
            string note = "")
        {
            this.coordinate = coordinate;
            this.tileType = tileType;
            this.linkId = linkId;
            this.heightLevel = Mathf.Max(0, heightLevel);
            this.requiredFace = requiredFace;
            this.plateActivationMode = plateActivationMode;
            this.startsActive = startsActive;
            this.singleUse = singleUse;
            this.note = note;
        }
    }
}
