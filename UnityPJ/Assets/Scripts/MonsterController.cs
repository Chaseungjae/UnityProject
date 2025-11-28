using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Audio;

public class MonsterController : MonoBehaviour
{
    GameManager gameManager;
    BackgroundMove background;

    [Header("Targets")]
    [SerializeField] Transform firstTarget;      // 1차 목적지
    [SerializeField] Transform secondTargetA;    // 2차 목적지 A (직접 이동)
    [SerializeField] Transform secondTargetB;    // 2차 목적지 B (직접 이동)
    [SerializeField] Transform cameraTransform;  // 카메라 바라보기용

    [Header("Cameras")]
    public Camera FixedCamera;
    public Camera FixedCamera_player;

    [Header("Refs")]
    public GameObject monsterPrefab;
    public GameObject canvas;
    public GameObject player;

    [Header("FX/UI")]
    public GameObject bloodEffectPrefab;
    public Transform canvasTransform;

    [Header("Move Speeds")]
    [SerializeField] private float walkSpeed = 3.5f; // 첫 구간(네비) 걷기 속도
    [SerializeField] private float directMoveSpeed = 7f; // 직접 이동 속도(달리기 느낌)

    private NavMeshAgent agent;
    private Animator animator;
    public AudioSource monsterAudioSource;
    public AudioClip sfxClip;           // Inspector에서 효과음 할당
    [Range(0f, 1f)] public float volume = 1f;
    // 상태
    private Transform currentTarget;
    private bool isFirstTargetReached = false;
    private bool spawnedMonster = true;

    // 애니메이터 레이어
    private int runLayerIndex;
    private int baseLayerIndex;

    // 직접 이동 상태
    private bool isDirectMoving = false;
    private Transform directMoveTarget = null;

    // 초기값 저장
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Transform initialParent;
    private float initialAgentSpeed; // 초기(걷기) 속도

    // 도착 판정 안정화
    private bool hasActivePath = false;
    private float arriveEpsilon = 0.15f;

    // OnEnable용 가드
    private bool _initialized = false;

    public GameObject PointLight;

    void Awake()
    {
        directMoveSpeed = 16f; // 직접 이동 속도 고정
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        background = FindObjectOfType<BackgroundMove>();
        gameManager = FindObjectOfType<GameManager>();
        monsterAudioSource = GetComponent<AudioSource>();

        animator.applyRootMotion = false;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        runLayerIndex = animator.GetLayerIndex("RunLayer");
        baseLayerIndex = animator.GetLayerIndex("Base Layer");

        if (baseLayerIndex != -1) animator.SetLayerWeight(baseLayerIndex, 1f);
        if (runLayerIndex != -1) animator.SetLayerWeight(runLayerIndex, 0f);

        PointLight.SetActive(false);
    }

    void Start()
    {
        // 초기 이동 시작 (걷기)
        if (firstTarget != null)
        {
            currentTarget = firstTarget;
            agent.speed = walkSpeed;
            agent.isStopped = false;
            agent.SetDestination(currentTarget.position);
            hasActivePath = true;

            if (baseLayerIndex != -1)
                animator.Play("Idle", baseLayerIndex, 0f);
        }
        else
        {
            Debug.LogError("첫 번째 타겟 미설정!");
        }

        // 초기값 백업 (1회)
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialParent = transform.parent;
        initialAgentSpeed = walkSpeed;

        _initialized = true;
    }

    void OnEnable()
    {
        // 재활성화 시 자동 초기화
        if (_initialized)
        {
            // 다음 프레임에 첫 타겟으로 출발(도착 오판정 방지)
            ResetMonsterState();
        }
    }

    public void ShowBllodEffect()
    {
        if (bloodEffectPrefab == null || canvasTransform == null) return;

        GameObject effect = Instantiate(bloodEffectPrefab, canvasTransform);
        effect.SetActive(true);
        effect.transform.localPosition = Vector3.zero;
        StartCoroutine(DestroyEffectAfterSeconds(effect, 2f));
        PointLight.SetActive(true);
    }

    private IEnumerator DestroyEffectAfterSeconds(GameObject effect, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (effect != null) Destroy(effect);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (spawnedMonster)
        {
            GameObject spawned_b = Instantiate(monsterPrefab, monsterPrefab.transform.position, monsterPrefab.transform.rotation);
            Vector3 spawnPosition = new Vector3(1.01f, -1.22f, -1.87f);
            Quaternion spawnRotation = Quaternion.Euler(34.7f, 0, 0);
            GameObject spawned_a = Instantiate(monsterPrefab, spawnPosition, spawnRotation);

            GameManager.Instance.StartCoroutine(GameManager.Instance.DeleteAfterDelay(spawned_a, 2f));
            GameManager.Instance.StartCoroutine(GameManager.Instance.DeleteAfterDelay(spawned_b, 2f));

            if (canvas != null) canvas.SetActive(true);
            monsterAudioSource.PlayOneShot(sfxClip, volume);
            spawnedMonster = false;
            PointLight.SetActive(true);

            PointLight.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 0.5f, player.transform.position.z);

        }

        // 카메라 연출
        if (FixedCamera != null)
        {
            FixedCamera.transform.rotation = Quaternion.Euler(-27.22f, -41f, 0f);
            FixedCamera.transform.position = new Vector3(10.48f, 1.2f, -8f);
        }
        if (FixedCamera_player != null)
        {
            FixedCamera_player.transform.rotation = Quaternion.Euler(-22.22f, -180f, 0f);
            FixedCamera_player.transform.position = new Vector3(1.12f, 0.4f, -0.15f);
        }

        // 이동 정지 후 잠깐 연출하고 비활성화(재활성화 시 OnEnable에서 자동 초기화)
        PauseMovementImmediate();
        StartCoroutine(CoDisableAfter(1.0f));
    }

    private IEnumerator CoDisableAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    void Update()
    {
        // 1) 직접 이동 우선 처리
        if (isDirectMoving && directMoveTarget != null)
        {
            Vector3 direction = (directMoveTarget.position - transform.position);
            direction.y = 0f;
            float distance = direction.magnitude;

            if (distance > 0.1f)
            {
                Vector3 moveDir = direction.normalized;
                transform.position += moveDir * directMoveSpeed * Time.deltaTime;
               // Debug.Log(moveDir * directMoveSpeed * Time.deltaTime);

                Quaternion lookRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
            }
            else
            {
                // 직접 이동 종료 → Idle
                isDirectMoving = false;
                directMoveTarget = null;

                if (runLayerIndex != -1) animator.SetLayerWeight(runLayerIndex, 0f);
                if (baseLayerIndex != -1) animator.SetLayerWeight(baseLayerIndex, 1f);
                animator.Play("Idle", baseLayerIndex, 0f);

                // NavMeshAgent 재활성화 필요 시
                if (!agent.enabled) agent.enabled = true;
                agent.isStopped = true; // 직접 이동 종료 후 멈춤 상태 유지
            }
            return; // 직접 이동 중엔 네비 로직 skip
        }

        // 2) NavMeshAgent 이동 처리(첫 구간만 네비 사용: 걷기)
        if (!agent.enabled) return;

        if (agent.pathPending) { hasActivePath = true; return; }

        if (hasActivePath && agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, arriveEpsilon))
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.001f)
            {
                hasActivePath = false;

                if (!isFirstTargetReached)
                {
                    // 첫 타겟 도착 처리
                    isFirstTargetReached = true;

                    if (FixedCamera != null)
                        FixedCamera.transform.rotation = Quaternion.Euler(0f, -60f, 0f);

                    // 도착했으니 네비 정지
                    agent.isStopped = true;
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;

                    // A/B 분기 모두 직접 이동 사용
                    if (gameManager != null)
                    {
                        if (gameManager.is_subway)
                        {
                            if(gameManager.is_success)
                            {
                                //깸
                                StartDirectMove(secondTargetB);
                            }
                            else
                            {
                                StartDirectMove(secondTargetA);
                            }
                        }
                        else
                        {
                            if (gameManager.is_success)
                            {
                                StartDirectMove(secondTargetA);
                            }
                            else
                            {
                                StartDirectMove(secondTargetB);
                            }
                        }
                    }
                    else
                    {
                        // gameManager가 없으면 기본 A로
                        StartDirectMove(secondTargetA != null ? secondTargetA : secondTargetB);
                    }
                }
                else
                {
                    // 최종 목적지 도착 후 정지
                    if (runLayerIndex != -1) animator.SetLayerWeight(runLayerIndex, 0f);
                    if (baseLayerIndex != -1) animator.SetLayerWeight(baseLayerIndex, 1f);
                    animator.Play("Idle", baseLayerIndex, 0f);

                    agent.isStopped = true;
                }
            }
        }
    }

    private void StartDirectMove(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning("StartDirectMove: target이 비어있습니다.");
            return;
        }

        // 네비 비활성화 및 정지
        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.enabled = false;
        }

        directMoveTarget = target;
        isDirectMoving = true;

        // 달리기 애니메이션으로 전환(레이어 가중치)
        if (runLayerIndex != -1 && baseLayerIndex != -1)
        {
            animator.SetLayerWeight(baseLayerIndex, 0f);
            animator.SetLayerWeight(runLayerIndex, 1f);
            animator.Play("run2", runLayerIndex, 0f);
        }
        else
        {
            animator.SetBool("run2", true);
        }
    }

    // 비활성화/리셋 전 즉시 이동 정지용
    private void PauseMovementImmediate()
    {
        // 직접 이동 중지
        isDirectMoving = false;
        directMoveTarget = null;

        // 네비 정지
        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        // 애니메이션 정지 상태(Idle)
        if (runLayerIndex != -1) animator.SetLayerWeight(runLayerIndex, 0f);
        if (baseLayerIndex != -1) animator.SetLayerWeight(baseLayerIndex, 1f);
        animator.Play("Idle", baseLayerIndex, 0f);
        animator.SetBool("Run", false);
        animator.SetBool("run2", false);
    }

    public void ResetMonsterState()
    {
        // 1) 이동 플래그 리셋
        isFirstTargetReached = false;
        isDirectMoving = false;
        directMoveTarget = null;
        currentTarget = null;

        // 2) 트랜스폼 복원
        transform.SetParent(initialParent);
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // 3) 네비 복원 (걷기 속도)
        if (!agent.enabled) agent.enabled = true;
        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
        agent.speed = initialAgentSpeed; // 항상 걷기 속도로 재시작

        // 4) 애니메이터 Idle
        if (runLayerIndex != -1) animator.SetLayerWeight(runLayerIndex, 0f);
        if (baseLayerIndex != -1) animator.SetLayerWeight(baseLayerIndex, 1f);
        animator.Play("Idle", baseLayerIndex, 0f);
        animator.SetBool("Run", false);
        animator.SetBool("run2", false);

        // 5) UI/스폰 플래그
        spawnedMonster = true;
        if (canvas != null) canvas.SetActive(false);

        // 6) 다음 프레임에 첫 타겟으로 출발 (도착 오판정 방지)
        StartCoroutine(CoStartFirstMoveNextFrame());
    }

    private IEnumerator CoStartFirstMoveNextFrame()
    {
        yield return null; // 한 프레임 대기
        if (firstTarget != null && agent.enabled)
        {
            currentTarget = firstTarget;
            agent.speed = initialAgentSpeed; // 걷기
            agent.isStopped = false;
            agent.SetDestination(currentTarget.position);
            hasActivePath = true;
        }
        else
        {
            Debug.LogWarning("Reset 후 firstTarget이 없어 이동 시작 실패");
        }
    }
}