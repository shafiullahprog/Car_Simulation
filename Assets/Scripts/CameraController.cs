using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 100f;

    [Header("Panning Settings")]
    public float panSpeed = 10f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 10f;
    public float minZoom = 5f;
    public float maxZoom = 50f;

    private Vector3 dragStart;

    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandlePanning();
        HandleZoom();
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float moveZ = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        transform.Translate(new Vector3(moveX, 0, moveZ), Space.Self);
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(1))
        {
            float rotateX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float rotateY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            transform.Rotate(Vector3.up, rotateX, Space.World);
            transform.Rotate(Vector3.right, -rotateY, Space.Self);
        }
    }

    private void HandlePanning()
    {
        if (Input.GetMouseButtonDown(2))
        {
            dragStart = Input.mousePosition;
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 dragEnd = Input.mousePosition;
            Vector3 difference = dragStart - dragEnd;

            transform.Translate(difference.x * panSpeed * Time.deltaTime, difference.y * panSpeed * Time.deltaTime, 0, Space.Self);

            dragStart = dragEnd;
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 position = transform.position;
        position += transform.forward * scroll * zoomSpeed * Time.deltaTime;

        float distance = Vector3.Distance(Vector3.zero, position);
        if (distance >= minZoom && distance <= maxZoom)
        {
            transform.position = position;
        }
    }
}
