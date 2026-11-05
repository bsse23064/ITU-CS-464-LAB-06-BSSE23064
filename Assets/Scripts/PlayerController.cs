using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveForce = 10f;
    float speedMultiplier = 1f;
    Rigidbody rb;
    Vector3 input;
    Coroutine boost;
    void Awake() => rb = GetComponent<Rigidbody>();
    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");   
        input = Vector3.ClampMagnitude(new Vector3(h,0f,v),1f);
    }

    void FixedUpdate()
    {
        rb.AddForce(moveForce * speedMultiplier * input,ForceMode.Force);
    }

    public void BoostSpeed(float multiplier,float seconds)
    {
        if (boost != null) StopCoroutine(boost);
        boost = StartCoroutine(Boost(multiplier,seconds));
    }

    IEnumerator Boost(float multiplier,float seconds)
    {
        speedMultiplier = multiplier;
        yield return new WaitForSeconds(seconds);
        speedMultiplier = 1f;
        boost = null;
    }
}
