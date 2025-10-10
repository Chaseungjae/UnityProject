//using Unity.Mathematics;
using UnityEngine;

public class MyCustomCamera : MonoBehaviour
{
    public GameObject Player;
    public float mouseSensitivity = 300f; //마우스감도

    private float MouseY;
    private float MouseX;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Rotate();
    }
    private void Rotate()
    {
        
   
        MouseY -= Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime; //마우스 y의 힘을 적용
  
        MouseY = Mathf.Clamp(MouseY, -90f, 60f); //너무 아래로는 못보게 하기

        transform.localRotation = Quaternion.Euler(MouseY, MouseX, 0f);// 각 축을 한꺼번에 계산
    }
}
