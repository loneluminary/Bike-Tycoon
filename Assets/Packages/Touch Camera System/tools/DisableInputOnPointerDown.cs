using UnityEngine;
using UnityEngine.EventSystems;

namespace TouchCameraSystem
{
    /// Helper class that will block Mobile Touch Camera Input while the pointer is over the
    /// UI element that this script is added to. You can add it for example to a Unity UI Button.
    /// Usage: Add to a Unity UI object that you want to block the touch camera from moving when clicked.
    public class DisableInputOnPointerDown : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private TouchInputController touchInputController;

        public void Reset()
        {
            touchInputController = FindObjectOfType<TouchInputController>();
        }

        public void Awake()
        {
            if (!touchInputController)
            {
                touchInputController = FindObjectOfType<TouchInputController>();
                if (touchInputController == null)
                {
                    Debug.LogError("Failed to find TouchInputController. Make sure the reference is assigned via inspector or by ensuring the Find method works.", gameObject);
                    this.enabled = false;
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            touchInputController?.OnEventTriggerPointerDown(eventData);
        }
    }
}