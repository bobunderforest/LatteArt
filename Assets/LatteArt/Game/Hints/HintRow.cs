using UnityEngine.UIElements;

namespace LatteArt.Hints
{
    public sealed class HintRow : VisualElement
    {
        private const string RowClass = "hint";
        private const string ClickableRowClass = "hint--clickable";
        private const string HighlightedRowClass = "hint--highlighted";
        private const string KeyClass = "hint__key";
        private const string CaptionClass = "hint__caption";

        private readonly HintEntry _entry;
        private readonly HintSlider _slider;

        public HintRow(HintEntry entry)
        {
            _entry = entry;
            AddToClassList(RowClass);
            Add(CreateLabel(entry.KeyLabel, KeyClass));
            Add(CreateLabel(entry.Caption, CaptionClass));

            if (entry.HasSlider)
            {
                _slider = new HintSlider();
                _slider.ValueChanged += entry.WriteSlider;
                Add(_slider);
            }

            if (!entry.IsClickable) return;

            AddToClassList(ClickableRowClass);
            RegisterCallback<ClickEvent>(_ => entry.Perform());
        }

        public void Refresh()
        {
            EnableInClassList(HighlightedRowClass, _entry.IsHighlighted != null && _entry.IsHighlighted());
            if (_slider != null) _slider.Value = _entry.ReadSlider();
        }

        private static Label CreateLabel(string text, string styleClass)
        {
            var label = new Label(text);
            label.AddToClassList(styleClass);
            return label;
        }
    }
}
