using TouchCameraSystem;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnClickToggleCameraFollow : MonoBehaviour, IPointerClickHandler
{
    private FocusCameraOnItem _focusCamera;

    private void Awake()
    {
        _focusCamera = FindFirstObjectByType<FocusCameraOnItem>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (UIManager.Instance.CurrentOpenPanel != UIManager.PanelType.None) return; // Project specifc check
        
        if (!_focusCamera ) return;

        if (_focusCamera.IsFollowing)
        {
            _focusCamera.StopFollowing();
            if (_focusCamera.FollowTarget != transform) _focusCamera.StartFollowing(transform);
        }
        else
        {
            _focusCamera.StartFollowing(transform);
        }
    }

    private void OnDestroy()
    {
        if (_focusCamera && _focusCamera.IsFollowing && _focusCamera.FollowTarget == transform) _focusCamera.StopFollowing();
    }
}