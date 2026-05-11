using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    public float rotationSpeed = 80f;

    private void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up, horizontalInput * rotationSpeed * Time.deltaTime);
    }
}
