using UnityEngine;

// Stealth vision for a guard.
// - Draws a field-of-view cone on the floor that the player can see (clipped by walls).
// - The cone fades from calm to alert as the guard notices the player.
// - Once the player has been in view for detectTime seconds, they take damage.
public class GuardVision : MonoBehaviour
{
    [Header("Vision")]
    [SerializeField] float viewDistance = 7f;
    [SerializeField, Range(10f, 180f)] float viewAngle = 60f;
    [SerializeField] LayerMask obstacleMask = ~0;       // what blocks sight (walls, crates...)

    [Header("Detection")]
    [SerializeField] float detectTime = 0.5f;           // seconds in view before the player is caught
    [SerializeField] float calmDownTime = 1f;           // seconds for the alert to fade after losing the player
    [SerializeField] int damage = 1;                    // use a big number (e.g. 99) for instant fail

    [Header("Cone look")]
    [SerializeField] Color calmColor = new Color(1f, 0.9f, 0.2f, 0.3f);
    [SerializeField] Color alertColor = new Color(1f, 0.1f, 0.1f, 0.5f);
    [SerializeField] float floorOffset = -0.95f;        // cone height relative to the guard's pivot (about floor level)
    [SerializeField] int rays = 40;                     // cone smoothness

    Transform player;
    Health playerHealth;
    Mesh mesh;
    Material coneMat;
    Vector3[] verts;
    float alert;                                        // 0 = unaware, 1 = spotted

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) { player = p.transform; playerHealth = p.GetComponent<Health>(); }

        // Build the cone object at runtime, so the prefab needs no extra setup
        GameObject cone = new GameObject("VisionCone");
        cone.transform.SetParent(transform, false);

        verts = new Vector3[rays + 2];
        int[] tris = new int[rays * 3];
        Color[] colors = new Color[rays + 2];
        for (int i = 0; i < rays; i++) { tris[i * 3] = 0; tris[i * 3 + 1] = i + 1; tris[i * 3 + 2] = i + 2; }
        for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;

        mesh = new Mesh { name = "VisionCone" };
        mesh.MarkDynamic();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.colors = colors;

        cone.AddComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer mr = cone.AddComponent<MeshRenderer>();
        coneMat = new Material(Shader.Find("Sprites/Default"));   // unlit + transparent, no material setup needed
        mr.sharedMaterial = coneMat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
    }

    void Update()
    {
        bool seen = CanSeePlayer();
        alert += (seen ? 1f / detectTime : -1f / calmDownTime) * Time.deltaTime;
        alert = Mathf.Clamp01(alert);

        // Health's i-frames limit how often this actually lands
        if (seen && alert >= 1f && playerHealth != null) playerHealth.TakeDamage(damage);

        coneMat.color = Color.Lerp(calmColor, alertColor, alert);
    }

    // Rebuilt after the guard has moved and turned this frame
    void LateUpdate()
    {
        Vector3 origin = transform.position;
        float half = viewAngle * 0.5f;
        verts[0] = new Vector3(0f, floorOffset, 0f);

        for (int i = 0; i <= rays; i++)
        {
            float a = Mathf.Lerp(-half, half, i / (float)rays);
            Vector3 dir = Quaternion.AngleAxis(a, Vector3.up) * transform.forward;

            float d = viewDistance;
            if (Physics.Raycast(origin, dir, out RaycastHit hit, viewDistance, obstacleMask, QueryTriggerInteraction.Ignore))
                d = hit.distance;

            Vector3 local = transform.InverseTransformDirection(dir) * d;
            verts[i + 1] = new Vector3(local.x, floorOffset, local.z);
        }

        mesh.vertices = verts;
        mesh.RecalculateBounds();
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 to = player.position - transform.position;
        Vector3 flat = new Vector3(to.x, 0f, to.z);
        if (flat.magnitude > viewDistance) return false;
        if (Vector3.Angle(transform.forward, flat) > viewAngle * 0.5f) return false;

        // Line of sight: the first thing the ray hits must be the player, not a wall
        if (Physics.Raycast(transform.position, to, out RaycastHit hit, viewDistance + 1f, obstacleMask, QueryTriggerInteraction.Ignore))
            return hit.transform == player || hit.transform.IsChildOf(player);
        return false;
    }

    void OnDestroy()
    {
        if (mesh != null) Destroy(mesh);
        if (coneMat != null) Destroy(coneMat);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        float half = viewAngle * 0.5f;
        Gizmos.DrawRay(transform.position, Quaternion.AngleAxis(-half, Vector3.up) * transform.forward * viewDistance);
        Gizmos.DrawRay(transform.position, Quaternion.AngleAxis(half, Vector3.up) * transform.forward * viewDistance);
    }
}