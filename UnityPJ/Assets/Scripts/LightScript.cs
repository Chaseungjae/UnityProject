using UnityEngine;
using System.Collections;
public class LightScript : MonoBehaviour
{
    [SerializeField]
    private Light sceneLight;

    void Start()
    {
        // 게임 시작 시, 반복 코루틴을 딱 한 번만 실행시킵니다.
        StartCoroutine(RepeatBlinkLoop());
    }

    // Update에서는 아무것도 하지 않아야 합니다.
    void Update()
    {

    }

    // 5초마다 깜빡임을 반복하는 통합 코루틴
    IEnumerator RepeatBlinkLoop()
    {
        // while(true)를 쓰면 이 게임 오브젝트가 켜져있는 동안 계속 반복합니다.
        while (true)
        {
            // 1. 5초를 기다립니다.
            yield return new WaitForSeconds(5.0f);

            // 2. 깜빡이는 기능을 수행합니다.
            //Debug.Log("Light Blinked");

            sceneLight.intensity = 0.5f;
            yield return new WaitForSeconds(0.1f);

            sceneLight.intensity = 4.0f;
            yield return new WaitForSeconds(0.1f);

            sceneLight.intensity = 0.5f;
            yield return new WaitForSeconds(0.1f);

            sceneLight.intensity = 4.0f;
            yield return new WaitForSeconds(0.1f);

            sceneLight.intensity = 0.5f;
            yield return new WaitForSeconds(0.1f);

            sceneLight.intensity = 4.0f;

            // 루프의 끝에 도달하면 다시 위로 올라가 5초를 기다립니다.
        }
    }
}
