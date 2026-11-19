using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GerenciadorFinalUnico : MonoBehaviour
{
    [Header("Imagens / Grupos de Cada Final")]
    public GameObject grupoFinalBom;
    public GameObject grupoFinalMedio;
    public GameObject grupoFinalRuim;

    [Header("Fade Suave de Entrada e Saída")]
    [Tooltip("Arraste aqui o CanvasGroup de uma Image preta que cobre 100% da tela.")]
    public CanvasGroup painelFade;
    public float duracaoFadeEntrada = 1.5f;
    public float duracaoFadeSaida = 1.5f;

    [Header("Músicas Específicas")]
    public AudioSource audioSourceFinal;
    public AudioClip musicaFinalBom;
    public AudioClip musicaFinalMedio;
    public AudioClip musicaFinalRuim;
    [Range(0f, 1f)]
    public float volumeMusica = 1f;

    [Header("Transição para a Cena Extra")]
    public string cenaExtra = "CenaExtra";
    public KeyCode teclaAvancar = KeyCode.Space;
    [Tooltip("Tempo mínimo antes de aceitar a tecla Espaço (evita pular a cena sem querer).")]
    public float atrasoLiberacaoEspaco = 1.5f;
    public GameObject avisoPressioneEspaco;

    private float cronometro;
    private bool podeAvancar = false;
    private bool jaCarregou = false;

    private void Awake()
    {
        if (audioSourceFinal == null)
            audioSourceFinal = GetComponent<AudioSource>();

        if (audioSourceFinal == null)
            audioSourceFinal = gameObject.AddComponent<AudioSource>();

        // Garante áudio 2D audível em qualquer posição da cena
        audioSourceFinal.playOnAwake = false;
        audioSourceFinal.loop = true;
        audioSourceFinal.spatialBlend = 0f;
        audioSourceFinal.volume = volumeMusica;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        cronometro = atrasoLiberacaoEspaco;

        if (avisoPressioneEspaco != null)
            avisoPressioneEspaco.SetActive(false);

        AtivarFinalCorreto();
        StartCoroutine(RotinaFadeEntrada());
    }

    private void AtivarFinalCorreto()
    {
        if (grupoFinalBom != null) grupoFinalBom.SetActive(false);
        if (grupoFinalMedio != null) grupoFinalMedio.SetActive(false);
        if (grupoFinalRuim != null) grupoFinalRuim.SetActive(false);

        if (CommunityManager.instance == null)
        {
            ExecutarFinal(grupoFinalBom, musicaFinalBom);
            return;
        }

        CommunityManager cm = CommunityManager.instance;
        float media = cm.MediaComunidade();
        bool temBarraCritica = cm.AlimentacaoCritica() || cm.SaudeCritica() || cm.FelicidadeCritica();

        // 1. FINAL RUIM: Média < 40 ou qualquer barra zerada
        if (media < 40f || cm.alimentacao <= 0f || cm.saude <= 0f || cm.felicidade <= 0f)
        {
            ExecutarFinal(grupoFinalRuim, musicaFinalRuim);
        }
        // 2. FINAL BOM: Média >= 70 e nenhuma barra em estado crítico
        else if (media >= 70f && !temBarraCritica)
        {
            ExecutarFinal(grupoFinalBom, musicaFinalBom);
        }
        // 3. FINAL MÉDIO: Casos intermediários
        else
        {
            ExecutarFinal(grupoFinalMedio, musicaFinalMedio);
        }
    }

    private void ExecutarFinal(GameObject grupoVisual, AudioClip musica)
    {
        if (grupoVisual != null)
            grupoVisual.SetActive(true);

        TocarMusica(musica);
    }

    private void TocarMusica(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[GerenciadorFinalUnico] Nenhum AudioClip de música foi atribuído no Inspector!");
            return;
        }

        // Toca pelo AudioSource local para garantir som mesmo sem MusicManager ou se ele estiver pausado
        if (MusicManager.instance != null)
        {
            MusicManager.instance.TocarMusicaSuave(clip);
        }

        if (audioSourceFinal != null)
        {
            // Se o MusicManager não estiver tocando esse clipe, toca localmente
            AudioSource fonteMusicManager = MusicManager.instance != null
                ? MusicManager.instance.GetComponent<AudioSource>()
                : null;

            if (fonteMusicManager == null || !fonteMusicManager.isPlaying || fonteMusicManager.clip != clip)
            {
                audioSourceFinal.clip = clip;
                audioSourceFinal.volume = volumeMusica;
                audioSourceFinal.loop = true;
                audioSourceFinal.Play();
            }
        }
    }

    private IEnumerator RotinaFadeEntrada()
    {
        if (painelFade == null)
            yield break;

        painelFade.gameObject.SetActive(true);
        painelFade.blocksRaycasts = true;
        painelFade.alpha = 1f;

        float tempo = 0f;
        float duracao = Mathf.Max(0.01f, duracaoFadeEntrada);

        while (tempo < duracao)
        {
            tempo += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, tempo / duracao);
            painelFade.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        painelFade.alpha = 0f;
        painelFade.blocksRaycasts = false;
    }

    private IEnumerator RotinaFadeSaidaECarregar()
    {
        if (painelFade != null)
        {
            painelFade.gameObject.SetActive(true);
            painelFade.blocksRaycasts = true;

            float tempo = 0f;
            float duracao = Mathf.Max(0.01f, duracaoFadeSaida);
            float alphaInicial = painelFade.alpha;
            float volumeInicial = audioSourceFinal != null ? audioSourceFinal.volume : volumeMusica;

            while (tempo < duracao)
            {
                tempo += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, tempo / duracao);
                painelFade.alpha = Mathf.Lerp(alphaInicial, 1f, t);

                if (audioSourceFinal != null && audioSourceFinal.isPlaying)
                    audioSourceFinal.volume = Mathf.Lerp(volumeInicial, 0f, t);

                yield return null;
            }

            painelFade.alpha = 1f;
        }

        SceneManager.LoadScene(cenaExtra);
    }

    private void Update()
    {
        if (jaCarregou)
            return;

        if (!podeAvancar)
        {
            cronometro -= Time.unscaledDeltaTime;
            if (cronometro <= 0f)
            {
                podeAvancar = true;
                if (avisoPressioneEspaco != null)
                    avisoPressioneEspaco.SetActive(true);
            }
            return;
        }

        if (Input.GetKeyDown(teclaAvancar))
        {
            jaCarregou = true;
            StartCoroutine(RotinaFadeSaidaECarregar());
        }
    }
}