using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

namespace MoleculeViewer.UI
{
    [Tooltip("Makes a selectable object always focused.")]
    public class AlwaysFocus : MonoBehaviour, IDeselectHandler
    {
        [field: SerializeField]
        private Selectable _target;
        public Selectable Target
        {
            get => _target;
            set
            {
                _target = value;
                _tmpField = _target.GetComponent<TMP_InputField>();
            }
        }
        private TMP_InputField _tmpField;

        private void Awake()
        {
            if (_target)
                _tmpField = _target.GetComponent<TMP_InputField>();
        }

        private void Start() => GrabFocus();

        private void OnEnable() => GrabFocus();

        public void OnDeselect(BaseEventData eventData) => GrabFocus();

        private void GrabFocus()
        {
            if (!EventSystem.current)
                return;

            EventSystem.current.SetSelectedGameObject(Target.gameObject);

            if (_tmpField)
                _tmpField.ActivateInputField();
        }
    }
}
