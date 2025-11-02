using UnityEngine;
using UnityEngine.AI;

public class MonsterController : MonoBehaviour
{
    GameManager gameManager;
    [SerializeField] Transform firstTarget;      // 1차 목적지 (예: 지하철 입구)
    [SerializeField] Transform secondTargetA;   // 2차 목적지 옵션1 (지하철 내부)
    [SerializeField] Transform secondTargetB;   // 2차 목적지 옵션2 (카메라 위치)
    [SerializeField] Transform cameraTransform; // 카메라 Transform, 바라볼 방향용

    public Camera FixedCamera; // 고정 카메라
    private NavMeshAgent agent;
    private Animator animator;
    private Transform currentTarget;
    public GameObject monsterPrefab; // 몬스터 프리팹

    private bool isFirstTargetReached = false;
    public bool goToSubway = true;

    private int runLayerIndex;
    private int baseLayerIndex;

    private bool spawnedMonster = true;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        animator.applyRootMotion = false; // Root Motion 비활성화 (NavMeshAgent 속도 적용을 위해)

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        // 레이어 인덱스 가져오기
        runLayerIndex = animator.GetLayerIndex("RunLayer");
        baseLayerIndex = animator.GetLayerIndex("Base Layer");

        if (runLayerIndex == -1)
            Debug.LogWarning("Animator에 'RunLayer' 레이어가 없습니다. 레이어 이름을 확인하세요.");

        if (baseLayerIndex == -1)
            Debug.LogWarning("Animator에 'Base Layer' 레이어가 없습니다. 레이어 이름을 확인하세요.");

        // 시작 시 Base Layer 활성화, RunLayer 비활성화
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

    void Update()
    {
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
                    if (goToSubway && secondTargetA != null)
                    {
                        currentTarget = secondTargetA;
                        agent.SetDestination(currentTarget.position);

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

                        agent.speed = 16f;  // 속도 변경 위치 조정
                        agent.SetDestination(currentTarget.position); // 적용 강제

                        Debug.Log("지하철 내부로 이동, RunLayer에서 달리기 애니메이션 실행");
                    }
                    else if (!goToSubway && secondTargetB != null)
                    {
                        currentTarget = secondTargetB;
                        FixedCamera.transform.rotation = Quaternion.Euler(0f, -51f, 0f);
                        agent.speed = 16.0f;  // 먼저 속도 증가
                        agent.SetDestination(currentTarget.position); // 목적지 재설정


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

                        Debug.Log("카메라 위치로 이동, RunLayer에서 달리기 애니메이션 실행");
                    }
                    else
                    {
                        Debug.LogWarning("두 번째 타겟 미설정");

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

                        return;
                    }
                }
                else
                {
                    Debug.Log("최종 목적지 도착");
                    if (runLayerIndex != -1 && baseLayerIndex != -1)
                    {
                        animator.SetLayerWeight(runLayerIndex, 0f);
                        animator.SetLayerWeight(baseLayerIndex, 1f);
                        animator.Play("Idle", baseLayerIndex);

                        if (spawnedMonster)
                        {
                            // 몬스터 현재 위치에 새 몬스터 생성
                            Instantiate(monsterPrefab, monsterPrefab.transform.position, monsterPrefab.transform.rotation);
                            spawnedMonster = false;
                            Debug.Log("몬스터 재생성 완료");
                        }

                        FixedCamera.transform.rotation = Quaternion.Euler(-27.22f, -41f, 0f);
                        FixedCamera.transform.position = new Vector3(10.48f, 1.2f, -8f);

                        Destroy(gameObject);
                    }
                    else
                    {
                        animator.SetBool("Run", false);
                    }

                    agent.isStopped = true;
                }
            }
        }

        // 이동 중 회전 처리
        if (agent.remainingDistance > agent.stoppingDistance)
        {
            if (currentTarget == secondTargetA && cameraTransform != null)
            {
                Vector3 directionToCamera = cameraTransform.position - transform.position;
                directionToCamera.y = 0;

                if (directionToCamera.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                }
            }
            else
            {
                Vector3 direction = (agent.steeringTarget - transform.position).normalized;
                direction.y = 0;

                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                }
            }
        }
    }
}