using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace TUTORIAL_SYSTEM
{
    [System.Serializable]
    public class Tutorial : MonoBehaviour
    {
        [ReadOnly, HideLabel, HorizontalGroup("Info")] public string TutorialName;
        [ReadOnly, ShowInInspector, HideLabel, HorizontalGroup("Info", Width = 50f)] public int TutorialIndex { get; private set; }

        [ReadOnly] public List<TutorialStage> Stages = new();

        [FoldoutGroup("Events")] public UnityEvent OnThisTutorialStarted;
        [FoldoutGroup("Events")] public UnityEvent OnThisTutorialCompleted;

        [HideInInspector] public bool IsTutorialCompleted;
        [HideInInspector] public int CurrentStageIndex;

        public void Init(int tutorialIndex)
        {
            IsTutorialCompleted = false;

            Stages = GetComponentsInChildren<TutorialStage>().ToList();

            TutorialIndex = tutorialIndex;
            for (int i = 0; i < Stages.Count; i++)
            {
                Stages[i].Init(TutorialIndex, i);
            }
        }

        public void StartTheTutorial()
        {
            if (IsTutorialCompleted)
            {
                TutorialManager.Instance.ShowNextTutorial();
                return;
            }
            OnThisTutorialStarted?.Invoke();
            CurrentStageIndex = 0;
            NextStage();
        }

        public void NextStage()
        {
            if (CurrentStageIndex > Stages.Count - 1)
            {
                FinishTheTutorial();
                return;
            }

            CurrentStageIndex++;
            Stages[CurrentStageIndex - 1].StartTheStage();


            if (TutorialManager.Instance.SaveTutorialProgress && TutorialManager.Instance.SaveStageProgress)
                PlayerPrefs.SetInt("ns_savedStageIndex_" + TutorialIndex + "_" + TutorialManager.Instance.SaveKey, CurrentStageIndex - 1 <= 0 ? 0 : CurrentStageIndex - 1);
        }

        public void FinishTheTutorial()
        {
            if (IsTutorialCompleted) return;
            IsTutorialCompleted = true;

            OnThisTutorialCompleted?.Invoke();
            TutorialManager.Instance.DebugLog("Tutorial: " + TutorialName + " completed", gameObject, TutorialManager.DebugType.Successful);
            TutorialManager.Instance.ShowNextTutorial();
        }

        public void SkipTutorial()
        {
            if (IsTutorialCompleted) return;
            OnThisTutorialStarted?.Invoke();
            for (int i = 0; i < Stages.Count; i++)
            {
                Stages[i].OnStageStart?.Invoke();
                Stages[i].IsStageCompleted = true;
                Stages[i].OnStageEnd?.Invoke();
                Stages[i].ModulesActiveStatus(false);
            }
            IsTutorialCompleted = true;
            OnThisTutorialCompleted?.Invoke();
            TutorialManager.Instance.DebugLog("Tutorial: " + TutorialName + " skipped", gameObject, TutorialManager.DebugType.Successful);
        }

#if UNITY_EDITOR
        [Button("Add New Stage", ButtonSizes.Large)]
        public void AddNewTutorialStage()
        {
            string name = transform.name + " - Stage " + (transform.childCount + 1);
            GameObject g = new GameObject(name.ToString());
            g.AddComponent<TutorialStage>().TutorialStageName = name;
            g.transform.SetParent(transform);
            UnityEditor.Selection.activeGameObject = g;
        }
#endif
    }
}