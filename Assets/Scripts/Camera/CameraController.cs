using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;

    [Header("Pan")]
    [SerializeField] private float keyPanSpeed = 25f;
    [SerializeField] private float dragPanSpeed = 0.15f; // world units per pixel
    [SerializeField] private bool useRightMouseDrag = true;
    [SerializeField] private bool useMiddleMouseDrag = true;

    [Header("Zoom (distance along pitch)")]
    [SerializeField] private float zoomSpeed = 80f;
    [SerializeField] private float minZoomDistance = 10f;
    [SerializeField] private float maxZoomDistance = 60f;

    [Header("View Angle")]
    [Range(10f, 80f)]
    [SerializeField] private float pitchDegrees = 55f; // downward tilt
    [SerializeField] private float yawDegrees = 0f;    // rotate around Y if you want

    [Header("Bounds (world space, XZ)")]
    [SerializeField] private Vector3 boundsMin = new Vector3(-50, 0, -50);
    [SerializeField] private Vector3 boundsMax = new Vector3( 50, 0,  50);

    [Header("Smoothing")]
    [SerializeField] private float positionLerp = 12f;
    [SerializeField] private float zoomLerp = 12f;

    private Vector3 _desiredRigPos;
    private float _desiredZoomDistance;
    private Vector3 _lastMousePos;

    private void Reset()
    {
        targetCamera = GetComponentInChildren<Camera>();
    }

    private void Awake()
    {
        if (!targetCamera) targetCamera = GetComponentInChildren<Camera>();
        if (!targetCamera)
        {
            Debug.LogError($"{nameof(CameraController)}: No Camera assigned/found.");
            enabled = false;
            return;
        }

        _desiredRigPos = transform.position;

        // Initialize distance from current camera placement (approx)
        _desiredZoomDistance = EstimateCurrentDistance();
        _desiredZoomDistance = Mathf.Clamp(_desiredZoomDistance, minZoomDistance, maxZoomDistance);

        ApplyCameraAngle();
        ApplyZoomImmediate(_desiredZoomDistance);
    }

    private void Update()
    {
        HandleKeyboardPan();
        HandleMouseDragPan();
        HandleZoom();

        // Clamp desired rig position to bounds (XZ)
        _desiredRigPos = ClampToBoundsXZ(_desiredRigPos);

        // Smooth rig movement
        transform.position = Vector3.Lerp(transform.position, _desiredRigPos, 1f - Mathf.Exp(-positionLerp * Time.deltaTime));

        // Smooth zoom
        float currentDistance = EstimateCurrentDistance();
        float newDistance = Mathf.Lerp(currentDistance, _desiredZoomDistance, 1f - Mathf.Exp(-zoomLerp * Time.deltaTime));
        ApplyZoomImmediate(newDistance);
    }

    private void HandleKeyboardPan()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        if (Mathf.Approximately(h, 0f) && Mathf.Approximately(v, 0f)) return;

        // Pan relative to yaw (so WASD matches camera orientation if you change yaw)
        Quaternion yawRot = Quaternion.Euler(0f, yawDegrees, 0f);
        Vector3 move = yawRot * new Vector3(h, 0f, v).normalized;

        _desiredRigPos += move * keyPanSpeed * Time.deltaTime;
    }

    private void HandleMouseDragPan()
    {
        bool dragging =
            (useMiddleMouseDrag && Input.GetMouseButton(2)) ||
            (useRightMouseDrag && Input.GetMouseButton(1));

        if (dragging)
        {
            Vector3 mouseDelta = Input.mousePosition - _lastMousePos;

            // Move opposite the drag direction (typical RTS feel)
            Vector3 right = targetCamera.transform.right;   right.y = 0f; right.Normalize();
            Vector3 forward = targetCamera.transform.forward; forward.y = 0f; forward.Normalize();

            Vector3 move = (-right * mouseDelta.x + -forward * mouseDelta.y) * dragPanSpeed;
            _desiredRigPos += move;
        }

        _lastMousePos = Input.mousePosition;
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Approximately(scroll, 0f)) return;

        // scroll>0 typically means zoom in
        _desiredZoomDistance -= scroll * zoomSpeed;
        _desiredZoomDistance = Mathf.Clamp(_desiredZoomDistance, minZoomDistance, maxZoomDistance);
    }

    private void ApplyCameraAngle()
    {
        // Keep camera rotation fixed relative to rig
        targetCamera.transform.localRotation = Quaternion.Euler(pitchDegrees, 0f, 0f);

        // Optionally rotate rig for yaw
        transform.rotation = Quaternion.Euler(0f, yawDegrees, 0f);
    }

    private void ApplyZoomImmediate(float distance)
    {
        // Place camera at a fixed pitch on a sphere segment around the rig:
        // local position is (0, sin(pitch)*d, -cos(pitch)*d)
        float rad = pitchDegrees * Mathf.Deg2Rad;
        float y = Mathf.Sin(rad) * distance;
        float z = -Mathf.Cos(rad) * distance;

        targetCamera.transform.localPosition = new Vector3(0f, y, z);
    }

    private float EstimateCurrentDistance()
    {
        // Inverse of ApplyZoomImmediate (approx). Uses y component and pitch.
        float rad = pitchDegrees * Mathf.Deg2Rad;
        float y = targetCamera.transform.localPosition.y;
        float sin = Mathf.Max(0.0001f, Mathf.Sin(rad));
        return Mathf.Abs(y / sin);
    }

    private Vector3 ClampToBoundsXZ(Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, boundsMin.x, boundsMax.x);
        pos.z = Mathf.Clamp(pos.z, boundsMin.z, boundsMax.z);
        return pos;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Draw bounds in editor (XZ box, with some height so you can see it)
        Vector3 center = (boundsMin + boundsMax) * 0.5f;
        Vector3 size = new Vector3(Mathf.Abs(boundsMax.x - boundsMin.x), 1f, Mathf.Abs(boundsMax.z - boundsMin.z));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center + Vector3.up * 0.5f, size);
    }
#endif
}