using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

public class BackButton : MonoBehaviour
{
    public GameObject settingsPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnButtonClick()
    {
        EventSystem.current.SetSelectedGameObject(null);
        GameManager.Instance.isOpen = !GameManager.Instance.isOpen;
        settingsPanel.SetActive(GameManager.Instance.isOpen);
        if (!GameManager.Instance.isOpen)
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }
        
    }
}
