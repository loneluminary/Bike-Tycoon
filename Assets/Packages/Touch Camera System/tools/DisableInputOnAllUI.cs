using UnityEngine;
using UnityEngine.EventSystems;

namespace TouchCameraSystem
{
    /// Helper class that will block Mobile Touch Camera Input while the pointer is over any
    /// UI element that has the RaycastTarget checkbox activated. Using this script may yield too many false blockings.
    /// It's more precise to mark each necessary UI element as described
    /// in the documentation for the asset.
    /// Usage: Add this to just one active GameObject in your scene. For example to your main camera or to a stand-alone GameObject that's always activated.
    [DefaultExecutionOrder(-10)]
    public class DisableInputOnAllUI : MonoBehaviour
    {
        [SerializeField] private TouchInputController touchInputController;

        private bool wasTouchingLastFrame;

        public void Reset()
        {
            touchInputController = FindFirstObjectByType<TouchInputController>();
        }

        public void Awake()
        {
            if (!touchInputController)
            {
                touchInputController = FindFirstObjectByType<TouchInputController>();
                if (!touchInputController)
                {
                    Debug.LogError("Failed to find TouchInputController. Make sure the reference is assigned via inspector or by ensuring the Find method works.");
                    enabled = false;
                }
            }
        }

        public void Update()
        {
            if (IsPointerOverGameObject())
            {
                if (TouchWrapper.IsFingerDown && !wasTouchingLastFrame)
                {
                    touchInputController.IsInputOnLockedArea = true;
                }
            }

            wasTouchingLastFrame = TouchWrapper.IsFingerDown;
        }

        private bool IsPointerOverGameObject()
        {
            if (!EventSystem.current) return false;

            // Check mouse
            if (EventSystem.current.IsPointerOverGameObject()) return true;

            // Check touches
            for (int i = 0; i < Input.touchCount; i++)
            {
                var touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began)
                {
                    if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}