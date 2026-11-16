using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace LatteArt.Hints
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class HintPanel : MonoBehaviour
    {
        private const string ListName = "hint-list";

        private readonly List<HintRow> _rows = new();
        private UIDocument _document;
        private IReadOnlyList<HintEntry> _entries = Array.Empty<HintEntry>();
        private bool _isPointerOver;
        private VisualElement _list;

        public event Action<bool> PointerOverChanged;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            _list = _document.rootVisualElement.Q(ListName);
            if (_list == null)
            {
                Debug.LogError($"HintPanel needs a UI Document with an element named '{ListName}'.", this);
                enabled = false;
                return;
            }

            _list.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            _list.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            BuildRows();
        }

        private void OnDisable()
        {
            if (_list != null)
            {
                _list.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
                _list.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
            }

            SetPointerOver(false);
            ClearRows();
            _list = null;
        }

        private void Update()
        {
            RefreshRows();
        }

        public void SetEntries(IReadOnlyList<HintEntry> entries)
        {
            _entries = entries;
            if (_list == null) return;

            ClearRows();
            BuildRows();
        }

        private void OnPointerEnter(PointerEnterEvent evt)
        {
            SetPointerOver(true);
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            SetPointerOver(false);
        }

        private void SetPointerOver(bool isPointerOver)
        {
            if (isPointerOver == _isPointerOver) return;

            _isPointerOver = isPointerOver;
            PointerOverChanged?.Invoke(isPointerOver);
        }

        private void BuildRows()
        {
            foreach (var entry in _entries)
            {
                var row = new HintRow(entry);
                _rows.Add(row);
                _list.Add(row);
            }

            RefreshRows();
        }

        private void RefreshRows()
        {
            foreach (var row in _rows)
            {
                row.Refresh();
            }
        }

        private void ClearRows()
        {
            _rows.Clear();
            _list?.Clear();
        }
    }
}
