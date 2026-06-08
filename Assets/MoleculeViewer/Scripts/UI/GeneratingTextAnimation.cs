using System.Collections;
using TMPro;
using UnityEngine;

namespace MoleculeViewer.UI
{
    public class GeneratingTextAnimation : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI text;

        public float delay = 0.5f;

        private Coroutine _animationCoroutine;

        private void Awake()
        {
            if (!text)
                text = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable() => _animationCoroutine = StartCoroutine(Animation());
        private void OnDisable() => StopCoroutine(_animationCoroutine);

        IEnumerator Animation()
        {
            while (true)
            {
                text.text = "Generating";
                yield return new WaitForSeconds(delay);

                text.text = "Generating.";
                yield return new WaitForSeconds(delay);

                text.text = "Generating..";
                yield return new WaitForSeconds(delay);

                text.text = "Generating...";
                yield return new WaitForSeconds(delay);
            }
        }
    }
}
