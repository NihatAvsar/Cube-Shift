using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CubeShift.UI
{
    public sealed class LevelButtonUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text numberText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text starsText;
        [SerializeField] private Image background;
        [SerializeField] private Button button;
        [SerializeField] private UIButtonAnimator animator;

        private int levelNumber;
        private bool locked;
        private string baseStatus;
        private Color baseColor;
        private readonly Color selectedColor = new(0.04f, 0.58f, 0.72f, 1f);

        public int LevelNumber => levelNumber;
        public bool Locked => locked;

        public void Configure(int number, bool isLocked, bool completed, int stars, bool lastPlayed, float bestTime = 0f, int bestMoves = 0)
        {
            levelNumber = number;
            locked = isLocked;
            if (numberText != null)
            {
                numberText.text = number.ToString("00");
            }

            if (statusText != null)
            {
                baseStatus = isLocked ? "LOCKED" : lastPlayed ? "LAST" : completed ? "DONE" : "OPEN";
                statusText.text = baseStatus;
            }

            if (starsText != null)
            {
                if (completed)
                {
                    string timeText = bestTime > 0f ? $"{bestTime:0}s" : "--";
                    string movesText = bestMoves > 0 ? bestMoves.ToString() : "--";
                    starsText.text = $"Rank {FormatRating(stars)}  {timeText}/{movesText}";
                }
                else
                {
                    starsText.text = "---";
                }
            }

            if (button != null)
            {
                button.interactable = !isLocked;
            }

            Color normal = isLocked ? new Color(0.08f, 0.1f, 0.13f, 0.72f) : new Color(0.07f, 0.15f, 0.2f, 0.9f);
            Color highlight = isLocked ? normal : new Color(0.1f, 0.33f, 0.42f, 0.95f);
            baseColor = normal;
            if (background != null)
            {
                background.color = normal;
            }

            if (animator != null)
            {
                animator.SetColors(normal, highlight, selectedColor);
            }
        }

        public void SetSelected(bool selected)
        {
            if (statusText != null && !locked)
            {
                statusText.text = selected ? "SELECTED" : baseStatus;
            }

            if (background != null)
            {
                background.color = selected && !locked ? selectedColor : baseColor;
            }

            if (animator != null)
            {
                animator.SetSelectedVisual(selected && !locked);
            }
        }

        public void Bind(Button targetButton, Image targetBackground, TMP_Text number, TMP_Text status, TMP_Text stars, UIButtonAnimator targetAnimator)
        {
            button = targetButton;
            background = targetBackground;
            numberText = number;
            statusText = status;
            starsText = stars;
            animator = targetAnimator;
        }

        private static string FormatRating(int stars)
        {
            return Mathf.Clamp(stars, 1, 3) switch
            {
                3 => "S",
                2 => "A",
                _ => "B"
            };
        }
    }
}
