using UnityEngine;
using UnityEngine.InputSystem;

namespace LatteArt.Player
{
    [RequireComponent(typeof(FirstPersonController))]
    public sealed class FirstPersonQuit : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference quitAction;

        private FirstPersonController _controller;
        private bool _wasInFirstPersonLastFrame;

        private void Awake()
        {
            _controller = GetComponent<FirstPersonController>();
            if (quitAction != null) return;

            Debug.LogError("FirstPersonQuit has no quit action assigned.", this);
            enabled = false;
        }

        private void Update()
        {
            if (_wasInFirstPersonLastFrame && quitAction.action.WasPressedThisFrame()) Quit();
        }

        private void LateUpdate()
        {
            _wasInFirstPersonLastFrame = _controller.enabled;
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
