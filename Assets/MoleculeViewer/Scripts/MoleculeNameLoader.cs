using System;
using System.Collections;
using System.Linq;
using MoleculeViewer.PubChem;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace MoleculeViewer
{
    public class MoleculeNameLoader : MonoBehaviour
    {
        public float requestsDelay = 0.5f;
        private float lastTextUpdate = 0;
        private string smiles;
        private bool newRequestSent = false;
        private bool textReady = false;

        public GameObject loadingAnimation;

        private UnityWebRequest currentRequest;
        Coroutine currentCoroutine;

        [field: SerializeField]
        public TextMeshProUGUI NameText { get; private set; }
        [field: SerializeField]
        public TextMeshProUGUI FormulaText { get; private set; }

        public string CurrentMoleculeName => NameText.text;
        
        public static MoleculeNameLoader Current { get; private set; }
        
        private bool ShouldDisplayText => textReady;
        private bool ShouldDisplayLoading => !textReady && newRequestSent;

        private void Awake()
        {
            if (Current)
                Debug.LogWarning($"{GetType().Name} found more than once!");

            Current = this;
        }

        public void SetSMILES(string smiles)
        {
            if (this.smiles == smiles)
                return;

            this.smiles = smiles;
            newRequestSent = false;
            lastTextUpdate = Time.time;
            textReady = false;

            AbortCurrentRequest();
        }

        void AbortCurrentRequest()
        {
            currentRequest?.Abort();
            currentRequest = null;

            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);
        }
        
        private void Update()
        {
            if (!string.IsNullOrWhiteSpace(smiles) && lastTextUpdate + requestsDelay < Time.time && !newRequestSent)
            {
                newRequestSent = true;
                currentCoroutine = StartCoroutine(GetMoleculeName(smiles));
            }

            bool shouldDisplayText = ShouldDisplayText;
            NameText.gameObject.SetActive(shouldDisplayText);
            FormulaText.gameObject.SetActive(shouldDisplayText);

            if (loadingAnimation)
                loadingAnimation.SetActive(ShouldDisplayLoading);
        }

        IEnumerator GetMoleculeName(string smiles)
        {
            string url = URIs.SmilesDescription(smiles);
            using (UnityWebRequest newRequest = UnityWebRequest.Get(url))
            {
                currentRequest = newRequest;
                yield return newRequest.SendWebRequest();

                NameText.text = newRequest.result == UnityWebRequest.Result.Success
                    ? JsonUtility.FromJson<PubChemDescriptionResponse>(newRequest.downloadHandler.text)
                      .InformationList.Information.First().Title
                    : string.Empty;
                textReady = true;

                if (newRequest.result != UnityWebRequest.Result.Success)
                    Debug.LogError("Error: " + newRequest.error);
            }

            currentRequest = null;
        }

        IEnumerator GetMoleculeIupac(string smiles)
        {
            string url = URIs.SmilesProperties(smiles, "IUPACName", "MolecularFormula");
            using (UnityWebRequest newRequest = UnityWebRequest.Get(url))
            {
                currentRequest = newRequest;
                yield return newRequest.SendWebRequest();

                if (newRequest.result == UnityWebRequest.Result.Success)
                {
                    var data = JsonUtility.FromJson<PubChemResponse>(newRequest.downloadHandler.text);

                    NameText.text = $"{data.PropertyTable.Properties.First().IUPACName}";
                    textReady = true;
                }
                else
                    Debug.LogError("Error: " + newRequest.error);
            }

            currentRequest = null;
        }
    }
}
