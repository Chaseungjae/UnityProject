// FadeManager.cs

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CanvasM : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1.0f;

    // 페이드아웃 코루틴 (public으로 변경)
    public IEnumerator FadeOut()
    {
        fadeImage.gameObject.SetActive(true);
        float timer = 0f;
        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        while (timer < fadeDuration)
        {
            // Time.unscaledDeltaTime: Time.timeScale이 0(게임 일시정지)이어도 동작
            timer += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        color.a = 1f;
        fadeImage.color = color;
    }

    // 페이드인 코루틴
    public IEnumerator FadeIn()
    {
        float timer = 0f;
        Color color = fadeImage.color;
        color.a = 1f; // 1(불투명)에서 시작
        fadeImage.color = color;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(1f, 0f, timer / fadeDuration); 
            fadeImage.color = color;
            yield return null;
        }
        color.a = 0f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false); // 끝나면 비활성화
    }

    public void set_black()
    {
        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;
    }

}