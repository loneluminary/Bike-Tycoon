using System.Collections.Generic;
using UnityEngine;

namespace Poke.UI
{
    [ExecuteAlways, RequireComponent(typeof(RectTransform))]
    public class LayoutRoot : MonoBehaviour
    {
        [SerializeField] private int m_tickRate = 60;

        private readonly SortedBucket<Layout, int, Layout> _layouts = new(l => l, l => l.GetInstanceID());
        private readonly Stack<Layout> _reverse = new(100);
        private float _tickInterval;
        private float _lastTickTimestamp;
        private bool _tick;

        private void Awake()
        {
            _tickInterval = 1.0f / m_tickRate;
        }

        private void Start()
        {
            ForceUpdate();
        }

        public void Update()
        {
            if (Time.unscaledTime - _lastTickTimestamp >= _tickInterval)
            {
                _tick = true;
            }
        }

        public void LateUpdate()
        {
            if (_tick)
            {
                _reverse.Clear();

                foreach (Layout l in _layouts)
                {
                    l.RefreshChildCache();
                    l.ComputeFitSize();
                    _reverse.Push(l);
                }

                foreach (Layout l in _layouts) l.GrowChildren();
                foreach (Layout l in _reverse) l.ComputeLayout();

                _lastTickTimestamp = Time.unscaledTime;
                _tick = false;
            }
        }

        public void ForceUpdate()
        {
            _tick = true;
            LateUpdate();
        }

        public void RegisterLayout(Layout layout)
        {
            //Debug.Log($"Registered \"{layout.name}\" at depth [{layout.Depth}]");
            _layouts.Add(layout);
        }

        public void UnregisterLayout(Layout layout)
        {
            if (_layouts.Remove(layout))
            {
                //Debug.Log($"Removed \"{layout.name}\"");
            }
            else
            {
                Debug.LogError($"Failed to remove \"{layout.name}\" (not found)");
            }
        }
    }
}