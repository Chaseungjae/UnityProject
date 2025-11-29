using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class SoundSettingsUI : MonoBehaviour
{
    public Slider volumeSlider;
    public TMP_InputField volumeInput;
    public AudioMixer mixer;

    void Start()
    {
        float saved = PlayerPrefs.GetFloat("MasterVolume", 1f);

        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 5f;
        volumeSlider.value = saved;

        volumeInput.text = saved.ToString("0.00");

        volumeSlider.onValueChanged.AddListener(OnSliderChanged);
        volumeInput.onEndEdit.AddListener(OnInputChanged);

        ApplyVolume(saved);
    }

    private void OnSliderChanged(float value)
    {
        volumeInput.text = value.ToString("0.00");
        ApplyVolume(value);
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    private void OnInputChanged(string text)
    {
        if (float.TryParse(text, out float value))
        {
            value = Mathf.Clamp(value, 0f, 5f);
            volumeSlider.value = value;
            ApplyVolume(value);
            PlayerPrefs.SetFloat("MasterVolume", value);
        }
        else
        {
            volumeInput.text = volumeSlider.value.ToString("0.00");
        }
    }

    private void ApplyVolume(float sliderValue)
    {
        float volumeDB;

        if (sliderValue <= 0.01f)
        {
            volumeDB = -80f; // 무음(믹서 최소)
        }
        else
        {
            // 1 → 0 dB, 2 → +6 dB, 5 → +24 dB
            volumeDB = (sliderValue - 1f) * 6f;
        }

        mixer.SetFloat("MasterVolume", volumeDB);
    }
}