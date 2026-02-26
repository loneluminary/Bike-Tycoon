using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;

namespace TouchCameraSystem
{
    [CustomEditor(typeof(MobileTouchCamera))]
    public class MobileTouchCameraEditor : OdinEditor
    {
        public void OnSceneGUI()
        {
            MobileTouchCamera mobileTouchCamera = (MobileTouchCamera)target;

            if (Event.current.rawType == EventType.MouseUp)
            {
                CheckSwapBoundary(mobileTouchCamera);
            }

            Vector2 boundaryMin = mobileTouchCamera.BoundaryMin;
            Vector2 boundaryMax = mobileTouchCamera.BoundaryMax;

            float offset = mobileTouchCamera.GroundLevelOffset;
            Vector3 pBottomLeft = mobileTouchCamera.UnprojectVector2(new Vector2(boundaryMin.x, boundaryMin.y), offset);
            Vector3 pBottomRight = mobileTouchCamera.UnprojectVector2(new Vector2(boundaryMax.x, boundaryMin.y), offset);
            Vector3 pTopLeft = mobileTouchCamera.UnprojectVector2(new Vector2(boundaryMin.x, boundaryMax.y), offset);
            Vector3 pTopRight = mobileTouchCamera.UnprojectVector2(new Vector2(boundaryMax.x, boundaryMax.y), offset);

            Handles.color = new Color(0, .4f, 1f, 1f);
            float handleSize = HandleUtility.GetHandleSize(mobileTouchCamera.Transform.position) * 0.1f;

            #region min/max handles

            pBottomLeft = DrawSphereHandle(pBottomLeft, handleSize);
            pTopRight = DrawSphereHandle(pTopRight, handleSize);
            boundaryMin = mobileTouchCamera.ProjectVector3(pBottomLeft);
            boundaryMax = mobileTouchCamera.ProjectVector3(pTopRight);

            #endregion

            #region min/max handles that need to be remapped

            Vector3 pBottomRightNew = DrawSphereHandle(pBottomRight, handleSize);
            Vector3 pTopLeftNew = DrawSphereHandle(pTopLeft, handleSize);

            if (Vector3.Distance(pBottomRight, pBottomRightNew) > 0)
            {
                Vector2 pBottomRight2d = mobileTouchCamera.ProjectVector3(pBottomRightNew);
                boundaryMin.y = pBottomRight2d.y;
                boundaryMax.x = pBottomRight2d.x;
            }

            if (Vector3.Distance(pTopLeft, pTopLeftNew) > 0)
            {
                Vector2 pTopLeftNew2d = mobileTouchCamera.ProjectVector3(pTopLeftNew);
                boundaryMin.x = pTopLeftNew2d.x;
                boundaryMax.y = pTopLeftNew2d.y;
            }

            #endregion

            #region one way handles

            Handles.color = new Color(1, 1, 1, 1);
            handleSize = HandleUtility.GetHandleSize(mobileTouchCamera.Transform.position) * 0.05f;
            boundaryMax.x = DrawOneWayHandle(mobileTouchCamera, handleSize, new Vector2(boundaryMax.x, 0.5f * (boundaryMax.y + boundaryMin.y)), offset).x;
            boundaryMax.y = DrawOneWayHandle(mobileTouchCamera, handleSize, new Vector2(0.5f * (boundaryMax.x + boundaryMin.x), boundaryMax.y), offset).y;
            boundaryMin.x = DrawOneWayHandle(mobileTouchCamera, handleSize, new Vector2(boundaryMin.x, 0.5f * (boundaryMax.y + boundaryMin.y)), offset).x;
            boundaryMin.y = DrawOneWayHandle(mobileTouchCamera, handleSize, new Vector2(0.5f * (boundaryMax.x + boundaryMin.x), boundaryMin.y), offset).y;

            #endregion

            if (Vector2.Distance(mobileTouchCamera.BoundaryMin, boundaryMin) > float.Epsilon || Vector2.Distance(mobileTouchCamera.BoundaryMax, boundaryMax) > float.Epsilon)
            {
                Undo.RecordObject(target, "Mobile Touch Camera Boundary Modification");
                mobileTouchCamera.BoundaryMin = boundaryMin;
                mobileTouchCamera.BoundaryMax = boundaryMax;
                EditorUtility.SetDirty(mobileTouchCamera);
            }
        }

        private Vector3 DrawSphereHandle(Vector3 point, float handleSize)
        {
#if UNITY_5_6_OR_NEWER
            var fmh_82_44_639038486915206740 = Quaternion.identity;
            return Handles.FreeMoveHandle(point, handleSize, Vector3.one, Handles.SphereHandleCap);
#else
            return Handles.FreeMoveHandle(point, Quaternion.identity, handleSize, Vector3.one, Handles.SphereCap);
#endif
        }

        private Vector3 DrawOneWayHandle(MobileTouchCamera mobileTouchCamera, float handleSize, Vector2 pRelative, float offset)
        {
            Vector3 point = mobileTouchCamera.UnprojectVector2(pRelative, offset);
#if UNITY_5_6_OR_NEWER
            var fmh_91_56_639038486915226417 = Quaternion.identity;
            Vector3 pointNew = Handles.FreeMoveHandle(point, handleSize, Vector3.one, Handles.DotHandleCap);
#else
            Vector3 pointNew = Handles.FreeMoveHandle(point, Quaternion.identity, handleSize, Vector3.one, Handles.DotCap);
#endif
            return mobileTouchCamera.ProjectVector3(pointNew);
        }

        /// Method to swap the boundary min/max values in case they aren't right.
        private void CheckSwapBoundary(MobileTouchCamera mobileTouchCamera)
        {
            Vector2 boundaryMin = mobileTouchCamera.BoundaryMin;
            Vector2 boundaryMax = mobileTouchCamera.BoundaryMax;

            //Automatically swap min with max when necessary.
            bool autoSwap = false;
            if (boundaryMax.x < boundaryMin.x)
            {
                Undo.RecordObject(target, "Mobile Touch Camera Boundary Auto Swap");
                Swap(ref boundaryMax.x, ref boundaryMin.x);
                autoSwap = true;
            }

            if (boundaryMax.y < boundaryMin.y)
            {
                Undo.RecordObject(target, "Mobile Touch Camera Boundary Auto Swap");
                Swap(ref boundaryMax.y, ref boundaryMin.y);
                autoSwap = true;
            }

            if (autoSwap == true)
            {
                EditorUtility.SetDirty(mobileTouchCamera);
            }

            mobileTouchCamera.BoundaryMin = boundaryMin;
            mobileTouchCamera.BoundaryMax = boundaryMax;
        }

        /// Helper method to swap 2 float variables.
        private void Swap(ref float a, ref float b) => (a, b) = (b, a);
    }
}