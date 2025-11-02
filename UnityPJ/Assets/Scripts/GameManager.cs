using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public bool strange_situation = false; // 이상현상 발현 여부
    public bool stage_clear = false; // 스테이지 클리어 여부
    public GameObject player; // 플레이어
    public BackgroundMove subway; //문열림 여부 확인을 위한 변수

    //차씨 변수 임시
    public bool isScaleIncreased = false; // 크기 변경 여부 확인용 변수
    public float scaleIncreaseProbability = 1f; // 크기 변경 확률 100%
    private GameObject targetOB; // 찾은 오브젝트 저장용
    private Vector3 originalScale; // 원래 크기 저장용

    public GameObject monsterPrefab; // 몬스터 프리팹
    public Camera mainCamera; // 메인 카메라
    public Camera FixedCamera; // 고정 카메라
    public GameObject Monster;

    private Vector3 originalCameraPosition; // 카메라의 원래 위치 저장용
    private float originalCameraFOV; // 카메라의 원래 FOV 저장용

    public PlayerMove playerControoler;
    public MyCustomCamera CameraControoler;
    private bool isStagetTransitioning = false; // 스테이지 전환 중인지 여부

    void Start()
    {
        targetOB = GameObject.Find("[ASSETS]/ADS---/SM_wallposter_Rules_01"); // 포스터
        if (targetOB == null)
        {
            Debug.Log("오브젝트를 찾지 못했습니다.");
        }
        else
        {
            originalScale = targetOB.transform.localScale; // 원래 크기 저장
            TryincreaseScale();
        }
        if(mainCamera != null)
        {
            originalCameraPosition = mainCamera.transform.position; // 카메라의 원래 위치 저장
            originalCameraFOV = mainCamera.fieldOfView; // 카메라의 원래 FOV 저장
        }
    }
    void Update()
    {
        //Debug.Log(player.transform.position.x);
        if (player.transform.position.x > 13)
        {
            Debug.Log("트리거작동");
            if (!isStagetTransitioning)
            {
                fun_strange_situation_exit_train(player);//지하철에서 내릴 때
                fun_strange_situation_keep_going();//지하철에 계속 타있을 때
            }
        }
    }

    void fun_strange_situation_exit_train(GameObject player)//지하철에서 내릴 때
    {
        mainCamera.enabled = false;
        FixedCamera.enabled = true;

        FixedCamera.gameObject.SetActive(true);
        Monster.gameObject.SetActive(true);

        if (player.transform.position.z > 3.5 || player.transform.position.z < -3.5)
        {
            if (subway.is_door_closing == true && strange_situation == false && stage_clear == false)
            {
                Debug.Log("YOU DIE!!!!");
                if(monsterPrefab != null)
                {
                    //Instantiate(monsterPrefab, monsterPrefab.transform.position, monsterPrefab.transform.rotation);

                }   
                //원래위치는 필요없을듯합니다 
                //player.transform.position = new Vector3(-0.6f, 1.5f, -1.5f); // 내렸으니 원래 위치로 
                stage_clear = true;//스테이지 클리어라고 작성했지만 재시작
                //추가적인 코드 필요
            }
            else if (subway.is_door_closing == true && strange_situation == true && stage_clear == false)
            {
                Debug.Log("CLEAR!!");
               // player.transform.position = new Vector3(-0.6f, 1.5f, -1.5f); // 내렸으니 원래 위치로
                stage_clear = true; // 다음 스테이지로
                //추가적인 코드 필요
            }
        }
    }
    void fun_strange_situation_keep_going()//지하철에 계속 타있을 때
    {
        if (subway.is_door_closing == true && strange_situation == false && stage_clear == false)
        {
            Debug.Log("CLEAR!!");
            stage_clear = true; // 다음 스테이지로
            //추가적인 코드 필요
        }
        else if(subway.is_door_closing == true && strange_situation == true && stage_clear == false)
        {
            Debug.Log("YOU DIE!!!!");
            stage_clear = true; // 스테이지 클리어라고 작성했지만 재시작
            //추가적인 코드 필요
        }
    }

    void TryincreaseScale()
    {
        if (isScaleIncreased) return;

        if(targetOB == null) return;
        
        if(Random.value < scaleIncreaseProbability)
        {
            targetOB.transform.localScale *= 1.5f; // 크기를 1.5배로 증가
            isScaleIncreased = true;
            Debug.Log("크기 증가 완료");
        }
    }
}


