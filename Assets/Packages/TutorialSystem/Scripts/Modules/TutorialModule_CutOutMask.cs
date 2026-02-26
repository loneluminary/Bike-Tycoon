using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TUTORIAL_SYSTEM
{
    [ExecuteAlways]
    public class TutorialModule_CutOutMask : TutorialModule
    {
        [SerializeField] private Image MaskObject;
        [SerializeField] private Image MaskFill;
        [SerializeField] private Image[] ClickBlockers;

        [SerializeField][Range(1, 20)] private float HoleScale = 1;
        [SerializeField][Range(0, 1)] private float HoleRadius = .8f;
        [SerializeField] private Color MaskColor;

        [SerializeField] private RectTransform TargetUI;
        [SerializeField] private bool RaycastTarget = true;

        public override IEnumerator ActivateModule()
        {
            if (!Application.isPlaying) yield break;

            for (int i = 0; i < ClickBlockers.Length; i++)
            {
                ClickBlockers[i].raycastTarget = RaycastTarget;
            }

            DOTween.To(() => 100f, (x) => HoleScale = x, HoleScale, 1f).OnUpdate(UpdateData);
        }

#if UNITY_EDITOR
        private void Update() => UpdateData();
#endif

        private void UpdateData()
        {
            float radius = Mathf.Clamp((1 - HoleRadius - .4f) * 10f, 0, 10f);
            MaskObject.pixelsPerUnitMultiplier = radius;
            MaskFill.color = MaskColor;

            if (TargetUI != null && ClickBlockers.Length >= 4)
            {
                RectTransform maskRect = MaskObject.rectTransform;

                // Sync Mask properties to Target
                maskRect.anchorMax = TargetUI.anchorMax;
                maskRect.anchorMin = TargetUI.anchorMin;
                maskRect.pivot = TargetUI.pivot;

                float padding = (HoleScale - 1) * 20f;
                maskRect.sizeDelta = TargetUI.sizeDelta + (Vector2.one * padding);
                maskRect.position = TargetUI.position;

                // Calculate hole dimensions in world space
                float worldWidth = maskRect.rect.width * maskRect.lossyScale.x;
                float worldHeight = maskRect.rect.height * maskRect.lossyScale.y;

                // Find the true center of the hole regardless of pivot
                Vector3 pivotOffset = new Vector3((0.5f - maskRect.pivot.x) * worldWidth, (0.5f - maskRect.pivot.y) * worldHeight, 0);
                Vector3 holeCenter = maskRect.position + pivotOffset;

                // Position blockers based on their actual sizes
                for (int i = 0; i < 4; i++)
                {
                    RectTransform bRect = ClickBlockers[i].rectTransform;
                    float bWidth = bRect.rect.width * bRect.lossyScale.x;
                    float bHeight = bRect.rect.height * bRect.lossyScale.y;

                    switch (i)
                    {
                        case 0: bRect.position = holeCenter + new Vector3(0, -(worldHeight * 0.5f + bHeight * 0.5f), 0);  // Bottom
                            break;
                        case 1: bRect.position = holeCenter + new Vector3(0, worldHeight * 0.5f + bHeight * 0.5f, 0);  // Top
                            break;
                        case 2: bRect.position = holeCenter + new Vector3(worldWidth * 0.5f + bWidth * 0.5f, 0, 0);  // Right
                            break;
                        case 3: bRect.position = holeCenter + new Vector3(-(worldWidth * 0.5f + bWidth * 0.5f), 0, 0);  // Left
                            break;
                    }
                }
            }
        }
    }
}