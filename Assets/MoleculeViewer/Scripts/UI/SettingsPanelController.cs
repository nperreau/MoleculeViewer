using MoleculeViewer;
using UnityEngine;
using UnityEngine.UIElements;

namespace MoleculeViewer
{
    public class SettingsPanelController : MonoBehaviour
{
    [field: SerializeField]
    private PanelRenderer panelRenderer;

    public string closeButtonID = "SettingsPanel-CloseButton";
    private Button closeButton;

    public string showMoleculeNameID = "PhotoMode-ShowMoleculeName";
    private Toggle showMoleculeNameToggle;

    public string showMoleculeFormulaID = "PhotoMode-ShowMoleculeFormula";
    private Toggle showMoleculeFormulaToggle;

    public string useTransparentBackgroundID = "PhotoMode-UseTransparentBackground";
    private Toggle useTransparentBackgroundToggle;

    public GameObject moleculeNameObject;
    public GameObject moleculeFormulaObject;
    
    private void OnEnable() => panelRenderer.RegisterUIReloadCallback(OnUIReload);

    private void OnUIReload(PanelRenderer panelRenderer1, VisualElement rootElement, int version)
    {
        closeButton = rootElement.Q<Button>(closeButtonID);
        closeButton?.RegisterCallback<ClickEvent>(OnCloseClicked);

        showMoleculeNameToggle = rootElement.Q<Toggle>(showMoleculeNameID);
        showMoleculeNameToggle.value = !Photo.Current.objectsToHide.Contains(moleculeNameObject);
        showMoleculeNameToggle?.RegisterCallback<ChangeEvent<bool>>(OnShowMoleculeNameToggleChanged);

        showMoleculeFormulaToggle = rootElement.Q<Toggle>(showMoleculeFormulaID);
        showMoleculeFormulaToggle.value = !Photo.Current.objectsToHide.Contains(moleculeFormulaObject);
        showMoleculeFormulaToggle?.RegisterCallback<ChangeEvent<bool>>(OnShowMoleculeFormulaToggleChanged);

        useTransparentBackgroundToggle = rootElement.Q<Toggle>(useTransparentBackgroundID);
        useTransparentBackgroundToggle?.RegisterCallback<ChangeEvent<bool>>(OnUseTransparentBackgroundToggleChanged);
    }

    private void OnCloseClicked(ClickEvent evt) => gameObject.SetActive(false);

    private void OnDisable()
    {
        showMoleculeNameToggle?.UnregisterCallback<ChangeEvent<bool>>(OnShowMoleculeNameToggleChanged);
        showMoleculeFormulaToggle?.UnregisterCallback<ChangeEvent<bool>>(OnShowMoleculeFormulaToggleChanged);
        useTransparentBackgroundToggle?.UnregisterCallback<ChangeEvent<bool>>(OnUseTransparentBackgroundToggleChanged);
    }

    private void OnShowMoleculeNameToggleChanged(ChangeEvent<bool> evt)
    {
        if (evt.newValue)
            Photo.Current.objectsToHide.Remove(moleculeNameObject);
        else
        {
            if (!Photo.Current.objectsToHide.Contains(moleculeNameObject))
                Photo.Current.objectsToHide.Add(moleculeNameObject);
        }
    }

    private void OnShowMoleculeFormulaToggleChanged(ChangeEvent<bool> evt)
    {
        if (evt.newValue)
            Photo.Current.objectsToHide.Remove(moleculeFormulaObject);
        else
        {
            if (!Photo.Current.objectsToHide.Contains(moleculeFormulaObject))
                Photo.Current.objectsToHide.Add(moleculeFormulaObject);
        }
    }

    private void OnUseTransparentBackgroundToggleChanged(ChangeEvent<bool> evt) => Photo.Current.useTransparentBackground = evt.newValue;
}

}
