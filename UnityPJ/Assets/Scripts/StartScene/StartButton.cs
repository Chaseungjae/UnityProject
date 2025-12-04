using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    public Fadein Panal;
    [SerializeField]
    private string sceneName = "NEW_SUBWAY";

    public void OnButtonClick()
    {
        // 페이드 아웃이 끝난 후 실행될 콜백 등록
        Panal.RegisterCallback(() =>
        {
            SceneManager.LoadScene(sceneName);
        });

        Panal.FadeOut();  // 페이드 시작
    }
}
