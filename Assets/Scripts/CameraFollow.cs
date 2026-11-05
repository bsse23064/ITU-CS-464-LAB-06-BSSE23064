using UnityEngine;

// Orbit camera: follows the player and is steered with the mouse.
//   Hold right mouse + move = orbit, scroll wheel = zoom.
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] Transform target;              // leave empty: found automatically by the "Player" tag
    [SerializeField] float heightOffset = 1f;       // look at a point this far above the player's pivot
    [SerializeField] float smoothTime = 0.15f;

    [Header("Mouse look")]
    [SerializeField] bool holdRightMouse = true;    // off = always look around, cursor locked
    [SerializeField] float sensitivity = 3f;
    [SerializeField] float minPitch = 10f, maxPitch = 80f;
    [SerializeField] float startPitch = 40f, startYaw = 0f;

    [Header("Zoom")]
    [SerializeField] float distance = 10f;
    [SerializeField] float minDistance = 3f, maxDistance = 16f;
    [SerializeField] float zoomSpeed = 15f;

    [Header("Wall collision")]
    [SerializeField] LayerMask collisionMask = ~0;  // the camera slides in front of anything solid
    [SerializeField] float collisionRadius = 0.3f;

    float yaw, pitch, currentDistance;
    Vector3 pivot, pivotVelocity;

    void Awake()
    {
        if (target == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }
    }

    void Start()
    {
        yaw = startYaw;
        pitch = startPitch;
        currentDistance = distance;
        if (target != null) pivot = target.position + Vector3.up * heightOffset;
    }

    void LateUpdate()
    {
        if (target == null) return;

        bool paused = Time.timeScale == 0f;          // win/lose screens: stop steering, free the cursor
        UpdateCursor(paused);

        if (!paused)
        {
            if (!holdRightMouse || Input.GetMouseButton(1))
            {
                yaw += Input.GetAxis("Mouse X") * sensitivity;
                pitch -= Input.GetAxis("Mouse Y") * sensitivity;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }
            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, minDistance, maxDistance);
        }

        // Follow the player smoothly
        Vector3 wanted = target.position + Vector3.up * heightOffset;
        pivot = Vector3.SmoothDamp(pivot, wanted, ref pivotVelocity, smoothTime);

        // Place the camera behind the pivot, pulled in if a wall is in the way
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 back = rot * Vector3.back;

        float allowed = distance;
        if (Physics.SphereCast(pivot, collisionRadius, back, out RaycastHit hit, distance, collisionMask, QueryTriggerInteraction.Ignore))
            allowed = hit.distance;

        // Snap in at once (no clipping), ease back out
        currentDistance = allowed < currentDistance
            ? allowed
            : Mathf.Lerp(currentDistance, allowed, 8f * Time.unscaledDeltaTime);

        transform.SetPositionAndRotation(pivot + back * currentDistance, rot);
    }

    void UpdateCursor(bool paused)
    {
        if (holdRightMouse) return;                  // cursor stays free in right-mouse mode
        bool lockIt = !paused;
        Cursor.lockState = lockIt ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockIt;
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}