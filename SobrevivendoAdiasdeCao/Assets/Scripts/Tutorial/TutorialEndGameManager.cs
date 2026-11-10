using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class TutorialEndGameManager : MonoBehaviour
{
    public static TutorialEndGameManager instance;

    // =====================================================
    // PAINÉIS DE TELA
    // =====================================================
    [Header("Painéis de Fim de Jogo")]
    [SerializeField] private GameObject painelVitoria;
    [SerializeField] private TextMeshProUGUI textoVitoria;
    [SerializeField] private GameObject painelDerrota;
    [SerializeField] private TextMeshProUGUI textoDerrota;

    [Header("Aviso de Continuidade (Vitória)")]
    [Tooltip("Texto informando 'Aperte ESPAÇO para continuar'")]
    [SerializeField] private TextMeshProUGUI textoAperteEspaco;
    [Tooltip("Nome exato da cena do Mapa no Build Settings.")]
    [SerializeField] private string nomeCenaMapa = "Mapa";

    // =====================================================
    // IMAGEM ANIMADA DA VITÓRIA
    // =====================================================
    [Header("Imagem Animada da Vitória")]
    [SerializeField] private RectTransform imagemVitoriaAnimada;
    [SerializeField] private float distanciaEntradaVitoria = 1920f;
    [SerializeField] private float duracaoEntradaVitoria = 0.9f;
    [SerializeField] private float tempoImagemVitoria = 0.5f;

    private Vector2 posicaoFinalVitoria;
    private Vector2 posicaoInicialVitoria;
    private Coroutine rotinaVitoria;
    private bool aguardandoAvancoMapa = false;

    // =====================================================
    // SONS E MÚSICAS
    // =====================================================
    [Header("Sons da Vitória")]
    [SerializeField] private AudioSource audioSourceVitoria;
    [SerializeField] private AudioClip somEntradaVitoria;
    [SerializeField] private AudioClip somConfetesVitoria;

    [Range(0f, 1f)][SerializeField] private float volumeEntradaVitoria = 0.8f;
    [Range(0f, 1f)][SerializeField] private float volumeConfetesVitoria = 1f;

    [Header("Músicas de Vitória e Derrota")]
    [SerializeField] private AudioSource audioSourceMusicaFinal;
    [SerializeField] private AudioClip musicaVitoria;
    [SerializeField] private AudioClip musicaDerrota;

    [Range(0f, 1f)][SerializeField] private float volumeMusicaVitoria = 0.7f;
    [Range(0f, 1f)][SerializeField] private float volumeMusicaDerrota = 0.7f;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        ConfigurarMusicaFinal();
    }

    private void Start()
    {
        if (painelVitoria != null) painelVitoria.SetActive(false);
        if (painelDerrota != null) painelDerrota.SetActive(false);
        if (textoAperteEspaco != null) textoAperteEspaco.gameObject.SetActive(false);

        if (imagemVitoriaAnimada != null)
        {
            posicaoFinalVitoria = imagemVitoriaAnimada.anchoredPosition;
            posicaoInicialVitoria = posicaoFinalVitoria + Vector2.left * distanciaEntradaVitoria;
            imagemVitoriaAnimada.anchoredPosition = posicaoInicialVitoria;
            imagemVitoriaAnimada.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // Ao vencer, escuta o pressionamento da tecla Espaço para ir ao Mapa
        if (aguardandoAvancoMapa && Input.GetKeyDown(KeyCode.Space))
        {
            CarregarCenaMapa();
        }
    }

    // =====================================================
    // MÚSICAS E SONS
    // =====================================================
    private void ConfigurarMusicaFinal()
    {
        if (audioSourceMusicaFinal == null)
        {
            audioSourceMusicaFinal = GetComponent<AudioSource>();
            if (audioSourceMusicaFinal == null)
                audioSourceMusicaFinal = gameObject.AddComponent<AudioSource>();
        }

        audioSourceMusicaFinal.playOnAwake = false;
        audioSourceMusicaFinal.spatialBlend = 0f;
        audioSourceMusicaFinal.Stop();
    }

    private void TocarMusicaFinal(AudioClip musica, float volume, bool emLoop = false)
    {
        if (audioSourceMusicaFinal == null || musica == null) return;
        audioSourceMusicaFinal.Stop();
        audioSourceMusicaFinal.clip = musica;
        audioSourceMusicaFinal.volume = volume;
        audioSourceMusicaFinal.loop = emLoop;
        audioSourceMusicaFinal.Play();
    }

    public void PararMusicaFinal()
    {
        if (audioSourceMusicaFinal != null)
        {
            audioSourceMusicaFinal.Stop();
            audioSourceMusicaFinal.clip = null;
        }
    }

    private void TocarSomVitoria(AudioClip clip, float volume)
    {
        if (audioSourceVitoria != null && clip != null)
        {
            audioSourceVitoria.PlayOneShot(clip, volume);
        }
    }

    // =====================================================
    // EXECUTAR VITÓRIA
    // =====================================================
    public void ExecutarVitoria()
    {
        aguardandoAvancoMapa = false;
        PararMusicaFinal();
        TocarMusicaFinal(musicaVitoria, volumeMusicaVitoria, false); // Toca 1 vez

        if (painelVitoria != null) painelVitoria.SetActive(true);
        if (painelDerrota != null) painelDerrota.SetActive(false);
        if (imagemVitoriaAnimada != null) imagemVitoriaAnimada.gameObject.SetActive(true);
        if (textoAperteEspaco != null) textoAperteEspaco.gameObject.SetActive(false);

        if (rotinaVitoria != null) StopCoroutine(rotinaVitoria);
        rotinaVitoria = StartCoroutine(AnimarSequenciaVitoria());
    }

    private IEnumerator AnimarSequenciaVitoria()
    {
        if (imagemVitoriaAnimada == null) yield break;

        imagemVitoriaAnimada.anchoredPosition = posicaoInicialVitoria;
        TocarSomVitoria(somEntradaVitoria, volumeEntradaVitoria);

        float duracao = Mathf.Max(0.01f, duracaoEntradaVitoria);
        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.unscaledDeltaTime;
            float progresso = Mathf.Clamp01(tempo / duracao);
            float suavizado = Mathf.SmoothStep(0f, 1f, progresso);

            imagemVitoriaAnimada.anchoredPosition = Vector2.Lerp(posicaoInicialVitoria, posicaoFinalVitoria, suavizado);
            yield return null;
        }

        imagemVitoriaAnimada.anchoredPosition = posicaoFinalVitoria;

        if (tempoImagemVitoria > 0f)
            yield return new WaitForSecondsRealtime(tempoImagemVitoria);

        // Solta os confetes em UI
        if (ConfetiUIManager.instance != null)
        {
            ConfetiUIManager.instance.SoltarConfetes();
        }
        TocarSomVitoria(somConfetesVitoria, volumeConfetesVitoria);

        // Exibe o texto e habilita o atalho da tecla Espaço
        if (textoAperteEspaco != null)
        {
            textoAperteEspaco.text = "Aperte ESPAÇO para continuar";
            textoAperteEspaco.gameObject.SetActive(true);
        }

        aguardandoAvancoMapa = true;
        rotinaVitoria = null;
    }

    // =====================================================
    // CARREGAR CENA DO MAPA
    // =====================================================
    public void CarregarCenaMapa()
    {
        aguardandoAvancoMapa = false;
        Time.timeScale = 1f;
        PararMusicaFinal();

        // Garanta que ao ir ao mapa, um futuro tutorial não comece no modo rápido
        TutorialManager.ehReinicioRapido = false;

        SceneManager.LoadScene(nomeCenaMapa);
    }

    // =====================================================
    // EXECUTAR DERROTA
    // =====================================================
    public void ExecutarDerrota()
    {
        aguardandoAvancoMapa = false;
        PararMusicaFinal();
        TocarMusicaFinal(musicaDerrota, volumeMusicaDerrota, false);

        if (rotinaVitoria != null)
        {
            StopCoroutine(rotinaVitoria);
            rotinaVitoria = null;
        }

        if (imagemVitoriaAnimada != null) imagemVitoriaAnimada.gameObject.SetActive(false);
        if (textoAperteEspaco != null) textoAperteEspaco.gameObject.SetActive(false);
        if (painelDerrota != null) painelDerrota.SetActive(true);

        if (textoDerrota != null)
        {
            textoDerrota.text = "A FUGA FALHOU\nSandy ficou sem energia.";
        }
    }

    // =====================================================
    // REINICIAR TUTORIAL (RETRY)
    // =====================================================
    public void ReiniciarTutorial()
    {
        aguardandoAvancoMapa = false;
        Time.timeScale = 1f;
        PararMusicaFinal();

        TutorialManager.ehReinicioRapido = true;
        Scene cenaAtual = SceneManager.GetActiveScene();
        SceneManager.LoadScene(cenaAtual.name);
    }
}