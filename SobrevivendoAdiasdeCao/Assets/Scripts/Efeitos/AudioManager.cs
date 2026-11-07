using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Listas de Audio Sources na Cena")]
    [SerializeField] private List<AudioSource> musicSources = new List<AudioSource>();
    [SerializeField] private List<AudioSource> sfxSources = new List<AudioSource>();

    private float currentMusicVolume = 1f;
    private float currentSFXVolume = 1f;

    private void Awake()
    {
        // Padrão Singleton: Garante que só existe 1 AudioManager e ele não é destruído entre cenas
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Aplica os volumes salvos no PlayerPrefs quando o jogo arranca
        SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume", 1f));
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 1f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", 1f));
    }

    // 1. Controlo do Volume Geral (afeta todo o áudio da Unity)
    public void SetMasterVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    // 2. Controlo do Volume das Músicas
    public void SetMusicVolume(float value)
    {
        currentMusicVolume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);

        // Remove elementos nulos (de cenas passadas) da lista
        musicSources.RemoveAll(source => source == null);

        foreach (var source in musicSources)
        {
            source.volume = currentMusicVolume;
        }
    }

    public void SetSFXVolume(float value)
    {
        currentSFXVolume = value;
        PlayerPrefs.SetFloat("SFXVolume", value);

        // Remove elementos nulos (de cenas passadas) da lista
        sfxSources.RemoveAll(source => source == null);

        foreach (var source in sfxSources)
        {
            source.volume = currentSFXVolume;
        }
    }

    // Métodos para registar novos sons ao carregar outras cenas
    public void RegisterMusicSource(AudioSource source)
    {
        if (!musicSources.Contains(source))
        {
            musicSources.Add(source);
            source.volume = currentMusicVolume;
        }
    }

    public void RegisterSFXSource(AudioSource source)
    {
        if (!sfxSources.Contains(source))
        {
            sfxSources.Add(source);
            source.volume = currentSFXVolume;
        }
    }
}