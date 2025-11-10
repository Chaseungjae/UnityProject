using UnityEngine;

public class MyCustomCamera : MonoBehaviour
{
    public Transform player;       // Rigidbody 포함된 Player
    public float mouseSensitivity = 300f;
    private float rotationX = 0f;
    private bool isActive = true;
    public float reversal_mouse = 1.0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate() // ? 카메라 갱신은 LateUpdate로 (딜레이 방지)
    {
        if (!isActive) return;
        RotateCamera();
    }

    private void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime * reversal_mouse;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 상하 회전 (CameraRoot 회전)
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -80f, 60f);
        transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        // 좌우 회전 (Player 회전)
        player.Rotate(Vector3.up * mouseX);
    }

    public void LockRotationAndStop()
    {
        isActive = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}