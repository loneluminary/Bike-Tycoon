using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Utilities.Extensions
{
    public static class GameObjectExtensions
    {
        public static void SetLayerRecursively(this GameObject gameObject, string layerName)
        {
            gameObject.layer = LayerMask.NameToLayer(layerName);
            foreach (Transform child in gameObject.transform) child.gameObject.SetLayerRecursively(layerName);
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (!gameObject.TryGetComponent<T>(out var attachedComponent)) attachedComponent = gameObject.AddComponent<T>();

            return attachedComponent;
        }

        public static bool HasComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.TryGetComponent<T>(out _);
        }

        public static GameObject ToggleActive(this GameObject gameObject)
        {
            gameObject.SetActive(!gameObject.activeSelf);
            return gameObject;
        }

        public static GameObject DestroyAllChildren(this GameObject gameObject)
        {
            foreach (Transform child in gameObject.transform)
            {
                if (Application.isPlaying) Object.Destroy(child.gameObject);
                else Object.DestroyImmediate(child.gameObject);
            }
            
            return gameObject;
        }

        public static GameObject AddComponentIfMissing<T>(this GameObject gameObject) where T : Component
        {
            if (!gameObject.GetComponent<T>()) gameObject.AddComponent<T>();
            return gameObject;
        }

        public static bool TryGetComponentInChildren<T>(this GameObject gameObject, out T component, bool includeInactive = false) where T : Component
        {
            component = gameObject.GetComponentInChildren<T>(includeInactive);
            return component;
        }

        public static bool TryGetComponentInParent<T>(this GameObject gameObject, out T component, bool includeInactive = false) where T : Component
        {
            component = gameObject.GetComponentInParent<T>(includeInactive);
            return component;
        }
        
        /// Returns the hierarchical path in the Unity scene hierarchy for this GameObject.
        /// <param name="gameObject">The GameObject to get the path for.</param>
        /// <returns>A string representing the full hierarchical path of this GameObject in the Unity scene.
        /// This is a '/'-separated string where each part is the name of a parent, starting from the root parent and ending
        /// with the name of the specified GameObjects parent.</returns>
        public static string Path(this GameObject gameObject) => "/" + string.Join("/", gameObject.GetComponentsInParent<Transform>().Select(t => t.name).Reverse().ToArray());

        /// Returns the full hierarchical path in the Unity scene hierarchy for this GameObject.
        /// <param name="gameObject">The GameObject to get the path for.</param>
        /// <returns>A string representing the full hierarchical path of this GameObject in the Unity scene.
        /// This is a '/'-separated string where each part is the name of a parent, starting from the root parent and ending
        /// with the name of the specified GameObject itself.</returns>
        public static string PathFull(this GameObject gameObject) => gameObject.Path() + "/" + gameObject.name;

        #region Gizmos Icon

        public static void SetIcon(this GameObject gameObject, LabelIcon labelIcon)
        {
            SetIcon(gameObject, $"sv_label_{(int) labelIcon}");
        }
        
        public static void SetIcon(this GameObject gameObject, ShapeIcon shapeIcon)
        {
            SetIcon(gameObject, $"sv_icon_dot{(int) shapeIcon}_pix16_gizmo");
        }
        
        public static void RemoveIcon(this GameObject gameObject)
        {
            #if UNITY_EDITOR
            EditorGUIUtility.SetIconForObject(gameObject, null);
            #endif
        }

        private static void SetIcon(GameObject gameObject, string contentName)
        {
            #if UNITY_EDITOR
            GUIContent iconContent = EditorGUIUtility.IconContent(contentName);
            EditorGUIUtility.SetIconForObject(gameObject, (Texture2D)iconContent.image);
            #endif
        }
        
        public enum LabelIcon
        {
            Gray,
            Blue,
            Teal,
            Green,
            Yellow,
            Orange,
            Red,
            Purple
        }
        
        public enum ShapeIcon
        {
            CircleGray,
            CircleBlue,
            CircleTeal,
            CircleGreen,
            CircleYellow,
            CircleOrange,
            CircleRed,
            CirclePurple,
            DiamondGray,
            DiamondBlue,
            DiamondTeal,
            DiamondGreen,
            DiamondYellow,
            DiamondOrange,
            DiamondRed,
            DiamondPurple
        }

        #endregion
    }
}