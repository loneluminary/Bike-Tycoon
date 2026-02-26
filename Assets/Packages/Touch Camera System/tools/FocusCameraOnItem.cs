using UnityEngine;
using DG.Tweening;

namespace TouchCameraSystem
{
    [RequireComponent(typeof(MobileTouchCamera))]
    public class FocusCameraOnItem : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField] private float followSmoothTime = 0.3f;

        private MobileTouchCamera _camera;

        // Follow state
        private Transform _followTarget;
        private bool _isFollowing;
        private Vector3 _followOffset;
        private float _followZoom;
        private bool _followRotation;

        // Active tweens
        private Tween _positionTween;
        private Tween _rotationTween;
        private Tween _zoomTween;
        private Tween _followTween;

        public bool IsFollowing => _isFollowing;
        public Transform FollowTarget => _followTarget;

        private void Awake()
        {
            _camera = GetComponent<MobileTouchCamera>();
        }

        private void LateUpdate()
        {
            // Stop follow if user interacts
            if (_isFollowing && (_camera.IsDragging || _camera.IsPinching)) StopFollowing();
        }

        /// Focus camera on target position with smooth transition.
        /// Returns a Sequence tween for chaining (.SetEase(), .OnComplete(), etc.)
        public Sequence FocusCameraOnTarget(Vector3 targetPosition, Quaternion targetRotation = default, float targetZoom = 0f, float duration = 1f)
        {
            StopFollowing();
            KillActiveTweens();

            var inputs = _camera.GetComponent<TouchInputController>();
            inputs.enabled = false; // Disable camera inputs while focusing 

            // Defaults
            if (targetRotation == Quaternion.identity) targetRotation = transform.rotation;
            if (targetZoom <= 0f) targetZoom = _camera.CamZoom;

            var rotTransitionStart = transform.rotation;
            float zoomTransitionStart = _camera.CamZoom;

            // Calculate target camera position
            _camera.transform.rotation = targetRotation;
            _camera.CamZoom = targetZoom;

            Vector3 screenCenter = new(Screen.width * 0.5f, Screen.height * 0.5f, 0);
            Vector3 intersection = _camera.GetIntersectionPoint(_camera.Cam.ScreenPointToRay(screenCenter));
            Vector3 focusVector = targetPosition - intersection;
            Vector3 targetCamPosition = _camera.GetClampToBoundaries(transform.position + focusVector, true);

            // Reset for transition
            _camera.transform.rotation = rotTransitionStart;
            _camera.CamZoom = zoomTransitionStart;

            // Create smooth transition sequence
            Sequence sequence = DOTween.Sequence().SetEase(Ease.InOutSine).OnComplete(() => inputs.enabled = true);

            _positionTween = transform.DOMove(targetCamPosition, duration);
            _rotationTween = transform.DORotateQuaternion(targetRotation, duration);
            _zoomTween = DOTween.To(() => _camera.CamZoom, x => _camera.CamZoom = x, targetZoom, duration);

            sequence.Append(_positionTween);
            sequence.Join(_rotationTween);
            sequence.Join(_zoomTween);

            return sequence;
        }

        /// Start following a target continuously.
        /// Returns a Tween for the follow motion (can be killed to stop).
        public Tween StartFollowing(Transform target, Vector3? offset = null, float zoom = 0f, bool followRot = false)
        {
            if (target == null)
            {
                Debug.LogWarning("Cannot follow null target!");
                return null;
            }

            KillActiveTweens();

            _followTarget = target;
            _isFollowing = true;
            _followOffset = offset ?? Vector3.zero;
            _followZoom = zoom;
            _followRotation = followRot;

            // Create continuous follow tween
            _followTween = DOTween.To(() => 0f, _ => UpdateFollowPosition(), 1f, followSmoothTime).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);

            return _followTween;
        }

        /// Stop following current target.
        public void StopFollowing()
        {
            if (!_isFollowing) return;

            _isFollowing = false;
            _followTarget = null;
            _followTween?.Kill();
            _followTween = null;
        }

        private void UpdateFollowPosition()
        {
            if (_followTarget == null)
            {
                StopFollowing();
                return;
            }

            // Set zoom
            if (_followZoom > 0f) _camera.CamZoom = _followZoom;

            // Calculate target position
            Vector3 screenCenter = new(Screen.width * 0.5f, Screen.height * 0.5f, 0);
            Vector3 intersection = _camera.GetIntersectionPoint(_camera.Cam.ScreenPointToRay(screenCenter));
            Vector3 focusVector = _followTarget.position - intersection;
            Vector3 targetPosition = _camera.GetClampToBoundaries(transform.position + focusVector + _followOffset, true);

            // Smooth move to target
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime / followSmoothTime);

            // Optional rotation follow
            if (_followRotation) transform.rotation = Quaternion.Slerp(transform.rotation, _followTarget.rotation, Time.deltaTime / followSmoothTime);
        }

        private void KillActiveTweens()
        {
            _positionTween?.Kill();
            _rotationTween?.Kill();
            _zoomTween?.Kill();
            _followTween?.Kill();

            _positionTween = null;
            _rotationTween = null;
            _zoomTween = null;
            _followTween = null;
        }

        private void OnDestroy() => KillActiveTweens();

        private void OnDrawGizmos()
        {
            if (_isFollowing && _followTarget)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, _followTarget.position);
                Gizmos.DrawWireSphere(_followTarget.position, 0.5f);
            }
        }
    }
}