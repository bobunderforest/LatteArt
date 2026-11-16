using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace LatteArt.Hints
{
    public sealed class HintSlider : VisualElement
    {
        private const string TrackClass = "hint__slider";
        private const string FillClass = "hint__slider-fill";
        private const int PrimaryButton = 0;

        private readonly VisualElement _fill = new();
        private float _value;

        public HintSlider()
        {
            AddToClassList(TrackClass);
            _fill.AddToClassList(FillClass);
            _fill.pickingMode = PickingMode.Ignore;
            _fill.style.width = Length.Percent(0f);
            Add(_fill);

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        public event Action<float> ValueChanged;

        public float Value
        {
            get => _value;
            set
            {
                var clampedValue = Mathf.Clamp01(value);
                if (Mathf.Approximately(clampedValue, _value)) return;

                _value = clampedValue;
                _fill.style.width = Length.Percent(_value * 100f);
            }
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != PrimaryButton) return;

            this.CapturePointer(evt.pointerId);
            SetFromPointer(evt.localPosition.x);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!this.HasPointerCapture(evt.pointerId)) return;

            SetFromPointer(evt.localPosition.x);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!this.HasPointerCapture(evt.pointerId)) return;

            this.ReleasePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void SetFromPointer(float localX)
        {
            var trackWidth = resolvedStyle.width;
            if (trackWidth <= 0f) return;

            Value = localX / trackWidth;
            ValueChanged?.Invoke(Value);
        }
    }
}
