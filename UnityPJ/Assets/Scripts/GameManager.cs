using Cinemachine;
using System.Collections;
using System.Collections.Generic; // List/Array 사용
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using static System.Collections.Specialized.BitVector32;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; //이거 사용하면 Find 안쓰고 간편하게 모든 스크립트에서 참조 가능 쥐기네
    public bool strange_situation = true;
    public bool stage_clear = false;
    public GameObject player;
    public BackgroundMove Background;

    public GameObject monsterPrefab; //잡아먹힐 때 나오는 놈
    //public GameObject monster_prefabs; // 플레이어에게 다가가는 놈
    public CinemachineVirtualCamera cinemachineVirtualCamera;
    public Camera mainCamera;
    public GameObject Monster;
    public MonsterController monsterController;

    public PlayerMove playerControoler;
    public MyCustomCamera CameraControoler;
    private bool isStagetTransitioning = false; // 스테이지 전환 중복 방지

    //지하철 프리펩 난이도별 구분
    public GameObject[] strange_situation_1;
    public GameObject[] strange_situation_2;
    public GameObject[] strange_situation_3;
    public GameObject normal_stage;

    public Vector3 playerSpawnPosition = new Vector3(-0.6f, 1.5f, -1.5f); // 플레이어 리스폰 위치

    private int stage_count = 0;
    private bool is_first_load = true;
    private GameObject current_subway; // '현재' 생성된 지하철 인스턴스
    public bool is_success = false;
    public bool subway_in_die = false; //이상현상이 있고 안내릴때 BackgroundMove.cs에 문 안닫히게 하기 위한 bool
    private float in_die_timer = 17.0f;
    //BackgroundMove.cs의 target_speed가 0 일때부터 Time.deltaTime만큼 빼서 1 미만이 되면 subway_in_die = true
    public bool is_subway = true;
    public CinemachineVirtualCamera FixedCamera_player;
    private Rigidbody player_rb;

    public bool is_stop = false;

    //스테이지 넘어가는 화면용 
    public CanvasM CanvasM;
    public TextMeshProUGUI Station;


    // [수정] 사용하지 않는 변수들은 Start()에서 제거 (originalCameraPosition 등)
    void Start()
    {
        player_rb = player.GetComponent<Rigidbody>();
        next_stage();
    }

    void Update()
    {
        is_stop = Background.target_speed == 0f;
        if (is_stop)
        {
            in_die_timer -= Time.deltaTime;
        }
        fun_strange_situation_exit_or_stay(player);
    }

    void fun_strange_situation_exit_or_stay(GameObject player)//지하철에서 내릴 때
    {
        
        if (player.transform.position.z > 3.5 || player.transform.position.z < -3.5)
        {
            is_subway = false;
            // [수정] 이미 전환 중이면(stage_clear == true) 중복 실행 방지
            if (Background.is_door_closing == true && strange_situation == false && stage_clear == false)
            {
                //몬스터 활성화 전 리셋
                Monster.gameObject.SetActive(true);
                cinemachineVirtualCamera.Priority = 1000;
                Debug.Log("YOU DIE!!!!");

                stage_clear = true;
                isStagetTransitioning = true;
                is_success = false;
                player.transform.position = new Vector3(12.47f, 0.7604864f, -8.78f);
                player_rb.constraints = RigidbodyConstraints.FreezeAll;

                StartCoroutine(DeathAndResetRoutine(15f));
            }
            else if (Background.is_door_closing == true && strange_situation == true && stage_clear == false)
            {
                Debug.Log("CLEAR!!");
                stage_clear = true;
                isStagetTransitioning = true;
                is_success = true;
                player.transform.position = new Vector3(12.47f, 0.7604864f, -8.78f);
                player_rb.constraints = RigidbodyConstraints.FreezeAll;

                StartCoroutine(DeathAndResetRoutine(10f));
            }
            
        }
        else
        {
            is_subway = true;

            // 이미 전환 중이면(stage_clear == true) 중복 실행 방지
            if (Background.is_door_closing == true && strange_situation == false && stage_clear == false)
            {
                Debug.Log("CLEAR!!");
                FixedCamera_player.Priority = 1000;

                stage_clear = true;
                isStagetTransitioning = true;
                is_success = true;
                Monster.gameObject.SetActive(true);
                player.transform.position = new Vector3(1.0999f, 0.7604864f, -1.43f);
                player_rb.constraints = RigidbodyConstraints.FreezeAll;

                StartCoroutine(DeathAndResetRoutine(10f));
            }
            else if (in_die_timer < 1.0f && strange_situation == true && stage_clear == false)
            {
                //몬스터 활성화 전 리셋
                FixedCamera_player.Priority = 1000;
                subway_in_die = true;
                stage_clear = true;
                is_success = false;
                
                Monster.gameObject.SetActive(true);
                Debug.Log("YOU DIE!!!!");
                isStagetTransitioning = true;
                player.transform.position = new Vector3(1.0999f, 0.7604864f, -1.43f);
                player_rb.constraints = RigidbodyConstraints.FreezeAll;

                StartCoroutine(DeathAndResetRoutine(15f));
            }
            
        }
    }

    void next_stage()
    {
        Debug.Log("next stage");
        subway_in_die = false;
        // 모든 연출을 리셋
        Monster.gameObject.SetActive(false);
        cinemachineVirtualCamera.Priority = 10;
        // monsterController.ResetMonster(); // 몬스터 컨트롤러 내부 상태도 리셋

        if (!is_first_load && current_subway != null)
        {
            Destroy(current_subway);
        }


        // 스폰할 프리팹 결정 
        GameObject prefab_spawn;
        float random = Random.value;

        if (random < 0.4f || is_first_load)
        {
            strange_situation = false;
            prefab_spawn = normal_stage;
        }
        else
        {
            strange_situation = true;
            if (stage_count < 4)
            {
                Debug.Log("~2: lv 1");
                prefab_spawn = strange_situation_1[Random.Range(0, strange_situation_1.Length)];
            }
            else if (stage_count < 6)
            {
                float levelChance = Random.value;
                if (levelChance < 0.3f)
                {
                    Debug.Log("2~4: lv 1");
                    prefab_spawn = strange_situation_1[Random.Range(0, strange_situation_1.Length)];
                }
                else
                {
                    Debug.Log("2~4: lv 2");
                    prefab_spawn = strange_situation_2[Random.Range(0, strange_situation_2.Length)];
                }
            }
            else
            {
                float levelChance = Random.value;
                if (levelChance < 0.5f)
                {
                    Debug.Log("5~: lv 2");
                    prefab_spawn = strange_situation_2[Random.Range(0, strange_situation_2.Length)];
                }
                else
                {
                    Debug.Log("5~: lv 3");
                    prefab_spawn = strange_situation_3[Random.Range(0, strange_situation_3.Length)];
                }
            }
        }

        current_subway = Instantiate(prefab_spawn, new Vector3(0, 0, 0), prefab_spawn.transform.rotation);
        var new_subway = current_subway.GetComponent<Subway>();
        Background.door_left = new_subway.door_left;
        Background.door_right = new_subway.door_right;
        //변수 초기화 
        stage_clear = false;
        isStagetTransitioning = false;
        is_success = false;
        is_first_load = false; // 첫 로드 완료
        Background.reset_background(); // 새 지하철의 배경 리셋
        player.transform.position = playerSpawnPosition; // 플레이어 위치 리셋
    }

    IEnumerator DeathAndResetRoutine(float delay)
    {
        Debug.Log($"연출 대기... ({delay}초)");
        yield return new WaitForSeconds(delay);
        Debug.Log("페이드아웃");
        yield return CanvasM.FadeOut();
        Station.text = "Station: "+ stage_count;
        Station.gameObject.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        next_stage();
        yield return new WaitForSeconds(2.0f);
        Station.gameObject.SetActive(false);
        Debug.Log("페이드인 시작");
        yield return CanvasM.FadeIn();
    }
    private void Awake()
    {
        Instance = this;
    }
    public IEnumerator DeleteAfterDelay(GameObject obj, float delay)//플레이어 놀래키는 몬스터 delay 후에 삭제
    {
        Debug.Log("코루틴 발동");

        yield return new WaitForSeconds(delay);

        Debug.Log(obj);

        if (obj != null)
        {
            Destroy(obj);
            Debug.Log("삭제");
        }
    }
}