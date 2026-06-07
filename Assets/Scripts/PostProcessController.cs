using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MoleculeViewer
{
    public class PostProcessController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Volume postProcessVolume;

        private Bloom _bloomComponent;
        private VolumeProfile _runtimeProfile;

        void Awake()
        {
            if (postProcessVolume == null) 
                postProcessVolume = GetComponent<Volume>();
        }

        void Start()
        {
            if (postProcessVolume == null) return;

            if (postProcessVolume.profile != null)
            {
                _runtimeProfile = postProcessVolume.profile;
                InitializeBloom();
            }
        }

        private void InitializeBloom()
        {
            if (_runtimeProfile == null) return;

            _runtimeProfile.TryGet(out _bloomComponent);
        }

        public void ToggleBloomIntensity()
        {
            if (_bloomComponent == null) return;

            _bloomComponent.intensity.Override(_bloomComponent.intensity.value == 0 ? 1 : 0);
        }
    }
}