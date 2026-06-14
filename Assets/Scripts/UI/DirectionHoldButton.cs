using CubeShift.Player;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CubeShift.UI
{
    public sealed class DirectionHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField, Min(0f)] private float repeatDelay = 0.06f;

        private PlayerCubeController player;
        private Vector2Int direction;
        private bool holding;
        private float nextMoveTime;

        public void Configure(PlayerCubeController targetPlayer, Vector2Int moveDirection)
        {
            player = targetPlayer;
            direction = moveDirection;
        }

        private void Update()
        {
            if (!holding)
            {
                return;
            }

            if (player == null)
            {
                player = FindAnyObjectByType<PlayerCubeController>();
            }

            if (player == null || !player.CanReceiveInput || Time.unscaledTime < nextMoveTime)
            {
                return;
            }

            if (player.TryMove(direction))
            {
                nextMoveTime = Time.unscaledTime + repeatDelay;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            holding = true;
            nextMoveTime = 0f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            holding = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            holding = false;
        }
    }
}
