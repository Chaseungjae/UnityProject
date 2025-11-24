using System;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerMove : MonoBehaviour
{
    public float Speed = 5f;
    private Rigidbody playerRB;
    private bool isActive = true;
    public float reversal_move = 2.0f;


    public CinemachineVirtualCamera vcam;

    public AudioSource step_sound;

    // 기본 퍼린 노이즈 컴포넌트
    private CinemachineBasicMultiChannelPerlin noise;

    [Header("Noise by Speed")]
    public float walkAmp = 0.0f;
    public float runAmp = 1.0f;

    public float walkFreq = 1.2f;
    public float runFreq = 2.4f;

    public float runSpeed = 5.5f;


    void Start()
    {
        playerRB = GetComponent<Rigidbody>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Awake()
    {
        noise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if (noise == null)
            Debug.LogError("VCam에 CinemachineBasicMultiChannelPerlin(Noise)를 추가하세요.");
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

        float planarSpeed = new Vector3(playerRB.linearVelocity.x, 0, playerRB.linearVelocity.z).magnitude;
        float t = Mathf.Max(Mathf.Abs(horizontal), Mathf.Abs(vertical));

        noise.m_AmplitudeGain = Mathf.Lerp(0f, Mathf.Lerp(walkAmp, runAmp, t), t);
        noise.m_FrequencyGain = Mathf.Lerp(0f, Mathf.Lerp(walkFreq, runFreq, t), t);

        if (horizontal != 0 || vertical != 0)
        {
            if (!step_sound.isPlaying)
            {
                step_sound.Play();
            }
        }
        else
        {
            step_sound.Pause();
            //step_sound.Stop();
        }
    }

    public void LockRotationAndStop()
    {
        isActive = false;
        transform.rotation = Quaternion.Euler(0f, 86.1f, 0f);
    }
}