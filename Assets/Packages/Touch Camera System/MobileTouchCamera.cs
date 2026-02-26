using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace TouchCameraSystem
{
    [RequireComponent(typeof(TouchInputController))]
    [RequireComponent(typeof(Camera))]
    public class MobileTouchCamera : MonoBehaviour
    {
        #region inspector
        [Tooltip("You need to define whether your camera is a side-view camera (which is the default when using the 2D mode of unity) or if you chose a top-down looking camera. This parameter tells the system whether to scroll in XY direction, or in XZ direction.")]
        [SerializeField] CameraPlaneAxes cameraAxes = CameraPlaneAxes.XY_2D_SIDESCROLL;
        [Tooltip("When using a perspective camera, the zoom can either be performed by changing the field of view, or by moving the camera closer to the scene.")]
        [SerializeField] PerspectiveZoomMode perspectiveZoomMode = PerspectiveZoomMode.FIELD_OF_VIEW;
        [Tooltip("For perspective cameras this value denotes the min field of view used for zooming (field of view zoom), or the min distance to the ground (translation zoom). For orthographic cameras it denotes the min camera size.")]
        [SerializeField] float camZoomMin = 4;
        [Tooltip("For perspective cameras this value denotes the max field of view used for zooming (field of view zoom), or the max distance to the ground (translation zoom). For orthographic cameras it denotes the max camera size.")]
        [SerializeField] float camZoomMax = 12;
        [Tooltip("The cam will overzoom the min/max values by this amount and spring back when the user releases the zoom.")]
        [SerializeField] float camOverzoomMargin = 1;
        [Tooltip("When dragging the camera close to the defined border, it will spring back when the user stops dragging. This value defines the distance from the border where the camera will spring back to.")]
        [SerializeField] float camOverdragMargin = 5.0f;
        [Tooltip("These values define the scrolling borders for the camera. The camera will not scroll further than defined here. When a top-down camera is used, these 2 values are applied to the X/Z position.")]
        [SerializeField] Vector2 boundaryMin = new Vector2(-1000, -1000);
        [Tooltip("These values define the scrolling borders for the camera. The camera will not scroll further than defined here. When a top-down camera is used, these 2 values are applied to the X/Z position.")]
        [SerializeField] Vector2 boundaryMax = new Vector2(1000, 1000);

        [Header("Advanced")]
        [Tooltip("The lower the value, the slower the camera will follow. The higher the value, the more direct the camera will follow movement updates. Necessary for keeping the camera smooth when the framerate is not in sync with the touch input update rate.")]
        [SerializeField] float camFollowFactor = 15.0f;
        [SerializeField][Tooltip("Set the behaviour of the damping (e.g. the slow-down) at the end of auto-scrolling.")] AutoScrollDampMode autoScrollDampMode = AutoScrollDampMode.DEFAULT;
        [SerializeField][Tooltip("When dragging quickly, the camera will keep autoscrolling in the last direction. The autoscrolling will slowly come to a halt. This value defines how fast the camera will come to a halt.")] float autoScrollDamp = 300;
        [SerializeField][Tooltip("This curve allows to modulate the auto scroll damp value over time.")] AnimationCurve autoScrollDampCurve = new AnimationCurve(new Keyframe(0, 1, 0, 0), new Keyframe(0.7f, 0.9f, -0.5f, -0.5f), new Keyframe(1, 0.01f, -0.85f, -0.85f));
        [SerializeField][Tooltip("The camera assumes that the scrollable content of your scene (e.g. the ground of your game-world) is located at y = 0 for top-down cameras or at z = 0 for side-scrolling cameras. In case this is not valid for your scene, you may adjust this property to the correct offset.")] float groundLevelOffset;
        [SerializeField][Tooltip("When enabled, the camera can be rotated using a 2-finger rotation gesture.")] bool enableRotation;
        [SerializeField][Tooltip("When enabled, the camera can be tilted using a synced 2-finger up or down motion.")] bool enableTilt;
        [SerializeField][Tooltip("The minimum tilt angle for the camera.")] float tiltAngleMin = 45;
        [SerializeField][Tooltip("The maximum tilt angle for the camera.")] float tiltAngleMax = 90;
        [SerializeField][Tooltip("When enabled, the camera is tilted automatically when zooming.")] bool enableZoomTilt;
        [SerializeField][Tooltip("The minimum tilt angle for the camera when using zoom tilt.")] float zoomTiltAngleMin = 45;
        [SerializeField][Tooltip("The maximum tilt angle for the camera when using zoom tilt.")] float zoomTiltAngleMax = 90;

        [FoldoutGroup("Event Callbacks")][Tooltip("Here you can set up callbacks to be invoked when an item with Collider is tapped on.")] public UnityEventWithRaycastHit OnPickItem;
        [FoldoutGroup("Event Callbacks")][Tooltip("Here you can set up callbacks to be invoked when an item with Collider2D is tapped on.")] public UnityEventWithRaycastHit2D OnPickItem2D;
        [FoldoutGroup("Event Callbacks")][Tooltip("Here you can set up callbacks to be invoked when an item with Collider is double-tapped on.")] public UnityEventWithRaycastHit OnPickItemDoubleClick;
        [FoldoutGroup("Event Callbacks")][Tooltip("Here you can set up callbacks to be invoked when an item with Collider2D is double-tapped on.")] public UnityEventWithRaycastHit2D OnPickItem2DDoubleClick;

        [FoldoutGroup("Keyboard & Mouse Input")][SerializeField][Tooltip("Here you can define all platforms that should support keyboard and mouse input for controlling the camera.")] List<RuntimePlatform> keyboardAndMousePlatforms = new List<RuntimePlatform> { RuntimePlatform.WindowsEditor, RuntimePlatform.WindowsPlayer, RuntimePlatform.OSXEditor, RuntimePlatform.OSXPlayer, RuntimePlatform.LinuxEditor, RuntimePlatform.LinuxPlayer, RuntimePlatform.WebGLPlayer };
        [FoldoutGroup("Keyboard & Mouse Input")][SerializeField] private bool controlTweakablesEnabled;
        [FoldoutGroup("Keyboard & Mouse Input")][SerializeField][Tooltip("When any of these modifiers is pressed the arrow keys can be used for rotating the camera.")] List<Key> keyboardControlsModifiers = new List<Key> { Key.LeftAlt, Key.RightAlt, Key.LeftCtrl, Key.RightCtrl };
        [FoldoutGroup("Keyboard & Mouse Input")][SerializeField] private float mouseRotationFactor = 0.01f;
        [FoldoutGroup("Keyboard & Mouse Input")][SerializeField] private float mouseTiltFactor = 0.005f;
        [FoldoutGroup("Keyboard & Mouse Input")][SerializeField] private float mouseZoomFactor = 1;
        [FoldoutGroup("Keyboard & Mouse Input")][SerializeField] private float keyboardRotationFactor = 1;
        [FoldoutGroup("Keyboard & Mouse Input")][SerializeField] private float keyboardTiltFactor = 1;
        [FoldoutGroup("Keyboard & Mouse Input")][SerializeField] private float keyboardZoomFactor = 1;
        [FoldoutGroup("Keyboard & Mouse Input")][SerializeField] private float keyboardRepeatFactor = 60;
        [FoldoutGroup("Keyboard & Mouse Input")][SerializeField][Tooltip("When holding down a key for rotate/tilt/zoom there's a small delay between the first press action and the repeated input action. This value allows to tweak that delay.")] float keyboardRepeatDelay = 0.5f;
        [FoldoutGroup("Keyboard & Mouse Input")][SerializeField][Tooltip("Depending on your settings the camera allows to zoom slightly over the defined value. When releasing the zoom the camera will spring back to the defined value. This variable defines the speed of the spring back.")] float zoomBackSpringFactor = 20;
        [FoldoutGroup("Keyboard & Mouse Input")][SerializeField][Tooltip("When close to the border the camera will spring back if the margin is bigger than 0. This variable defines the speed of the spring back.")] float dragBackSpringFactor = 10;
        [FoldoutGroup("Keyboard & Mouse Input")][SerializeField][Tooltip("When swiping over the screen the camera will keep scrolling a while before coming to a halt. This variable limits the maximum velocity of the auto scroll.")] float autoScrollVelocityMax = 60;
        [FoldoutGroup("Keyboard & Mouse Input")][SerializeField][Tooltip("This value defines how quickly the camera comes to a halt when auto scrolling.")] float dampFactorTimeMultiplier = 2;
        [FoldoutGroup("Keyboard & Mouse Input")][SerializeField][Tooltip("When setting this flag to true, the camera will behave like a popular tower defense game. It will either go into an exclusive tilt mode, or into a combined zoom/rotate mode. When set to false, the camera will behave like a popular city building game. The camera won't pan with 2 fingers, and instead zoom, rotate and tilt are done in parallel.")] bool isPinchModeExclusive = true;
        [FoldoutGroup("Keyboard & Mouse Input")][SerializeField][Tooltip("This value should be kept at 1 for pixel perfect zoom. In case you need a non-pixel perfect, slower or faster zoom however, you can change this value. 0.5f for example will make the camera zoom half as fast as in pixel perfect mode. This value is currently tested only in perspective camera mode with translation based zoom.")] float customZoomSensitivity = 1.0f;
        [FoldoutGroup("Keyboard & Mouse Input")][SerializeField][Tooltip("Optional. When assigned, the terrain collider will be used to align items on the ground following the terrain.")] TerrainCollider terrainCollider;
        [FoldoutGroup("Keyboard & Mouse Input")][SerializeField][Tooltip("Optional. When assigned, the given transform will be moved and rotated instead of the one where this component is located on.")] Transform cameraTransform;
        [FoldoutGroup("Keyboard & Mouse Input")][SerializeField][Tooltip("When setting this flag to true, the uniform camOverdragMargin will be overrided with the values set in camOverdragMargin2d.")] bool is2dOverdragMarginEnabled;
        [FoldoutGroup("Keyboard & Mouse Input")][SerializeField][Tooltip("Using this field allows to set the horizontal overdrag to a different value than the vertical one.")] Vector2 camOverdragMargin2d = Vector2.one * 5.0f;

        [SerializeField]
        [Tooltip("A gesture may be interpreted as intended rotation in case the relative rotation angle between 2 frames becomes bigger than this value.")]
        private float rotationDetectionDeltaThreshold = 0.25f;

        [SerializeField]
        [Tooltip("Relative pinch distance must be bigger than this value in order to detect a rotation. This is to prevent errors that occur when the fingers are too close together to properly detect a clean rotation.")]
        private float rotationMinPinchDistance = 0.125f;

        [SerializeField]
        [Tooltip("The rotation mode is enabled as soon as the rotation by the user becomes bigger than this value (in degrees). The value is used to prevent micro rotations from regular jittering of the fingers to be interpreted as rotation and helps keeping the camera more steady and less jittery.")]
        private float rotationLockThreshold = 2.5f;

        [SerializeField]
        [Tooltip("After this amount of finger-movement (relative to screen size), the pinch mode is decided. E.g. whether tilt mode or regular mode is used.")]
        private float pinchModeDetectionMoveTreshold = 0.025f;

        [SerializeField]
        [Tooltip("A threshold used to detect the up or down tilting motion.")]
        private float pinchTiltModeThreshold = 0.0075f;

        [SerializeField]
        [Tooltip("The tilt sensitivity once the tilt mode has started.")]
        private float pinchTiltSpeed = 180;
        #endregion

        private bool isStarted;
        public CameraPlaneAxes CameraAxes { get => cameraAxes; set => cameraAxes = value; }
        private TouchInputController touchInputController;

        private Vector3 dragStartCamPos;
        private Vector3 cameraScrollVelocity;
        private float pinchStartCamZoomSize;
        private Vector3 pinchStartIntersectionCenter;
        private Vector3 pinchCenterCurrent;
        private float pinchDistanceCurrent;
        private float pinchAngleCurrent;
        private float pinchDistanceStart;
        private Vector3 pinchCenterCurrentLerp;
        private float pinchDistanceCurrentLerp;
        private float pinchAngleCurrentLerp;
        private bool isRotationLock = true;
        private bool isRotationActivated;
        private float pinchAngleLastFrame;
        private float pinchTiltCurrent;
        private float pinchTiltAccumulated;
        private bool isTiltModeEvaluated;
        private float pinchTiltLastFrame;
        private bool isPinchTiltMode;
        private Vector3 camVelocity = Vector3.zero;
        private Vector3 posLastFrame = Vector3.zero;

        private float timeRealDragStop;

        public bool IsAutoScrolling => cameraScrollVelocity.sqrMagnitude > float.Epsilon;

        public bool IsPinching { get; private set; }
        public bool IsDragging { get; private set; }

        public Camera Cam { get; private set; }

        public Transform Transform => cameraTransform ? cameraTransform : transform;

        private bool IsTranslationZoom => !Cam.orthographic && perspectiveZoomMode == PerspectiveZoomMode.TRANSLATION;

        public float CamZoom
        {
            get
            {
                if (Cam.orthographic) return Cam.orthographicSize;

                if (IsTranslationZoom)
                {
                    Vector3 camCenterIntersection = GetIntersectionPoint(GetCamCenterRay());
                    return Vector3.Distance(camCenterIntersection, Transform.position);
                }

                return Cam.fieldOfView;
            }
            set
            {
                if (Cam.orthographic) Cam.orthographicSize = value;
                else
                {
                    if (IsTranslationZoom)
                    {
                        Vector3 camCenterIntersection = GetIntersectionPoint(GetCamCenterRay());
                        Transform.position = camCenterIntersection - Transform.forward * value;
                    }
                    else Cam.fieldOfView = value;
                }

                ComputeCamBoundaries();
            }
        }

        public float GroundLevelOffset
        {
            get => groundLevelOffset;
            set
            {
                groundLevelOffset = value;
                ComputeCamBoundaries();
            }
        }

        public float CamZoomMin { get => camZoomMin; set => camZoomMin = value; }
        public float CamZoomMax { get => camZoomMax; set => camZoomMax = value; }
        public float CamOverzoomMargin { get => camOverzoomMargin; set => camOverzoomMargin = value; }
        public float CamFollowFactor { get => camFollowFactor; set => camFollowFactor = value; }
        public float AutoScrollDamp { get => autoScrollDamp; set => autoScrollDamp = value; }
        public AnimationCurve AutoScrollDampCurve { get => autoScrollDampCurve; set => autoScrollDampCurve = value; }
        public Vector2 BoundaryMin { get => boundaryMin; set => boundaryMin = value; }
        public Vector2 BoundaryMax { get => boundaryMax; set => boundaryMax = value; }
        public PerspectiveZoomMode PerspectiveZoomMode { get => perspectiveZoomMode; set => perspectiveZoomMode = value; }
        public bool EnableRotation { get => enableRotation; set => enableRotation = value; }
        public bool EnableTilt { get => enableTilt; set => enableTilt = value; }
        public float TiltAngleMin { get => tiltAngleMin; set => tiltAngleMin = value; }
        public float TiltAngleMax { get => tiltAngleMax; set => tiltAngleMax = value; }
        public bool EnableZoomTilt { get => enableZoomTilt; set => enableZoomTilt = value; }
        public float ZoomTiltAngleMin { get => zoomTiltAngleMin; set => zoomTiltAngleMin = value; }
        public float ZoomTiltAngleMax { get => zoomTiltAngleMax; set => zoomTiltAngleMax = value; }
        public List<Key> KeyboardControlsModifiers { get => keyboardControlsModifiers; set => keyboardControlsModifiers = value; }
        public List<RuntimePlatform> KeyboardAndMousePlatforms { get => keyboardAndMousePlatforms; set => keyboardAndMousePlatforms = value; }
        public float MouseRotationFactor { get => mouseRotationFactor; set => mouseRotationFactor = value; }
        public float MouseTiltFactor { get => mouseTiltFactor; set => mouseTiltFactor = value; }
        public float MouseZoomFactor { get => mouseZoomFactor; set => mouseZoomFactor = value; }
        public float KeyboardRotationFactor { get => keyboardRotationFactor; set => keyboardRotationFactor = value; }
        public float KeyboardTiltFactor { get => keyboardTiltFactor; set => keyboardTiltFactor = value; }
        public float KeyboardZoomFactor { get => keyboardZoomFactor; set => keyboardZoomFactor = value; }
        public float KeyboardRepeatFactor { get => keyboardRepeatFactor; set => keyboardRepeatFactor = value; }
        public float KeyboardRepeatDelay { get => keyboardRepeatDelay; set => keyboardRepeatDelay = value; }

        public Vector2 CamOverdragMargin2d
        {
            get
            {
                return is2dOverdragMarginEnabled ? camOverdragMargin2d : Vector2.one * camOverdragMargin;
            }
            set
            {
                camOverdragMargin2d = value;
                camOverdragMargin = value.x;
            }
        }

        private bool isDraggingSceneObject;

        private Plane refPlaneXY = new(new Vector3(0, 0, -1), 0);
        private Plane refPlaneXZ = new(new Vector3(0, 1, 0), 0);

        public Plane RefPlane => CameraAxes == CameraPlaneAxes.XZ_TOP_DOWN ? refPlaneXZ : refPlaneXY;

        private List<Vector3> DragCameraMoveVector { get; set; }
        private const int momentumSamplesCount = 5;

        private const float pinchDistanceForTiltBreakout = 0.05f;
        private const float pinchAccumBreakout = 0.025f;

        private Vector3 targetPositionClamped = Vector3.zero;

        public bool IsSmoothingEnabled { get; set; }

        private Vector2 CamPosMin { get; set; }
        private Vector2 CamPosMax { get; set; }

        public TerrainCollider TerrainCollider { get => terrainCollider; set => terrainCollider = value; }
        public Vector2 CameraScrollVelocity { get => cameraScrollVelocity; set => cameraScrollVelocity = value; }

        private bool showHorizonError = true;

        #region work in progress //Features that are currently being worked on, but not fully polished and documented yet. Use them at your own risk.

        private const bool enableOvertiltSpring = false; //Allows enabling the camera to spring being when being tilted over the limits.
        private const float camOvertiltMargin = 5.0f;
        private const float tiltBackSpringFactor = 30;
        private const float minOvertiltSpringPositionThreshold = 0.1f; //This value is necessary to reposition the camera and do boundary update computations while the auto spring back from overtilt is active and larger than this value.
        private const bool useUntransformedCamBoundary = false;
        private const float maxHorizonFallbackDistance = 10000; //This value may need to be tweaked when using the camera in a low-tilt, see above the horizon scenario.
        private const bool useOldScrollDamp = false;

        #endregion

        #region Keyboard & Mouse Input

        private Vector3 mousePosLastFrame = Vector3.zero;
        private Vector3 mouseRotationCenter = Vector3.zero;
        private float timeKeyDown;

        #endregion

        public void Awake()
        {
            Cam = GetComponent<Camera>();

            IsSmoothingEnabled = true;
            touchInputController = GetComponent<TouchInputController>();
            dragStartCamPos = Vector3.zero;
            cameraScrollVelocity = Vector3.zero;
            timeRealDragStop = 0;
            pinchStartCamZoomSize = 0;
            IsPinching = false;
            IsDragging = false;
            DragCameraMoveVector = new List<Vector3>();
            refPlaneXY = new Plane(new Vector3(0, 0, -1), groundLevelOffset);
            refPlaneXZ = new Plane(new Vector3(0, 1, 0), -groundLevelOffset);
            if (EnableZoomTilt) ResetZoomTilt();

            ComputeCamBoundaries();

            if (CamZoomMax < CamZoomMin)
            {
                Debug.LogWarning("The defined max camera zoom (" + CamZoomMax + ") is smaller than the defined min (" + CamZoomMin + "). Automatically switching the values.");
                (CamZoomMin, CamZoomMax) = (CamZoomMax, CamZoomMin);
            }

            //Errors for certain incorrect settings.
            string cameraAxesError = CheckCameraAxesErrors();
            if (!string.IsNullOrEmpty(cameraAxesError)) Debug.LogError(cameraAxesError);
        }

        public void Start()
        {
            touchInputController.OnInputClick += InputControllerOnInputClick;
            touchInputController.OnDragStart += InputControllerOnDragStart;
            touchInputController.OnDragUpdate += InputControllerOnDragUpdate;
            touchInputController.OnDragStop += InputControllerOnDragStop;
            touchInputController.OnFingerDown += InputControllerOnFingerDown;
            touchInputController.OnFingerUp += InputControllerOnFingerUp;
            touchInputController.OnPinchStart += InputControllerOnPinchStart;
            touchInputController.OnPinchUpdateExtended += InputControllerOnPinchUpdate;
            touchInputController.OnPinchStop += InputControllerOnPinchStop;
            isStarted = true;
            StartCoroutine(InitCamBoundariesDelayed());
        }

        public void OnDestroy()
        {
            if (isStarted)
            {
                touchInputController.OnInputClick -= InputControllerOnInputClick;
                touchInputController.OnDragStart -= InputControllerOnDragStart;
                touchInputController.OnDragUpdate -= InputControllerOnDragUpdate;
                touchInputController.OnDragStop -= InputControllerOnDragStop;
                touchInputController.OnFingerDown -= InputControllerOnFingerDown;
                touchInputController.OnFingerUp -= InputControllerOnFingerUp;
                touchInputController.OnPinchStart -= InputControllerOnPinchStart;
                touchInputController.OnPinchUpdateExtended -= InputControllerOnPinchUpdate;
                touchInputController.OnPinchStop -= InputControllerOnPinchStop;
            }
        }

        #region public interface

        /// Method for resetting the camera boundaries. This method may need to be invoked
        /// when resetting the camera transform (rotation, tilt) by code for example.
        public void ResetCameraBoundaries()
        {
            ComputeCamBoundaries();
        }

        /// This method tilts the camera based on the values
        /// defined for the zoom tilt mode.
        public void ResetZoomTilt()
        {
            UpdateTiltForAutoTilt(CamZoom);
        }

        /// Helper method for retrieving the world position of the
        /// finger with id 0. This method may only return a valid value when
        /// there is at least 1 finger touching the device.
        public Vector3 GetFinger0PosWorld()
        {
            Vector3 posWorld = Vector3.zero;
            if (TouchWrapper.TouchCount > 0)
            {
                Vector3 fingerPos = TouchWrapper.Touch0.Position;
                RaycastGround(Cam.ScreenPointToRay(fingerPos), out posWorld);
            }

            return posWorld;
        }

        /// Method for performing a raycast against either the refplane, or
        /// against a terrain-collider in case the collider is set.
        public bool RaycastGround(Ray ray, out Vector3 hitPoint)
        {
            bool hitSuccess;
            hitPoint = Vector3.zero;
            if (TerrainCollider != null)
            {
                hitSuccess = TerrainCollider.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity);
                if (hitSuccess)
                {
                    hitPoint = hitInfo.point;
                }
            }
            else
            {
                hitSuccess = RefPlane.Raycast(ray, out float hitDistance);
                if (hitSuccess)
                {
                    hitPoint = ray.GetPoint(hitDistance);
                }
            }

            return hitSuccess;
        }

        /// Method for retrieving the intersection-point between the given ray and the ref plane.
        public Vector3 GetIntersectionPoint(Ray ray)
        {
            bool success = RefPlane.Raycast(ray, out float distance);
            if (!success || (!Cam.orthographic && distance > maxHorizonFallbackDistance))
            {
                if (showHorizonError)
                {
                    Debug.LogError("Failed to compute intersection between camera ray and reference plane. Make sure the camera Axes are set up correctly.");
                    showHorizonError = false;
                }

                //Fallback: Compute a sphere-cap on the ground and use the border point at the direction of the ray as maximum point in the distance.
                Vector3 rayOriginProjected = UnprojectVector2(ProjectVector3(ray.origin));
                Vector3 rayDirProjected = UnprojectVector2(ProjectVector3(ray.direction));
                return rayOriginProjected + rayDirProjected.normalized * maxHorizonFallbackDistance;
            }

            return ray.origin + ray.direction * distance;
        }

        /// Custom planet intersection method that doesn't take into account rays parallel to the plane or rays shooting in the wrong direction and thus never hitting.
        /// May yield slightly better performance however and should be safe for use when the camera setup is correct (e.g. axes set correctly in this script, and camera actually pointing towards floor).
        public Vector3 GetIntersectionPointUnsafe(Ray ray)
        {
            float distance = Vector3.Dot(RefPlane.normal, Vector3.zero - ray.origin) / Vector3.Dot(RefPlane.normal, ray.origin + ray.direction - ray.origin);
            return ray.origin + ray.direction * distance;
        }

        /// Returns whether the camera is at the defined boundary.
        public bool GetIsBoundaryPosition(Vector3 testPosition)
        {
            return GetIsBoundaryPosition(testPosition, Vector2.zero);
        }

        /// Returns whether the camera is at the defined boundary.
        public bool GetIsBoundaryPosition(Vector3 testPosition, Vector2 margin)
        {
            bool isBoundaryPosition = false;
            switch (cameraAxes)
            {
                case CameraPlaneAxes.XY_2D_SIDESCROLL:
                    isBoundaryPosition = testPosition.x <= CamPosMin.x + margin.x;
                    isBoundaryPosition |= testPosition.x >= CamPosMax.x - margin.x;
                    isBoundaryPosition |= testPosition.y <= CamPosMin.y + margin.y;
                    isBoundaryPosition |= testPosition.y >= CamPosMax.y - margin.y;
                    break;
                case CameraPlaneAxes.XZ_TOP_DOWN:
                    isBoundaryPosition = testPosition.x <= CamPosMin.x + margin.x;
                    isBoundaryPosition |= testPosition.x >= CamPosMax.x - margin.x;
                    isBoundaryPosition |= testPosition.z <= CamPosMin.y + margin.y;
                    isBoundaryPosition |= testPosition.z >= CamPosMax.y - margin.y;
                    break;
            }

            return isBoundaryPosition;
        }

        /// Returns a position that is clamped to the defined boundary.
        public Vector3 GetClampToBoundaries(Vector3 newPosition, bool includeSpringBackMargin = false)
        {
            Vector2 margin = Vector2.zero;
            if (includeSpringBackMargin)
            {
                margin = CamOverdragMargin2d;
            }

            switch (cameraAxes)
            {
                case CameraPlaneAxes.XY_2D_SIDESCROLL:
                    newPosition.x = Mathf.Clamp(newPosition.x, CamPosMin.x + margin.x, CamPosMax.x - margin.x);
                    newPosition.y = Mathf.Clamp(newPosition.y, CamPosMin.y + margin.y, CamPosMax.y - margin.y);
                    break;
                case CameraPlaneAxes.XZ_TOP_DOWN:
                    newPosition.x = Mathf.Clamp(newPosition.x, CamPosMin.x + margin.x, CamPosMax.x - margin.x);
                    newPosition.z = Mathf.Clamp(newPosition.z, CamPosMin.y + margin.y, CamPosMax.y - margin.y);
                    break;
            }

            return newPosition;
        }

        public void OnDragSceneObject()
        {
            isDraggingSceneObject = true;
        }

        public string CheckCameraAxesErrors()
        {
            string error = "";
            if (Transform.forward == Vector3.down && cameraAxes != CameraPlaneAxes.XZ_TOP_DOWN)
            {
                error = "Camera is pointing down but the cameraAxes is not set to TOP_DOWN. Make sure to set the cameraAxes variable properly.";
            }

            if (Transform.forward == Vector3.forward && cameraAxes != CameraPlaneAxes.XY_2D_SIDESCROLL)
            {
                error = "Camera is pointing sidewards but the cameraAxes is not set to 2D_SIDESCROLL. Make sure to set the cameraAxes variable properly.";
            }

            return error;
        }

        /// Helper method that unprojects the given Vector2 to a Vector3
        /// according to the camera axes setting.
        public Vector3 UnprojectVector2(Vector2 v2, float offset = 0)
        {
            return CameraAxes == CameraPlaneAxes.XY_2D_SIDESCROLL ? new Vector3(v2.x, v2.y, offset) : new Vector3(v2.x, offset, v2.y);
        }

        public Vector2 ProjectVector3(Vector3 v3)
        {
            return CameraAxes == CameraPlaneAxes.XY_2D_SIDESCROLL ? new Vector2(v3.x, v3.y) : new Vector2(v3.x, v3.z);
        }

        #endregion

        private IEnumerator InitCamBoundariesDelayed()
        {
            yield return null;
            ComputeCamBoundaries();
        }

        /// MonoBehaviour method override to assign proper default values depending on
        /// the camera parameters and orientation.
        private void Reset()
        {
            //Compute camera tilt to find out the camera orientation.
            Vector3 camForwardOnPlane = Vector3.Cross(Vector3.up, GetTiltRotationAxis());
            float tiltAngle = Vector3.Angle(camForwardOnPlane, -Transform.forward);
            CameraAxes = tiltAngle < 45 ? CameraPlaneAxes.XY_2D_SIDESCROLL : CameraPlaneAxes.XZ_TOP_DOWN;

            //Compute zoom default values based on the camera type.
            Camera cameraComponent = GetComponent<Camera>();
            if (cameraComponent.orthographic)
            {
                CamZoomMin = 4;
                CamZoomMax = 13;
                CamOverzoomMargin = 1;
            }
            else
            {
                CamZoomMin = 5;
                CamZoomMax = 40;
                CamOverzoomMargin = 3;
                PerspectiveZoomMode = PerspectiveZoomMode.TRANSLATION;
            }
        }

        /// Method that does all the computation necessary when the pinch gesture of the user
        /// has changed.
        private void UpdatePinch(float deltaTime)
        {
            if (IsPinching)
            {
                if (isTiltModeEvaluated)
                {
                    if (isPinchTiltMode || !isPinchModeExclusive)
                    {
                        //Tilt
                        float pinchTiltDelta = pinchTiltLastFrame - pinchTiltCurrent;
                        UpdateCameraTilt(pinchTiltDelta * pinchTiltSpeed);
                        pinchTiltLastFrame = pinchTiltCurrent;
                    }

                    if (!isPinchTiltMode || !isPinchModeExclusive)
                    {
                        if (isRotationActivated && isRotationLock && Mathf.Abs(pinchAngleCurrent) >= rotationLockThreshold)
                        {
                            isRotationLock = false;
                        }

                        if (IsSmoothingEnabled)
                        {
                            float lerpFactor = Mathf.Clamp01(Time.unscaledDeltaTime * camFollowFactor);
                            pinchDistanceCurrentLerp = Mathf.Lerp(pinchDistanceCurrentLerp, pinchDistanceCurrent, lerpFactor);
                            pinchCenterCurrentLerp = Vector3.Lerp(pinchCenterCurrentLerp, pinchCenterCurrent, lerpFactor);
                            if (!isRotationLock)
                            {
                                pinchAngleCurrentLerp = Mathf.Lerp(pinchAngleCurrentLerp, pinchAngleCurrent, lerpFactor);
                            }
                        }
                        else
                        {
                            pinchDistanceCurrentLerp = pinchDistanceCurrent;
                            pinchCenterCurrentLerp = pinchCenterCurrent;
                            if (!isRotationLock) pinchAngleCurrentLerp = pinchAngleCurrent;
                        }

                        //Rotation
                        if (isRotationActivated && !isRotationLock)
                        {
                            float pinchAngleDelta = pinchAngleCurrentLerp - pinchAngleLastFrame;
                            Vector3 rotationAxis = GetRotationAxis();
                            Transform.RotateAround(pinchCenterCurrent, rotationAxis, pinchAngleDelta);
                            pinchAngleLastFrame = pinchAngleCurrentLerp;
                            ComputeCamBoundaries();
                        }

                        //Zoom
                        float zoomFactor = pinchDistanceStart / Mathf.Max((pinchDistanceCurrentLerp - pinchDistanceStart) * customZoomSensitivity + pinchDistanceStart, 0.0001f);
                        float cameraSize = pinchStartCamZoomSize * zoomFactor;
                        cameraSize = Mathf.Clamp(cameraSize, camZoomMin - camOverzoomMargin, camZoomMax + camOverzoomMargin);
                        if (enableZoomTilt)
                        {
                            UpdateTiltForAutoTilt(cameraSize);
                        }

                        CamZoom = cameraSize;
                    }

                    //Position update.
                    DoPositionUpdateForTilt(false);
                }
            }
            else
            {
                //Spring back.
                if (EnableTilt && enableOvertiltSpring)
                {
                    float overtiltSpringValue = ComputeOvertiltSpringBackFactor(camOvertiltMargin);
                    if (Mathf.Abs(overtiltSpringValue) > minOvertiltSpringPositionThreshold)
                    {
                        UpdateCameraTilt(overtiltSpringValue * deltaTime * tiltBackSpringFactor);
                        DoPositionUpdateForTilt(true);
                    }
                }
            }
        }

        private void UpdateTiltForAutoTilt(float newCameraSize)
        {
            float zoomProgress = Mathf.Clamp01((newCameraSize - camZoomMin) / (camZoomMax - camZoomMin));
            float tiltTarget = Mathf.Lerp(zoomTiltAngleMin, zoomTiltAngleMax, zoomProgress);
            float tiltAngleDiff = tiltTarget - GetCurrentTiltAngleDeg(GetTiltRotationAxis());
            UpdateCameraTilt(tiltAngleDiff);
        }

        /// Method that computes the updated camera position when the user tilts the camera.
        private void DoPositionUpdateForTilt(bool isSpringBack)
        {
            //Position update.
            Vector3 intersectionDragCurrent;
            if (isSpringBack || (isPinchTiltMode && isPinchModeExclusive))
            {
                intersectionDragCurrent = GetIntersectionPoint(GetCamCenterRay()); //In exclusive tilt mode always rotate around the screen center.
            }
            else
            {
                intersectionDragCurrent = GetIntersectionPoint(Cam.ScreenPointToRay(pinchCenterCurrentLerp));
            }

            Vector3 dragUpdateVector = intersectionDragCurrent - pinchStartIntersectionCenter;
            if (isSpringBack && !isPinchModeExclusive) dragUpdateVector = Vector3.zero;

            Vector3 targetPos = GetClampToBoundaries(Transform.position - dragUpdateVector);

            Transform.position = targetPos; //Disable smooth follow for the pinch-move update to prevent oscillation during the zoom phase.
            SetTargetPosition(targetPos);
        }

        /// Helper method for computing the tilt spring back.
        private float ComputeOvertiltSpringBackFactor(float margin)
        {
            float springBackValue = 0;
            Vector3 rotationAxis = GetTiltRotationAxis();
            float tiltAngle = GetCurrentTiltAngleDeg(rotationAxis);
            if (tiltAngle < tiltAngleMin + margin)
            {
                springBackValue = tiltAngleMin + margin - tiltAngle;
            }
            else if (tiltAngle > tiltAngleMax - margin)
            {
                springBackValue = tiltAngleMax - margin - tiltAngle;
            }

            return springBackValue;
        }

        /// Method that computes all necessary parameters for a tilt update caused by the user's tilt gesture.
        private void UpdateCameraTilt(float angle)
        {
            Vector3 rotationAxis = GetTiltRotationAxis();
            Vector3 rotationPoint = GetIntersectionPoint(new Ray(Transform.position, Transform.forward));
            Transform.RotateAround(rotationPoint, rotationAxis, angle);
            ClampCameraTilt(rotationPoint, rotationAxis);
            ComputeCamBoundaries();
        }

        /// Method that ensures that all limits are met when the user tilts the camera.
        private void ClampCameraTilt(Vector3 rotationPoint, Vector3 rotationAxis)
        {
            float tiltAngle = GetCurrentTiltAngleDeg(rotationAxis);
            if (tiltAngle < tiltAngleMin)
            {
                float tiltClampDiff = tiltAngleMin - tiltAngle;
                Transform.RotateAround(rotationPoint, rotationAxis, tiltClampDiff);
            }
            else if (tiltAngle > tiltAngleMax)
            {
                float tiltClampDiff = tiltAngleMax - tiltAngle;
                Transform.RotateAround(rotationPoint, rotationAxis, tiltClampDiff);
            }
        }

        /// Method to get the current tilt angle of the camera.
        private float GetCurrentTiltAngleDeg(Vector3 rotationAxis)
        {
            Vector3 camForwardOnPlane = Vector3.Cross(RefPlane.normal, rotationAxis);
            float tiltAngle = Vector3.Angle(camForwardOnPlane, -Transform.forward);
            return tiltAngle;
        }

        /// Returns the rotation axis of the camera. This purely depends
        /// on the defined camera axis.
        private Vector3 GetRotationAxis()
        {
            return RefPlane.normal;
        }

        /// Returns the rotation of the camera.
        private float GetRotationDeg()
        {
            return CameraAxes == CameraPlaneAxes.XY_2D_SIDESCROLL ? Transform.rotation.eulerAngles.z : Transform.rotation.eulerAngles.y;
        }

        /// Returns the tilt rotation axis.
        private Vector3 GetTiltRotationAxis()
        {
            Vector3 rotationAxis = Transform.right;
            return rotationAxis;
        }

        /// Method to compute all the necessary updates when the user moves the camera.
        private void UpdatePosition(float deltaTime)
        {
            if (IsPinching && isPinchTiltMode)
            {
                return;
            }

            if (IsDragging || IsPinching)
            {
                Vector3 posOld = Transform.position;
                Transform.position = IsSmoothingEnabled ? Vector3.Lerp(Transform.position, targetPositionClamped, Mathf.Clamp01(Time.unscaledDeltaTime * camFollowFactor)) : targetPositionClamped;

                DragCameraMoveVector.Add((posOld - Transform.position) / Time.unscaledDeltaTime);
                if (DragCameraMoveVector.Count > momentumSamplesCount)
                {
                    DragCameraMoveVector.RemoveAt(0);
                }
            }

            Vector2 autoScrollVector = -cameraScrollVelocity * deltaTime;
            Vector3 camPos = Transform.position;
            switch (cameraAxes)
            {
                case CameraPlaneAxes.XY_2D_SIDESCROLL:
                    camPos.x += autoScrollVector.x;
                    camPos.y += autoScrollVector.y;
                    break;
                case CameraPlaneAxes.XZ_TOP_DOWN:
                    camPos.x += autoScrollVector.x;
                    camPos.z += autoScrollVector.y;
                    break;
            }

            if (!IsDragging && !IsPinching)
            {
                Vector3 overdragSpringVector = ComputeOverDragSpringBackVector(camPos, CamOverdragMargin2d, ref cameraScrollVelocity);
                if (overdragSpringVector.magnitude > float.Epsilon)
                {
                    camPos += Time.unscaledDeltaTime * overdragSpringVector * dragBackSpringFactor;
                }
            }

            Transform.position = GetClampToBoundaries(camPos);
        }

        /// Computes the camera drag spring back when the user is close to a boundary.
        private Vector3 ComputeOverDragSpringBackVector(Vector3 camPos, Vector2 margin, ref Vector3 currentCamScrollVelocity)
        {
            Vector3 springBackVector = Vector3.zero;
            if (camPos.x < CamPosMin.x + margin.x)
            {
                springBackVector.x = CamPosMin.x + margin.x - camPos.x;
                currentCamScrollVelocity.x = 0;
            }
            else if (camPos.x > CamPosMax.x - margin.x)
            {
                springBackVector.x = CamPosMax.x - margin.x - camPos.x;
                currentCamScrollVelocity.x = 0;
            }

            switch (cameraAxes)
            {
                case CameraPlaneAxes.XY_2D_SIDESCROLL:
                    if (camPos.y < CamPosMin.y + margin.y)
                    {
                        springBackVector.y = CamPosMin.y + margin.y - camPos.y;
                        currentCamScrollVelocity.y = 0;
                    }
                    else if (camPos.y > CamPosMax.y - margin.y)
                    {
                        springBackVector.y = CamPosMax.y - margin.y - camPos.y;
                        currentCamScrollVelocity.y = 0;
                    }

                    break;
                case CameraPlaneAxes.XZ_TOP_DOWN:
                    if (camPos.z < CamPosMin.y + margin.y)
                    {
                        springBackVector.z = CamPosMin.y + margin.y - camPos.z;
                        currentCamScrollVelocity.z = 0;
                    }
                    else if (camPos.z > CamPosMax.y - margin.y)
                    {
                        springBackVector.z = CamPosMax.y - margin.y - camPos.z;
                        currentCamScrollVelocity.z = 0;
                    }

                    break;
            }

            return springBackVector;
        }

        /// Internal helper method for setting the desired cam position.
        private void SetTargetPosition(Vector3 newPositionClamped)
        {
            targetPositionClamped = newPositionClamped;
        }

        /// Method that computes the cam boundaries used for the current rotation and tilt of the camera.
        /// This computation is complex and needs to be invoked when the camera is rotated or tilted.
        private void ComputeCamBoundaries()
        {
            refPlaneXY = new Plane(Vector3.back, groundLevelOffset);
            refPlaneXZ = new Plane(Vector3.up, -groundLevelOffset);

            float camRotation = GetRotationDeg();

            Vector2 camProjectedMin = Vector2.zero;
            Vector2 camProjectedMax = Vector2.zero;

            Vector2 camProjectedCenter = GetIntersection2d(new Ray(Transform.position, -RefPlane.normal)); //Get camera position projected vertically onto the ref plane. This allows to compute the offset that arises from camera tilt.

            //Fetch camera boundary as world-space coordinates projected to the ground.
            Vector2 camRight = GetIntersection2d(Cam.ScreenPointToRay(new Vector3(Screen.width, Screen.height * 0.5f, 0)));
            Vector2 camLeft = GetIntersection2d(Cam.ScreenPointToRay(new Vector3(0, Screen.height * 0.5f, 0)));
            Vector2 camUp = GetIntersection2d(Cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height, 0)));
            Vector2 camDown = GetIntersection2d(Cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, 0, 0)));
            camProjectedMin = GetVector2Min(camRight, camLeft, camUp, camDown);
            camProjectedMax = GetVector2Max(camRight, camLeft, camUp, camDown);

            Vector2 projectionCorrectionMin = camProjectedCenter - camProjectedMin;
            Vector2 projectionCorrectionMax = camProjectedCenter - camProjectedMax;

            CamPosMin = boundaryMin + projectionCorrectionMin;
            CamPosMax = boundaryMax + projectionCorrectionMax;

            Vector2 margin = CamOverdragMargin2d;
            if (CamPosMax.x - CamPosMin.x < margin.x * 2)
            {
                float midPoint = (CamPosMax.x + CamPosMin.x) * 0.5f;
                CamPosMax = new Vector2(midPoint + margin.x, CamPosMax.y);
                CamPosMin = new Vector2(midPoint - margin.x, CamPosMin.y);
            }

            if (CamPosMax.y - CamPosMin.y < margin.y * 2)
            {
                float midPoint = (CamPosMax.y + CamPosMin.y) * 0.5f;
                CamPosMax = new Vector2(CamPosMax.x, midPoint + margin.y);
                CamPosMin = new Vector2(CamPosMin.x, midPoint - margin.y);
            }
        }

        /// Method for retrieving the intersection of the given ray with the defined ground
        /// in 2d space.
        private Vector2 GetIntersection2d(Ray ray)
        {
            Vector3 intersection3d = GetIntersectionPoint(ray);
            Vector2 intersection2d = new Vector2(intersection3d.x, 0);
            switch (cameraAxes)
            {
                case CameraPlaneAxes.XY_2D_SIDESCROLL:
                    intersection2d.y = intersection3d.y;
                    break;
                case CameraPlaneAxes.XZ_TOP_DOWN:
                    intersection2d.y = intersection3d.z;
                    break;
            }

            return intersection2d;
        }

        private Vector2 GetVector2Min(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            return new Vector2(Mathf.Min(v0.x, v1.x, v2.x, v3.x), Mathf.Min(v0.y, v1.y, v2.y, v3.y));
        }

        private Vector2 GetVector2Max(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            return new Vector2(Mathf.Max(v0.x, v1.x, v2.x, v3.x), Mathf.Max(v0.y, v1.y, v2.y, v3.y));
        }

        public void Update()
        {
            #region auto scroll code

            if (cameraScrollVelocity.sqrMagnitude > float.Epsilon)
            {
                float timeSinceDragStop = Time.realtimeSinceStartup - timeRealDragStop;
                float dampFactor = Mathf.Clamp01(timeSinceDragStop * dampFactorTimeMultiplier);
                float camScrollVel = cameraScrollVelocity.magnitude;
                float camScrollVelRelative = camScrollVel / autoScrollVelocityMax;
                Vector3 camVelDamp = dampFactor * cameraScrollVelocity.normalized * autoScrollDamp * Time.unscaledDeltaTime;
                camVelDamp *= EvaluateAutoScrollDampCurve(Mathf.Clamp01(1.0f - camScrollVelRelative));
                if (camVelDamp.sqrMagnitude >= cameraScrollVelocity.sqrMagnitude)
                {
                    cameraScrollVelocity = Vector3.zero;
                }
                else
                {
                    cameraScrollVelocity -= camVelDamp;
                }
            }

            #endregion
        }

        public void LateUpdate()
        {
            //Pinch & Translation
            UpdatePinch(Time.unscaledDeltaTime);
            UpdatePosition(Time.unscaledDeltaTime);

            #region Keyboard & Mouse Input

            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            bool noTouches = Touchscreen.current == null || Touchscreen.current.touches.Count == 0;

            // Ensure hardware is connected and valid platform
            if (keyboard != null && mouse != null && keyboardAndMousePlatforms.Contains(Application.platform) && noTouches)
            {
                Vector2 scrollRaw = mouse.scroll.ReadValue();
                float scrollValue = 0;
                // Use a small threshold to avoid drift
                if (Mathf.Abs(scrollRaw.y) > 0.01f) scrollValue = Mathf.Sign(scrollRaw.y) * 0.1f;
                float mouseScrollDelta = scrollValue * mouseZoomFactor;

                Vector2 currentMousePos = mouse.position.ReadValue();
                Vector3 mousePosDelta = mousePosLastFrame - (Vector3)currentMousePos;

                bool isEditorInputRotate = false;
                bool isEditorInputTilt = false;

                // Right Click Down (Start Rotation)
                if (mouse.rightButton.wasPressedThisFrame)
                {
                    mouseRotationCenter = Pointer.current.position.value;
                }

                // Right Click Hold (Rotating)
                if (mouse.rightButton.isPressed)
                {
                    mouseScrollDelta = mousePosDelta.x * mouseRotationFactor;
                    isEditorInputRotate = true;
                }
                // Middle Click Hold (Tilting)
                else if (mouse.middleButton.isPressed)
                {
                    mouseScrollDelta = mousePosDelta.y * mouseTiltFactor;
                    isEditorInputTilt = true;
                }

                // 3. Modifier Keys (Check List<Key>)
                bool anyModifierPressed = false;
                foreach (var modKey in keyboardControlsModifiers)
                {
                    if (keyboard[modKey].isPressed) { anyModifierPressed = true; break; }
                }
                if (anyModifierPressed)
                {
                    if (GetKeyWithRepeat(Key.NumpadPlus, out float timeFactor))
                    {
                        mouseScrollDelta = 0.05f * keyboardZoomFactor * timeFactor;
                    }
                    else if (GetKeyWithRepeat(Key.NumpadMinus, out timeFactor))
                    {
                        mouseScrollDelta = -0.05f * keyboardZoomFactor * timeFactor;
                    }
                    else if (GetKeyWithRepeat(Key.LeftArrow, out timeFactor))
                    {
                        mouseScrollDelta = 0.05f * keyboardRotationFactor * timeFactor;
                        isEditorInputRotate = true;
                        mouseRotationCenter = currentMousePos;
                    }
                    else if (GetKeyWithRepeat(Key.RightArrow, out timeFactor))
                    {
                        mouseScrollDelta = -0.05f * keyboardRotationFactor * timeFactor;
                        isEditorInputRotate = true;
                        mouseRotationCenter = currentMousePos;
                    }
                    else if (GetKeyWithRepeat(Key.UpArrow, out timeFactor))
                    {
                        mouseScrollDelta = 0.05f * keyboardTiltFactor * timeFactor;
                        isEditorInputTilt = true;
                    }
                    else if (GetKeyWithRepeat(Key.DownArrow, out timeFactor))
                    {
                        mouseScrollDelta = -0.05f * keyboardTiltFactor * timeFactor;
                        isEditorInputTilt = true;
                    }
                }

                // Transform Logic (Rotation / Tilt / Zoom)
                if (!Mathf.Approximately(mouseScrollDelta, 0))
                {
                    if (isEditorInputRotate)
                    {
                        if (EnableRotation)
                        {
                            Vector3 rotationAxis = GetRotationAxis();
                            Vector3 intersectionScreenCenter = GetIntersectionPoint(Cam.ScreenPointToRay(mouseRotationCenter));
                            Transform.RotateAround(intersectionScreenCenter, rotationAxis, mouseScrollDelta * 100);
                            ComputeCamBoundaries();
                        }
                    }
                    else if (isEditorInputTilt)
                    {
                        if (EnableTilt)
                        {
                            UpdateCameraTilt(Mathf.Sign(mouseScrollDelta) * Mathf.Min(25, Mathf.Abs(mouseScrollDelta * 100)));
                        }
                    }
                    else // ZOOM LOGIC
                    {
                        float editorZoomFactor = Cam.orthographic ? 15 : (IsTranslationZoom ? 30 : 100);
                        float zoomAmount = mouseScrollDelta * editorZoomFactor;
                        float camSizeDiff = DoEditorCameraZoom(zoomAmount);

                        Vector3 intersectionScreenCenter = GetIntersectionPoint(Cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0)));
                        Vector3 pinchFocusVector = GetIntersectionPoint(Cam.ScreenPointToRay(currentMousePos)) - intersectionScreenCenter;

                        float multiplier = 1.0f / CamZoom * camSizeDiff;
                        Transform.position += pinchFocusVector * multiplier;
                    }
                }

                // Quick Zoom Presets
                for (int i = 0; i < 3; ++i)
                {
                    Key digitKey = (Key)((int)Key.Digit1 + i);
                    if (keyboard[digitKey].wasPressedThisFrame)
                    {
                        StartCoroutine(ZoomToTargetValueCoroutine(Mathf.Lerp(CamZoomMin, CamZoomMax, i / 2.0f)));
                    }
                }
            }

            #endregion

            // When the camera is zoomed in further than the defined normal value, it will snap back to normal using the code below.
            if (!IsPinching && !IsDragging)
            {
                float camZoomDeltaToNormal = 0;
                if (CamZoom > camZoomMax) camZoomDeltaToNormal = CamZoom - camZoomMax;
                else if (CamZoom < camZoomMin) camZoomDeltaToNormal = CamZoom - camZoomMin;

                if (!Mathf.Approximately(camZoomDeltaToNormal, 0))
                {
                    float cameraSizeCorrection = Mathf.Lerp(0, camZoomDeltaToNormal, zoomBackSpringFactor * Time.unscaledDeltaTime);
                    if (Mathf.Abs(cameraSizeCorrection) > Mathf.Abs(camZoomDeltaToNormal)) cameraSizeCorrection = camZoomDeltaToNormal;
                    CamZoom -= cameraSizeCorrection;
                }
            }

            mousePosLastFrame = Pointer.current.position.value;
            camVelocity = (Transform.position - posLastFrame) / Time.unscaledDeltaTime;
            posLastFrame = Transform.position;
        }

        /// Editor helper code.
        private float DoEditorCameraZoom(float amount)
        {
            float newCamZoom = CamZoom - amount;
            newCamZoom = Mathf.Clamp(newCamZoom, camZoomMin, camZoomMax);
            float camSizeDiff = CamZoom - newCamZoom;
            if (enableZoomTilt) UpdateTiltForAutoTilt(newCamZoom);

            CamZoom = newCamZoom;
            return camSizeDiff;
        }

        /// Helper method used for auto scroll.
        private float EvaluateAutoScrollDampCurve(float t)
        {
            if (autoScrollDampCurve == null || autoScrollDampCurve.length == 0)
            {
                return 1;
            }

            return autoScrollDampCurve.Evaluate(t);
        }

        private void InputControllerOnFingerDown(Vector3 pos)
        {
            cameraScrollVelocity = Vector3.zero;
        }

        private void InputControllerOnFingerUp()
        {
            isDraggingSceneObject = false;
        }

        private Vector3 GetDragVector(Vector3 dragPosStart, Vector3 dragPosCurrent)
        {
            Vector3 intersectionDragStart = GetIntersectionPoint(Cam.ScreenPointToRay(dragPosStart));
            Vector3 intersectionDragCurrent = GetIntersectionPoint(Cam.ScreenPointToRay(dragPosCurrent));
            return intersectionDragCurrent - intersectionDragStart;
        }

        /// Helper method that computes the suggested auto cam velocity from
        /// the last few frames of the user drag.
        private Vector3 GetVelocityFromMoveHistory()
        {
            Vector3 momentum = Vector3.zero;
            if (DragCameraMoveVector.Count > 0)
            {
                for (int i = 0; i < DragCameraMoveVector.Count; ++i)
                {
                    momentum += DragCameraMoveVector[i];
                }

                momentum /= DragCameraMoveVector.Count;
            }

            if (CameraAxes == CameraPlaneAxes.XZ_TOP_DOWN)
            {
                momentum.y = momentum.z;
                momentum.z = 0;
            }

            return momentum;
        }

        private void InputControllerOnDragStart(Vector3 dragPosStart, bool isLongTap)
        {
            if (TerrainCollider != null)
            {
                UpdateRefPlaneForTerrain(dragPosStart);
            }

            if (!isDraggingSceneObject)
            {
                cameraScrollVelocity = Vector3.zero;
                dragStartCamPos = Transform.position;
                IsDragging = true;
                DragCameraMoveVector.Clear();
                SetTargetPosition(Transform.position);
            }
        }

        private void InputControllerOnDragUpdate(Vector3 dragPosStart, Vector3 dragPosCurrent, Vector3 correctionOffset)
        {
            if (!isDraggingSceneObject)
            {
                Vector3 dragVector = GetDragVector(dragPosStart, dragPosCurrent + correctionOffset);
                Vector3 posNewClamped = GetClampToBoundaries(dragStartCamPos - dragVector);
                SetTargetPosition(posNewClamped);
            }
            else
            {
                IsDragging = false;
            }
        }

        private void InputControllerOnDragStop(Vector3 dragStopPos, Vector3 dragFinalMomentum)
        {
            if (!isDraggingSceneObject)
            {
                if (useOldScrollDamp)
                {
                    cameraScrollVelocity = GetVelocityFromMoveHistory();
                    if (cameraScrollVelocity.sqrMagnitude >= autoScrollVelocityMax * autoScrollVelocityMax)
                    {
                        cameraScrollVelocity = cameraScrollVelocity.normalized * autoScrollVelocityMax;
                    }
                }

                cameraScrollVelocity = -ProjectVector3(camVelocity) * 0.5f;

                timeRealDragStop = Time.realtimeSinceStartup;
                DragCameraMoveVector.Clear();
            }

            IsDragging = false;
        }

        private void InputControllerOnPinchStart(Vector3 pinchCenter, float pinchDistance)
        {
            if (TerrainCollider != null)
            {
                UpdateRefPlaneForTerrain(pinchCenter);
            }

            pinchStartCamZoomSize = CamZoom;
            pinchStartIntersectionCenter = GetIntersectionPoint(Cam.ScreenPointToRay(pinchCenter));

            pinchCenterCurrent = pinchCenter;
            pinchDistanceCurrent = pinchDistance;
            pinchDistanceStart = pinchDistance;

            pinchCenterCurrentLerp = pinchCenter;
            pinchDistanceCurrentLerp = pinchDistance;

            SetTargetPosition(Transform.position);
            IsPinching = true;
            isRotationActivated = false;
            ResetPinchRotation(0);

            pinchTiltCurrent = 0;
            pinchTiltAccumulated = 0;
            pinchTiltLastFrame = 0;
            isTiltModeEvaluated = false;
            isPinchTiltMode = false;

            if (!EnableTilt)
            {
                isTiltModeEvaluated = true; //Early out of this evaluation in case tilt is not enabled.
            }
        }

        private void InputControllerOnPinchUpdate(PinchUpdateData pinchUpdateData)
        {
            if (EnableTilt)
            {
                pinchTiltCurrent += pinchUpdateData.pinchTiltDelta;
                pinchTiltAccumulated += Mathf.Abs(pinchUpdateData.pinchTiltDelta);

                if (!isTiltModeEvaluated && pinchUpdateData.pinchTotalFingerMovement > pinchModeDetectionMoveTreshold)
                {
                    isPinchTiltMode = Mathf.Abs(pinchTiltCurrent) > pinchTiltModeThreshold;
                    isTiltModeEvaluated = true;
                    if (isPinchTiltMode && isPinchModeExclusive)
                    {
                        pinchStartIntersectionCenter = GetIntersectionPoint(GetCamCenterRay());
                    }
                }
            }

            if (isTiltModeEvaluated)
            {
                if (isPinchModeExclusive)
                {
                    pinchCenterCurrent = pinchUpdateData.pinchCenter;

                    if (isPinchTiltMode)
                    {
                        //Evaluate a potential break-out from a tilt. Under certain tweak-settings the tilt may trigger prematurely and needs to be overrided.
                        if (pinchTiltAccumulated < pinchAccumBreakout)
                        {
                            bool breakoutZoom = Mathf.Abs(pinchDistanceStart - pinchUpdateData.pinchDistance) > pinchDistanceForTiltBreakout;
                            bool breakoutRot = enableRotation && Mathf.Abs(pinchAngleCurrent) > rotationLockThreshold;
                            if (breakoutZoom || breakoutRot)
                            {
                                InputControllerOnPinchStart(pinchUpdateData.pinchCenter, pinchUpdateData.pinchDistance);
                                isTiltModeEvaluated = true;
                                isPinchTiltMode = false;
                            }
                        }
                    }
                }
                pinchDistanceCurrent = pinchUpdateData.pinchDistance;

                if (enableRotation)
                {
                    if (Mathf.Abs(pinchUpdateData.pinchAngleDeltaNormalized) > rotationDetectionDeltaThreshold)
                    {
                        pinchAngleCurrent += pinchUpdateData.pinchAngleDelta;
                    }

                    if (pinchDistanceCurrent > rotationMinPinchDistance)
                    {
                        if (!isRotationActivated)
                        {
                            ResetPinchRotation(0);
                            isRotationActivated = true;
                        }
                    }
                    else
                    {
                        isRotationActivated = false;
                    }
                }
            }
        }

        private void ResetPinchRotation(float currentPinchRotation)
        {
            pinchAngleCurrent = currentPinchRotation;
            pinchAngleCurrentLerp = currentPinchRotation;
            pinchAngleLastFrame = currentPinchRotation;
            isRotationLock = true;
        }

        private void InputControllerOnPinchStop()
        {
            IsPinching = false;
            DragCameraMoveVector.Clear();
            isPinchTiltMode = false;
            isTiltModeEvaluated = false;
        }

        private void InputControllerOnInputClick(Vector3 clickPosition, bool isDoubleClick, bool isLongTap)
        {
            if (isLongTap) return;

            Ray camRay = Cam.ScreenPointToRay(clickPosition);
            if (OnPickItem != null || OnPickItemDoubleClick != null)
            {
                if (Physics.Raycast(camRay, out RaycastHit hitInfo))
                {
                    OnPickItem?.Invoke(hitInfo);
                    HandlePointerCall(hitInfo, false, clickPosition);

                    if (isDoubleClick)
                    {
                        HandlePointerCall(hitInfo, true, clickPosition);
                        OnPickItemDoubleClick?.Invoke(hitInfo);
                    }
                }
            }

            if (OnPickItem2D != null || OnPickItem2DDoubleClick != null)
            {
                RaycastHit2D hitInfo2D = Physics2D.Raycast(camRay.origin, camRay.direction);
                if (hitInfo2D == true)
                {
                    OnPickItem2D?.Invoke(hitInfo2D);
                    if (isDoubleClick) OnPickItem2DDoubleClick?.Invoke(hitInfo2D);
                }
            }
        }

        private void HandlePointerCall(RaycastHit hitInfo, bool isDoubleClick, Vector2 clickPosition)
        {
            GameObject target = hitInfo.collider.gameObject;
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = clickPosition,
                pointerId = -1,
                clickCount = isDoubleClick ? 2 : 1, // Correctly identifies the double click to listeners
                pointerPress = target, // Critical for the system to identify the pressed object
                rawPointerPress = target
            };

            // Execute in the standard engine order
            ExecuteEvents.ExecuteHierarchy(target, eventData, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.ExecuteHierarchy(target, eventData, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.ExecuteHierarchy(target, eventData, ExecuteEvents.pointerClickHandler);
        }

        private IEnumerator ZoomToTargetValueCoroutine(float target)
        {
            if (!Mathf.Approximately(target, CamZoom))
            {
                float startValue = CamZoom;
                const float duration = 0.3f;
                float timeStart = Time.time;
                while (Time.time < timeStart + duration)
                {
                    float progress = (Time.time - timeStart) / duration;
                    CamZoom = Mathf.Lerp(startValue, target, Mathf.Sin(-Mathf.PI * 0.5f + progress * Mathf.PI) * 0.5f + 0.5f);
                    yield return null;
                }

                CamZoom = target;
            }
        }

        private Ray GetCamCenterRay()
        {
            return Cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
        }

        private void UpdateRefPlaneForTerrain(Vector3 touchPosition)
        {
            var dragRay = Cam.ScreenPointToRay(touchPosition);
            if (TerrainCollider.Raycast(dragRay, out RaycastHit hitInfo, float.MaxValue))
            {
                refPlaneXZ = new Plane(new Vector3(0, 1, 0), -hitInfo.point.y);
            }
        }

        private bool GetKeyWithRepeat(Key key, out float factor)
        {
            var keyboard = Keyboard.current;
            factor = 1f;

            // Safety check if no keyboard is connected
            if (keyboard == null) return false;

            bool isDown = false;
            var keyControl = keyboard[key];

            if (keyControl.wasPressedThisFrame)
            {
                timeKeyDown = (float)Time.realtimeSinceStartupAsDouble;
                isDown = true;
            }
            else if (keyControl.isPressed && Time.realtimeSinceStartupAsDouble > timeKeyDown + keyboardRepeatDelay)
            {
                isDown = true;
                factor = Time.unscaledDeltaTime * keyboardRepeatFactor;
            }

            return isDown;
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector2 boundaryCenter2d = 0.5f * (boundaryMin + boundaryMax);
            Vector2 boundarySize2d = boundaryMax - boundaryMin;
            Vector3 boundaryCenter = UnprojectVector2(boundaryCenter2d, groundLevelOffset);
            Vector3 boundarySize = UnprojectVector2(boundarySize2d);
            Gizmos.DrawWireCube(boundaryCenter, boundarySize);
        }
    }
}

public enum CameraPlaneAxes
{
    XY_2D_SIDESCROLL,
    XZ_TOP_DOWN
}

public enum PerspectiveZoomMode
{
    FIELD_OF_VIEW,
    TRANSLATION,
}

public enum AutoScrollDampMode
{
    DEFAULT,
    SLOW_FADE_OUT,
    CUSTOM
}

public enum SnapAngle
{
    Straight_0_Degrees,
    Diagonal_45_Degrees,
}