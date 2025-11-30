using UnityEngine;
using UnityEngine.InputSystem.Controls;
using System.Collections;

public class subway_ksw_door : MonoBehaviour
{
    public GameObject[] door_left;
    public GameObject[] door_right;
    private float door_animation_time = 0.4f; //문열림 시간  
    private float door_open_pos = 0.7f; // 문 열림 포지션 차이 
    private Vector3[] left_door_closed_pos;  // 왼쪽 문들의 닫힌 위치 저장
    private Vector3[] right_door_closed_pos; // 오른쪽 문들의 닫힌 위치 저장
    private bool is_door_open = false; //문 열림 닫힘 상태 저장
    private bool is_door_closing = true; //문 닫힘 끝났는지 

    private bool is_open = false;
    
    private float timer = 5f;

    public GameObject cude;
    public AudioSource[] boor_open;
    void Start()
    {
        GameManager.Instance.Background.is_door_sound = false;
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
        cude.GetComponent<MeshRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0) timer -= Time.deltaTime;
        if (timer < 0&&!is_open&& !GameManager.Instance.is_stop)
        {
            is_open= true; 
            Debug.Log("open"); 
            for(int i=0;i< boor_open.Length; i++)
            {
                boor_open[i].Play();
            }
            StartCoroutine(animate_doors_coroutine(true));
        }

        if(GameManager.Instance.is_stop&&is_open)
        {
            is_open = false;
            Debug.Log("cls");
            cude.SetActive(false);
        }
    }
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

}
