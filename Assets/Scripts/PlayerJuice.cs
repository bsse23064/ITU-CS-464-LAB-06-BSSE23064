using System.Collections;
using UnityEngine;

public enum HitFeedback { None, Squash, Shake, Both }

public class PlayerJuice : MonoBehaviour
{
    [SerializeField] Transform body;        // visual child of the player to squash
    [SerializeField] Transform cameraRig;   // object to shake (e.g. Main Camera)
    [SerializeField] float squashAmount = 0.28f;
    [SerializeField] float shakeAmount = 0.6f, shakeSize = 1f;

    float shake;                            // 0 to 1, decays over time
    Vector3 bodyStart, camStart;
    Coroutine squashRoutine;

    void Start()
    {
        if (body != null) bodyStart = body.localScale;
        if (cameraRig != null) camStart = cameraRig.localPosition;
    }

    public void Play(HitFeedback feedback, float strength = 1f)
    {
        if (feedback == HitFeedback.Squash || feedback == HitFeedback.Both) Squash(strength);
        if (feedback == HitFeedback.Shake || feedback == HitFeedback.Both) Shake(strength);
    }

    public void Squash(float strength)
    {
        if (body == null) return;
        if (squashRoutine != null) StopCoroutine(squashRoutine);
        squashRoutine = StartCoroutine(SquashRoutine(squashAmount * strength));
    }

    public void Shake(float strength) => shake = Mathf.Min(1f, shake + shakeAmount * strength);

    IEnumerator SquashRoutine(float amount)
    {
        for (float t = 0f; t < 0.5f; t += Time.deltaTime)
        {
            float y = 1f - amount * Mathf.Exp(-7f * t) * Mathf.Cos(22f * t);
            float xz = 1f / Mathf.Sqrt(y);                       // keeps the volume the same
            body.localScale = Vector3.Scale(bodyStart, new Vector3(xz, y, xz));
            yield return null;
        }
        body.localScale = bodyStart;
        squashRoutine = null;
    }

    void LateUpdate()
    {
        if (cameraRig == null) return;
        shake *= Mathf.Exp(-12f * Time.unscaledDeltaTime);
        float t = Time.unscaledTime * 25f;
        Vector3 noise = new Vector3(Mathf.PerlinNoise(t, 0f) - 0.5f, Mathf.PerlinNoise(0f, t) - 0.5f, 0f);
        cameraRig.localPosition = camStart + noise * shakeSize * shake;
    }
}