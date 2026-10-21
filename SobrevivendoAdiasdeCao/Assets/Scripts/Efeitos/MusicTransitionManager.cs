
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicTransitionManager : MonoBehaviour
{
    public static MusicTransitionManager Instance;

    [Header("Fontes de áudio")]
    public AudioSource audioA;
    public AudioSource audioB;

    [Header("Músicas")]
    public AudioClip musicaIntro;
    public AudioClip musicaMenu;
    public AudioClip musicaTutorial;

    [Header("Nomes das cenas")]
    public string cenaIntro1 = "Intro1";
    public string cenaIntro2 = "Intro2";
    public string cenaMenu = "Menu";
    public string cenaTutorial = "Tutorial";

    [Header("Transição")]
    [Min(0f)]
    public float duracaoTransicao = 3f;

    private AudioSource audioAtual;
    private AudioSource proximoAudio;
    private Coroutine transicaoAtual;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        PrepararAudio(audioA);
        PrepararAudio(audioB);

        audioAtual = audioA;
        proximoAudio = audioB;

        if (musicaIntro != null)
        {
            audioAtual.clip = musicaIntro;
            audioAtual.volume = 1f;
            audioAtual.Play();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += AoCarregarCena;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AoCarregarCena;
    }

    private void PrepararAudio(AudioSource fonte)
    {
        if (fonte == null)
            return;

        fonte.playOnAwake = false;
        fonte.loop = true;
        fonte.volume = 0f;
    }

    private void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        string nome = cena.name;

        // A mesma música continua nas duas intros.
        if (nome == cenaIntro1 || nome == cenaIntro2)
            return;

        if (nome == cenaMenu)
        {
            TrocarMusica(musicaMenu);
        }
        else if (nome == cenaTutorial)
        {
            TrocarMusica(musicaTutorial);
        }
    }

    public void TrocarMusica(AudioClip novaMusica)
    {
        if (novaMusica == null)
        {
            Debug.LogWarning("A música de destino não foi configurada.");
            return;
        }

        if (audioAtual != null &&
            audioAtual.isPlaying &&
            audioAtual.clip == novaMusica)
        {
            return;
        }

        if (transicaoAtual != null)
            StopCoroutine(transicaoAtual);

        transicaoAtual = StartCoroutine(
            FazerCrossfade(novaMusica)
        );
    }

    private IEnumerator FazerCrossfade(AudioClip novaMusica)
    {
        proximoAudio.Stop();
        proximoAudio.clip = novaMusica;
        proximoAudio.volume = 0f;
        proximoAudio.Play();

        float volumeInicial = audioAtual.volume;
        float tempo = 0f;

        if (duracaoTransicao <= 0f)
        {
            audioAtual.Stop();
            audioAtual.volume = 0f;
            proximoAudio.volume = 1f;
        }
        else
        {
            while (tempo < duracaoTransicao)
            {
                tempo += Time.unscaledDeltaTime;

                float progresso = Mathf.Clamp01(
                    tempo / duracaoTransicao
                );

                audioAtual.volume = Mathf.Lerp(
                    volumeInicial, 0f, progresso
                );

                proximoAudio.volume = progresso;

                yield return null;
            }

            audioAtual.volume = 0f;
            audioAtual.Stop();
            proximoAudio.volume = 1f;
        }

        AudioSource temporario = audioAtual;
        audioAtual = proximoAudio;
        proximoAudio = temporario;

        transicaoAtual = null;
    }
}
