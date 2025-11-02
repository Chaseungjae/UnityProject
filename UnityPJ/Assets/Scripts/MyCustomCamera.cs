using UnityEngine;

public class MyCustomCamera : MonoBehaviour
{
    public GameObject Player;
    public float mouseSensitivity = 300f;

    private float MouseY = 0f;
    private float MouseX = 0f;

    private bool isActive = true;

    void Start()
    {
        // 마우스 잠금 및 커서 숨김 (필요에 따라)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!isActive) return;

        if (Player.transform.position.x <= 13)
        {
            Rotate();
        }
    }

    public void LockRotationAndStop()
    {
        isActive = false;
        transform.rotation = Quaternion.Euler(22f, 0f, 0f);
        Cursor.lockState = CursorLockMode.None; // 필요 시 커서 다시 보이게
        Cursor.visible = true;
    }

    private void Rotate()
    {
        MouseX += Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        MouseY -= Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        MouseY = Mathf.Clamp(MouseY, -90f, 60f);

        transform.localRotation = Quaternion.Euler(MouseY, MouseX, 0f);
    }
}