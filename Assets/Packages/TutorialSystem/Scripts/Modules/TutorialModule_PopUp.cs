using TMPro;
using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace TUTORIAL_SYSTEM
{
    public class TutorialModule_PopUp : TutorialModule
    {
        [SerializeField] private float startDelay;

        [SerializeField, TextArea(4, 10)] private string content;
        [SerializeField, HideLabel] private string button;

        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private TextMeshProUGUI buttonText;

        [FoldoutGroup("Events")] public UnityEvent OnNextButtonClicked;
        [FoldoutGroup("Events")] public UnityEvent OnPopUpOpened;
        [FoldoutGroup("Events")] public UnityEvent OnPopUpClosed;

        public override IEnumerator ActivateModule()
        {
            var child = transform.GetChild(0).gameObject; // idk but this shit aint child 0 its this self object
            child.SetActive(false);
            yield return new WaitForSeconds(startDelay);
            child.SetActive(true);
        }

        public void CloseMe()
        {
            StopAllCoroutines();
            OnNextButtonClicked?.Invoke();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            OnPopUpOpened?.Invoke();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            OnPopUpClosed?.Invoke();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            contentText?.SetText(content);
            buttonText?.SetText(button);
        }
#endif
    }
}