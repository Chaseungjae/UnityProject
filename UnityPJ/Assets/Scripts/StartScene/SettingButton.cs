using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingButton : MonoBehaviour
{


    [SerializeField] 
    private string sceneName = "SettingScene"; // 빌드용 이름 저장



    public void OnButtonClick()
    {
        SceneManager.LoadScene(sceneName);
    }
}
