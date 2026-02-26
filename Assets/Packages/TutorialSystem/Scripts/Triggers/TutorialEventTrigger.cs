using UnityEngine;
namespace TUTORIAL_SYSTEM
{
    public class TutorialEventTrigger : MonoBehaviour
    {
        public TutorialManager.EventType EventType = TutorialManager.EventType.ManualCall;
        public TutorialManager.StandardAction MyStandardEvent;

        private TutorialManager.StandardAction onEnable;
        private TutorialManager.StandardAction onDisable;
        private TutorialManager.StandardAction onDestroy;
        private TutorialManager.StandardAction onClick;

        private void Awake()
        {
            switch (EventType)
            {
                case TutorialManager.EventType.None:
                    EventType = TutorialManager.EventType.ManualCall;
                    break;
                case TutorialManager.EventType.OnEnable:
                    onEnable += CallMyEvent;
                    break;
                case TutorialManager.EventType.OnDisable:
                    onDisable += CallMyEvent;
                    break;
                case TutorialManager.EventType.OnDestroy:
                    onDestroy += CallMyEvent;
                    break;
                case TutorialManager.EventType.OnClick:
                    if (TryGetComponent<Collider>(out var col)) onClick += CallMyEvent;
                    else TutorialManager.Instance.DebugLog("Please add a collider! Event Trigger:" + gameObject.name, gameObject);
                    break;
                case TutorialManager.EventType.ManualCall:
                    break;
                default:
                    break;
            }
        }

        public void CallMyEvent() => MyStandardEvent?.Invoke();
        private void OnEnable() => onEnable?.Invoke();
        private void OnDisable() => onDisable?.Invoke();
        private void OnDestroy() => onDestroy?.Invoke();
        private void OnMouseDown() => onClick?.Invoke();
    }
}