using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class GerenciadorFinalUnico : MonoBehaviour
{
    [Header("Objetos Visuais de Cada Final (Opcional)")]
    public GameObject grupoFinalBom;
    public GameObject grupoFinalMedio;
    public GameObject grupoFinalRuim;

    [Header("Animações dos Finais (Em Looping)")]
    public Animator animatorFinais;
    public AnimationClip animacaoFinalBom;
    public AnimationClip animacaoFinalMedio;
    public AnimationClip animacaoFinalRuim;

    [Header("Ou Timelines de Cada Final (Em Looping - Opcional)")]
    public PlayableDirector timelineFinalBom;
    public PlayableDirector timelineFinalMedio;
    public PlayableDirector timelineFinalRuim;

    [Header("Músicas Específicas (Opcional)")]
    public AudioClip musicaFinalBom;
    public AudioClip musicaFinalMedio;
    public AudioClip musicaFinalRuim;

    [Header("Transição para a Cena Extra")]
    public string cenaExtra = "CenaExtra";
    public KeyCode teclaAvancar = KeyCode.Space;
    [Tooltip("Tempo mínimo antes de aceitar a tecla Espaço (evita pular a cena sem querer).")]
    public float atrasoLiberacaoEspaco = 1.5f;
    public GameObject avisoPressioneEspaco;

    private float cronometro;
    private bool podeAvancar = false;
    private bool jaCarregou = false;

    private void Start()
    {
        Time.timeScale = 1f;
        cronometro = atrasoLiberacaoEspaco;

        if (avisoPressioneEspaco != null)
            avisoPressioneEspaco.SetActive(false);

        AtivarFinalCorreto();
    }

    private void AtivarFinalCorreto()
    {
        if (grupoFinalBom != null) grupoFinalBom.SetActive(false);
        if (grupoFinalMedio != null) grupoFinalMedio.SetActive(false);
        if (grupoFinalRuim != null) grupoFinalRuim.SetActive(false);

        if (timelineFinalBom != null) timelineFinalBom.Stop();
        if (timelineFinalMedio != null) timelineFinalMedio.Stop();
        if (timelineFinalRuim != null) timelineFinalRuim.Stop();

        if (CommunityManager.instance == null)
        {
            ExecutarFinal(grupoFinalMedio, animacaoFinalMedio, timelineFinalMedio, musicaFinalMedio);
            return;
        }

        CommunityManager cm = CommunityManager.instance;
        float media = cm.MediaComunidade();
        bool temBarraCritica = cm.AlimentacaoCritica() || cm.SaudeCritica() || cm.FelicidadeCritica();

        // 1. FINAL RUIM: Média < 40 ou qualquer barra zerada
        if (media < 40f || cm.alimentacao <= 0f || cm.saude <= 0f || cm.felicidade <= 0f)
        {
            ExecutarFinal(grupoFinalRuim, animacaoFinalRuim, timelineFinalRuim, musicaFinalRuim);
        }
        // 2. FINAL BOM: Média >= 70 e nenhuma barra em estado crítico
        else if (media >= 70f && !temBarraCritica)
        {
            ExecutarFinal(grupoFinalBom, animacaoFinalBom, timelineFinalBom, musicaFinalBom);
        }
        // 3. FINAL MÉDIO: Casos intermediários
        else
        {
            ExecutarFinal(grupoFinalMedio, animacaoFinalMedio, timelineFinalMedio, musicaFinalMedio);
        }
    }

    private void ExecutarFinal(
        GameObject grupoVisual,
        AnimationClip clipeAnimacao,
        PlayableDirector timeline,
        AudioClip musica)
    {
        if (grupoVisual != null)
            grupoVisual.SetActive(true);

        if (clipeAnimacao != null)
            clipeAnimacao.wrapMode = WrapMode.Loop;

        if (animatorFinais != null && clipeAnimacao != null)
        {
            animatorFinais.gameObject.SetActive(true);
            animatorFinais.speed = 1f;
            animatorFinais.Play(clipeAnimacao.name, 0, 0f);
        }
        else if (grupoVisual != null && clipeAnimacao != null)
        {
            Animator animDoGrupo = grupoVisual.GetComponentInChildren<Animator>();
            if (animDoGrupo != null)
            {
                animDoGrupo.speed = 1f;
                animDoGrupo.Play(clipeAnimacao.name, 0, 0f);
            }
        }

        if (timeline != null)
        {
            timeline.gameObject.SetActive(true);
            timeline.extrapolationMode = DirectorWrapMode.Loop;
            timeline.Play();
        }

        TocarMusica(musica);
    }

    private void TocarMusica(AudioClip clip)
    {
        if (MusicManager.instance != null && clip != null)
        {
            MusicManager.instance.TocarMusicaSuave(clip);
        }
    }

    private void Update()
    {
        if (jaCarregou)
            return;

        if (!podeAvancar)
        {
            cronometro -= Time.deltaTime;
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
            SceneManager.LoadScene(cenaExtra);
        }
    }
}