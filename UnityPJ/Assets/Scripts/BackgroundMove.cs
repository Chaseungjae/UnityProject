using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class BackgroundMove : MonoBehaviour
{
    public float background_move_speed=5.0f; //벽 최대 움직임 속도

    public GameObject tunnel; // 반복 터널 게임 오브젝트 
    private float tunnel_check_pos = 60.0f; // 터널 반복 구간 
    private float tunnel_reset_pos = 95.0f; //터널 처음 포지션 
    private float start_tunnel_posx = -70.0f; //역 출발 후 터널 구간 반복 시작 포지션
    private float end_pos = -140.0f; //역 도착 포지션 

    //문열림 조작 
    public GameObject[] door_left; // 문 배열 
    public GameObject[] door_right; // 문 배열
    private float door_animation_time = 1.0f; //문열림 시간  
    public float door_open_pos = 0.7f; // 문 열림 포지션 차이 
    private Vector3[] left_door_closed_pos;  // 왼쪽 문들의 닫힌 위치 저장
    private Vector3[] right_door_closed_pos; // 오른쪽 문들의 닫힌 위치 저장
    private bool is_door_open = false; //문 열림 닫힘 상태 저장
    private bool is_door_closing = false; //문 닫힘 끝났는지 

    //타이머 
    private float move_timer = 300.0f; //터널 반복 시간
    private float stop_timer = 12.0f; //역 정차 시간 +2초 해줘야함 
    void Start()
    {
        left_door_closed_pos = new Vector3[door_left.Length];
        for (int i = 0; i < door_left.Length; i++)
        {
            left_door_closed_pos[i] = door_left[i].transform.position;
        }
        // 오른쪽 문들의 초기(닫힌) 위치 저장
        right_door_closed_pos = new Vector3[door_right.Length];
        for (int i = 0; i < door_right.Length; i++)
        {
            right_door_closed_pos[i] = door_right[i].transform.position;
        }

    }

    void Update()
    {
        if (transform.position.x <= start_tunnel_posx && move_timer>0.0f) { // 터널 구간 반복 여기서 무브 타이머 if문으로 소리 방송 구현 하면 될듯 
            move_timer-=Time.deltaTime;
            tunnel.transform.Translate(Vector3.left * background_move_speed * Time.deltaTime);
            if(tunnel.transform.position.x <= tunnel_check_pos) {
                tunnel.transform.position = new Vector3(tunnel_reset_pos, 0, 0);}
        }

        else if (end_pos < transform.position.x) // 시작하고 터널/ 터널에서 엔드포지션전까지 
        {
            transform.Translate(Vector3.right * -background_move_speed * Time.deltaTime);
        }

        else //멈추고 타이머 후 움직임 문 열림 닫힘 구현 
        {
            stop_timer-=Time.deltaTime;
            if (stop_timer > 0.0f && !is_door_open&&!is_door_closing)
            {
                is_door_open = true;
                print("open");
                StartCoroutine(AnimateDoorsCoroutine(true)); //문열림 코루틴 시작
            }
            else if (stop_timer < 1.0f && is_door_open && !is_door_closing)
            {
                print("close");
                is_door_closing = true; 
                is_door_open = false;
                StartCoroutine(AnimateDoorsCoroutine(false)); //문닫힘 코루틴 시작
            }
            if (stop_timer < 0.0f)
            {
                transform.Translate(Vector3.right * -background_move_speed * Time.deltaTime);
            }
        }
    }

    IEnumerator AnimateDoorsCoroutine(bool open)
    {
        float elapsedTime = 0f;
        Vector3[] leftStartPositions = new Vector3[door_left.Length];
        Vector3[] rightStartPositions = new Vector3[door_right.Length];
        Vector3[] leftTargetPositions = new Vector3[door_left.Length];
        Vector3[] rightTargetPositions = new Vector3[door_right.Length];
        for (int i = 0; i < door_left.Length; i++)
        {
            leftStartPositions[i] = door_left[i].transform.position;
            rightStartPositions[i] = door_right[i].transform.position;

            if (open)
            {
                leftTargetPositions[i] = left_door_closed_pos[i] + Vector3.left * door_open_pos;
                rightTargetPositions[i] = right_door_closed_pos[i] + Vector3.right * door_open_pos;
            }
            else
            {
                leftTargetPositions[i] = left_door_closed_pos[i];
                rightTargetPositions[i] = right_door_closed_pos[i];
            }
        }

        while (elapsedTime < door_animation_time)
        {
            float t = elapsedTime / door_animation_time;

            for (int i = 0; i < door_left.Length; i++)
            {
                door_left[i].transform.position = Vector3.Lerp(leftStartPositions[i], leftTargetPositions[i], t);
                door_right[i].transform.position = Vector3.Lerp(rightStartPositions[i], rightTargetPositions[i], t);
            }

            elapsedTime += Time.deltaTime;
            yield return null;

        }
        for (int i = 0; i < door_left.Length; i++)
        {
            door_left[i].transform.position = leftTargetPositions[i];
            door_right[i].transform.position = rightTargetPositions[i];
        }
    }
}
