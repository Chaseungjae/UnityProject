using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public bool strange_situation = false; // 이상현상 발현 여부
    public bool stage_clear = false; // 스테이지 클리어 여부
    public GameObject player; // 플레이어
    public BackgroundMove subway; //문열림 여부 확인을 위한 변수
    void Start()
    {
        
    }
    void Update()
    {
        fun_strange_situation_exit_train(player);//지하철에서 내릴 때
        fun_strange_situation_keep_going();//지하철에 계속 타있을 때
    }

    void fun_strange_situation_exit_train(GameObject player)//지하철에서 내릴 때
    {
        if (player.transform.position.z > 3.5 || player.transform.position.z < -3.5)
        {
            if (subway.is_door_closing == true && strange_situation == false && stage_clear == false)
            {
                Debug.Log("YOU DIE!!!!");
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
}


