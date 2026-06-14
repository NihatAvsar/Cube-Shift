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
        [SerializeField, Min(10f)] private float swipeThreshold = 55f;
        [SerializeField] private bool allowMouseSwipeInEditor = true;
        [SerializeField] private bool safeSwipeMoves;
        [SerializeField, Min(0f)] private float holdRepeatDelay = 0.06f;

        private bool pointerIsDown;
        private Vector2 pointerStartPosition;
        private Vector2 pointerLastPosition;
        private Vector2Int heldDirection;
        private float nextHoldMoveTime;

        private void Awake()
        {
            if (player == null)
            {
                player = GetComponent<PlayerCubeController>();
            }

            swipeThreshold = Mathf.Min(swipeThreshold, 55f);
            safeSwipeMoves = false;
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
                TryMoveFromInput(keyboardDirection);
                return;
            }

            if (TryReadSwipeDirection(out Vector2Int swipeDirection))
            {
                if (safeSwipeMoves)
                {
                    player.TrySafeMove(swipeDirection);
                }
                else
                {
                    player.TryMove(swipeDirection);
                }
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
                    heldDirection = Vector2Int.up;
                    nextHoldMoveTime = 0f;
                }
                else if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
                {
                    heldDirection = Vector2Int.down;
                    nextHoldMoveTime = 0f;
                }
                else if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
                {
                    heldDirection = Vector2Int.left;
                    nextHoldMoveTime = 0f;
                }
                else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
                {
                    heldDirection = Vector2Int.right;
                    nextHoldMoveTime = 0f;
                }
                else if (heldDirection == Vector2Int.zero)
                {
                    heldDirection = ReadHeldInputSystemDirection(keyboard);
                }

                if (heldDirection != Vector2Int.zero && IsInputSystemDirectionHeld(keyboard, heldDirection))
                {
                    if (Time.unscaledTime >= nextHoldMoveTime)
                    {
                        direction = heldDirection;
                        return true;
                    }

                    return false;
                }

                heldDirection = Vector2Int.zero;

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
                heldDirection = Vector2Int.up;
                nextHoldMoveTime = 0f;
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                heldDirection = Vector2Int.down;
                nextHoldMoveTime = 0f;
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                heldDirection = Vector2Int.left;
                nextHoldMoveTime = 0f;
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                heldDirection = Vector2Int.right;
                nextHoldMoveTime = 0f;
            }
            else if (heldDirection == Vector2Int.zero)
            {
                heldDirection = ReadHeldLegacyDirection();
            }

            if (heldDirection != Vector2Int.zero && IsLegacyDirectionHeld(heldDirection))
            {
                if (Time.unscaledTime >= nextHoldMoveTime)
                {
                    direction = heldDirection;
                    return true;
                }

                return false;
            }

            heldDirection = Vector2Int.zero;

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

        private void TryMoveFromInput(Vector2Int direction)
        {
            if (player.TryMove(direction))
            {
                nextHoldMoveTime = Time.unscaledTime + holdRepeatDelay;
                CancelSwipeTracking();
            }
        }

#if ENABLE_INPUT_SYSTEM
        private static bool IsInputSystemDirectionHeld(Keyboard keyboard, Vector2Int direction)
        {
            if (direction == Vector2Int.up)
            {
                return keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed;
            }

            if (direction == Vector2Int.down)
            {
                return keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed;
            }

            if (direction == Vector2Int.left)
            {
                return keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
            }

            return direction == Vector2Int.right && (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed);
        }

        private static Vector2Int ReadHeldInputSystemDirection(Keyboard keyboard)
        {
            if (keyboard == null)
            {
                return Vector2Int.zero;
            }

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                return Vector2Int.up;
            }

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                return Vector2Int.down;
            }

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                return Vector2Int.left;
            }

            return keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? Vector2Int.right : Vector2Int.zero;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        private static bool IsLegacyDirectionHeld(Vector2Int direction)
        {
            if (direction == Vector2Int.up)
            {
                return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
            }

            if (direction == Vector2Int.down)
            {
                return Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
            }

            if (direction == Vector2Int.left)
            {
                return Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
            }

            return direction == Vector2Int.right && (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow));
        }

        private static Vector2Int ReadHeldLegacyDirection()
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                return Vector2Int.up;
            }

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                return Vector2Int.down;
            }

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                return Vector2Int.left;
            }

            return Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? Vector2Int.right : Vector2Int.zero;
        }
#endif

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
