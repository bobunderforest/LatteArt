using UnityEngine;
using UnityEngine.UIElements;

namespace LatteArt.Compatibility
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class ComputeSupportNotice : MonoBehaviour
    {
        private void Awake()
        {
            if (SystemInfo.supportsComputeShaders) GetComponent<UIDocument>().enabled = false;
        }
    }
}
