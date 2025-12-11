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
    public float reversal_move = 2.0f; // 이 변수가 단순 배율로 쓰인다면 이름이 헷갈릴 수 있습니다. (SpeedMultiplier 등으로 변경 고려)

    public CinemachineVirtualCamera vcam;
    public AudioSource step_sound;

    private CinemachineBasicMultiChannelPerlin noise;
    public ClearController clearplayer;

    [Header("Noise by Speed")]
    public float walkAmp = 0.0f;
    public float runAmp = 1.0f;
    public float walkFreq = 1.2f;
    public float runFreq = 2.4f;
    public float runSpeed = 5.5f;

    // [수정 1] 입력을 FixedUpdate로 넘겨주기 위한 변수
    private Vector2 inputDirection;

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

        // [수정 2] 입력 처리는 반응성을 위해 Update에서 계속 받습니다.
        // GetAxisRaw를 사용하면 키를 뗄 때 즉시 멈추므로 더 빠릿한 조작감을 줍니다. (부드러운 감속 원하면 GetAxis 유지)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        inputDirection = new Vector2(h, v); // 입력 저장

        // 사운드 및 노이즈 처리는 시각/청각적 요소이므로 Update에서 처리해도 무방합니다.
        ProcessEffects(h, v);
    }

    // [수정 3] 물리 이동은 반드시 FixedUpdate에서 처리
    void FixedUpdate()
    {
        if (!isActive) return;
        MovePhysics();
    }

    private void MovePhysics()
    {
        // 카메라 기준 방향 벡터 계산
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        // [수정 4] 방향 벡터 합성 후 정규화(Normalize)하여 대각선 속도 뻥튀기 방지
        Vector3 moveDir = (forward * inputDirection.y + right * inputDirection.x).normalized;

        if (moveDir.magnitude > 0.01f) // 입력이 있을 때만 이동
        {
            // reversal_move가 속도 배율이라면 Speed와 곱해줍니다.
            // Time.deltaTime 대신 FixedUpdate에서는 Time.fixedDeltaTime을 사용해야 하지만, 
            // MovePosition은 내부적으로 프레임 처리를 하므로 보통 속도 * 시간으로 계산합니다.
            Vector3 targetPosition = playerRB.position + moveDir * (Speed * reversal_move) * Time.fixedDeltaTime;
            playerRB.MovePosition(targetPosition);
        }
    }

    private void ProcessEffects(float h, float v)
    {
        // 효과 처리는 기존 로직 유지 (linearVelocity는 Unity 6 버전이 아니면 velocity로 변경 필요할 수 있음)
        float planarSpeed = new Vector3(playerRB.linearVelocity.x, 0, playerRB.linearVelocity.z).magnitude;
        float t = Mathf.Max(Mathf.Abs(h), Mathf.Abs(v)); // 입력 크기 기준으로 t 설정

        noise.m_AmplitudeGain = Mathf.Lerp(0f, Mathf.Lerp(walkAmp, runAmp, t), t);
        noise.m_FrequencyGain = Mathf.Lerp(0f, Mathf.Lerp(walkFreq, runFreq, t), t);

        if (h != 0 || v != 0)
        {
            if (!step_sound.isPlaying) step_sound.Play();
        }
        else
        {
            step_sound.Pause();
        }
    }

    // ... 나머지 함수들 (LockRotationAndStop, OnCollisionEnter) 동일 ...
    public void LockRotationAndStop()
    {
        isActive = false;
        transform.rotation = Quaternion.Euler(0f, 86.1f, 0f);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Clear_Cube"))
        {
            if (!clearplayer.sequenceStarted && GameManager.Instance.game_clear)
            {
                clearplayer.cam.Priority = 10000;
                clearplayer.sequenceStarted = true;
                StartCoroutine(clearplayer.MoveSequence());
            }
            if (GameManager.Instance.game_clear == true)
            {
                clearplayer.light_clear.SetActive(true);
            }
        }
    }
}