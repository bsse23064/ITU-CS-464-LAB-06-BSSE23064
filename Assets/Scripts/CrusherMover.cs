using UnityEngine;

// Raises and slams the whole crusher, driven by the Hazard script's own timing.
// The Hazard's collider is OFF while resting/warning and ON during the strike,
// so we simply follow its state: collider on = slam down, collider off = rise up.
public class CrusherMover : MonoBehaviour
{
    [SerializeField] Collider hazardCollider;   // the collider on the Cube (the one Hazard toggles)
    [SerializeField] float raiseHeight = 3f;
    [SerializeField] float slamTime = 0.1f, riseTime = 0.5f;

    Vector3 down, up;

    void Start()
    {
        down = transform.localPosition;          // the position set in the scene = slammed down
        up = down + Vector3.up * raiseHeight;
        transform.localPosition = up;            // start raised
    }

    void Update()
    {
        if (hazardCollider == null) return;

        bool striking = hazardCollider.enabled;
        Vector3 target = striking ? down : up;
        float time = striking ? slamTime : riseTime;
        float speed = raiseHeight / Mathf.Max(time, 0.01f);

        transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, speed * Time.deltaTime);
    }
}