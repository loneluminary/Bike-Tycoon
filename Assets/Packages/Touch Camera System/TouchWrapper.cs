using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

namespace TouchCameraSystem
{
    public static class TouchWrapper
    {
        public static int TouchCount
        {
            get
            {
                var screen = Touchscreen.current;
                var mouse = Mouse.current;

                // Check for actual finger touches first
                if (screen != null)
                {
                    int activeTouches = 0;
                    foreach (var touch in screen.touches)
                    {
                        // Check if the touch is currently active/pressed
                        if (touch.press.isPressed) activeTouches++;
                    }
                    if (activeTouches > 0) return activeTouches;
                }

                // Fallback to mouse for Editor/Standalone
                if (mouse != null && mouse.leftButton.isPressed) return 1;

                return 0;
            }
        }

        public static WrappedTouch Touch0
        {
            get
            {
                var screen = Touchscreen.current;
                var mouse = Mouse.current;

                if (screen != null)
                {
                    // Find the first active touch slot
                    var firstTouch = screen.touches.FirstOrDefault(t => t.press.isPressed);
                    if (firstTouch != null)
                    {
                        return WrappedTouch.FromTouchControl(firstTouch);
                    }
                }

                // Fallback to mouse
                if (mouse != null && mouse.leftButton.isPressed)
                {
                    return new WrappedTouch()
                    {
                        Position = mouse.position.ReadValue(),
                        FingerId = 0
                    };
                }

                return null;
            }
        }

        public static bool IsFingerDown => TouchCount > 0;

        public static List<WrappedTouch> Touches
        {
            get
            {
                var screen = Touchscreen.current;
                List<WrappedTouch> wrappedTouches = new();

                if (screen != null)
                {
                    foreach (var touch in screen.touches)
                    {
                        if (touch.press.isPressed)
                        {
                            wrappedTouches.Add(WrappedTouch.FromTouchControl(touch));
                        }
                    }
                }

                // If no touches but mouse is pressed, treat mouse as Touch0
                if (wrappedTouches.Count == 0 && TouchCount > 0) wrappedTouches.Add(Touch0);

                return wrappedTouches;
            }
        }

        public static Vector2 AverageTouchPos
        {
            get
            {
                var screen = Touchscreen.current;
                var mouse = Mouse.current;

                if (screen != null)
                {
                    Vector2 sum = Vector2.zero;
                    int count = 0;
                    foreach (var touch in screen.touches)
                    {
                        if (touch.press.isPressed)
                        {
                            sum += touch.position.ReadValue();
                            count++;
                        }
                    }

                    if (count > 0) return sum / count;
                }

                return mouse != null ? mouse.position.ReadValue() : Vector2.zero;
            }
        }
    }

    public class WrappedTouch
    {
        public Vector3 Position { get; set; }
        public int FingerId { get; set; } = -1;

        public static WrappedTouch FromTouchControl(UnityEngine.InputSystem.Controls.TouchControl touch)
        {
            return new WrappedTouch
            {
                Position = touch.position.ReadValue(),
                // Using touchId as it persists while the finger is down
                FingerId = touch.touchId.ReadValue()
            };
        }
    }
}