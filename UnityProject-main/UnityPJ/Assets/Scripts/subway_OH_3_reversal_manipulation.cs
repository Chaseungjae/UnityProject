using Cinemachine;
using UnityEngine;

public class subway_OH_reversal_manipulation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public PlayerMove playermove;
    public MyCustomCamera mycamera;
    public CinemachineVirtualCamera C_V_camera;
    private CinemachinePOV reversal_vertical;
    void Start()
    {
        playermove = FindObjectOfType<PlayerMove>();
        playermove.reversal_move = -1.0f;

        mycamera = FindObjectOfType<MyCustomCamera>();
        mycamera.reversal_mouse = -1.0f;

        C_V_camera = GameObject.FindWithTag("PlayerCamera").GetComponent<CinemachineVirtualCamera>();
        reversal_vertical = C_V_camera.GetCinemachineComponent<CinemachinePOV>();
        reversal_vertical.m_VerticalAxis.m_InvertAxis = false;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
