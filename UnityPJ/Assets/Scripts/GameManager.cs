using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public bool strange_situation = false; // �̻����� ���� ����
    public bool stage_clear = false; // �������� Ŭ���� ����
    public GameObject player; // �÷��̾�
    public BackgroundMove subway; //������ ���� Ȯ���� ���� ����
    void Start()
    {
        
    }
    void Update()
    {
        fun_strange_situation_exit_train(player);//����ö���� ���� ��
        fun_strange_situation_keep_going();//����ö�� ��� Ÿ���� ��
    }

    void fun_strange_situation_exit_train(GameObject player)//����ö���� ���� ��
    {
        if (player.transform.position.z > 3.5 || player.transform.position.z < -3.5)
        {
            if (subway.is_door_closing == true && strange_situation == false && stage_clear == false)
            {
                Debug.Log("YOU DIE!!!!");
                //������ġ�� �ʿ�������մϴ� 
                //player.transform.position = new Vector3(-0.6f, 1.5f, -1.5f); // �������� ���� ��ġ�� 
                stage_clear = true;//�������� Ŭ������ �ۼ������� �����
                //�߰����� �ڵ� �ʿ�
            }
            else if (subway.is_door_closing == true && strange_situation == true && stage_clear == false)
            {
                Debug.Log("CLEAR!!");
               // player.transform.position = new Vector3(-0.6f, 1.5f, -1.5f); // �������� ���� ��ġ��
                stage_clear = true; // ���� ����������
                //�߰����� �ڵ� �ʿ�
            }
        }
    }
    void fun_strange_situation_keep_going()//����ö�� ��� Ÿ���� ��
    {
        if (subway.is_door_closing == true && strange_situation == false && stage_clear == false)
        {
            Debug.Log("CLEAR!!");
            stage_clear = true; // ���� ����������
            //�߰����� �ڵ� �ʿ�
        }
        else if(subway.is_door_closing == true && strange_situation == true && stage_clear == false)
        {
            Debug.Log("YOU DIE!!!!");
            stage_clear = true; // �������� Ŭ������ �ۼ������� �����
            //�߰����� �ڵ� �ʿ�
        }
    }
}


