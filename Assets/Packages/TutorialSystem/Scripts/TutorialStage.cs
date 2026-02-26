using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TUTORIAL_SYSTEM
{
    [Serializable]
    public class TutorialStage : MonoBehaviour
    {
        [ReadOnly, HideLabel, HorizontalGroup("Info")] public string TutorialStageName;
        [ReadOnly, HideLabel, HorizontalGroup("Info", Width = 50f)] public int StageIndex;

        [TabGroup("Group_1", "Start Trgger")][SerializeField] float startDelay;
        [TabGroup("Group_1", "Start Trgger")] public TutorialManager.TriggerType StageStartTrigger = TutorialManager.TriggerType.AutomaticRun;
        [TabGroup("Group_1", "Start Trgger")][ShowIf("StageStartTrigger", TutorialManager.TriggerType.Collision)] public TutorialColliderTrigger StartCollisionTarget;
        [TabGroup("Group_1", "Start Trgger")][ShowIf("StageStartTrigger", TutorialManager.TriggerType.ButtonClick)] public Button StartButtonTarget;
        [TabGroup("Group_1", "Start Trgger")][ShowIf("StageStartTrigger", TutorialManager.TriggerType.ToggleChange)] public Toggle StartToggleTarget;
        [TabGroup("Group_1", "Start Trgger")][ShowIf("StageStartTrigger", TutorialManager.TriggerType.Event)] public TutorialEventTrigger StartEventTriggerTarget;
        [TabGroup("Group_1", "Start Trgger")][ShowIf("StageStartTrigger", TutorialManager.TriggerType.Distance)] public TutorialDistanceTrigger StartDistanceTriggerTarget;

        [TabGroup("Group_1", "End Trgger")][SerializeField] float endDelay;
        [TabGroup("Group_1", "End Trgger")] public TutorialManager.TriggerType StageEndTrigger;
        [TabGroup("Group_1", "End Trgger")][ShowIf("StageEndTrigger", TutorialManager.TriggerType.Collision)] public TutorialColliderTrigger EndCollisionTarget;
        [TabGroup("Group_1", "End Trgger")][ShowIf("StageEndTrigger", TutorialManager.TriggerType.ButtonClick)] public Button EndButtonTarget;
        [TabGroup("Group_1", "End Trgger")][ShowIf("StageEndTrigger", TutorialManager.TriggerType.ToggleChange)] public Toggle EndToggleTarget;
        [TabGroup("Group_1", "End Trgger")][ShowIf("StageEndTrigger", TutorialManager.TriggerType.Event)] public TutorialEventTrigger EndEventTriggerTarget;
        [TabGroup("Group_1", "End Trgger")][ShowIf("StageEndTrigger", TutorialManager.TriggerType.Distance)] public TutorialDistanceTrigger EndDistanceTriggerTarget;

        public List<TutorialModule> MyModules = new();

        [FoldoutGroup("Events")] public UnityEvent OnStageStart;
        [FoldoutGroup("Events")] public UnityEvent OnStageEnd;

        [HideInInspector] public int TutorialIndex;
        [HideInInspector] public bool IsStageCompleted;

        private static readonly WaitForSeconds _waitForSeconds0_2 = new(0.2f);

        public void Init(int tutorialIndex, int stageIndex)
        {
            TutorialIndex = tutorialIndex;
            StageIndex = stageIndex;

            ModulesActiveStatus(false);
        }

        public void StartTheStage()
        {
            switch (StageStartTrigger)
            {
                case TutorialManager.TriggerType.AutomaticRun:
                    StageStarted();
                    break;
                case TutorialManager.TriggerType.Collision:
                    if (EndCollisionTarget != null) StartCollisionTarget.Initialize(TutorialIndex, StageIndex, true);
                    break;
                case TutorialManager.TriggerType.ButtonClick:
                    if (EndButtonTarget) StartButtonTarget.onClick.AddListener(StageStarted);
                    break;
                case TutorialManager.TriggerType.ToggleChange:
                    if (EndToggleTarget) StartToggleTarget.onValueChanged.AddListener(isOn => { if (isOn) StageStarted(); });
                    break;
                case TutorialManager.TriggerType.Event:
                    if (EndEventTriggerTarget) StartEventTriggerTarget.MyStandardEvent += StageStarted;
                    break;
                case TutorialManager.TriggerType.Distance:
                    if (EndDistanceTriggerTarget != null)
                    {
                        TutorialManager.Instance.OnUpdate -= StartDistanceTriggerTarget.CheckDistance;
                        TutorialManager.Instance.OnUpdate += StartDistanceTriggerTarget.CheckDistance;
                        StartDistanceTriggerTarget.OnApproached += StageStarted;
                    }
                    break;
            }

            switch (StageEndTrigger)
            {
                case TutorialManager.TriggerType.AutomaticRun:
                    StageCompleted();
                    break;
                case TutorialManager.TriggerType.Collision:
                    if (EndCollisionTarget != null) EndCollisionTarget.Initialize(TutorialIndex, StageIndex, false);
                    break;
                case TutorialManager.TriggerType.ButtonClick:
                    if (EndButtonTarget) EndButtonTarget.onClick.AddListener(StageCompleted);
                    break;
                case TutorialManager.TriggerType.ToggleChange:
                    if (EndToggleTarget) EndToggleTarget.onValueChanged.AddListener(isOn => { if (isOn) StageCompleted(); });
                    break;
                case TutorialManager.TriggerType.Event:
                    if (EndEventTriggerTarget) EndEventTriggerTarget.MyStandardEvent += StageCompleted;
                    break;
                case TutorialManager.TriggerType.Distance:
                    if (EndDistanceTriggerTarget != null)
                    {
                        TutorialManager.Instance.OnUpdate -= EndDistanceTriggerTarget.CheckDistance;
                        TutorialManager.Instance.OnUpdate += EndDistanceTriggerTarget.CheckDistance;
                        EndDistanceTriggerTarget.OnApproached += StageCompleted;
                    }
                    break;
            }
        }

        public void StageStarted()
        {
            TutorialManager.Instance.StartCoroutine(StageStartedEnum());
        }

        public void StageCompleted()
        {
            TutorialManager.Instance.StartCoroutine(StageCompletedEnum());
        }

        private IEnumerator StageStartedEnum()
        {
            if (IsStageCompleted) { TutorialManager.Instance.Tutorials[TutorialIndex].NextStage(); yield break; }
            yield return new WaitForSeconds(startDelay);

            TutorialManager.Instance.DebugLog("Stage: " + TutorialStageName + " started", gameObject, TutorialManager.DebugType.Info);
            OnStageStart?.Invoke();

            MyModules.RemoveAll(m => m == null);


            yield return _waitForSeconds0_2; // small delay before activating the modules to avoid issues with ui initialization

            ModulesActiveStatus(true);
        }

        private IEnumerator StageCompletedEnum()
        {
            yield return new WaitForSeconds(endDelay + 0.05f);

            if (IsStageCompleted) yield break;

            IsStageCompleted = true;

            TutorialManager.Instance.DebugLog("Stage: " + TutorialStageName + " completed", gameObject, TutorialManager.DebugType.Successful);
            OnStageEnd?.Invoke();

            ModulesActiveStatus(false);

            TutorialManager.Instance.Tutorials[TutorialIndex].NextStage();
        }

        public void ModulesActiveStatus(bool active)
        {
            for (int i = 0; i < MyModules.Count; i++)
            {
                if (MyModules[i] != null) MyModules[i].gameObject.SetActive(active);
            }
        }
    }
}