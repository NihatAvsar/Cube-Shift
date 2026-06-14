using CubeShift.Core;
using UnityEngine;

namespace CubeShift.Player
{
    public enum CubeFace
    {
        White,
        Blue,
        Red,
        Yellow,
        Green,
        Purple
    }

    public struct CubeFaceOrientation
    {
        public CubeFace Top;
        public CubeFace Bottom;
        public CubeFace Front;
        public CubeFace Back;
        public CubeFace Left;
        public CubeFace Right;
    }

    /// <summary>
    /// Tracks the logical cube-face orientation independently from the visual mesh rotation.
    /// Tile mechanics can read CurrentBottomFace to decide which effect is active.
    /// </summary>
    public sealed class CubeFaceTracker : MonoBehaviour
    {
        [Header("Current Orientation")]
        [SerializeField] private CubeFace currentTopFace = CubeFace.Blue;
        [SerializeField] private CubeFace currentBottomFace = CubeFace.White;
        [SerializeField] private CubeFace currentFrontFace = CubeFace.Green;
        [SerializeField] private CubeFace currentBackFace = CubeFace.Purple;
        [SerializeField] private CubeFace currentLeftFace = CubeFace.Red;
        [SerializeField] private CubeFace currentRightFace = CubeFace.Yellow;

        public CubeFace CurrentTopFace => currentTopFace;
        public CubeFace CurrentBottomFace => currentBottomFace;
        public CubeFace CurrentFrontFace => currentFrontFace;
        public CubeFace CurrentBackFace => currentBackFace;
        public CubeFace CurrentLeftFace => currentLeftFace;
        public CubeFace CurrentRightFace => currentRightFace;

        public CubeFaceOrientation CaptureOrientation()
        {
            return new CubeFaceOrientation
            {
                Top = currentTopFace,
                Bottom = currentBottomFace,
                Front = currentFrontFace,
                Back = currentBackFace,
                Left = currentLeftFace,
                Right = currentRightFace
            };
        }

        public void RestoreOrientation(CubeFaceOrientation orientation)
        {
            currentTopFace = orientation.Top;
            currentBottomFace = orientation.Bottom;
            currentFrontFace = orientation.Front;
            currentBackFace = orientation.Back;
            currentLeftFace = orientation.Left;
            currentRightFace = orientation.Right;
        }

        public void Roll(Vector2Int gridDirection)
        {
            if (!GridDirectionUtility.TryNormalize(gridDirection, out Vector2Int direction))
            {
                return;
            }

            if (direction.y > 0)
            {
                RollForward();
            }
            else if (direction.y < 0)
            {
                RollBackward();
            }
            else if (direction.x < 0)
            {
                RollLeft();
            }
            else if (direction.x > 0)
            {
                RollRight();
            }
        }

        [ContextMenu("Reset Default Orientation")]
        public void ResetDefaultOrientation()
        {
            currentTopFace = CubeFace.Blue;
            currentBottomFace = CubeFace.White;
            currentFrontFace = CubeFace.Green;
            currentBackFace = CubeFace.Purple;
            currentLeftFace = CubeFace.Red;
            currentRightFace = CubeFace.Yellow;
        }

        private void RollForward()
        {
            CubeFace oldTop = currentTopFace;
            CubeFace oldBottom = currentBottomFace;
            CubeFace oldFront = currentFrontFace;
            CubeFace oldBack = currentBackFace;

            currentTopFace = oldBack;
            currentBottomFace = oldFront;
            currentFrontFace = oldTop;
            currentBackFace = oldBottom;
        }

        private void RollBackward()
        {
            CubeFace oldTop = currentTopFace;
            CubeFace oldBottom = currentBottomFace;
            CubeFace oldFront = currentFrontFace;
            CubeFace oldBack = currentBackFace;

            currentTopFace = oldFront;
            currentBottomFace = oldBack;
            currentFrontFace = oldBottom;
            currentBackFace = oldTop;
        }

        private void RollLeft()
        {
            CubeFace oldTop = currentTopFace;
            CubeFace oldBottom = currentBottomFace;
            CubeFace oldLeft = currentLeftFace;
            CubeFace oldRight = currentRightFace;

            currentTopFace = oldRight;
            currentBottomFace = oldLeft;
            currentLeftFace = oldTop;
            currentRightFace = oldBottom;
        }

        private void RollRight()
        {
            CubeFace oldTop = currentTopFace;
            CubeFace oldBottom = currentBottomFace;
            CubeFace oldLeft = currentLeftFace;
            CubeFace oldRight = currentRightFace;

            currentTopFace = oldLeft;
            currentBottomFace = oldRight;
            currentLeftFace = oldBottom;
            currentRightFace = oldTop;
        }
    }
}
