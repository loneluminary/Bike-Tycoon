using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SceneLoading
{
    /// Settings for a loading screen.
    [CreateAssetMenu(fileName = "LoadingScreenSettings", menuName = "Freeloader/Loading Screen Settings", order = 1)]
    public class LoadingScreenSettings : ScriptableObject
    {
        [Tooltip("The name of the loading screen.")]
        public string settingsName = "Name";

        [Tooltip("The name of the pseudo class used by the loading screen to animate the tips.")]
        public string tipAnimatorPseudoClassName;

        [Tooltip("The name of the pseudo class used by the loading screen to animate the loading label.")]
        public string loadingLabelAnimatorPseudoClassName;

        [Tooltip("The name of the pseudo class used by the loading screen to animate the continue label.")]
        public string continueLabelAnimatorPseudoClassName;

        [Tooltip("The name of the pseudo class used by the loading screen to animate the background zoom.")]
        public string backgroundScaleAnimatorPseudoClassName;

        [Tooltip("If the loading bar should be hidden.")]
        public bool hideBar;

        [Tooltip("The default loading text to display.")]
        public string defaultLoadingText = "Loading...";

        [Tooltip("If the progress percentage should be displayed.")]
        public bool showPercentage = true;

        [Tooltip("If the animated spinner icon should be displayed.")]
        public bool showSpinner = true;

        [Tooltip("The spinning speed of the spinner icon.")]
        public float spinnerSpeed = 1f;

        [Tooltip("The spinner icon.")]
        public Sprite spinnerIcon;

        [Tooltip("A list of backgrounds. If there's more than 1, the backgrounds will work as a slideshow.")]
        public List<Sprite> backgrounds;

        [Tooltip("The length a single background is displayed for before being replaced by another.")]
        public float backgroundDisplayLength = 6;

        [Tooltip("If this is true, the background will play a zoom in and out looped animation.")]
        public bool enableBackgroundZoom;

        [Tooltip("If tips should be enabled.")]
        public bool showTips = true;
        [Tooltip("A list of tips. The loading screen will rotate through each tip if there's more than 1.")]
        [ShowIf("showTips")] public List<string> tips;
        [Tooltip("The title of the tips.")]
        [ShowIf("showTips")] public string tipsTitle = "Tip";
        [Tooltip("The length a single tip is displayed for before being replaced by another.")]
        [ShowIf("showTips")] public float tipDisplayLength = 6;

        [Tooltip("If this is enabled, the loading screen will require user input to close after loading is completed.")]
        public bool requireInputToContinue;

        [Tooltip("The text to display when input is required to continue.")]
        public string continueText = "Continue";

#if UNITY_EDITOR
        /// Saves the object (editor only).
        public void Save()
        {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
        }
#endif
    }
}