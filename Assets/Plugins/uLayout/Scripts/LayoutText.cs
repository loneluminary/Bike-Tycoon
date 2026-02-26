using TMPro;
using UnityEngine;

namespace Poke.UI
{
    [ExecuteAlways, RequireComponent(typeof(TMP_Text))]
    public class LayoutText : LayoutItem
    {
        private TMP_Text _text;
        private DrivenRectTransformTracker _rectTracker;

        protected override void Awake()
        {
            base.Awake();
            _text = GetComponent<TMP_Text>();
            _rectTracker = new DrivenRectTransformTracker();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _rectTracker.Clear();
        }

        public override void Update()
        {
            base.Update();

            _rectTracker.Clear();

            _text.textWrappingMode = m_sizing.x == SizingMode.Grow ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;

            if (m_sizing.x == SizingMode.FitContent)
            {
                _rectTracker.Add(this, _rect, DrivenTransformProperties.SizeDeltaX);
                _rect.sizeDelta = _rect.sizeDelta.With(x: _text.preferredWidth);
            }

            if (m_sizing.y == SizingMode.FitContent)
            {
                _rectTracker.Add(this, _rect, DrivenTransformProperties.SizeDeltaY);

                float height = 0;
                for (int i = 0; i < _text.textInfo.lineCount; i++) height += _text.textInfo.lineInfo[i].lineHeight;
                _rect.sizeDelta = _rect.sizeDelta.With(y: height);
            }
        }
    }
}