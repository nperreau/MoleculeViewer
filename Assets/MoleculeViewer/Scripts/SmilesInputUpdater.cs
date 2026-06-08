using System.Collections;
using MoleculeViewer;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

public class SmilesInputUpdater : MonoBehaviour
{
    [field: SerializeField]
    public TMP_InputField Text { get; private set; }
    [field: SerializeField, FormerlySerializedAs("<Visualizer>k__BackingField")]
    public RDKit3DGenerator Generator { get; private set; }
    [field: SerializeField]
    public MoleculeNameLoader NameLoader { get; private set; }

    private bool _debouncing = false;

    private void OnEnable()
    {
        if (Text)
            Text.onValueChanged.AddListener(OnChanged);
    }

    private void OnDisable()
    {
        if (Text)
            Text.onValueChanged.RemoveListener(OnChanged);
    }

    private void OnChanged(string smiles) => Debounce();

    private void Debounce()
    {
        if (_debouncing)
            return;

        StartCoroutine(DebounceRoutine());
    }

    IEnumerator DebounceRoutine()
    {
        _debouncing = true;

        yield return new WaitForEndOfFrame();

        if (Generator)
            _ = Generator.GenerateAndPrepareMolecule(Text.text);

        if (NameLoader)
            NameLoader.SetSMILES(Text.text);

        _debouncing = false;
    }
}
