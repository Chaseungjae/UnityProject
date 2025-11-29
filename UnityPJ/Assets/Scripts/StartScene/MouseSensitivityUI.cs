using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MouseSensitivityUI : MonoBehaviour
{
    public Slider sensitivitySlider;
    public TMP_InputField sensitivityInput;
    private MyCustomCamera myCamera;   // ★ MyCustomCamera 연결

    void Start()
    {
        myCamera = FindObjectOfType<MyCustomCamera>();

        float saved = PlayerPrefs.GetFloat("MouseSensitivity", 300f);

        sensitivitySlider.value = saved;
        sensitivityInput.text = saved.ToString("0.00");

        // 슬라이더 → 텍스트
        sensitivitySlider.onValueChanged.AddListener(OnSliderChanged);

        // 텍스트 입력 → 슬라이더
        sensitivityInput.onEndEdit.AddListener(OnInputChanged);
    }

    // 슬라이더 조작 시
    private void OnSliderChanged(float value)
    {
        sensitivityInput.text = value.ToString("0.00");
        PlayerPrefs.SetFloat("MouseSensitivity", value);

        // ★ MyCustomCamera에 적용
        if (myCamera != null)
            myCamera.ApplySensitivity(value);
    }

    // 텍스트 입력 완료 시
    private void OnInputChanged(string text)
    {
        if (float.TryParse(text, out float value))
        {
            value = Mathf.Clamp(value, 100f, 1000f);
            sensitivitySlider.value = value;
            PlayerPrefs.SetFloat("MouseSensitivity", value);

            // ★ MyCustomCamera에 적용
            if (myCamera != null)
                myCamera.ApplySensitivity(value);
        }
        else
        {
            sensitivityInput.text = sensitivitySlider.value.ToString("0.00");
        }
    }
}
