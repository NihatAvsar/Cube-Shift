using CubeShift.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CubeShift.UI
{
    [RequireComponent(typeof(Selectable))]
    public sealed class UIAudioFeedback : MonoBehaviour, IPointerClickHandler, ISubmitHandler
    {
        [SerializeField] private bool backSound;

        public void SetBackSound(bool value)
        {
            backSound = value;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Play();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            Play();
        }

        private void Play()
        {
            if (backSound)
            {
                CubeShiftAudio.Instance.PlayUIBack();
            }
            else
            {
                CubeShiftAudio.Instance.PlayUIClick();
            }
        }
    }
}
