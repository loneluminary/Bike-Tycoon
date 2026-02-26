using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Extensions;

namespace TUTORIAL_SYSTEM
{
    public class TutorialModule_DynamicHand : TutorialModule
    {
        [SerializeField, HideLabel] private TutorialManager.TransformSpaceType TransformSpace;

        [SerializeField] float speed = 1;
        [SerializeField] float waitingTime = .5f;
        [SerializeField] float waitTimeForReplay = 1f;
        [SerializeField] bool hideBeforeReplay;

        public List<HandPointStruct> Points = new();

        [FoldoutGroup("Visuals")][SerializeField] Sprite normalHand;
        [FoldoutGroup("Visuals")][SerializeField] Sprite clickHand;
        [FoldoutGroup("Visuals")][SerializeField] SpriteRenderer hand;
        [FoldoutGroup("Visuals")][SerializeField] Image handImage;

        private Coroutine loopCoroutine;
        private static readonly WaitForSeconds _waitForSeconds_025 = new(0.25f);

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Points.Count > 0 && Points[0].Point != null)
            {
                transform.position = Points[0].GetOffsetPosition();
            }
        }
#endif

        public override IEnumerator ActivateModule()
        {
            if (loopCoroutine != null) StopCoroutine(loopCoroutine);
            loopCoroutine = StartCoroutine(StartLoop());

            yield return new WaitForEndOfFrame();
        }

        private IEnumerator StartLoop()
        {
            if (!ArePointsNullOrDisable()) transform.position = Points[0].GetOffsetPosition();
            else yield break;

            Vector3 handScale = TransformSpace == TutorialManager.TransformSpaceType.ThreeD ? hand.transform.localScale : handImage.transform.localScale;
            if (hideBeforeReplay)
            {
                if (TransformSpace == TutorialManager.TransformSpaceType.ThreeD) hand.transform.localScale = Vector3.one * .001f;
                else handImage.transform.localScale = Vector3.one * .001f;

                if (TransformSpace == TutorialManager.TransformSpaceType.ThreeD) yield return hand.transform.DOScale(handScale, speed).WaitForCompletion();
                else yield return handImage.transform.DOScale(handScale, speed).WaitForCompletion();
            }

            if (TransformSpace == TutorialManager.TransformSpaceType.ThreeD) hand.sprite = normalHand;
            else handImage.sprite = normalHand;

            while (true)
            {
                if (ArePointsNullOrDisable()) yield break;

                for (int i = 0; i < Points.Count; i++)
                {
                    yield return transform.DOMove(Points[i].GetOffsetPosition(), speed).SetEase(Ease.Linear).WaitForCompletion();

                    switch (Points[i].HandEventType)
                    {
                        case TutorialManager.HandEventType.Normal:
                            {
                                if (TransformSpace == TutorialManager.TransformSpaceType.ThreeD) hand.sprite = normalHand;
                                else handImage.sprite = normalHand;
                                break;
                            }
                        case TutorialManager.HandEventType.Holding:
                            {
                                if (TransformSpace == TutorialManager.TransformSpaceType.ThreeD) hand.sprite = clickHand;
                                else handImage.sprite = clickHand;
                                break;
                            }
                        case TutorialManager.HandEventType.Click:
                            {
                                if (TransformSpace == TutorialManager.TransformSpaceType.ThreeD) hand.sprite = clickHand;
                                else handImage.sprite = clickHand;
                                yield return _waitForSeconds_025;
                                if (TransformSpace == TutorialManager.TransformSpaceType.ThreeD) hand.sprite = normalHand;
                                else handImage.sprite = normalHand;
                                break;
                            }
                        case TutorialManager.HandEventType.DoubleClick:
                            {
                                if (TransformSpace == TutorialManager.TransformSpaceType.ThreeD) hand.sprite = clickHand;
                                else handImage.sprite = clickHand;
                                yield return _waitForSeconds_025;
                                if (TransformSpace == TutorialManager.TransformSpaceType.ThreeD) hand.sprite = normalHand;
                                else handImage.sprite = normalHand;
                                yield return new WaitForSeconds(.15f);
                                if (TransformSpace == TutorialManager.TransformSpaceType.ThreeD) hand.sprite = clickHand;
                                else handImage.sprite = clickHand;
                                yield return _waitForSeconds_025;
                                if (TransformSpace == TutorialManager.TransformSpaceType.ThreeD) hand.sprite = normalHand;
                                else handImage.sprite = normalHand;
                                break;
                            }
                        default:
                            break;
                    }

                    yield return new WaitForSeconds(waitingTime);
                }

                if (hideBeforeReplay)
                {
                    if (TransformSpace == TutorialManager.TransformSpaceType.ThreeD) yield return hand.transform.DOScale(handScale * .01f, speed).WaitForCompletion();
                    else yield return handImage.transform.DOScale(handScale * .01f, speed).WaitForCompletion();

                    transform.position = Points[0].GetOffsetPosition();
                }

                if (TransformSpace == TutorialManager.TransformSpaceType.ThreeD) hand.sprite = normalHand;
                else handImage.sprite = normalHand;

                yield return new WaitForSeconds(waitTimeForReplay);

                if (TransformSpace == TutorialManager.TransformSpaceType.ThreeD) hand.gameObject.SetActive(true);
                else handImage.gameObject.SetActive(true);

                if (hideBeforeReplay)
                {
                    if (TransformSpace == TutorialManager.TransformSpaceType.ThreeD) yield return hand.transform.DOScale(handScale, speed).WaitForCompletion();
                    else yield return handImage.transform.DOScale(handScale, speed).WaitForCompletion();
                }
                else yield return transform.DOMove(Points[0].GetOffsetPosition(), speed).SetEase(Ease.Linear).WaitForCompletion();

                yield return new WaitForEndOfFrame();
            }
        }

        private bool ArePointsNullOrDisable()
        {
            Points.RemoveAll(point => point.Point == null);
            if (!Points.IsNullOrEmpty() && Points.TrueForAll(p => p.Point.gameObject.activeInHierarchy))
            {
                gameObject.SetActive(true);
                return false;
            }
            else
            {
                TutorialManager.Instance.DebugLog("Point list is null!", gameObject, TutorialManager.DebugType.Error);
                gameObject.SetActive(false);
                return true;
            }
        }

        [System.Serializable]
        public struct HandPointStruct
        {
            public Transform Point;
            [Tooltip("For UI (RectTransform): X,Y are percentages of rect size (0-1). For 3D: world space offset.")]
            public Vector3 Offset;
            public TutorialManager.HandEventType HandEventType;

            public Vector3 GetOffsetPosition()
            {
                if (Point == null) return Offset;

                if (Point is RectTransform rect)
                {
                    // Get the actual world corners of the RectTransform
                    Vector3[] corners = new Vector3[4];
                    rect.GetWorldCorners(corners);

                    // Calculate center position
                    Vector3 center = (corners[0] + corners[2]) / 2f;

                    // Calculate size in world space
                    float worldWidth = Vector3.Distance(corners[1], corners[2]);
                    float worldHeight = Vector3.Distance(corners[1], corners[0]);

                    // Apply percentage offset
                    return center + new Vector3(worldWidth * (Offset.x - 0.5f), worldHeight * (Offset.y - 0.5f), Offset.z);
                }

                // For 3D Transform: treat Offset as world space offset
                return Point.position + Offset;
            }
        }
    }
}