using System.Collections;
using UnityEngine;

namespace TUTORIAL_SYSTEM
{
    public class TutorialModule_Arrow : TutorialModule
    {
        [SerializeField] private TutorialManager.ArrowMovementType ArrowType;

        public Transform Target;
        [SerializeField] private Vector3 followOffset;
        [SerializeField] private float followSpeed;

        public TweenData TweenData;

        public override IEnumerator ActivateModule()
        {
            TweenData.TweenAnimation(this);
            yield return new WaitForEndOfFrame();
        }

        private void LateUpdate()
        {
            if (!IsActive) return;
            if (ArrowType == TutorialManager.ArrowMovementType.Static) return;
            if (Target == null) { TutorialManager.Instance.DebugLog("(" + gameObject.name + ") Follow Target is null", gameObject, TutorialManager.DebugType.Error); return; }

            transform.position = Vector3.Lerp(transform.position, Target.position + followOffset, Time.deltaTime * followSpeed);
        }
    }
}