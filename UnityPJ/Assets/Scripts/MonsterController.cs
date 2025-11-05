using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MonsterController : MonoBehaviour
{
    GameManager gameManager;
    BackgroundMove background; // BackgroundMove 스크립트 참조 변수

    [SerializeField] Transform firstTarget;      // 1차 목적지 (예: 지하철 입구)
    [SerializeField] Transform secondTargetA;   // 2차 목적지 옵션1 (지하철 내부)
    [SerializeField] Transform secondTargetB;   // 2차 목적지 옵션2 (카메라 위치)
    [SerializeField] Transform cameraTransform; // 카메라 Transform, 바라볼 방향용

    public Camera FixedCamera; // 고정 카메라
    public Camera FixedCamera_player; // 플레이어 고정 카메라
    private NavMeshAgent agent;
    private Animator animator;
    private Transform currentTarget;
    public GameObject monsterPrefab; // 몬스터 프리팹
    public GameObject canvas;

    public GameObject bloodEffectPrefab; // 피 이펙트 프리팹
    public Transform canvasTransform; // 캔버스 트랜스폼

    private bool isFirstTargetReached = false;
    public bool goToSubway = true;

    private int runLayerIndex;
    private int baseLayerIndex;

    private bool spawnedMonster = true;

    // 직접 이동 관련 변수
    public float directMoveSpeed = 5f;  // 직접 이동 속도
    private bool isDirectMoving = false; // 직접 이동 중인지 여부
    private Transform directMoveTarget = null;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        background = FindObjectOfType<BackgroundMove>();
        gameManager = FindObjectOfType<GameManager>();

        animator.applyRootMotion = false; // Root Motion 비활성화 (NavMeshAgent 속도 적용을 위해)

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        runLayerIndex = animator.GetLayerIndex("RunLayer");
        baseLayerIndex = animator.GetLayerIndex("Base Layer");

        if (runLayerIndex == -1)
            Debug.LogWarning("Animator에 'RunLayer' 레이어가 없습니다. 레이어 이름을 확인하세요.");

        if (baseLayerIndex == -1)
            Debug.LogWarning("Animator에 'Base Layer' 레이어가 없습니다. 레이어 이름을 확인하세요.");

        if (baseLayerIndex != -1)
            animator.SetLayerWeight(baseLayerIndex, 1f);
        if (runLayerIndex != -1)
            animator.SetLayerWeight(runLayerIndex, 0f);
    }

    void Start()
    {
        if (firstTarget != null)
        {
            currentTarget = firstTarget;
            agent.SetDestination(currentTarget.position);

            if (baseLayerIndex != -1)
                animator.Play("Idle", baseLayerIndex);
        }
        else
        {
            Debug.LogError("첫 번째 타겟 미설정!");
        }
    }

    public void ShowBllodEffect()
    {
        GameObject effect = Instantiate(bloodEffectPrefab, canvasTransform);
        effect.SetActive(true);

        effect.transform.localPosition = Vector3.zero;
        StartCoroutine(DestroyEffectAfterSeconds(effect, 2f));
    }

    private IEnumerator DestroyEffectAfterSeconds(GameObject effect, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(effect);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (spawnedMonster)
            {
                GameObject spawned_b = Instantiate(monsterPrefab, monsterPrefab.transform.position, monsterPrefab.transform.rotation);
                Vector3 spawnPosition = new Vector3(1.01f, -1.22f, -1.87f);
                Quaternion spawnRotation = Quaternion.Euler(34.7f, 0, 0);
                GameObject spawned_a = Instantiate(monsterPrefab, spawnPosition, spawnRotation);
                Debug.Log("코루틴 전");
                GameManager.Instance.StartCoroutine(GameManager.Instance.DeleteAfterDelay(spawned_a, 1f));//1초 후에 spawned_a삭제
                GameManager.Instance.StartCoroutine(GameManager.Instance.DeleteAfterDelay(spawned_b, 1f));//1초 후에 spawned_b삭제
                Debug.Log("코루틴 후");
                canvas.SetActive(true);
                spawnedMonster = false;
                Debug.Log("몬스터 재생성 완료");
                
            }
            FixedCamera.transform.rotation = Quaternion.Euler(-27.22f, -41f, 0f);
            FixedCamera.transform.position = new Vector3(10.48f, 1.2f, -8f);

            FixedCamera_player.transform.rotation = Quaternion.Euler(-22.22f, -180f, 0f);
            FixedCamera_player.transform.position = new Vector3(1.12f, 0.4f, -0.15f);

        }
    }

    void Update()
    {
        // 직접 이동 처리 우선
        if (isDirectMoving && directMoveTarget != null)
        {
            Vector3 direction = (directMoveTarget.position - transform.position);
            direction.y = 0;
            float distance = direction.magnitude;

            if (distance > 0.1f)
            {
                Vector3 moveDir = direction.normalized;
                transform.position += moveDir * directMoveSpeed * Time.deltaTime;

                Quaternion lookRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
            }
            else
            {
                isDirectMoving = false;
                directMoveTarget = null;

                // 도착 처리 및 애니메이션 Idle로 변경
                animator.SetLayerWeight(runLayerIndex, 0f);
                animator.SetLayerWeight(baseLayerIndex, 1f);
                animator.Play("Idle", baseLayerIndex);

                // NavMeshAgent 재활성화 가능 (필요하면)
                agent.enabled = true;
                agent.isStopped = false;
            }
            return;  // 직접 이동 중 NavMeshAgent 이동 로직 Skip
        }

        // NavMeshAgent 이동 처리
        if (agent.pathPending)
            return;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                if (!isFirstTargetReached)
                {
                    isFirstTargetReached = true;
                    FixedCamera.transform.rotation = Quaternion.Euler(0f, -60f, 0f);

                    if (gameManager.is_success)
                    {
                        if (gameManager.is_subway)
                        {
                            // 직접 이동 시작(A타겟)
                            StartDirectMove(secondTargetA);
                        }
                        else
                        {
                            // NavMeshAgent 사용(B타겟)
                            currentTarget = secondTargetB;
                            SetAgentDestination(currentTarget.position);
                        }
                    }
                    else
                    {
                        if (gameManager.is_subway)
                        {
                            // 직접 이동 시작(A타겟)
                            StartDirectMove(secondTargetA);
                        }
                        else
                        {
                            // NavMeshAgent 사용(B타겟)
                            currentTarget = secondTargetB;
                            SetAgentDestination(currentTarget.position);
                        }
                    }
                }
                else
                {
                    // 최종 목적지 도착 및 정지 처리
                    if (runLayerIndex != -1 && baseLayerIndex != -1)
                    {
                        animator.SetLayerWeight(runLayerIndex, 0f);
                        animator.SetLayerWeight(baseLayerIndex, 1f);
                        animator.Play("Idle", baseLayerIndex);
                    }
                    else
                    {
                        animator.SetBool("Run", false);
                    }

                    agent.isStopped = true;
                }
            }
        }

        
    }


    private void StartDirectMove(Transform target)
    {
        // NavMeshAgent 비활성화
        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
        directMoveTarget = target;
        isDirectMoving = true;

        if (runLayerIndex != -1 && baseLayerIndex != -1)
        {
            animator.SetLayerWeight(baseLayerIndex, 0f);
            animator.SetLayerWeight(runLayerIndex, 1f);
            animator.Play("run2", runLayerIndex);
        }
        else
        {
            animator.SetBool("run2", true);
        }
    }


    private void SetAgentDestination(Vector3 position)
    {
        if (!agent.enabled)
        {
            agent.enabled = true;
        }
        agent.isStopped = false;
        currentTarget = null;
        agent.SetDestination(position);
        agent.speed = 16f;

        if (runLayerIndex != -1 && baseLayerIndex != -1)
        {
            animator.SetLayerWeight(baseLayerIndex, 0f);
            animator.SetLayerWeight(runLayerIndex, 1f);
            animator.Play("run2", runLayerIndex);
        }
        else
        {
            animator.SetBool("run2", true);
        }
    }
}