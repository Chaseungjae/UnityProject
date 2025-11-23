using UnityEngine;
using UnityEngine.InputSystem.Controls;
using System.Collections;
using UnityEditor.Build.Content;

public class subway_ksw_door_tril : MonoBehaviour
{
    public AudioSource door_sound;
    public GameObject[] door_left;
    public GameObject[] door_right;
    private Vector3[] left_door_closed_pos;  // 왼쪽 문들의 닫힌 위치 저장
    private Vector3[] right_door_closed_pos; // 오른쪽 문들의 닫힌 위치 저장
    [Tooltip("떨림의 최대 강도 (좌우/상하 변위)")]
    public float shakeIntensity = 0.05f; // (값이 클수록 격렬하게 떨림)

    [Tooltip("다음 떨림까지의 최소 대기 시간 (고요함)")]
    public float minWaitTime = 3.0f;
    [Tooltip("다음 떨림까지의 최대 대기 시간 (고요함)")]
    public float maxWaitTime = 10.0f;

    [Tooltip("떨림이 지속되는 최소 시간")]
    public float minShakeDuration = 0.5f;
    [Tooltip("떨림이 지속되는 최대 시간")]
    public float maxShakeDuration = 2.0f;

    private bool isShaking = false;
    private bool issound = false;
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
        StartCoroutine(ShakeControlLoop());
    }

    void Update()
    {
        // 역에 정차했을 때 
        if (GameManager.Instance.is_stop)
        {
            isShaking = false;
        }
        else if (isShaking)
        {
            if (!issound)  {  door_sound.Play(); issound = true; }
            Debug.Log("흔들");
            ApplyShakeEffect();
        }
        else
        {
            door_sound.Stop();
            issound = false; 
            ReturnDoorsToClosedPosition();
        }
    }

    IEnumerator ShakeControlLoop()
    {
        while (true)
        {
            //랜덤한 시간 동안 대기 
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            // 떨림 상태 시작 
            isShaking = true;
            float shakeDuration = Random.Range(minShakeDuration, maxShakeDuration);
            yield return new WaitForSeconds(shakeDuration);

            //떨림 상태 종료
            isShaking = false;
        }
    }

    void ApplyShakeEffect()
    {
        for (int i = 0; i < door_left.Length; i++)
        {
            // 원본 닫힘 위치를 가져옴
            Vector3 originalLeftPos = left_door_closed_pos[i];
            Vector3 originalRightPos = right_door_closed_pos[i];

            // 좌우(x) 상하(y)로 랜덤한 오프셋(변위) 생성
            float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
            float offsetY = Random.Range(-shakeIntensity, shakeIntensity);
            Vector3 randomOffset = new Vector3(offsetX, offsetY, 0f);

            // 원본 위치 + 랜덤 변위를 적용
            door_left[i].transform.position = originalLeftPos + randomOffset;

            // 오른쪽 문도 다른 랜덤값으로 떨리게
            offsetX = Random.Range(-shakeIntensity, shakeIntensity);
            offsetY = Random.Range(-shakeIntensity, shakeIntensity);
            randomOffset = new Vector3(offsetX, offsetY, 0f);

            door_right[i].transform.position = originalRightPos + randomOffset;
        }
    }
    void ReturnDoorsToClosedPosition()
    {

        for (int i = 0; i < door_left.Length; i++)
        {
            door_left[i].transform.position = left_door_closed_pos[i];
            door_right[i].transform.position = right_door_closed_pos[i];
        }
    }
}
