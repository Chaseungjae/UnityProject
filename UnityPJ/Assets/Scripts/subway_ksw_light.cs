using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class subway_ksw_light : MonoBehaviour
{
    [Header("--- 깜박임 속도 (무작위) ---")]
    [Tooltip("조명이 켜져있는 최소 시간")]
    public float minTimeOn = 1.0f;
    [Tooltip("조명이 켜져있는 최대 시간")]
    public float maxTimeOn = 4.0f;
    [Tooltip("조명이 꺼져있는 최소 시간 (정전)")]
    public float minTimeOff = 0.1f;
    [Tooltip("조명이 꺼져있는 최대 시간 (정전)")]
    public float maxTimeOff = 0.3f;
   
    //깜박임
    [Tooltip("'번쩍' 이벤트가 발생할 확률 ")]
    public float scaryFlickerProbability = 0.4f;
    [Tooltip("최소 '번쩍' 횟수")]
    public int minFlashes = 3;
    [Tooltip("최대 '번쩍' 횟수")]
    public int maxFlashes = 7;
    [Tooltip("'번쩍'할 때 켜지는 최소 시간 (매우 짧게)")]
    public float minFlashTimeOn = 0.03f;
    [Tooltip("'번쩍'할 때 켜지는 최대 시간 (매우 짧게)")]
    public float maxFlashTimeOn = 0.08f;
    [Tooltip("'번쩍'할 때 꺼지는 최소 시간 (매우 짧게)")]
    public float minFlashTimeOff = 0.03f;
    [Tooltip("'번쩍'할 때 꺼지는 최대 시간 (매우 짧게)")]
    public float maxFlashTimeOff = 0.08f;

    public Light[] lightsToControl;

    void Start()
    {
        StartCoroutine(FlickerGroup());
    }
    IEnumerator FlickerGroup()
    {
        while (true)
        {
            // 모두 켠다
            ToggleLights(true);
            Debug.Log("on");
            float waitTimeOn = Random.Range(minTimeOn, maxTimeOn);
            yield return new WaitForSeconds(waitTimeOn);

            // 확률에 따라 "일반 꺼짐" 또는 "공포 깜박임"을 결정
            if (Random.value < scaryFlickerProbability)
            {
                //  공포 깜박임
                Debug.Log("!!! SCARY FLICKER EVENT !!!");

                // 몇 번 깜박일지 무작위로 결정
                int flashCount = Random.Range(minFlashes, maxFlashes + 1);
                for (int i = 0; i < flashCount; i++)
                {
                    // 짧게 끄기 끄는소리 넣기
                    ToggleLights(false);
                    yield return new WaitForSeconds(Random.Range(minFlashTimeOff, maxFlashTimeOff));

                    // 짧게 켜기 키는 소리 넣기
                    ToggleLights(true);
                    yield return new WaitForSeconds(Random.Range(minFlashTimeOn, maxFlashTimeOn));
                }

                // "일반 꺼짐" 상태로 돌아가기 위해 최종적으로 끈다.
                ToggleLights(false);
                float waitTimeOff = Random.Range(minTimeOff, maxTimeOff);
                yield return new WaitForSeconds(waitTimeOff);
            }
            else
            {
                // 일반 꺼짐 이벤트 실행 
                ToggleLights(false);
                Debug.Log("Normal off");
                float waitTimeOff = Random.Range(minTimeOff, maxTimeOff);
                yield return new WaitForSeconds(waitTimeOff);
            }
        }
    }

    // 모든 Light 컴포넌트를 한꺼번에 켜고 끄기
    void ToggleLights(bool state)
    {
        foreach (Light light in lightsToControl)
        {
            if (light != null)
            {
                light.enabled = state;
            }
        }
    }

    // 게임 종료 또는 스크립트 비활성화 시, 모든 조명을 켠 상태로 원상복구
    void OnDisable()
    {
        if (lightsToControl != null && lightsToControl.Length > 0)
        {
            ToggleLights(true);
        }
    }
}
