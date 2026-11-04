using UnityEngine;

public class Spin : MonoBehaviour
{
    [SerializeField] float spinSpeed = 90f;

    void Update() => transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
}
