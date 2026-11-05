using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController_2 : MonoBehaviour
{
    // Movement
    [SerializeField] float speed = 5f;
    [SerializeField] float jumpHeight = 1.8f;
    [SerializeField] float fallMultiplier = 2.2f;

    // Coyote time and jump buffer
    [SerializeField] float coyoteTime = 0.1f;
    [SerializeField] float jumpBuffer = 0.12f;

    Rigidbody rb;
    Transform cam;
    Vector3 move;
    Vector3 flatForward, flatRight;   // stored for Vector3.Dot and Gizmos
    float moveAway, moveRight;        // dot product values

    bool jumpQueued;
    float lastGroundedTime = -1f, lastPressedTime = -1f, lastJumpTime = -1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.sleepThreshold = 0f;   // a sleeping body stops reporting ground contacts

        // Zero friction so the capsule doesn't stick to walls while airborne
        GetComponent<CapsuleCollider>().material = new PhysicsMaterial
        {
            dynamicFriction = 0f,
            staticFriction = 0f,
            frictionCombine = PhysicsMaterialCombine.Minimum
        };

        if (Camera.main != null)
            cam = Camera.main.transform;
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (cam != null)
        {
            flatForward = cam.forward;
            flatForward.y = 0f;
            flatForward.Normalize();

            flatRight = cam.right;
            flatRight.y = 0f;
            flatRight.Normalize();

            move = Vector3.ClampMagnitude(flatRight * h + flatForward * v, 1f);

            // Positive = moving away (W), negative = moving toward (S)
            moveAway = Vector3.Dot(move, flatForward);
            // Positive = moving right (D), negative = moving left (A)
            moveRight = Vector3.Dot(move, flatRight);
        }
        else
        {
            move = Vector3.ClampMagnitude(new Vector3(h, 0f, v), 1f);
            flatForward = Vector3.forward;
            flatRight = Vector3.right;
            moveAway = v;
            moveRight = h;
        }

        if (Input.GetKeyDown(KeyCode.Space))
            lastPressedTime = Time.time;

        bool canJump  = Time.time - lastGroundedTime <= coyoteTime;
        bool wantJump = Time.time - lastPressedTime  <= jumpBuffer;
        if (canJump && wantJump)
        {
            jumpQueued = true;
            lastJumpTime = Time.time;
            lastGroundedTime = lastPressedTime = -1f;   // used up
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(move.x * speed,
                                        rb.linearVelocity.y,
                                        move.z * speed);

        if (jumpQueued)
        {
            jumpQueued = false;
            Vector3 vel = rb.linearVelocity;
            rb.linearVelocity = new Vector3(vel.x, 0f, vel.z);
            float g  = -Physics.gravity.y;
            float v0 = Mathf.Sqrt(2f * g * jumpHeight);
            rb.AddForce(Vector3.up * rb.mass * v0, ForceMode.Impulse);
        }

        if (rb.linearVelocity.y < 0f)
            rb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration);
    }

    // Grounded = touching something whose surface faces mostly upward
    void OnCollisionStay(Collision collision)
    {
        if (Time.time - lastJumpTime < 0.15f) return;   // don't re-ground on the jump frame

        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.5f)
            {
                lastGroundedTime = Time.time;
                return;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = moveAway >= 0f ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up, flatForward * moveAway * 2f);
    }
}