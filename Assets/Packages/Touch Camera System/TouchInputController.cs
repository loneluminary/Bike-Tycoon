using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TouchCameraSystem
{
    public class TouchInputController : MonoBehaviour
    {
        [SerializeField][Tooltip("When the finger is held on an item for at least this duration without moving, the gesture is recognized as a long tap.")] float clickDurationThreshold = 0.7f;
        [SerializeField][Tooltip("A double click gesture is recognized when the time between two consecutive taps is shorter than this duration.")] float doubleclickDurationThreshold = 0.5f;
        [SerializeField][Tooltip("This value controls how close to a vertical line the user has to perform a tilt gesture for it to be recognized as such.")] float tiltMoveDotTreshold = 0.7f;
        [SerializeField][Tooltip("Threshold value for detecting whether the fingers are horizontal enough for starting the tilt. Using this value you can prevent vertical finger placement to be counted as tilt gesture.")] float tiltHorizontalDotThreshold = 0.5f;
        [SerializeField][Tooltip("A drag is started as soon as the user moves his finger over a longer distance than this value. The value is defined as normalized value. Dragging the entire width of the screen equals 1. Dragging the entire height of the screen also equals 1.")] float dragStartDistanceThresholdRelative = 0.05f;
        [SerializeField][Tooltip("When this flag is enabled the drag started event is invoked immediately when the long tap time is succeeded.")] bool longTapStartsDrag = true;

        private float lastFingerDownTimeReal;
        private float lastClickTimeReal;
        private bool wasFingerDownLastFrame;
        private Vector3 lastFinger0DownPos;

        private const float dragDurationThreshold = 0.01f;

        private bool isDragging;
        private Vector3 dragStartPos;
        private Vector3 dragStartOffset;

        private List<Vector3> DragFinalMomentumVector { get; set; }
        private const int momentumSamplesCount = 5;

        private float pinchStartDistance;
        private List<Vector3> pinchStartPositions;
        private List<Vector3> touchPositionLastFrame;
        private Vector3 pinchRotationVectorStart = Vector3.zero;
        private Vector3 pinchVectorLastFrame = Vector3.zero;
        private float totalFingerMovement;

        private bool wasDraggingLastFrame;
        private bool wasPinchingLastFrame;

        private bool isPinching;

        private float timeSinceDragStart;

        public delegate void InputDragStartDelegate(Vector3 pos, bool isLongTap);
        public delegate void Input1PositionDelegate(Vector3 pos);

        public event InputDragStartDelegate OnDragStart;
        public event Input1PositionDelegate OnFingerDown;
        public event Action OnFingerUp;

        public delegate void DragUpdateDelegate(Vector3 dragPosStart, Vector3 dragPosCurrent, Vector3 correctionOffset);
        public event DragUpdateDelegate OnDragUpdate;
        public delegate void DragStopDelegate(Vector3 dragStopPos, Vector3 dragFinalMomentum);
        public event DragStopDelegate OnDragStop;

        public delegate void PinchStartDelegate(Vector3 pinchCenter, float pinchDistance);
        public event PinchStartDelegate OnPinchStart;
        public delegate void PinchUpdateDelegate(Vector3 pinchCenter, float pinchDistance, float pinchStartDistance);
        public event PinchUpdateDelegate OnPinchUpdate;
        public delegate void PinchUpdateExtendedDelegate(PinchUpdateData pinchUpdateData);
        public event PinchUpdateExtendedDelegate OnPinchUpdateExtended;
        public event Action OnPinchStop;

        public delegate void InputLongTapProgress(float progress);
        public event InputLongTapProgress OnLongTapProgress;

        private bool isClickPrevented;

        public delegate void InputClickDelegate(Vector3 clickPosition, bool isDoubleClick, bool isLongTap);
        public event InputClickDelegate OnInputClick;

        private bool isFingerDown;
        public bool LongTapStartsDrag => longTapStartsDrag;

        public bool IsInputOnLockedArea { get; set; }

        public void Awake()
        {
            lastFingerDownTimeReal = 0;
            lastClickTimeReal = 0;
            lastFinger0DownPos = Vector3.zero;
            dragStartPos = Vector3.zero;
            isDragging = false;
            wasFingerDownLastFrame = false;
            DragFinalMomentumVector = new List<Vector3>();
            pinchStartPositions = new List<Vector3> { Vector3.zero, Vector3.zero };
            touchPositionLastFrame = new List<Vector3> { Vector3.zero, Vector3.zero };
            pinchStartDistance = 1;
            isPinching = false;
            isClickPrevented = false;
        }

        public void OnEventTriggerPointerDown(BaseEventData baseEventData)
        {
            IsInputOnLockedArea = true;
        }

        public void Update()
        {
            if (!TouchWrapper.IsFingerDown)
            {
                IsInputOnLockedArea = false;
            }

            bool pinchToDragCurrentFrame = false;

            if (!IsInputOnLockedArea)
            {
                #region pinch

                if (!isPinching)
                {
                    if (TouchWrapper.TouchCount == 2)
                    {
                        StartPinch();
                        isPinching = true;
                    }
                }
                else
                {
                    switch (TouchWrapper.TouchCount)
                    {
                        case < 2:
                            StopPinch();
                            isPinching = false;
                            break;
                        case 2:
                            UpdatePinch();
                            break;
                    }
                }

                #endregion

                #region drag

                if (!isPinching)
                {
                    if (!wasPinchingLastFrame)
                    {
                        if (wasFingerDownLastFrame && TouchWrapper.IsFingerDown)
                        {
                            if (!isDragging)
                            {
                                float dragDistance = GetRelativeDragDistance(TouchWrapper.Touch0.Position, dragStartPos);
                                float dragTime = Time.realtimeSinceStartup - lastFingerDownTimeReal;

                                bool isLongTap = dragTime > clickDurationThreshold;
                                if (OnLongTapProgress != null)
                                {
                                    float longTapProgress = 0;
                                    if (!Mathf.Approximately(clickDurationThreshold, 0))
                                    {
                                        longTapProgress = Mathf.Clamp01(dragTime / clickDurationThreshold);
                                    }

                                    OnLongTapProgress(longTapProgress);
                                }

                                if ((dragDistance >= dragStartDistanceThresholdRelative && dragTime >= dragDurationThreshold) || (longTapStartsDrag && isLongTap))
                                {
                                    isDragging = true;
                                    dragStartOffset = lastFinger0DownPos - dragStartPos;
                                    dragStartPos = lastFinger0DownPos;
                                    DragStart(dragStartPos, isLongTap, true);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (TouchWrapper.IsFingerDown)
                        {
                            isDragging = true;
                            dragStartPos = TouchWrapper.Touch0.Position;
                            DragStart(dragStartPos, false, false);
                            pinchToDragCurrentFrame = true;
                        }
                    }

                    if (isDragging && TouchWrapper.IsFingerDown)
                    {
                        DragUpdate(TouchWrapper.Touch0.Position);
                    }

                    if (isDragging && !TouchWrapper.IsFingerDown)
                    {
                        isDragging = false;
                        DragStop(lastFinger0DownPos);
                    }
                }

                #endregion

                #region click

                if (!isPinching && !isDragging && !wasPinchingLastFrame && !wasDraggingLastFrame && !isClickPrevented)
                {
                    if (!wasFingerDownLastFrame && TouchWrapper.IsFingerDown)
                    {
                        lastFingerDownTimeReal = Time.realtimeSinceStartup;
                        dragStartPos = TouchWrapper.Touch0.Position;
                        FingerDown(TouchWrapper.AverageTouchPos);
                    }

                    if (wasFingerDownLastFrame && !TouchWrapper.IsFingerDown)
                    {
                        float fingerDownUpDuration = Time.realtimeSinceStartup - lastFingerDownTimeReal;

                        if (!wasDraggingLastFrame && !wasPinchingLastFrame)
                        {
                            float clickDuration = Time.realtimeSinceStartup - lastClickTimeReal;

                            bool isDoubleClick = clickDuration < doubleclickDurationThreshold;
                            bool isLongTap = fingerDownUpDuration > clickDurationThreshold;

                            OnInputClick?.Invoke(lastFinger0DownPos, isDoubleClick, isLongTap);

                            lastClickTimeReal = Time.realtimeSinceStartup;
                        }
                    }
                }

                #endregion
            }

            if (isDragging && TouchWrapper.IsFingerDown && !pinchToDragCurrentFrame)
            {
                DragFinalMomentumVector.Add(TouchWrapper.Touch0.Position - lastFinger0DownPos);
                if (DragFinalMomentumVector.Count > momentumSamplesCount) DragFinalMomentumVector.RemoveAt(0);
            }

            if (!IsInputOnLockedArea) wasFingerDownLastFrame = TouchWrapper.IsFingerDown;
            if (wasFingerDownLastFrame) lastFinger0DownPos = TouchWrapper.Touch0.Position;

            wasDraggingLastFrame = isDragging;
            wasPinchingLastFrame = isPinching;

            if (TouchWrapper.TouchCount == 0)
            {
                isClickPrevented = false;
                if (isFingerDown) FingerUp();
            }
        }

        private void StartPinch()
        {
            pinchStartPositions[0] = touchPositionLastFrame[0] = TouchWrapper.Touches[0].Position;
            pinchStartPositions[1] = touchPositionLastFrame[1] = TouchWrapper.Touches[1].Position;

            pinchStartDistance = GetPinchDistance(pinchStartPositions[0], pinchStartPositions[1]);
            OnPinchStart?.Invoke((pinchStartPositions[0] + pinchStartPositions[1]) * 0.5f, pinchStartDistance);

            isClickPrevented = true;
            pinchRotationVectorStart = TouchWrapper.Touches[1].Position - TouchWrapper.Touches[0].Position;
            pinchVectorLastFrame = pinchRotationVectorStart;
            totalFingerMovement = 0;
        }

        private void UpdatePinch()
        {
            float pinchDistance = GetPinchDistance(TouchWrapper.Touches[0].Position, TouchWrapper.Touches[1].Position);
            Vector3 pinchVector = TouchWrapper.Touches[1].Position - TouchWrapper.Touches[0].Position;
            float pinchAngleSign = Vector3.Cross(pinchVectorLastFrame, pinchVector).z < 0 ? -1 : 1;
            float pinchAngleDelta = 0;
            if (!Mathf.Approximately(Vector3.Distance(pinchVectorLastFrame, pinchVector), 0))
            {
                pinchAngleDelta = Vector3.Angle(pinchVectorLastFrame, pinchVector) * pinchAngleSign;
            }

            float pinchVectorDeltaMag = Mathf.Abs(pinchVectorLastFrame.magnitude - pinchVector.magnitude);
            float pinchAngleDeltaNormalized = 0;
            if (!Mathf.Approximately(pinchVectorDeltaMag, 0))
            {
                pinchAngleDeltaNormalized = pinchAngleDelta / pinchVectorDeltaMag;
            }

            Vector3 pinchCenter = (TouchWrapper.Touches[0].Position + TouchWrapper.Touches[1].Position) * 0.5f;

            #region tilting gesture

            float pinchTiltDelta = 0;
            Vector3 touch0DeltaRelative = GetTouchPositionRelative(TouchWrapper.Touches[0].Position - touchPositionLastFrame[0]);
            Vector3 touch1DeltaRelative = GetTouchPositionRelative(TouchWrapper.Touches[1].Position - touchPositionLastFrame[1]);
            float touch0DotUp = Vector2.Dot(touch0DeltaRelative.normalized, Vector2.up);
            float touch1DotUp = Vector2.Dot(touch1DeltaRelative.normalized, Vector2.up);
            float pinchVectorDotHorizontal = Vector3.Dot(pinchVector.normalized, Vector3.right);
            if (Mathf.Sign(touch0DotUp) == Mathf.Sign(touch1DotUp))
            {
                if (Mathf.Abs(touch0DotUp) > tiltMoveDotTreshold && Mathf.Abs(touch1DotUp) > tiltMoveDotTreshold)
                {
                    if (Mathf.Abs(pinchVectorDotHorizontal) >= tiltHorizontalDotThreshold)
                    {
                        pinchTiltDelta = 0.5f * (touch0DeltaRelative.y + touch1DeltaRelative.y);
                    }
                }
            }

            totalFingerMovement += touch0DeltaRelative.magnitude + touch1DeltaRelative.magnitude;

            #endregion

            OnPinchUpdate?.Invoke(pinchCenter, pinchDistance, pinchStartDistance);

            OnPinchUpdateExtended?.Invoke(new PinchUpdateData
            {
                pinchCenter = pinchCenter,
                pinchDistance = pinchDistance,
                pinchStartDistance = pinchStartDistance,
                pinchAngleDelta = pinchAngleDelta,
                pinchAngleDeltaNormalized = pinchAngleDeltaNormalized,
                pinchTiltDelta = pinchTiltDelta,
                pinchTotalFingerMovement = totalFingerMovement
            });

            pinchVectorLastFrame = pinchVector;
            touchPositionLastFrame[0] = TouchWrapper.Touches[0].Position;
            touchPositionLastFrame[1] = TouchWrapper.Touches[1].Position;
        }

        private float GetPinchDistance(Vector3 pos0, Vector3 pos1)
        {
            float distanceX = Mathf.Abs(pos0.x - pos1.x) / Screen.width;
            float distanceY = Mathf.Abs(pos0.y - pos1.y) / Screen.height;
            return Mathf.Sqrt(distanceX * distanceX + distanceY * distanceY);
        }

        private void StopPinch()
        {
            dragStartOffset = Vector3.zero;
            OnPinchStop?.Invoke();
        }

        private void DragStart(Vector3 pos, bool isLongTap, bool isInitialDrag)
        {
            OnDragStart?.Invoke(pos, isLongTap);

            isClickPrevented = true;
            timeSinceDragStart = 0;
            DragFinalMomentumVector.Clear();
        }

        private void DragUpdate(Vector3 pos)
        {
            if (OnDragUpdate != null)
            {
                timeSinceDragStart += Time.unscaledDeltaTime;
                Vector3 offset = Vector3.Lerp(Vector3.zero, dragStartOffset, Mathf.Clamp01(timeSinceDragStart * 10.0f));
                OnDragUpdate(dragStartPos, pos, offset);
            }
        }

        private void DragStop(Vector3 pos)
        {
            if (OnDragStop != null)
            {
                Vector3 momentum = Vector3.zero;
                if (DragFinalMomentumVector.Count > 0)
                {
                    momentum = DragFinalMomentumVector.Aggregate(momentum, (current, t) => current + t);
                    momentum /= DragFinalMomentumVector.Count;
                }

                OnDragStop(pos, momentum);
            }

            DragFinalMomentumVector.Clear();
        }

        private void FingerDown(Vector3 pos)
        {
            isFingerDown = true;
            OnFingerDown?.Invoke(pos);
        }

        private void FingerUp()
        {
            isFingerDown = false;
            OnFingerUp?.Invoke();
        }

        private Vector3 GetTouchPositionRelative(Vector3 touchPosScreen)
        {
            return new Vector3(touchPosScreen.x / Screen.width, touchPosScreen.y / Screen.height, touchPosScreen.z);
        }

        private float GetRelativeDragDistance(Vector3 pos0, Vector3 pos1)
        {
            Vector2 dragVector = pos0 - pos1;
            float dragDistance = new Vector2(dragVector.x / Screen.width, dragVector.y / Screen.height).magnitude;
            return dragDistance;
        }
    }
}

public class PinchUpdateData
{
    public Vector3 pinchCenter;
    public float pinchDistance;
    public float pinchStartDistance;
    public float pinchAngleDelta;
    public float pinchAngleDeltaNormalized;
    public float pinchTiltDelta;
    public float pinchTotalFingerMovement;
}