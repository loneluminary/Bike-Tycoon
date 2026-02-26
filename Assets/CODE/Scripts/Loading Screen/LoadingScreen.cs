using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace SceneLoading
{
    /// A screen shown to the player while the game is loading.
    public class LoadingScreen : MonoSingleton<LoadingScreen>
    {
        /// The settings that this loading screen should use.
        public LoadingScreenSettings settings;

        /// The current progress as a percentage out of 100.
        [ShowInInspector, ReadOnly] public float Progress { get; private set; }

        [FoldoutGroup("Events")] public UnityEvent onStart = new();
        [FoldoutGroup("Events")] public UnityEvent<float> onProgressChanged = new();
        [FoldoutGroup("Events")] public UnityEvent onComplete = new();
        [FoldoutGroup("Events")] public UnityEvent onClose = new();

        protected UIDocument document;
        /// The label that displays the tips title.
        protected Label tipsTitleLabel;
        /// The label that displays the current tip.
        protected Label tipLabel;
        /// The element that contains the tip.
        protected VisualElement tipsContainer;
        /// The loading bar element.
        protected VisualElement loadingBar;
        /// The loading label.
        protected Label loadingLabel;
        /// The loading icon container.
        protected VisualElement loadingIconContainer;
        /// The element that displays the spinner icon.
        protected VisualElement spinnerIcon;
        /// The root element of the loading screen.
        protected VisualElement root;
        /// The element that contains loading content.
        protected VisualElement loadingContent;
        /// The label that displays the continue text.
        protected Label continueLabel;
        /// The first background element.
        protected VisualElement backgroundOne;
        /// The second background element.
        protected VisualElement backgroundTwo;
        /// The spinner action.
        protected IVisualElementScheduledItem spinnerAction;

        /// The index of the displayed tip.
        protected int currentTipIndex = -1;
        /// The index of the current process.
        protected int currentProcessIndex;
        /// The index of the displayed background.
        protected int currentBackgroundIndex;
        /// A list of loading processes to undergo. This list will be automatically cleared when loading is complete.
        protected LoadingProgressTracker[] processes;
        /// If loading processes are running.
        public bool IsLoading { get; private set; }
        /// If the loading screen is opened.
        public bool IsOpen => root.enabledSelf;

        protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);

            document = GetComponent<UIDocument>();
            tipsTitleLabel = document.rootVisualElement.Q<Label>("TipsTitleLabel");
            tipLabel = document.rootVisualElement.Q<Label>("TipLabel");
            tipsContainer = document.rootVisualElement.Q<VisualElement>("TipsContainer");
            loadingBar = document.rootVisualElement.Q<VisualElement>("LoadingBar");
            root = document.rootVisualElement.Q<VisualElement>("Root");
            loadingLabel = document.rootVisualElement.Q<Label>("LoadingLabel");
            loadingIconContainer = document.rootVisualElement.Q<VisualElement>("LoadingIconContainer");
            spinnerIcon = document.rootVisualElement.Q<VisualElement>("SpinnerIcon");
            loadingContent = document.rootVisualElement.Q<VisualElement>("LoadingContent");
            continueLabel = document.rootVisualElement.Q<Label>("ContinueLabel");
            backgroundOne = document.rootVisualElement.Q<VisualElement>("BackgroundOne");
            backgroundTwo = document.rootVisualElement.Q<VisualElement>("BackgroundTwo");

            RefreshElementStates();

            Close(false);
        }

        private void SetPickingModeRecursively(VisualElement element, PickingMode pickingMode)
        {
            element.pickingMode = pickingMode;

            foreach (var child in element.hierarchy.Children())
            {
                // Recursively set picking mode for all children
                SetPickingModeRecursively(child, pickingMode);
            }
        }

        /// Refreshes the states of each element based on the loading screen settings.
        protected virtual void RefreshElementStates()
        {
            tipsTitleLabel.text = settings.tipsTitle;

            tipsContainer.style.display = settings.showTips ? DisplayStyle.Flex : DisplayStyle.None;

            loadingBar.style.display = settings.hideBar ? DisplayStyle.None : DisplayStyle.Flex;
            loadingIconContainer.style.display = settings.showSpinner ? DisplayStyle.Flex : DisplayStyle.None;
            spinnerIcon.style.backgroundImage = new StyleBackground(settings.spinnerIcon);
        }

        /// Toggles the loading label animator pseudo class.
        protected virtual void ToggleLoadingLabelClass(TransitionEndEvent endEvent)
        {
            loadingLabel.ToggleInClassList(settings.loadingLabelAnimatorPseudoClassName);
        }

        /// Toggles the continue label animator pseudo class.
        protected virtual void ToggleContinueLabelClass(TransitionEndEvent endEvent)
        {
            continueLabel.ToggleInClassList(settings.continueLabelAnimatorPseudoClassName);
        }

        /// Toggles the background one scale animator pseudo class.
        protected virtual void ToggleBackgroundOneScaleClass(TransitionEndEvent endEvent)
        {
            backgroundOne.ToggleInClassList(settings.backgroundScaleAnimatorPseudoClassName);
        }

        /// Toggles the background two scale animator pseudo class.
        protected virtual void ToggleBackgroundTwoScaleClass(TransitionEndEvent endEvent)
        {
            backgroundTwo.ToggleInClassList(settings.backgroundScaleAnimatorPseudoClassName);
        }

        /// Toggles the tip animator pseudo class.
        protected virtual void ToggleTipClass()
        {
            tipLabel.ToggleInClassList(settings.tipAnimatorPseudoClassName);
        }

        /// Toggles the tip animator pseudo class and increments the tip if the animator class is in the
        /// disabled state.
        protected virtual void ToggleTipClassAndDisplayNextTip()
        {
            ToggleTipClass();

            if (!tipLabel.ClassListContains(settings.tipAnimatorPseudoClassName))
            {
                NextTip();
            }
        }

        /// Displays the next tip.
        protected virtual void NextTip()
        {
            currentTipIndex++;

            if (currentTipIndex >= settings.tips.Count)
            {
                currentTipIndex = 0;
            }

            tipLabel.text = settings.tips[currentTipIndex];
        }

        /// Displays the continue content.
        protected virtual void ShowContinueContent()
        {
            continueLabel.text = settings.continueText;
            continueLabel.style.display = DisplayStyle.Flex;
            continueLabel.RegisterCallback<TransitionEndEvent>(ToggleContinueLabelClass);
            loadingContent.AddToClassList("fadeOut");

            // Nullify spinner action
            if (spinnerAction != null)
            {
                spinnerAction.Pause();
                spinnerAction = null;
            }

            InputSystem.onAnyButtonPress.CallOnce(_ => Close());
        }

        /// Updates the spinner icon rotation.
        /// <param name="changeValue">The amount of rotation to add.</param>
        protected virtual void UpdateSpinner(float changeValue)
        {
            spinnerIcon.style.rotate = new Rotate(new Angle(spinnerIcon.style.rotate.value.angle.value + changeValue));
        }

        /// Updates the loading bar based on the completed processes.
        protected virtual void UpdateLoadingBar()
        {
            // Make the loader bar length represent the progress value
            loadingBar.style.width = new Length(Progress, LengthUnit.Percent);

            if (processes is { Length: > 0 })
            {
                string percentText = !settings.showPercentage ? string.Empty : $" {Progress:0.##}%";
                loadingLabel.text = $"{processes[currentProcessIndex].displayText}{percentText}";
            }
            else
            {
                loadingLabel.text = settings.defaultLoadingText;
            }
        }

        /// Handles the background slideshow.
        /// <returns>Yields for a delay.</returns>
        protected IEnumerator BackgroundSlideshowLoopCoroutine()
        {
            currentBackgroundIndex++;

            if (currentBackgroundIndex >= settings.backgrounds.Count)
            {
                currentBackgroundIndex = 0;
            }

            yield return new WaitForSeconds(0.5f);

            var bg = new StyleBackground(settings.backgrounds[currentBackgroundIndex]);
            if (backgroundOne.enabledSelf)
            {
                backgroundTwo.style.backgroundImage = bg;
            }
            else
            {
                backgroundOne.style.backgroundImage = bg;
            }

            yield return new WaitForSeconds(settings.backgroundDisplayLength);

            backgroundOne.SetEnabled(!backgroundOne.enabledSelf);

            if (IsOpen)
            {
                StartCoroutine(BackgroundSlideshowLoopCoroutine());
            }
        }

        /// Begins the background slideshow.
        protected virtual void BeginBackgroundSlideshow()
        {
            if (settings.backgrounds.Count == 0)
            {
                return;
            }

            currentBackgroundIndex = 0;

            backgroundOne.SetEnabled(true);
            backgroundOne.style.backgroundImage = new StyleBackground(settings.backgrounds[currentBackgroundIndex]);

            if (settings.backgrounds.Count > 1)
            {
                StartCoroutine(BackgroundSlideshowLoopCoroutine());
            }
        }

        /// Handles the background slideshow.
        /// <returns>Yields for a delay.</returns>
        protected IEnumerator TipSlideshowLoopCoroutine()
        {
            if (tipLabel.ClassListContains(settings.tipAnimatorPseudoClassName))
            {
                yield return new WaitForSeconds(1f);
            }
            else
            {
                yield return new WaitForSeconds(settings.tipDisplayLength);
            }

            if (IsOpen)
            {
                ToggleTipClassAndDisplayNextTip();
                StartCoroutine(TipSlideshowLoopCoroutine());
            }
        }

        /// Begins the background slideshow.
        protected virtual void BeginTipSlideshow()
        {
            if (settings.tips.Count == 0) return;

            if (settings.tips.Count > 1)
            {
                StartCoroutine(TipSlideshowLoopCoroutine());
            }
            else
            {
                tipLabel.EnableInClassList(settings.tipAnimatorPseudoClassName, false);
                tipLabel.text = settings.tips[0];
            }
        }

        /// Loads a scene. You can use loading processes to track other things that need to be loaded.
        /// <param name="buildIndex">The build index of the scene.</param>
        /// <param name="processes">A list of loading processes other than the scene.</param>
        public virtual void Load(int buildIndex = -1, params LoadingProgressTracker[] processes)
        {
            var sceneName = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(buildIndex));
            Load(sceneName, processes);
        }

        /// Loads a scene. You can use loading processes to track other things that need to be loaded.
        /// <param name="sceneName">The name of the scene to load.</param>
        /// <param name="processes">A list of loading processes other than the scene.</param>
        public virtual void Load(string sceneName = "", params LoadingProgressTracker[] processes)
        {
            if (IsLoading)
            {
                Debug.LogWarning("Cannot load a scene while loading is ongoing.");
                return;
            }

            if (string.IsNullOrEmpty(sceneName) && processes.Length == 0)
            {
                Debug.LogWarning("Nothing to load.");
                return;
            }

            Open();
            StartCoroutine(LoadingCoroutine(sceneName, processes));
        }

        /// Handles the loading process.
        /// <param name="sceneName">The name of the scene to load.</param>
        /// <param name="processes">A list of loading processes other than the scene.</param>
        /// <returns>Yields until all processes are complete.</returns>
        protected IEnumerator LoadingCoroutine(string sceneName, params LoadingProgressTracker[] additionalProcesses)
        {
            // Optional delay for loading screen animation
            yield return new WaitForSeconds(0.5f);

            IsLoading = true;
            onStart.Invoke();

            List<LoadingProgressTracker> allProcesses = new List<LoadingProgressTracker>();

            AsyncOperation sceneOperation = null;

            // Set up scene loading as a progress tracker
            if (!string.IsNullOrEmpty(sceneName))
            {
                sceneOperation = SceneManager.LoadSceneAsync(sceneName);
                sceneOperation.allowSceneActivation = false;
                var sceneTracker = new LoadingProgressTracker(settings.defaultLoadingText, () => Mathf.Clamp01(sceneOperation.progress / 0.9f) * 100f); // Normalize to 0�100
                allProcesses.Add(sceneTracker);
            }

            // Add any extra processes
            if (additionalProcesses != null) allProcesses.AddRange(additionalProcesses);

            // Store trackers
            processes = allProcesses.ToArray();

            // Run each process one by one
            for (int i = 0; i < processes.Length; i++)
            {
                currentProcessIndex = i;
                var process = processes[i];
                float previousProgress = -1f;

                // Wait until this process reaches 100%
                yield return new WaitUntil(() =>
                {
                    Progress = (process.Progress + 100 * i) / (100 * processes.Length) * 100f;

                    if (!Mathf.Approximately(previousProgress, Progress))
                    {
                        previousProgress = Progress;
                        onProgressChanged.Invoke(Progress);
                        UpdateLoadingBar();
                    }

                    return process.Progress >= 100f;
                });
            }

            // Wait for scene load to reach 90% (ready to activate)
            if (sceneOperation != null)
            {
                while (sceneOperation.progress < 0.9f) yield return null;

                // Final update to 100%
                Progress = 100f;
                onProgressChanged.Invoke(Progress);
                UpdateLoadingBar();

                // Activate the loaded scene
                sceneOperation.allowSceneActivation = true;

                yield return new WaitUntil(() => sceneOperation.isDone);
            }

            IsLoading = false;
            onComplete.Invoke();

            // End loading
            if (settings.requireInputToContinue)
            {
                ShowContinueContent();
            }
            else
            {
                Close();
            }
        }

        /// Opens the loading screen.
        public virtual void Open()
        {
            SetPickingModeRecursively(root, PickingMode.Position);

            // Register events
            loadingLabel.RegisterCallback<TransitionEndEvent>(ToggleLoadingLabelClass);

            if (settings.enableBackgroundZoom)
            {
                backgroundOne.RegisterCallback<TransitionEndEvent>(ToggleBackgroundOneScaleClass);
                backgroundTwo.RegisterCallback<TransitionEndEvent>(ToggleBackgroundTwoScaleClass);
            }
            else
            {
                backgroundOne.EnableInClassList(settings.backgroundScaleAnimatorPseudoClassName, false);
            }

            // Enable and start
            root.SetEnabled(true);
            BeginBackgroundSlideshow();
            BeginTipSlideshow();
            UpdateLoadingBar();

            // Start animtions
            if (settings.showSpinner)
            {
                float speed = -12 * settings.spinnerSpeed;
                spinnerAction = spinnerIcon.schedule.Execute(() => UpdateSpinner(speed)).Every(17);
            }

            ToggleLoadingLabelClass(null);

            if (loadingContent.ClassListContains("fadeOut"))
            {
                loadingContent.RemoveFromClassList("fadeOut");
            }
        }

        /// Closes the loading screen.
        public virtual void Close(bool invokeEvent = true)
        {
            // Unregister events
            loadingLabel.UnregisterCallback<TransitionEndEvent>(ToggleLoadingLabelClass);
            continueLabel.UnregisterCallback<TransitionEndEvent>(ToggleContinueLabelClass);
            backgroundOne.UnregisterCallback<TransitionEndEvent>(ToggleBackgroundOneScaleClass);
            backgroundTwo.UnregisterCallback<TransitionEndEvent>(ToggleBackgroundTwoScaleClass);

            // Nullify spinner action
            if (spinnerAction != null)
            {
                spinnerAction.Pause();
                spinnerAction = null;
            }

            // Disable
            continueLabel.style.display = DisplayStyle.None;
            root.SetEnabled(false);

            Progress = 0f;
            UpdateLoadingBar();

            if (invokeEvent)
            {
                onClose.Invoke();
            }

            SetPickingModeRecursively(root, PickingMode.Ignore);
        }
    }
}