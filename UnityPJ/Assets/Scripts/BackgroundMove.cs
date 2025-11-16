using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Jobs;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class BackgroundMove : MonoBehaviour
{
    public float max_speed = 5.0f; // 지하철의 최대 속도
    public float acceleration_time = 1.5f; // 최대 속도까지 도달하는 데 걸리는 시간 (초)
    public float deceleration_time = 1.0f; // 멈추는 데 걸리는 시간 (초)

    private float current_speed = 0.0f; // 현재 속도
    public float target_speed = 0.0f;  // 목표 속도 //GameManager에게 값 전달을 위해 public으로 변경
    private float velocity = 0.0f;

    public GameObject tunnel; // 반복 터널 게임 오브젝트 
    private float tunnel_check_pos = 60.0f; // 터널 반복 구간 
    private float tunnel_reset_pos = 95.0f; //터널 처음 포지션 
    private float start_tunnel_posx = -70.0f; //역 출발 후 터널 구간 반복 시작 포지션
    private float end_pos = -140.0f; //역 도착 포지션 
    private bool is_end=false; //도착하고 다시 출발하기 위한 끝까지 갔는지에대한 

    //문열림 조작 
    public GameObject[] door_left; // 문 배열 
    public GameObject[] door_right; // 문 배열
    private float door_animation_time = 1.0f; //문열림 시간  
    public float door_open_pos = 0.7f; // 문 열림 포지션 차이 
    private Vector3[] left_door_closed_pos;  // 왼쪽 문들의 닫힌 위치 저장
    private Vector3[] right_door_closed_pos; // 오른쪽 문들의 닫힌 위치 저장
    public bool is_door_open = false; //문 열림 닫힘 상태 저장
    public bool is_door_closing = true; //문 닫힘 끝났는지 

    //타이머 
    private float move_timer = 10.0f; //터널 반복 시간
    private float stop_timer = 12.0f; //역 정차 시간 +2초 해줘야함 

    //지하철 다시 출발할지 말지 플레이어 내리는지 판별
    public GameObject player_p;
    private bool player_is_in = true;

    //초기화용 변수 
    private Vector3 original_tunnel_position = new Vector3(150f, 0f, 0f);
    private Vector3 original_tunnel_position2 = new Vector3(-5f, 0f, 0f);
    public GameManager game_manager;

    public AudioSource moveSound;
    public AudioSource stopSound;
    public AudioSource bellSound;
    public AudioSource announcement;
    public AudioSource[] door_open;

    public bool is_sound = true;
    public bool is_door_sound = true;

    void Update()
    {
        //플레이어가 내렸는지 판별 
        if (player_p.transform.position.z > 3.5 || player_p.transform.position.z < -3.5) { player_is_in = false; }

        //멈출때 속도 줄이는 부분
        if (!is_end&&transform.position.x < end_pos+10.0f) { target_speed = 0f; 
            if (moveSound.isPlaying)
            {
                stopSound.Play();
                moveSound.Stop();
            }
        }
        // 현재 속도가 목표 속도와 다른지 확인
        if (!Mathf.Approximately(current_speed, target_speed))
        {
            float smooth_time = (target_speed > 0) ? acceleration_time : deceleration_time;
            current_speed = Mathf.SmoothDamp(
                current_speed,     // 현재 값
                target_speed,      // 목표 값
                ref velocity,     // 현재 속도 (참조로 전달)
                smooth_time        // 목표 도달까지 걸리는 시간
            );
        }
        // 이동 중 상태 처리 (속도가 0보다 클 때)
        if (current_speed > 0.001f) // 속도가 0이 아니면 무조건 이동 중
        {
            if (!moveSound.isPlaying && target_speed != 0f&& is_sound)
            {
                moveSound.Play();
            }
            // 터널 구간인지, 일반 구간인지에 따라 누가 움직일지 결정
            if (transform.position.x <= start_tunnel_posx && move_timer > 0.0f)
            {
                // 터널 구간: 터널을 움직임
                move_timer -= Time.deltaTime;
                tunnel.transform.Translate(Vector3.left * current_speed * Time.deltaTime);
                if (tunnel.transform.position.x <= tunnel_check_pos)
                {
                    tunnel.transform.position = new Vector3(tunnel_reset_pos, 0, 0);
                }
                if (move_timer < 1.0f && is_sound) announcement.Play();
            }
            else
            {
                // 일반 구간 배경(자신)을 움직임
                transform.Translate(Vector3.right * -current_speed * Time.deltaTime);
            }
        }
        // 정지 상태 처리 (속도가 0일 때)
        else
        {
            // 속도가 0일 때, 우리가 멈춘 것인지(target_speed == 0) 확인
            // (게임 시작 시 speed 0 상태와 구분하기 위함)
            if (target_speed == 0f)
            {
                stop_timer -= Time.deltaTime;
                if (stop_timer > 0.0f && !is_door_open && !is_door_closing)
                {
                    is_door_open = true;
                    is_door_closing = false;
                    print("open");
                    for (int i = 0; i < 3; i++)
                    {
                        if(is_door_sound)door_open[i].Play();
                    }
                    StartCoroutine(animate_doors_coroutine(true)); //문열림 코루틴 시작
                }
                else if (stop_timer < 6.0f&& !bellSound.isPlaying&&stop_timer>5.0f) { bellSound.Play(); }

                else if (stop_timer < 1.0f && is_door_open && !is_door_closing)
                {
                    print("close");
                    is_door_closing = true;
                    is_door_open = false;
                    if (game_manager.subway_in_die == true)//GameManager의 subway_in_die == true이면 문 안닫음
                    {
                        Debug.Log("not closing");
                        return;
                    }
                    StartCoroutine(animate_doors_coroutine(false)); //문닫힘 코루틴 시작
                }
                /*
                // 다시 출발 (타이머 0되고, 문 닫힘이 끝났을 때, 플레이어가 타고있을때)
                if (stop_timer < 0.0f && is_door_closing&& player_is_in)
                {
                    print("dd");
                    is_end=true; 
                    target_speed = max_speed;
                }*/
            }
        }
    }

    //문열림 함수
    IEnumerator animate_doors_coroutine(bool open)
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
    public void reset_background()
    {
        Debug.Log("Resetting BackgroundMove state");
        StopAllCoroutines();
        // Subway 자체의 위치는 GameManager가 스폰 시점에 지정하므로 여기선 리셋 X
        gameObject.transform.position = original_tunnel_position2;
        tunnel.transform.position = original_tunnel_position;
        // 속도 및 이동 관련 변수 리셋
        current_speed = 0.0f;
        target_speed = max_speed; // Start()에서처럼 즉시 출발하도록 설정
        velocity = 0.0f;
        is_end = false;
        // 타이머 리셋
        move_timer = 10.0f;
        stop_timer = 12.0f;
        // 상태 변수 리셋
        is_door_open = false;
        is_door_closing = false; 
        player_is_in = true;    //시작시 목표 속도 변수 초기화하면서 움직이게 하는 구문 
        is_door_sound = true;

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
}
