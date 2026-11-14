using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace LatteArt.Hints
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class HintPanel : MonoBehaviour
    {
        private const string ListName = "hint-list";
        private const string RowClass = "hint";
        private const string HighlightedRowClass = "hint--highlighted";
        private const string KeyClass = "hint__key";
        private const string CaptionClass = "hint__caption";

        [SerializeField] private Hint[] hints = { };

        private UIDocument _document;
        private bool _hasValidSetup;
        private InputAction _highlightedAction;
        private VisualElement _list;
        private VisualElement[] _rows;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            _rows = new VisualElement[hints.Length];
            _hasValidSetup = HasValidSetup();
        }

        private void OnEnable()
        {
            if (!_hasValidSetup)
            {
                enabled = false;
                return;
            }

            _list = _document.rootVisualElement.Q(ListName);
            if (_list == null)
            {
                Debug.LogError($"HintPanel needs a UI Document with an element named '{ListName}'.", this);
                enabled = false;
                return;
            }

            for (var i = 0; i < hints.Length; i++)
            {
                _rows[i] = CreateRow(hints[i]);
                _list.Add(_rows[i]);
            }

            ApplyHighlight();
        }

        private void OnDisable()
        {
            _list?.Clear();
            _list = null;
        }

        public void Highlight(InputAction action)
        {
            _highlightedAction = action;
            ApplyHighlight();
        }

        private void ApplyHighlight()
        {
            if (_list == null) return;

            for (var i = 0; i < hints.Length; i++)
            {
                _rows[i].EnableInClassList(HighlightedRowClass, hints[i].IsFor(_highlightedAction));
            }
        }

        private static VisualElement CreateRow(Hint hint)
        {
            var row = new VisualElement();
            row.AddToClassList(RowClass);
            row.Add(CreateLabel(hint.GetKeyLabel(), KeyClass));
            row.Add(CreateLabel(hint.Caption, CaptionClass));
            return row;
        }

        private static Label CreateLabel(string text, string styleClass)
        {
            var label = new Label(text);
            label.AddToClassList(styleClass);
            return label;
        }

        private bool HasValidSetup()
        {
            var hasAllHintsAssigned = hints.Length > 0;
            foreach (var hint in hints)
            {
                hasAllHintsAssigned &= hint != null && hint.IsAssigned;
            }

            if (hasAllHintsAssigned) return true;

            Debug.LogError("HintPanel has hints without an action or caption.", this);
            return false;
        }
    }
}
