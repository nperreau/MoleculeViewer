using UnityEngine;
using UnityEngine.InputSystem;

namespace MoleculeViewer
{
    [DisallowMultipleComponent]
    public sealed class MoleculeRotator : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference pointerPosition;
        [SerializeField] private InputActionReference grab;

        [Header("References")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Transform moleculeRoot;
        [SerializeField] private Collider grabCollider;

        [Header("Rotation Feel")]
        [SerializeField, Min(0.001f)] private float rotationSensitivity = 0.2f;
        [SerializeField, Min(0.1f)] private float damping = 18f;

        [Header("Raycast")]
        [SerializeField, Min(0.1f)] private float maxRayDistance = 1000f;

        private bool dragging;
        private Vector2 lastPointerPosition;
        private Quaternion targetRotation;

        [SerializeField]
        private float maxPointerDelta = 80f;

        private void Awake()
        {
            if (!targetCamera)
                targetCamera = Camera.main;

            if (!moleculeRoot)
                moleculeRoot = transform;

            targetRotation = moleculeRoot.rotation;
        }

        private void OnDisable() => dragging = false;

        private void Update()
        {
            if (!CanRotate())
                return;

            Vector2 pointer = pointerPosition.action.ReadValue<Vector2>();
            bool isPressed = grab.action.IsPressed();

            if (isPressed && !dragging && IsPointerOverMolecule(pointer))
                BeginDrag(pointer);

            if (!isPressed && dragging)
                EndDrag();

            if (dragging)
                UpdateDrag(pointer);

            ApplySmoothRotation();
        }

        private bool CanRotate()
        {
            return pointerPosition
                   && pointerPosition.action != null
                   && grab
                   && grab.action != null
                   && moleculeRoot
                   && targetCamera
                   && grabCollider;
        }

        private void BeginDrag(Vector2 pointer)
        {
            dragging = true;
            lastPointerPosition = pointer;
        }

        private void EndDrag()
        {
            dragging = false;
        }

        private void UpdateDrag(Vector2 pointer)
        {
            Vector2 delta = pointer - lastPointerPosition;

            delta = Vector2.ClampMagnitude(delta, maxPointerDelta);

            Quaternion yaw = Quaternion.AngleAxis(
                -delta.x * rotationSensitivity,
                targetCamera.transform.up
            );

            Quaternion pitch = Quaternion.AngleAxis(
                delta.y * rotationSensitivity,
                targetCamera.transform.right
            );

            targetRotation = yaw * pitch * targetRotation;
            lastPointerPosition = pointer;
        }

        private void ApplySmoothRotation()
        {
            moleculeRoot.rotation = Quaternion.Slerp(
                moleculeRoot.rotation,
                targetRotation,
                1f - Mathf.Exp(-damping * Time.deltaTime)
            );
        }

        private bool IsPointerOverMolecule(Vector2 screenPosition)
        {
            Ray ray = targetCamera.ScreenPointToRay(screenPosition);

            return Physics.Raycast(
                ray,
                out RaycastHit hitInfo,
                maxRayDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Collide
            ) && hitInfo.collider == grabCollider;
        }

        public void ResetRotation()
        {
            targetRotation = Quaternion.identity;
            moleculeRoot.rotation = Quaternion.identity;
            dragging = false;
        }
    }
}