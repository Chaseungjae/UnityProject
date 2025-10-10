using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class BackgroundMove : MonoBehaviour
{
    public float background_move_speed=5.0f; //�� �ִ� ������ �ӵ�

    public GameObject tunnel; // �ݺ� �ͳ� ���� ������Ʈ 
    private float tunnel_check_pos = 60.0f; // �ͳ� �ݺ� ���� 
    private float tunnel_reset_pos = 95.0f; //�ͳ� ó�� ������ 
    private float start_tunnel_posx = -70.0f; //�� ��� �� �ͳ� ���� �ݺ� ���� ������
    private float end_pos = -140.0f; //�� ���� ������ 

    //������ ���� 
    public GameObject[] door_left; // �� �迭 
    public GameObject[] door_right; // �� �迭
    private float door_animation_time = 1.0f; //������ �ð�  
    public float door_open_pos = 0.7f; // �� ���� ������ ���� 
    private Vector3[] left_door_closed_pos;  // ���� ������ ���� ��ġ ����
    private Vector3[] right_door_closed_pos; // ������ ������ ���� ��ġ ����
    private bool is_door_open = false; //�� ���� ���� ���� ����
    private bool is_door_closing = false; //�� ���� �������� 

    //Ÿ�̸� 
    private float move_timer = 300.0f; //�ͳ� �ݺ� �ð�
    private float stop_timer = 12.0f; //�� ���� �ð� +2�� ������� 
    void Start()
    {
        left_door_closed_pos = new Vector3[door_left.Length];
        for (int i = 0; i < door_left.Length; i++)
        {
            left_door_closed_pos[i] = door_left[i].transform.position;
        }
        // ������ ������ �ʱ�(����) ��ġ ����
        right_door_closed_pos = new Vector3[door_right.Length];
        for (int i = 0; i < door_right.Length; i++)
        {
            right_door_closed_pos[i] = door_right[i].transform.position;
        }

    }

    void Update()
    {
        if (transform.position.x <= start_tunnel_posx && move_timer>0.0f) { // �ͳ� ���� �ݺ� ���⼭ ���� Ÿ�̸� if������ �Ҹ� ��� ���� �ϸ� �ɵ� 
            move_timer-=Time.deltaTime;
            tunnel.transform.Translate(Vector3.left * background_move_speed * Time.deltaTime);
            if(tunnel.transform.position.x <= tunnel_check_pos) {
                tunnel.transform.position = new Vector3(tunnel_reset_pos, 0, 0);}
        }

        else if (end_pos < transform.position.x) // �����ϰ� �ͳ�/ �ͳο��� ���������������� 
        {
            transform.Translate(Vector3.right * -background_move_speed * Time.deltaTime);
        }

        else //���߰� Ÿ�̸� �� ������ �� ���� ���� ���� 
        {
            stop_timer-=Time.deltaTime;
            if (stop_timer > 0.0f && !is_door_open&&!is_door_closing)
            {
                is_door_open = true;
                print("open");
                StartCoroutine(AnimateDoorsCoroutine(true)); //������ �ڷ�ƾ ����
            }
            else if (stop_timer < 1.0f && is_door_open && !is_door_closing)
            {
                print("close");
                is_door_closing = true; 
                is_door_open = false;
                StartCoroutine(AnimateDoorsCoroutine(false)); //������ �ڷ�ƾ ����
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
