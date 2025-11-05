using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float Speed = 5f;
    private Rigidbody playerRB;
    private bool isActive = true;

    void Start()
    {
        playerRB = GetComponent<Rigidbody>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (!isActive) return;
        Move();
    }

    private void Move()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = (transform.forward * vertical + transform.right * horizontal);
        playerRB.MovePosition(transform.position + move * Speed * Time.deltaTime);
    }

    public void LockRotationAndStop()
    {
        isActive = false;
        transform.rotation = Quaternion.Euler(0f, 86.1f, 0f);
    }
}