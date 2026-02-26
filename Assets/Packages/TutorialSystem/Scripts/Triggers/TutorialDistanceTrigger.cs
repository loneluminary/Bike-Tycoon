using UnityEngine;

namespace TUTORIAL_SYSTEM
{
    [System.Serializable]
    public class TutorialDistanceTrigger
    {
        public Transform A;
        public Transform B;

        public float MinDistance = 1f;
        private bool _isCompleted;

        public TutorialManager.StandardAction OnApproached;

        public void CheckDistance()
        {
            if (_isCompleted) return;
            if (A == null || B == null)
            {
                TutorialManager.Instance.DebugLog("Tutorial Distance Trigger Error: " + (A == null ? "A" : "B") + " is null !!", A != null ? A.gameObject : (B != null ? B.gameObject : null), TutorialManager.DebugType.Error);
                return;
            }
            if (Vector3.Distance(A.position, B.position) < MinDistance)
            {
                OnApproached?.Invoke();
                _isCompleted = true;
                TutorialManager.Instance.OnUpdate -= CheckDistance;
            }
        }
    }
}