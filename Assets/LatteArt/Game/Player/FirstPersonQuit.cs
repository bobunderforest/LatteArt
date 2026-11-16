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
#if UNITY_WEBGL && !UNITY_EDITOR
            LeaveEmbeddingPageOnPointerUnlock();
#endif
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
#elif UNITY_WEBGL
            LeaveEmbeddingPage();
#else
            Application.Quit();
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void LeaveEmbeddingPage();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void LeaveEmbeddingPageOnPointerUnlock();
#endif
    }
}
