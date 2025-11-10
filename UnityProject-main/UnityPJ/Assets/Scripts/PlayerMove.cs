using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float Speed = 5f;
    private Rigidbody playerRB;
    private bool isActive = true;
    public float reversal_move = 1.0f;

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

        Vector3 move = (transform.forward * vertical * reversal_move  + transform.right * horizontal * reversal_move);
        playerRB.MovePosition(transform.position + move * Speed * Time.deltaTime);
    }

    public void LockRotationAndStop()
    {
        isActive = false;
        transform.rotation = Quaternion.Euler(0f, 86.1f, 0f);
    }
}