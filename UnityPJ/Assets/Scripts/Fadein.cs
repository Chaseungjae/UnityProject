using System;
using System.Collections;
using UnityEngine;

public class Fadein : MonoBehaviour
{
    public bool isFadeIn;
    public GameObject panel;
    private CanvasGroup group;

    private Action onCompleteCallback;

    private void Awake()
    {
        group = panel.GetComponent<CanvasGroup>();
    }

    public void FadeOut()
    {
        panel.SetActive(true);
        StartCoroutine(CoFadeOut());
    }

    IEnumerator CoFadeOut()
    {
        float elapsedTime = 0f;
        float fadedTime = 2f;

        while (elapsedTime < fadedTime)
        {
            group.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadedTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        group.alpha = 1f;
        onCompleteCallback?.Invoke();
    }

    

    public void RegisterCallback(Action callback)
    {
        onCompleteCallback = callback;
    }
}
