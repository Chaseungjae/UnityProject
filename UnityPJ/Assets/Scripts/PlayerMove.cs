using System;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float Speed = 5f;
    private Rigidbody playerRB;
    public float horizontalInput;
    public float verticalInput;
    public float mouseSensitivity = 300f; //마우스감도

    private float MouseY;
    private float MouseX;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRB = GetComponent<Rigidbody>(); //캐릭터 Rigidbody 처리
        Cursor.visible = false; //마우스 커서 안보이게 설정
        Cursor.lockState = CursorLockMode.Locked; //커서 화면 중심에 고정시키기(마우스 움직임 편하게 해줌)
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        transform.Translate(Vector3.forward * Speed * verticalInput * Time.deltaTime);
        transform.Translate(Vector3.right * Speed * horizontalInput * Time.deltaTime); //캐릭터 이동
        Rotate();
    }
    private void Rotate()// 마우스 움직임 처리
    {
        MouseX += Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime; //마우스 x와 y의 이동 값 처리
        transform.localRotation = Quaternion.Euler(MouseY, MouseX, 0f);
    }
}
