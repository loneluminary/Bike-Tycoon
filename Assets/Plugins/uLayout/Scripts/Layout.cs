using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Poke.UI
{
    public class Layout : LayoutItem, IComparable<Layout>
    {
        [Title("Layout")]
        public Margins m_padding;
        public LayoutDirection m_direction;
        public Justification m_justifyContent;
        public Alignment m_alignContent;
        public float m_innerSpacing;

        private readonly Vector3[] _rectCorners = new Vector3[4];
        private DrivenRectTransformTracker _rectTracker;
        private LayoutRoot _root;
        private List<ChildEntry> _childrenCached = new();
        private Vector2 _contentSize;
        private List<LayoutItem> _growChildren;
        private Vector2Int _growChildCount;

        public int ChildCount => _childrenCached.Count;
        public int GrowChildCount => _growChildren.Count;
        [HideInInspector] public int Depth;

        private readonly int MAX_DEPTH = 100;

        #region TypeDef

        public enum Justification
        {
            Start,
            Center,
            End,
            SpaceBetween
        }

        public enum Alignment
        {
            Start,
            Center,
            End
        }

        public enum LayoutDirection
        {
            Row,
            Column,
            RowReverse,
            ColumnReverse
        }

        #endregion

        #region Layout MonoBehavior

        protected override void Awake()
        {
            base.Awake();
            _rectTracker = new DrivenRectTransformTracker();
            _growChildren = new List<LayoutItem>();

            _root = null;
            Depth = 0;
            Transform t = transform;

            while (t != null) // Keep going as long as we have an object to check
            {
                // Check the current object first
                if (t.TryGetComponent(out LayoutRoot root))
                {
                    _root = root;
                    break;
                }

                // Check depth
                if (Depth >= MAX_DEPTH)
                {
                    Debug.LogWarning($"Hit max search depth ({MAX_DEPTH})! Aborting.");
                    break;
                }

                // Move up the chain
                t = t.parent;
                Depth++;
            }

            if (_root == null) Debug.LogWarning("No UILayoutRoot found in hierarchy!");
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _root?.RegisterLayout(this);
            RefreshChildCache();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _root?.UnregisterLayout(this);
        }

        private void OnDrawGizmosSelected()
        {
            if (!_rect) _rect = GetComponent<RectTransform>();

            Gizmos.matrix = Matrix4x4.identity;
            _rect.GetWorldCorners(_rectCorners);

            foreach (Vector3 v in _rectCorners) LayoutUtil.DrawCenteredDebugBox(v, 0.15f, 0.15f, Color.red);

            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = _rect.localToWorldMatrix;
            Rect localRect = _rect.rect;

            // Calculate the inner padding area relative to the pivot
            float px = localRect.x + m_padding.left;
            float py = localRect.y + m_padding.bottom;
            float pw = localRect.width - (m_padding.left + m_padding.right);
            float ph = localRect.height - (m_padding.top + m_padding.bottom);

            LayoutUtil.DrawDebugBox(new Vector3(px, py, 0), pw, ph, Color.green);
            Gizmos.matrix = oldMatrix;
        }

        #endregion

        #region LAYOUT PASSES

        public void ComputeFitSize()
        {
            if (_childrenCached.Count <= 0)
            {
                _contentSize = Vector2.zero;
                return;
            }

            _growChildren.Clear();
            _growChildCount = new Vector2Int(0, 0);

            _rectTracker.Clear();
            if (m_sizing.x == SizingMode.FitContent || (!_parent && m_sizing.x == SizingMode.Grow)) _rectTracker.Add(this, _rect, DrivenTransformProperties.SizeDeltaX);
            if (m_sizing.y == SizingMode.FitContent || (!_parent && m_sizing.y == SizingMode.Grow)) _rectTracker.Add(this, _rect, DrivenTransformProperties.SizeDeltaY);

            float primarySize = m_innerSpacing * (_childrenCached.Count - 1);
            float crossSize = 0;

            switch (m_direction)
            {
                case LayoutDirection.Row:
                case LayoutDirection.RowReverse:
                    primarySize += m_padding.left + m_padding.right;
                    crossSize += m_padding.top + m_padding.bottom;
                    break;
                case LayoutDirection.Column:
                case LayoutDirection.ColumnReverse:
                    primarySize += m_padding.top + m_padding.bottom;
                    crossSize += m_padding.left + m_padding.right;
                    break;
            }

            // calculate content size
            float maxCrossSize = 0;
            for (int i = 0; i < _childrenCached.Count; i++)
            {
                var rt = _childrenCached[i];
                if (!rt.Rect) continue;

                bool growX = false, growY = false;

                if (rt.Layout)
                {
                    growX = rt.Layout.SizeMode.x == SizingMode.Grow;
                    growY = rt.Layout.SizeMode.y == SizingMode.Grow;
                    if (growX || growY)
                    {
                        _growChildren.Add(rt.Layout);
                        _growChildCount.x += growX ? 1 : 0;
                        _growChildCount.y += growY ? 1 : 0;
                    }
                }

                switch (m_direction)
                {
                    case LayoutDirection.Row:
                    case LayoutDirection.RowReverse:
                        primarySize += growX ? 0 : rt.Rect.sizeDelta.x;
                        maxCrossSize = Mathf.Max(maxCrossSize, growY ? 0 : rt.Rect.sizeDelta.y);
                        break;
                    case LayoutDirection.Column:
                    case LayoutDirection.ColumnReverse:
                        primarySize += growY ? 0 : rt.Rect.sizeDelta.y;
                        maxCrossSize = Mathf.Max(maxCrossSize, growX ? 0 : rt.Rect.sizeDelta.x);
                        break;
                }

            }

            crossSize += maxCrossSize;

            // save content size for later
            _contentSize = m_direction switch
            {
                LayoutDirection.Row or LayoutDirection.RowReverse => new Vector2(primarySize, crossSize),
                LayoutDirection.Column or LayoutDirection.ColumnReverse => new Vector2(crossSize, primarySize),
                _ => _contentSize
            };

            // apply fit sizing X
            if (m_sizing.x == SizingMode.FitContent)
            {
                switch (m_direction)
                {
                    case LayoutDirection.Row:
                    case LayoutDirection.RowReverse:
                        _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, primarySize);
                        break;
                    case LayoutDirection.Column:
                    case LayoutDirection.ColumnReverse:
                        _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, crossSize);
                        break;
                }
            }

            // apply fit sizing Y
            if (m_sizing.y == SizingMode.FitContent)
            {
                switch (m_direction)
                {
                    case LayoutDirection.Row:
                    case LayoutDirection.RowReverse:
                        _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, crossSize);
                        break;
                    case LayoutDirection.Column:
                    case LayoutDirection.ColumnReverse:
                        _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, primarySize);
                        break;
                }
            }
        }

        public void GrowChildren()
        {
            for (int i = 0; i < _growChildren.Count; i++)
            {
                LayoutItem li = _growChildren[i];
                if (!li) continue;

                Vector2 size;
                float crossSize;
                float leftover;
                switch (m_direction)
                {
                    case LayoutDirection.Row:
                    case LayoutDirection.RowReverse:
                        {
                            leftover = _rect.rect.size.x - _contentSize.x - m_padding.left - m_padding.right;
                            crossSize = _rect.rect.size.y - m_padding.top - m_padding.bottom;
                            size = new Vector2(leftover / _growChildCount.x, crossSize);

                            if (li.SizeMode.x == SizingMode.Grow)
                            {
                                _rectTracker.Add(this, li.Rect, DrivenTransformProperties.SizeDeltaX);
                                li.Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
                            }

                            if (li.SizeMode.y == SizingMode.Grow)
                            {
                                _rectTracker.Add(this, li.Rect, DrivenTransformProperties.SizeDeltaY);
                                li.Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
                            }

                            break;
                        }
                    case LayoutDirection.Column:
                    case LayoutDirection.ColumnReverse:
                        {
                            leftover = _rect.rect.size.y - _contentSize.y - m_padding.top - m_padding.bottom;
                            crossSize = _rect.rect.size.x - m_padding.left - m_padding.right;
                            size = new Vector2(crossSize, leftover / _growChildCount.y);

                            if (li.SizeMode.y == SizingMode.Grow)
                            {
                                _rectTracker.Add(this, li.Rect, DrivenTransformProperties.SizeDeltaY);
                                li.Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
                            }

                            if (li.SizeMode.x == SizingMode.Grow)
                            {
                                _rectTracker.Add(this, li.Rect, DrivenTransformProperties.SizeDeltaX);
                                li.Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
                            }

                            break;
                        }
                }
            }
        }

        public void ComputeLayout()
        {
            if (_childrenCached.Count < 1) return;

            // apply RectTransform DrivenTransformProperties
            for (int i = 0; i < _childrenCached.Count; i++)
            {
                RectTransform rt = _childrenCached[i].Rect;
                if (!rt) continue;
                _rectTracker.Add(this, rt, DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.Pivot | DrivenTransformProperties.Anchors);
            }

            // primary axis pass
            float primaryOffset = 0;
            float spacing = 0;
            float leftover;
            int index = 0;

            switch (m_direction)
            {
                // ROW -> PRIMARY AXIS
                case LayoutDirection.Row:
                    {
                        switch (m_justifyContent)
                        {
                            case Justification.Start:
                                {
                                    primaryOffset += m_padding.left;

                                    for (int i = 0; i < _childrenCached.Count; i++)
                                    {
                                        RectTransform rt = _childrenCached[i].Rect;
                                        if (!rt) continue;

                                        rt.anchorMin = rt.anchorMin.With(x: 0);
                                        rt.anchorMax = rt.anchorMax.With(x: 0);
                                        rt.pivot = rt.pivot.With(x: 0);

                                        rt.anchoredPosition = rt.anchoredPosition.With(x: primaryOffset);
                                        primaryOffset += rt.sizeDelta.x + m_innerSpacing;
                                    }

                                    break;
                                }
                            case Justification.Center:
                                {
                                    primaryOffset -= _contentSize.x / 2;

                                    for (int i = 0; i < _childrenCached.Count; i++)
                                    {
                                        RectTransform rt = _childrenCached[i].Rect;
                                        if (!rt) continue;

                                        rt.anchorMin = rt.anchorMin.With(x: 0.5f);
                                        rt.anchorMax = rt.anchorMax.With(x: 0.5f);
                                        rt.pivot = rt.pivot.With(x: 0.5f);

                                        primaryOffset += rt.sizeDelta.x / 2;
                                        rt.anchoredPosition = rt.anchoredPosition.With(x: primaryOffset + m_padding.left);
                                        primaryOffset += rt.sizeDelta.x / 2 + m_innerSpacing;
                                    }

                                    break;
                                }
                            case Justification.End:
                                {
                                    primaryOffset += m_padding.right + _contentSize.x;

                                    for (int i = 0; i < _childrenCached.Count; i++)
                                    {
                                        RectTransform rt = _childrenCached[i].Rect;
                                        if (!rt) continue;

                                        rt.anchorMin = rt.anchorMin.With(x: 1);
                                        rt.anchorMax = rt.anchorMax.With(x: 1);
                                        rt.pivot = rt.pivot.With(x: 1);

                                        primaryOffset -= rt.sizeDelta.x;
                                        rt.anchoredPosition = rt.anchoredPosition.With(x: -primaryOffset + m_padding.left + m_padding.right);
                                        primaryOffset -= m_innerSpacing;
                                    }

                                    break;
                                }
                            case Justification.SpaceBetween:
                                primaryOffset += m_padding.left;
                                leftover = _rect.rect.size.x - _contentSize.x;

                                if (_childrenCached.Count > 1) spacing = leftover / (_childrenCached.Count - 1);

                                for (int i = 0; i < _childrenCached.Count; i++)
                                {
                                    RectTransform rt = _childrenCached[i].Rect;
                                    if (!rt) continue;

                                    rt.anchorMin = rt.anchorMin.With(x: 0);
                                    rt.anchorMax = rt.anchorMax.With(x: 0);
                                    rt.pivot = rt.pivot.With(x: 0);

                                    if (index != 0)
                                    {
                                        primaryOffset += spacing;
                                    }

                                    rt.anchoredPosition = rt.anchoredPosition.With(x: primaryOffset);
                                    primaryOffset += rt.sizeDelta.x;
                                    index++;
                                }

                                break;
                        }
                        break;
                    }
                // ROW_REVERSE -> PRIMARY AXIS
                case LayoutDirection.RowReverse:
                    {
                        switch (m_justifyContent)
                        {
                            case Justification.Start:
                                {
                                    primaryOffset += m_padding.left + _contentSize.x;

                                    for (int i = 0; i < _childrenCached.Count; i++)
                                    {
                                        RectTransform rt = _childrenCached[i].Rect;
                                        if (!rt) continue;

                                        rt.anchorMin = rt.anchorMin.With(x: 0);
                                        rt.anchorMax = rt.anchorMax.With(x: 0);
                                        rt.pivot = rt.pivot.With(x: 0);

                                        primaryOffset -= rt.sizeDelta.x + m_innerSpacing;
                                        rt.anchoredPosition = rt.anchoredPosition.With(x: primaryOffset);
                                    }

                                    break;
                                }
                            case Justification.Center:
                                {
                                    primaryOffset += _contentSize.x / 2;

                                    for (int i = 0; i < _childrenCached.Count; i++)
                                    {
                                        RectTransform rt = _childrenCached[i].Rect;
                                        if (!rt) continue;

                                        rt.anchorMin = rt.anchorMin.With(x: 0.5f);
                                        rt.anchorMax = rt.anchorMax.With(x: 0.5f);
                                        rt.pivot = rt.pivot.With(x: 0.5f);

                                        primaryOffset -= rt.sizeDelta.x / 2;
                                        rt.anchoredPosition = rt.anchoredPosition.With(x: primaryOffset - m_padding.right);
                                        primaryOffset -= rt.sizeDelta.x / 2 + m_innerSpacing;
                                    }

                                    break;
                                }
                            case Justification.End:
                                {
                                    primaryOffset += m_padding.right;

                                    for (int i = 0; i < _childrenCached.Count; i++)
                                    {
                                        RectTransform rt = _childrenCached[i].Rect;
                                        if (!rt) continue;

                                        rt.anchorMin = rt.anchorMin.With(x: 1);
                                        rt.anchorMax = rt.anchorMax.With(x: 1);
                                        rt.pivot = rt.pivot.With(x: 1);

                                        rt.anchoredPosition = rt.anchoredPosition.With(x: -primaryOffset);
                                        primaryOffset += rt.sizeDelta.x + m_innerSpacing;
                                    }

                                    break;
                                }
                            case Justification.SpaceBetween:
                                {
                                    primaryOffset += m_padding.right;

                                    leftover = _rect.rect.size.x - _contentSize.x;

                                    if (_childrenCached.Count > 1) spacing = leftover / (_childrenCached.Count - 1);

                                    for (int i = 0; i < _childrenCached.Count; i++)
                                    {
                                        RectTransform rt = _childrenCached[i].Rect;
                                        if (!rt) continue;

                                        rt.anchorMin = rt.anchorMin.With(x: 1);
                                        rt.anchorMax = rt.anchorMax.With(x: 1);
                                        rt.pivot = rt.pivot.With(x: 1);

                                        rt.anchoredPosition = rt.anchoredPosition.With(x: -primaryOffset);
                                        primaryOffset += rt.sizeDelta.x + spacing;
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                // COLUMN -> PRIMARY AXIS
                case LayoutDirection.Column:
                    {
                        switch (m_justifyContent)
                        {
                            case Justification.Start:
                                {
                                    primaryOffset -= m_padding.top;

                                    for (int i = 0; i < _childrenCached.Count; i++)
                                    {
                                        RectTransform rt = _childrenCached[i].Rect;
                                        if (!rt) continue;

                                        rt.anchorMin = rt.anchorMin.With(y: 1);
                                        rt.anchorMax = rt.anchorMax.With(y: 1);
                                        rt.pivot = rt.pivot.With(y: 1);

                                        rt.anchoredPosition = rt.anchoredPosition.With(y: primaryOffset);
                                        primaryOffset -= rt.sizeDelta.y + m_innerSpacing;
                                    }

                                    break;
                                }
                            case Justification.Center:
                                {
                                    primaryOffset += _contentSize.y / 2;

                                    for (int i = 0; i < _childrenCached.Count; i++)
                                    {
                                        RectTransform rt = _childrenCached[i].Rect;
                                        if (!rt) continue;

                                        rt.anchorMin = rt.anchorMin.With(y: 0.5f);
                                        rt.anchorMax = rt.anchorMax.With(y: 0.5f);
                                        rt.pivot = rt.pivot.With(y: 0.5f);

                                        primaryOffset -= rt.sizeDelta.y / 2;
                                        rt.anchoredPosition = rt.anchoredPosition.With(y: primaryOffset - m_padding.top);
                                        primaryOffset -= rt.sizeDelta.y / 2 + m_innerSpacing;
                                    }

                                    break;
                                }
                            case Justification.End:
                                {
                                    primaryOffset += _contentSize.y;

                                    for (int i = 0; i < _childrenCached.Count; i++)
                                    {
                                        RectTransform rt = _childrenCached[i].Rect;
                                        if (!rt) continue;

                                        rt.anchorMin = rt.anchorMin.With(y: 0);
                                        rt.anchorMax = rt.anchorMax.With(y: 0);
                                        rt.pivot = rt.pivot.With(y: 0);

                                        primaryOffset -= rt.sizeDelta.y;
                                        rt.anchoredPosition = rt.anchoredPosition.With(y: primaryOffset - m_padding.top);
                                        primaryOffset -= m_innerSpacing;
                                    }

                                    break;
                                }
                            case Justification.SpaceBetween:
                                {
                                    primaryOffset += m_padding.top;
                                    leftover = _rect.rect.size.y - _contentSize.y;

                                    if (_childrenCached.Count > 1) spacing = leftover / (_childrenCached.Count - 1);

                                    for (int i = 0; i < _childrenCached.Count; i++)
                                    {
                                        RectTransform rt = _childrenCached[i].Rect;
                                        if (!rt) continue;

                                        rt.anchorMin = rt.anchorMin.With(y: 1);
                                        rt.anchorMax = rt.anchorMax.With(y: 1);
                                        rt.pivot = rt.pivot.With(y: 1);

                                        if (index != 0)
                                        {
                                            primaryOffset += spacing;
                                        }

                                        rt.anchoredPosition = rt.anchoredPosition.With(y: -primaryOffset);
                                        primaryOffset += rt.sizeDelta.y;

                                        index++;
                                    }

                                    break;
                                }
                        }
                        break;
                    }
                // COLUMN_REVERSE -> PRIMARY AXIS
                case LayoutDirection.ColumnReverse:
                    {
                        switch (m_justifyContent)
                        {
                            case Justification.Start:
                                {
                                    primaryOffset -= m_padding.top + _contentSize.y;

                                    for (int i = 0; i < _childrenCached.Count; i++)
                                    {
                                        RectTransform rt = _childrenCached[i].Rect;
                                        if (!rt) continue;

                                        rt.anchorMin = rt.anchorMin.With(y: 1);
                                        rt.anchorMax = rt.anchorMax.With(y: 1);
                                        rt.pivot = rt.pivot.With(y: 1);

                                        primaryOffset += rt.sizeDelta.y;
                                        rt.anchoredPosition = rt.anchoredPosition.With(y: primaryOffset);
                                        primaryOffset += m_innerSpacing;
                                    }

                                    break;
                                }
                            case Justification.Center:
                                {
                                    primaryOffset -= _contentSize.y / 2;

                                    for (int i = 0; i < _childrenCached.Count; i++)
                                    {
                                        RectTransform rt = _childrenCached[i].Rect;
                                        if (!rt) continue;

                                        rt.anchorMin = rt.anchorMin.With(y: 0.5f);
                                        rt.anchorMax = rt.anchorMax.With(y: 0.5f);
                                        rt.pivot = rt.pivot.With(y: 0.5f);

                                        primaryOffset += rt.sizeDelta.y / 2;
                                        rt.anchoredPosition = rt.anchoredPosition.With(y: primaryOffset - m_padding.top);
                                        primaryOffset += rt.sizeDelta.y / 2 + m_innerSpacing;
                                    }

                                    break;
                                }
                            case Justification.End:
                                {
                                    primaryOffset += m_padding.bottom;

                                    for (int i = 0; i < _childrenCached.Count; i++)
                                    {
                                        RectTransform rt = _childrenCached[i].Rect;
                                        if (!rt) continue;

                                        rt.anchorMin = rt.anchorMin.With(y: 0);
                                        rt.anchorMax = rt.anchorMax.With(y: 0);
                                        rt.pivot = rt.pivot.With(y: 0);

                                        rt.anchoredPosition = rt.anchoredPosition.With(y: primaryOffset);
                                        primaryOffset += rt.sizeDelta.y + m_innerSpacing;
                                    }

                                    break;
                                }
                            case Justification.SpaceBetween:
                                {
                                    primaryOffset += m_padding.bottom;

                                    leftover = _rect.rect.size.y - _contentSize.y;

                                    if (_childrenCached.Count > 1) spacing = leftover / (_childrenCached.Count - 1);

                                    for (int i = 0; i < _childrenCached.Count; i++)
                                    {
                                        RectTransform rt = _childrenCached[i].Rect;
                                        if (!rt) continue;

                                        rt.anchorMin = rt.anchorMin.With(y: 0);
                                        rt.anchorMax = rt.anchorMax.With(y: 0);
                                        rt.pivot = rt.pivot.With(y: 0);

                                        rt.anchoredPosition = rt.anchoredPosition.With(y: primaryOffset);
                                        primaryOffset += rt.sizeDelta.y + spacing;
                                    }

                                    break;
                                }
                        }
                        break;
                    }
            }

            // cross-axis pass
            float crossOffset = 0;
            switch (m_direction)
            {
                // ROW -> CROSS
                // ROW_REVERSE -> CROSS
                case LayoutDirection.Row:
                case LayoutDirection.RowReverse:
                    {
                        switch (m_alignContent)
                        {
                            case Alignment.Start:
                                {
                                    crossOffset += m_padding.top;

                                    for (int i = 0; i < _childrenCached.Count; i++)
                                    {
                                        RectTransform rt = _childrenCached[i].Rect;
                                        if (!rt) continue;

                                        rt.anchorMin = rt.anchorMin.With(y: 1);
                                        rt.anchorMax = rt.anchorMax.With(y: 1);
                                        rt.pivot = rt.pivot.With(y: 1);

                                        rt.anchoredPosition = rt.anchoredPosition.With(y: -crossOffset);
                                    }

                                    break;
                                }
                            case Alignment.Center:
                                {
                                    for (int i = 0; i < _childrenCached.Count; i++)
                                    {
                                        RectTransform rt = _childrenCached[i].Rect;
                                        if (!rt) continue;

                                        rt.anchorMin = rt.anchorMin.With(y: 0.5f);
                                        rt.anchorMax = rt.anchorMax.With(y: 0.5f);
                                        rt.pivot = rt.pivot.With(y: 0.5f);

                                        rt.anchoredPosition = rt.anchoredPosition.With(y: m_padding.bottom / 2 - m_padding.top / 2);
                                    }

                                    break;
                                }
                            case Alignment.End:
                                {
                                    crossOffset += m_padding.bottom;

                                    for (int i = 0; i < _childrenCached.Count; i++)
                                    {
                                        RectTransform rt = _childrenCached[i].Rect;
                                        if (!rt) continue;

                                        rt.anchorMin = rt.anchorMin.With(y: 0);
                                        rt.anchorMax = rt.anchorMax.With(y: 0);
                                        rt.pivot = rt.pivot.With(y: 0);

                                        rt.anchoredPosition = rt.anchoredPosition.With(y: crossOffset);
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                // COLUMN -> CROSS
                // COLUMN_REVERSE -> CROSS
                case LayoutDirection.Column:
                case LayoutDirection.ColumnReverse:
                    {
                        switch (m_alignContent)
                        {
                            case Alignment.Start:
                                {
                                    crossOffset += m_padding.left;

                                    for (int i = 0; i < _childrenCached.Count; i++)
                                    {
                                        RectTransform rt = _childrenCached[i].Rect;
                                        if (!rt) continue;

                                        rt.anchorMin = rt.anchorMin.With(x: 0);
                                        rt.anchorMax = rt.anchorMax.With(x: 0);
                                        rt.pivot = rt.pivot.With(x: 0);

                                        rt.anchoredPosition = rt.anchoredPosition.With(x: crossOffset);
                                    }
                                    break;
                                }
                            case Alignment.Center:
                                {
                                    for (int i = 0; i < _childrenCached.Count; i++)
                                    {
                                        RectTransform rt = _childrenCached[i].Rect;
                                        if (!rt) continue;

                                        rt.anchorMin = rt.anchorMin.With(x: 0.5f);
                                        rt.anchorMax = rt.anchorMax.With(x: 0.5f);
                                        rt.pivot = rt.pivot.With(x: 0.5f);

                                        rt.anchoredPosition = rt.anchoredPosition.With(x: m_padding.left / 2 - m_padding.right / 2);
                                    }
                                    break;
                                }
                            case Alignment.End:
                                {
                                    crossOffset += m_padding.right;

                                    for (int i = 0; i < _childrenCached.Count; i++)
                                    {
                                        RectTransform rt = _childrenCached[i].Rect;
                                        if (!rt) continue;

                                        rt.anchorMin = rt.anchorMin.With(x: 1);
                                        rt.anchorMax = rt.anchorMax.With(x: 1);
                                        rt.pivot = rt.pivot.With(x: 1);

                                        rt.anchoredPosition = rt.anchoredPosition.With(x: -crossOffset);
                                    }
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        #endregion

        public int CompareTo(Layout other)
        {
            if (Depth < other.Depth) return 1;
            if (Depth == other.Depth) return 0;

            return -1;
        }

        [Button(ButtonSizes.Large)]
        public void RefreshChildCache()
        {
            _childrenCached.Clear();

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform t = transform.GetChild(i);

                // 1. Skip if the object is turned off
                if (!t.gameObject.activeSelf) continue;

                // 2. We only care about UI objects
                if (t is RectTransform rt)
                {
                    t.TryGetComponent(out LayoutItem li);

                    // 3. Logic: If it has NO LayoutItem, we add it. 
                    // If it HAS a LayoutItem, we only add it if Ignore is false.
                    if (li == null || !li.IgnoreLayout)
                    {
                        _childrenCached.Add(new ChildEntry
                        {
                            Rect = rt,
                            Layout = li
                        });
                    }
                }
            }
        }

        private struct ChildEntry
        {
            public RectTransform Rect; // Every child has this
            public LayoutItem Layout;  // Only "Smart" children have this (otherwise null)
        }
    }

    [Serializable]
    public struct Margins
    {
        public float top, bottom, left, right;
    }

    public enum SizingMode
    {
        FitContent,
        Fixed,
        Grow,
    }
}