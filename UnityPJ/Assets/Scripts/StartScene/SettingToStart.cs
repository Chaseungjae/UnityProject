using UnityEngine;
using UnityEngine.SceneManagement;



public class SettingToStart : MonoBehaviour
{

    [SerializeField] 
    private string sceneName = "StartScene"; // 빌드용 이름 저장

    

    public void OnButtonClick()
    {
        SceneManager.LoadScene(sceneName);
    }
}
