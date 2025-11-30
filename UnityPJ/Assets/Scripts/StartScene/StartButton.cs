using UnityEngine;
using UnityEngine.SceneManagement;



public class StartButton : MonoBehaviour
{

    [SerializeField] 
    private string sceneName = "NEW_SUBWAY"; // 빌드용 이름 저장

    public void OnButtonClick()
    {
        SceneManager.LoadScene(sceneName);
    }
}
