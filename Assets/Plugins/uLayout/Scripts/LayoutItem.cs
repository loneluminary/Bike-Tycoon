using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Poke.UI
{
    [ExecuteAlways, RequireComponent(typeof(RectTransform))]
    public class LayoutItem : MonoBehaviour
    {
        [SerializeField] protected bool m_ignoreLayout = false;

        [Title("Sizing")]
        [SerializeField, HideIf("m_ignoreLayout")] protected SizeModes m_sizing;

        public bool IgnoreLayout
        {
            get => m_ignoreLayout;
            set
            {
                m_ignoreLayout = value;
                if (_parent) _parent.RefreshChildCache();
            }
        }

        public RectTransform Rect => _rect;
        public SizeModes SizeMode => m_sizing;

        protected RectTransform _rect;
        protected RectTransform _parentRect;
        protected Layout _parent;

        [Serializable]
        public struct SizeModes
        {
            public SizingMode x;
            public SizingMode y;
        }

        protected virtual void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _parentRect = transform.parent?.GetComponent<RectTransform>();
        }

        protected virtual void OnEnable()
        {
            _parent = transform.parent?.GetComponent<Layout>();
            if (_parent) _parent.RefreshChildCache();
        }

        protected virtual void OnDisable()
        {
            if (_parent) _parent.RefreshChildCache();
        }

        public virtual void Update()
        {
            // Do grow sizing here if the parent is not a Layout
            if (!_parent && _parentRect)
            {
                if (m_sizing.x == SizingMode.Grow)
                {
                    _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _parentRect.rect.size.x);
                }

                if (m_sizing.y == SizingMode.Grow)
                {
                    _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _parentRect.rect.size.y);
                }
            }
        }
    }
}