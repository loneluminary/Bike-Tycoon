using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace TUTORIAL_SYSTEM
{
    public class TutorialManager : MonoSingleton<TutorialManager>
    {
        public List<Tutorial> Tutorials = new();

        public bool SkipAfterCompletion;

        [FoldoutGroup("Save & Load")] public bool SaveTutorialProgress;
        [FoldoutGroup("Save & Load")] public bool SaveStageProgress;
        [FoldoutGroup("Save & Load")][ReadOnly] public string SaveKey;

        [ListDrawerSettings(ListElementLabelName = "Name")] public List<TutorialModuleStruct> TutorialModulePrefabs;

        [SerializeField] bool DebugMode = true;

        public UnityEvent OnAllTutorialsCompleted;

        private int currentTutorialIndex;
        [HideInInspector] public StandardAction OnUpdate;

        public bool AllTutorialsCompleted
        {
            get => PlayerPrefs.GetInt("TutorialCompleted_" + SaveKey) == 1;
            private set => PlayerPrefs.SetInt("TutorialCompleted_" + SaveKey, value ? 1 : 0);
        }

        private void Start()
        {
            InitializeTutorials();

            if (SkipAfterCompletion)
            {
                if (AllTutorialsCompleted)
                {
                    gameObject.SetActive(false);
                    return;
                }

                OnAllTutorialsCompleted.AddListener(() =>
                {
                    AllTutorialsCompleted = true;
                    gameObject.SetActive(false);
                });
            }

            ShowNextTutorial();
        }

        private void Update()
        {
            OnUpdate?.Invoke();
        }

        private void InitializeTutorials()
        {
            try
            {
                Tutorials = GetComponentsInChildren<Tutorial>().ToList();

                currentTutorialIndex = 0;
                for (int i = 0; i < Tutorials.Count; i++)
                {
                    Tutorials[i].Init(i);
                }

                if (SaveTutorialProgress)
                {
                    int c = PlayerPrefs.GetInt("SavedTutorialIndex_" + SaveKey);
                    for (int i = 0; i < c; i++)
                    {
                        Tutorials[i].OnThisTutorialStarted?.Invoke();

                        if (SaveStageProgress)
                        {
                            int x = PlayerPrefs.GetInt("SavedStageIndex_" + i + "_" + SaveKey);
                            for (int j = 0; j <= x; j++)
                            {
                                Tutorials[i].Stages[j].OnStageStart?.Invoke();
                                Tutorials[i].Stages[j].OnStageEnd?.Invoke();
                            }

                            Tutorials[i].CurrentStageIndex = x;
                        }

                        Tutorials[i].IsTutorialCompleted = true;
                        Tutorials[i].OnThisTutorialCompleted?.Invoke();
                    }

                    currentTutorialIndex = c;
                }
            }
            catch { }
        }

        public void ShowNextTutorial()
        {
            if (currentTutorialIndex > Tutorials.Count - 1)
            {
                OnAllTutorialsCompleted?.Invoke();
                DebugLog("All tutorials completed!", gameObject, DebugType.Successful);
                return;
            }

            currentTutorialIndex++;
            Tutorials[currentTutorialIndex - 1].StartTheTutorial();

            if (SaveTutorialProgress) PlayerPrefs.SetInt("SavedTutorialIndex_" + SaveKey, currentTutorialIndex - 1 <= 0 ? 0 : currentTutorialIndex - 1);
        }

        public bool StageStarted(int tutorialIndex, int stageIndex)
        {
            if (tutorialIndex < 0 || tutorialIndex > Tutorials.Count - 1)
            {
                DebugLog("Tutorial not found! tutorial index: " + tutorialIndex, gameObject, DebugType.Error);
                return false;
            }

            if (stageIndex < 0 || stageIndex > Tutorials[tutorialIndex].Stages.Count - 1)
            {
                DebugLog("Stage not found! tutorial index: " + tutorialIndex + ", stage index: " + stageIndex, gameObject, DebugType.Error);
                return false;
            }

            if (currentTutorialIndex - 1 == tutorialIndex && Tutorials[tutorialIndex].CurrentStageIndex - 1 == stageIndex)
            {
                Tutorials[tutorialIndex].Stages[stageIndex].StageStarted();
                return true;
            }

            return false;
        }

        public bool StageCompleted(int tutorialIndex, int stageIndex)
        {
            if (tutorialIndex < 0 || tutorialIndex > Tutorials.Count - 1)
            {
                DebugLog("Tutorial not found! tutorial index: " + tutorialIndex, gameObject, DebugType.Error);
                return false;
            }

            if (stageIndex < 0 || stageIndex > Tutorials[tutorialIndex].Stages.Count - 1)
            {
                DebugLog("Stage not found! tutorial index: " + tutorialIndex + ", stage index: " + stageIndex, gameObject, DebugType.Error);
                return false;
            }

            if (currentTutorialIndex - 1 == tutorialIndex && Tutorials[tutorialIndex].CurrentStageIndex - 1 == stageIndex)
            {
                Tutorials[tutorialIndex].Stages[stageIndex].StageCompleted();
                return true;
            }

            return false;
        }

        public bool CompleteTheTutorial(int tutorialIndex)
        {
            if (tutorialIndex > Tutorials.Count - 1 || tutorialIndex < 0) return false;

            Tutorials[tutorialIndex].FinishTheTutorial();
            return true;
        }

        public bool CompleteTheTutorialStage(int tutorialIndex, int stageIndex)
        {
            if (tutorialIndex > Tutorials.Count - 1 || tutorialIndex < 0) return false;
            if (stageIndex > Tutorials[currentTutorialIndex].Stages.Count - 1 || stageIndex < 0) return false;

            Tutorials[tutorialIndex].Stages[stageIndex].StageCompleted();
            return true;
        }

        public void SkipAllTutorials()
        {
            if (AllTutorialsCompleted) return;

            for (int i = 0; i < Tutorials.Count; i++)
            {
                Tutorials[i].SkipTutorial();
            }

            OnAllTutorialsCompleted?.Invoke();
            AllTutorialsCompleted = true;
            DebugLog("Skipped All Tutorials!", gameObject, DebugType.Successful);

            gameObject.SetActive(false);
        }

        public void SkipCurrentTutorial()
        {
            if (currentTutorialIndex - 1 > Tutorials.Count - 1)
            {
                SkipAllTutorials();
                return;
            }

            var curTutorial = Tutorials[currentTutorialIndex - 1];
            curTutorial.SkipTutorial();

            if (currentTutorialIndex > Tutorials.Count - 1) return;

            currentTutorialIndex++;
            Tutorials[currentTutorialIndex - 1].StartTheTutorial();

            if (SaveTutorialProgress) PlayerPrefs.SetInt("SavedTutorialIndex_" + SaveKey, currentTutorialIndex - 1 <= 0 ? 0 : currentTutorialIndex - 1);
        }

        public void DebugLog(string message, GameObject context, DebugType type = DebugType.Normal)
        {
            if (!DebugMode) return;

            string whiteColor = "FFFFFF";
            string redColor = "FF4058";
            string blueColor = "00D2FF";
            string greenColor = "0EFF0B";

            string msg = "<color=#9F7FFF>TUTORIAL SYSTEM:</color>\n";

            msg += "<color=#";
            switch (type)
            {
                case DebugType.None:
                    break;
                case DebugType.Normal:
                    msg += whiteColor;
                    break;
                case DebugType.Info:
                    msg += blueColor;
                    break;
                case DebugType.Successful:
                    msg += greenColor;
                    break;
                case DebugType.Error:
                    msg += redColor;
                    break;
            }

            msg += ">";
            msg += message;
            msg += "</color>";

            if (context != null) Debug.Log(msg, context);
            else Debug.Log(msg);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            try
            {
                if (SaveKey.Length < 2)
                {
                    if (gameObject.scene.name != null) SaveKey = RuntimeUtilities.GetRandomKey();
                }

                if (gameObject.scene.name == null) SaveKey = "0";
            }
            catch { }

            InitializeTutorials();
        }

        [Button(ButtonSizes.Large)]
        public void AddNewTutorial()
        {
            string name = "Tutorial " + (transform.childCount + 1);
            GameObject g = new(name);
            g.AddComponent<Tutorial>().TutorialName = name;
            g.transform.SetParent(transform);

            Selection.activeGameObject = g;
        }
#endif

        [System.Serializable]
        public struct TutorialModuleStruct
        {
            public string Name;
            public TransformSpaceType WorldType;
            public TutorialModule TutorialModule;
        }

        public delegate void StandardAction();
        public delegate void StringAction(string value);
        public delegate void FloatAction(float value);
        public delegate void IntAction(int value);
        public delegate void ObjectAction(object value);

        public enum DebugType { None, Normal, Info, Successful, Error }

        public enum TriggerType { AutomaticRun, Collision, ButtonClick, ToggleChange, Event, Distance, ManualCall }
        public enum EventType { None, OnEnable, OnDisable, OnDestroy, OnClick, ManualCall }

        public enum HandEventType { Normal, Holding, Click, DoubleClick }
        public enum ArrowMovementType { Follow, Static }

        public enum TransformSpaceType { [InspectorName("3D")] ThreeD, UI }
    }
}