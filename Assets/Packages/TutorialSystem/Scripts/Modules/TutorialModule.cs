using System.Collections;
using UnityEngine;

namespace TUTORIAL_SYSTEM
{
    public abstract class TutorialModule : MonoBehaviour
    {
        protected bool IsActive;

        protected virtual void OnEnable()
        {
            IsActive = true;
            ActiveTheModule();
        }

        protected virtual void OnDisable()
        {
            IsActive = false;
        }

        public void ActiveTheModule() => StartCoroutine(ActivateModule());

        public abstract IEnumerator ActivateModule();
    }
}