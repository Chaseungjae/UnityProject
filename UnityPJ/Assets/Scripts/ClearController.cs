using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Audio;
using Cinemachine;
using UnityEngine.SceneManagement;



public class ClearController : MonoBehaviour
{
    BackgroundMove background;

    [SerializeField] 
    private string sceneName = "Credit"; // 빌드용 이름 저장

    [Header("Targets")]
    [SerializeField] Transform firstTarget;      // 1차 목적지
    [SerializeField] Transform secondTarget;  
    [SerializeField] Transform thirdTarget;   
    [SerializeField] Transform cameraTransform;  // 카메라 바라보기용

    [Header("Move Speeds")]
    [SerializeField] public float walkSpeed = 1.0f; 
    [SerializeField] private float directMoveSpeed = 7f;

    private NavMeshAgent agent;

    private float arriveEpsilon = 0.15f;
    private bool hasActivePath = false;

    public CinemachineVirtualCamera cam;
    [SerializeField] private float rotationSpeed = 5f;
    public GameObject light_clear;
    public GameObject light_clear_sign;
    public bool sequenceStarted = false;
    public GameObject clear_cube;
    private CinemachineBasicMultiChannelPerlin noise; 
    [Header("Camera Shake Settings")]
    public float moveShakeAmp = 1.0f;      // 이동 중 흔들림 Amplitude
    public float moveShakeFreq = 2.0f;     // 이동 중 흔들림 Frequency
    public float idleShakeAmp = 0f;        // 멈췄을 때 흔들림 0
    public float idleShakeFreq = 0f;



    void Awake()
    {
        directMoveSpeed = 16f;
        agent = GetComponent<NavMeshAgent>();
        background = FindObjectOfType<BackgroundMove>();
        transform.rotation = Quaternion.Euler(0, 0, 0);
        cam.transform.rotation = Quaternion.Euler(0, 0, 0);
        noise = cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    void Start()
    {
        //light_clear.gameObject.SetActive(true);
        agent.speed = walkSpeed;
        agent.isStopped = false;

    }

    void Update()
    {
        if(GameManager.Instance.game_clear == true)
        {
            clear_cube.SetActive(true);
            light_clear_sign.SetActive(true);
        }
    }


    private IEnumerator MoveToTarget(Transform target)
    {
        if (target == null) yield break;

        //  이동 시작 → 흔들림 ON
        noise.m_AmplitudeGain = moveShakeAmp;
        noise.m_FrequencyGain = moveShakeFreq;

        agent.enabled = true;
        agent.isStopped = false;
        agent.SetDestination(target.position);

        while (true)
        {
            if (!agent.pathPending &&
                agent.remainingDistance <= arriveEpsilon &&
                (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f))
            {
                break;
            }
            yield return null;
        }

        agent.isStopped = true;

        // ★ 도착 → 흔들림 OFF
        noise.m_AmplitudeGain = idleShakeAmp;
        noise.m_FrequencyGain = idleShakeFreq;
    }



    private IEnumerator RotateTowardsTarget(Transform target)
    {
        if (target == null) yield break;

        Vector3 dir = (target.position - transform.position);
        dir.y = 0f;

        Quaternion targetRot = Quaternion.LookRotation(dir);

        // 부드럽게 회전
        while (Quaternion.Angle(transform.rotation, targetRot) > 0.5f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * rotationSpeed
            );

            yield return null;
        }

        transform.rotation = targetRot;  // 최종 정렬
        yield return new WaitForSeconds(1f);
    }


    private IEnumerator SmoothMoveToTarget(Transform target)
    {
        if (target == null) yield break;

        // 네브 꺼서 수동 이동 시작
        agent.enabled = false;

        float speed = 5f;
        float rotationSpeed = 5f;

        while (true)
        {
            Vector3 dir = (target.position - transform.position);
            float distance = dir.magnitude;

            if (distance < 0.05f)
                break;

            // -------------------------
            // 1) 위치 이동 (y 포함)
            // -------------------------
            Vector3 moveDir = dir.normalized;
            transform.position += moveDir * speed * Time.deltaTime;

            // -------------------------
            // 2) 회전
            // -------------------------
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * rotationSpeed
            );

            yield return null;
        }

        transform.position = target.position;
    }
    public IEnumerator MoveSequence()
    {
        // 회전 → secondTarget 방향
        yield return RotateTowardsTarget(secondTarget);

        // 2 → secondTarget 이동
        yield return MoveToTarget(secondTarget);

        // 회전 → thirdTarget 방향
        yield return RotateTowardsTarget(thirdTarget);

        // 3 → thirdTarget 이동
        yield return SmoothMoveToTarget(thirdTarget);

        yield return new WaitForSeconds(1f);

        //크레딧 씬으로 전환
        SceneManager.LoadScene(sceneName);

        
    }

    
}
