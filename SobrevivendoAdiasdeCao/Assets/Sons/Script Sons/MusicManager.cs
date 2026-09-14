using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [Header("AudioSources")]
    public AudioSource musicMenu;
    public AudioSource musicCutscene;

    [Header("Configuração")]
    public float fadeDuration = 2f;

    private bool mudandoDeCena = false;

    private void Awake()
    {
        // Evita dois MusicManagers
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // Mantém o MusicManager e seus filhos entre as cenas
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Música do Menu
        if (musicMenu != null)
        {
            musicMenu.volume = 1f;
            musicMenu.loop = true;

            if (!musicMenu.isPlaying)
            {
                musicMenu.Play();
            }
        }

        // Música da Cutscene começa parada
        if (musicCutscene != null)
        {
            musicCutscene.volume = 0f;
            musicCutscene.loop = true;
            musicCutscene.Stop();
        }
    }

    // =====================================================
    // MENU → INTRO
    // =====================================================

    public void IrParaIntro()
    {
        if (mudandoDeCena)
            return;

        StartCoroutine(MudarParaIntro());
    }

    private IEnumerator MudarParaIntro()
    {
        mudandoDeCena = true;

        // ---------------------------------------------
        // FADE OUT DO MENU
        // ---------------------------------------------

        if (musicMenu != null)
        {
            float volumeInicial = musicMenu.volume;
            float tempo = 0f;

            while (tempo < fadeDuration)
            {
                tempo += Time.unscaledDeltaTime;

                float progresso = tempo / fadeDuration;

                musicMenu.volume = Mathf.Lerp(
                    volumeInicial,
                    0f,
                    progresso
                );

                yield return null;
            }

            musicMenu.volume = 0f;
            musicMenu.Stop();
        }

        // ---------------------------------------------
        // TROCA PARA INTRO
        // ---------------------------------------------

        SceneManager.LoadScene("Intro1");

        yield return null;

        // ---------------------------------------------
        // COMEÇA A MÚSICA DA CUTSCENE
        // ---------------------------------------------

        if (musicCutscene != null)
        {
            musicCutscene.volume = 0f;

            if (!musicCutscene.isPlaying)
            {
                musicCutscene.Play();
            }

            float tempo = 0f;

            while (tempo < fadeDuration)
            {
                tempo += Time.unscaledDeltaTime;

                float progresso = tempo / fadeDuration;

                musicCutscene.volume = Mathf.Lerp(
                    0f,
                    1f,
                    progresso
                );

                yield return null;
            }

            musicCutscene.volume = 1f;
        }

        mudandoDeCena = false;
    }

    // =====================================================
    // INTRO → INTRO1
    // =====================================================

    public void IrParaIntro2()
    {
        if (mudandoDeCena)
            return;

        StartCoroutine(MudarParaIntro2());
    }

    private IEnumerator MudarParaIntro2()
    {
        mudandoDeCena = true;

        SceneManager.LoadScene("Intro2");

        yield return null;

        // Não fazemos absolutamente nada com a música.
        // Ela continua tocando.

        mudandoDeCena = false;
    }
}