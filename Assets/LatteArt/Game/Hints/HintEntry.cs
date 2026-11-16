using System;

namespace LatteArt.Hints
{
    public sealed class HintEntry
    {
        public HintEntry(string keyLabel, string caption, Action perform = null, Func<bool> isHighlighted = null,
            Func<float> readSlider = null, Action<float> writeSlider = null)
        {
            KeyLabel = keyLabel;
            Caption = caption;
            Perform = perform;
            IsHighlighted = isHighlighted;
            ReadSlider = readSlider;
            WriteSlider = writeSlider;
        }

        public string KeyLabel { get; }
        public string Caption { get; }
        public Action Perform { get; }
        public Func<bool> IsHighlighted { get; }
        public Func<float> ReadSlider { get; }
        public Action<float> WriteSlider { get; }

        public bool IsClickable => Perform != null;
        public bool HasSlider => ReadSlider != null && WriteSlider != null;
    }
}
