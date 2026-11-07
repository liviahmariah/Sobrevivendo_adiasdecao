using UnityEngine;
using UnityEngine.UI;

public class AudioMenuUI : MonoBehaviour
{
    [Header("Sliders do Menu")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        // Define o limite dos sliders entre 0 (0%) e 1 (100%)
        masterSlider.minValue = 0f; masterSlider.maxValue = 1f;
        musicSlider.minValue = 0f; musicSlider.maxValue = 1f;
        sfxSlider.minValue = 0f; sfxSlider.maxValue = 1f;

        // Carrega o valor atual do volume guardado
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // Associa os sliders às funções do AudioManager
        masterSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetMasterVolume(val));
        musicSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetMusicVolume(val));
        sfxSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetSFXVolume(val));
    }
}