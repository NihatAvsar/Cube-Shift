using CubeShift.Core;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CubeShift.Player
{
    /// <summary>
    /// Reads keyboard and swipe input, then forwards a single grid direction to PlayerCubeController.
    /// The controller owns the input lock, so this class stays focused on translating input only.
    /// </summary>
    [RequireComponent(typeof(PlayerCubeController))]
    public sealed class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] private PlayerCubeController player;
        [SerializeField, Min(10f)] private float swipeThreshold = 80f;
        [SerializeField] private bool allowMouseSwipeInEditor = true;

        private bool pointerIsDown;
        private Vector2 pointerStartPosition;
        private Vector2 pointerLastPosition;

        private void Awake()
        {
            if (player == null)
            {
                player = GetComponent<PlayerCubeController>();
            }
        }

        private void Update()
        {
            if (player == null)
            {
                return;
            }

            if (!player.CanReceiveInput)
            {
                CancelSwipeTracking();
                return;
            }

            if (TryReadKeyboardDirection(out Vector2Int keyboardDirection))
            {
                if (player.TryMove(keyboardDirection))
                {
                    CancelSwipeTracking();
                }

                return;
            }

            if (TryReadSwipeDirection(out Vector2Int swipeDirection))
            {
                player.TryMove(swipeDirection);
            }
        }

        private bool TryReadKeyboardDirection(out Vector2Int direction)
        {
            direction = Vector2Int.zero;

#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
                {
                    direction = Vector2Int.up;
                    return true;
                }

                if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
                {
                    direction = Vector2Int.down;
                    return true;
                }

                if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
                {
                    direction = Vector2Int.left;
                    return true;
                }

                if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
                {
                    direction = Vector2Int.right;
                    return true;
                }
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                direction = Vector2Int.up;
                return true;
            }

            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                direction = Vector2Int.down;
                return true;
            }

            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                direction = Vector2Int.left;
                return true;
            }

            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                direction = Vector2Int.right;
                return true;
            }
#endif

            return false;
        }

        private bool TryReadSwipeDirection(out Vector2Int direction)
        {
            direction = Vector2Int.zero;

#if ENABLE_INPUT_SYSTEM
            if (TryReadInputSystemTouch(out direction))
            {
                return true;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (allowMouseSwipeInEditor && TryReadInputSystemMouse(out direction))
            {
                return true;
            }
#endif
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (TryReadLegacyTouch(out direction))
            {
                return true;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (allowMouseSwipeInEditor && TryReadLegacyMouse(out direction))
            {
                return true;
            }
#endif
#endif

            return false;
        }

#if ENABLE_INPUT_SYSTEM
        private bool TryReadInputSystemTouch(out Vector2Int direction)
        {
            direction = Vector2Int.zero;

            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen == null)
            {
                return false;
            }

            bool isPressed = touchscreen.primaryTouch.press.isPressed;
            Vector2 position = touchscreen.primaryTouch.position.ReadValue();
            return UpdateSwipeState(isPressed, position, out direction);
        }

        private bool TryReadInputSystemMouse(out Vector2Int direction)
        {
            direction = Vector2Int.zero;

            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return false;
            }

            bool isPressed = mouse.leftButton.isPressed;
            Vector2 position = mouse.position.ReadValue();
            return UpdateSwipeState(isPressed, position, out direction);
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        private bool TryReadLegacyTouch(out Vector2Int direction)
        {
            direction = Vector2Int.zero;

            if (Input.touchCount <= 0)
            {
                return false;
            }

            Touch touch = Input.GetTouch(0);
            bool isPressed = touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled;
            return UpdateSwipeState(isPressed, touch.position, out direction);
        }

        private bool TryReadLegacyMouse(out Vector2Int direction)
        {
            bool isPressed = Input.GetMouseButton(0);
            return UpdateSwipeState(isPressed, Input.mousePosition, out direction);
        }
#endif

        private bool UpdateSwipeState(bool isPressed, Vector2 position, out Vector2Int direction)
        {
            direction = Vector2Int.zero;

            if (isPressed && !pointerIsDown)
            {
                pointerIsDown = true;
                pointerStartPosition = position;
                pointerLastPosition = position;
                return false;
            }

            if (isPressed)
            {
                pointerLastPosition = position;
                return false;
            }

            if (!pointerIsDown)
            {
                return false;
            }

            pointerIsDown = false;
            Vector2 swipeDelta = pointerLastPosition - pointerStartPosition;
            return TryConvertSwipeToDirection(swipeDelta, out direction);
        }

        private bool TryConvertSwipeToDirection(Vector2 swipeDelta, out Vector2Int direction)
        {
            return GridDirectionUtility.TryFromSwipeDelta(swipeDelta, swipeThreshold, out direction);
        }

        private void CancelSwipeTracking()
        {
            pointerIsDown = false;
            pointerStartPosition = Vector2.zero;
            pointerLastPosition = Vector2.zero;
        }
    }
}
