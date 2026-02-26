using UnityEngine;
using UnityEngine.Events;

namespace TouchCameraSystem
{
    [System.Serializable]
    public class UnityEventWithRaycastHit : UnityEvent<RaycastHit> { }

    [System.Serializable]
    public class UnityEventWithRaycastHit2D : UnityEvent<RaycastHit2D> { }
}