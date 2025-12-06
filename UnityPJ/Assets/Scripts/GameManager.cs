using Cinemachine;
using System.Collections;
using System.Collections.Generic; // List/Array 사용
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;
using static System.Collections.Specialized.BitVector32;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; //이거 사용하면 Find 안쓰고 간편하게 모든 스크립트에서 참조 가능 쥐기네
    public bool strange_situation = true;
    public bool stage_clear = false;
    public GameObject player;
    public BackgroundMove Background;

    public GameObject monsterPrefab; //잡아먹힐 때 나오는 놈
    public Canvas blood_effect; // 피 이팩트
    public CinemachineVirtualCamera cinemachineVirtualCamera;
    public Camera mainCamera;
    public GameObject Monster; // 플레이어한테 다가오는놈
    public MonsterController monsterController;

    public GameObject tutorial;

    public PlayerMove playerControoler;
    //public MyCustomCamera CameraControoler;
    private bool isStagetTransitioning = false; // 스테이지 전환 중복 방지

    //지하철 프리펩 난이도별 구분
    public GameObject[] strange_situation_array; // 13 까지가 이지 다음부터 하드 
    public GameObject normal_stage;

    public Vector3 playerSpawnPosition = new Vector3(-0.6f, 1.5f, -1.5f); // 플레이어 리스폰 위치

    public int stage_count = 0;
    private bool is_first_load = true;
    private GameObject current_subway; // '현재' 생성된 지하철 인스턴스
    public bool is_success = false;
    public bool subway_in_die = false; //이상현상이 있고 안내릴때 BackgroundMove.cs에 문 안닫히게 하기 위한 bool
    private float in_die_timer = 17.0f;
    //BackgroundMove.cs의 target_speed가 0 일때부터 Time.deltaTime만큼 빼서 1 미만이 되면 subway_in_die = true
    public bool is_subway = true;
    public CinemachineVirtualCamera FixedCamera_player;
    private Rigidbody player_rb;

    private int[] duplication=new int[10]; // 중복 체크용

    public bool is_stop = false;

    //스테이지 넘어가는 화면용 
    public CanvasM CanvasM;
    public TextMeshProUGUI Station;

    public bool game_clear = false;
    public CinemachineVirtualCamera camera_clear;
    private int stage_clear_number = 9;

    public GameObject PointLight;
    public GameObject settingsPanel;
    public bool isOpen = false;


    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(startfadein());


        for (int i = 0; i < 10; i++)
        {
            duplication[i] = -1;
        }
        player_rb = player.GetComponent<Rigidbody>();
        if(stage_count>=stage_clear_number)
        {
            clear_stage();
        }
        else
        {
            next_stage();
        }
    }

    void Update()
    {
        UpdateTimer();
        fun_strange_situation_exit_or_stay(player);
        if (Input.GetKeyDown(KeyCode.Escape) && tutorial.activeInHierarchy == false)
        {
            ToggleSettings();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && tutorial.activeInHierarchy == true)
        {
            tutorial.SetActive(false);
        }
    }
    public void ToggleSettings()
    {
        isOpen = !isOpen;
        settingsPanel.SetActive(isOpen);

        if (isOpen)
        {
            // 메뉴 열릴 때
            EventSystem.current.SetSelectedGameObject(null);
            Time.timeScale = 0f; // 게임 일시정지
            AudioListener.pause = true;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }
        else
        {
            // 메뉴 닫힐 때
            EventSystem.current.SetSelectedGameObject(null);
            Time.timeScale = 1f;
            AudioListener.pause = false;
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }
    }
    // 타이머 업데이트
    private void UpdateTimer()
    {
        is_stop = Background.target_speed == 0f;
        if (is_stop)
        {
            in_die_timer -= Time.deltaTime;
        }
    }
    void fun_strange_situation_exit_or_stay(GameObject player)//지하철에서 내릴 때
    {
        
        if (player.transform.position.z > 3.0f || player.transform.position.z < -3.0f)
        {
            // [수정] 이미 전환 중이면(stage_clear == true) 중복 실행 방지
            if (Background.is_door_closing == true && strange_situation == false && stage_clear == false && game_clear == false)
            {
                //몬스터 활성화 전 리셋
                Debug.Log("YOU DIE!!!!");
                stage_count = 0;
                Monster.gameObject.SetActive(true);
                cinemachineVirtualCamera.Priority = 1000;
                player.transform.position = new Vector3(12.47f, 0.7604864f, -8.78f);
                player_rb.constraints = RigidbodyConstraints.FreezeAll;
                is_subway = false;
                stage_clear = true;
                isStagetTransitioning = true;
                is_success = false; 
                for (int i = 0; i < 10; i++)
                {
                    duplication[i] = -1;
                }
                StartCoroutine(DeathAndResetRoutine(5f));
            }
            else if (Background.is_door_closing == true && strange_situation == true && stage_clear == false&& game_clear == false)
            {
                Debug.Log("CLEAR!!");
                stage_count = stage_count + 1;
                Monster.gameObject.SetActive(true);
                cinemachineVirtualCamera.Priority = 1000;
                player.transform.position = new Vector3(12.47f, 0.7604864f, -8.78f);
                player_rb.constraints = RigidbodyConstraints.FreezeAll;
                is_subway = false;
                stage_clear = true;
                isStagetTransitioning = true;
                is_success = true;
                StartCoroutine(DeathAndResetRoutine(5f));
            }
        }
        else
        {
            // 이미 전환 중이면(stage_clear == true) 중복 실행 방지
            if (Background.is_door_closing == true && strange_situation == false && stage_clear == false&& game_clear == false)
            {
                Debug.Log("CLEAR!!");
                is_subway = true;
                stage_count=stage_count+1;
                 Monster.gameObject.SetActive(true);
                // Debug.Log("몬스터 생성하기!!!!!!!!!!!!!!!!!");
                FixedCamera_player.Priority = 1000;
                player.transform.position = new Vector3(1.0999f, 0.7604864f, -1.43f);
                player_rb.constraints = RigidbodyConstraints.FreezeAll;
                stage_clear = true;
                isStagetTransitioning = true;
                is_success = true;
                StartCoroutine(DeathAndResetRoutine(5f));
            }
            else if (Background.is_door_closing == true && strange_situation == true && stage_clear == false&& game_clear == false)
            {
                Debug.Log("YouDie");
                is_subway = true;
                stage_count = 0;
                Monster.gameObject.SetActive(true);
                FixedCamera_player.Priority = 1000;
                player.transform.position = new Vector3(1.0999f, 0.7604864f, -1.43f);
                player_rb.constraints = RigidbodyConstraints.FreezeAll;
                subway_in_die = true;
                stage_clear = true;
                is_success = false;
                isStagetTransitioning = true;
                for (int i = 0; i < 10; i++)
                {
                    duplication[i] = -1;
                }
                StartCoroutine(DeathAndResetRoutine(5f));
            }
        }
    }

    void next_stage()
    {
        Debug.Log("next stage");
        Debug.Log(stage_count);
        subway_in_die = false;
        // 모든 연출을 리셋
        Monster.gameObject.SetActive(false);
        cinemachineVirtualCamera.Priority = 10;
        if (!is_first_load && current_subway != null)
        {
            Destroy(current_subway);
        }
        // 스폰할 프리팹 결정 
        GameObject prefab_spawn;
        float random = Random.value;
        if (random < 0.35f || is_first_load)
        {
            Debug.Log("노말 35%");
            strange_situation = false;
            prefab_spawn = normal_stage;
        }
        else
        {
            strange_situation = true;
            random = Random.value;
            int prefab_idx=-1;
            if (random < 0.4f) //발견쉬운거 40%
            {
                Debug.Log("쉬운거 40%");
                bool go = false;
                while (!go) {
                    go = true;
                    prefab_idx = Random.Range(0, 14); //0~13
                    for (int i = 0; i < stage_count; i++)
                    {
                        if (duplication[i] == prefab_idx) go = false; 
                    }
                }
            }
            else //발견어려운거 60%
            {
                Debug.Log("어려운거 60%");
                bool go = false;
                while (!go)
                {
                    go = true;
                    prefab_idx = Random.Range(14, strange_situation_array.Length); //14~
                    for (int i = 0; i < stage_count; i++)
                    {
                        if (duplication[i] == prefab_idx) go = false;
                    }
                }
            }
            Debug.Log("인덱스: "+prefab_idx);
            prefab_spawn = strange_situation_array[prefab_idx];
        }
        current_subway = Instantiate(prefab_spawn, new Vector3(0, -0.2f, 0), prefab_spawn.transform.rotation);
        var new_subway = current_subway.GetComponent<Subway>();
        Background.door_left = new_subway.door_left;
        Background.door_right = new_subway.door_right;
        //변수 초기화 
        //stage_clear = false;
        //isStagetTransitioning = false;
        //is_success = false;
        is_subway = true;
        is_first_load = false; // 첫 로드 완료
        Background.reset_background(); // 새 지하철의 배경 리셋
        player.transform.position = playerSpawnPosition; // 플레이어 위치 리셋
    }

    IEnumerator startfadein()
    {
        Debug.Log("start fade in");
        CanvasM.fadeImage.gameObject.SetActive(true);
        CanvasM.set_black();
        yield return new WaitForSeconds(1f);
        yield return CanvasM.FadeIn();
        tutorial.SetActive(true);
    }

    IEnumerator DeathAndResetRoutine(float delay)
    {
        Debug.Log($"연출 대기... ({delay}초)");
        yield return new WaitForSeconds(delay);
        Debug.Log("페이드아웃");
        yield return CanvasM.FadeOut();
        PointLight.SetActive(false);
        //Station.text = "Station: "+ stage_count;
        // Station.gameObject.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        if(stage_count>=stage_clear_number)
        {
            clear_stage();
        }
        else
        {
            next_stage();
        }
        yield return new WaitForSeconds(2.0f);
        Station.gameObject.SetActive(false);
        Debug.Log("페이드인 시작");
        yield return CanvasM.FadeIn();

        player.transform.position = new Vector3(0.6f, 1.67f, -1.5f); // 플레이어 처음 위치로

        cinemachineVirtualCamera.transform.position = new Vector3(10.48f, 1.591435f, -8f); // 카메라 처음 위치로
        cinemachineVirtualCamera.transform.rotation = Quaternion.Euler(0, -40.91f, 0f);
        cinemachineVirtualCamera.Priority = 10;

        FixedCamera_player.transform.position = new Vector3(1.12f, 1.2f, 0.15f); // 카메라 처음 위치로
        FixedCamera_player.transform.rotation = Quaternion.Euler(0, -180f, 0f);
        FixedCamera_player.Priority = 11;

        player_rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ| RigidbodyConstraints.FreezeRotationY| RigidbodyConstraints.FreezePositionY;
        Background.is_door_closing = false;
        stage_clear = false;
        isStagetTransitioning = false;
        is_success = false;

    }
    
    public IEnumerator DeleteAfterDelay(GameObject obj, float delay)//플레이어 놀래키는 몬스터 delay 후에 삭제
    {
        Debug.Log("코루틴 발동");

        yield return new WaitForSeconds(delay);

        Debug.Log(obj);

        if (obj != null)
        {
            Destroy(obj);
            blood_effect.gameObject.SetActive(false);
            // 이거하기전에 페이드인 아웃 하면 될듯
            // 카메라 처음 위치로
            Debug.Log("삭제");
            //DeathAndResetRoutine(0.1f);
        }
    }
    void clear_stage()
    {
        Debug.Log("clear stage");
        //camera_clear.Priority = 10000;
        game_clear = true;
        subway_in_die = false;
        // 모든 연출을 리셋
        Monster.gameObject.SetActive(false);
        cinemachineVirtualCamera.Priority = 10;
        if (!is_first_load && current_subway != null)
        {
            Destroy(current_subway);
        }
        // 스폰할 프리팹 결정 
        GameObject prefab_spawn = normal_stage;

        current_subway = Instantiate(prefab_spawn, new Vector3(0, -0.2f, 0), prefab_spawn.transform.rotation);
        var new_subway = current_subway.GetComponent<Subway>();
        Background.door_left = new_subway.door_left;
        Background.door_right = new_subway.door_right;
        //변수 초기화 
        //stage_clear = false;
        //isStagetTransitioning = false;
        //is_success = false;
        is_subway = true;
        is_first_load = false; // 첫 로드 완료
        Background.reset_background(); // 새 지하철의 배경 리셋
        player.transform.position = playerSpawnPosition; // 플레이어 위치 리셋
    }
}