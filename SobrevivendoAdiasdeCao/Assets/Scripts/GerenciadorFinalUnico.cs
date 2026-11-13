using UnityEngine;
using UnityEngine.SceneManagement;

public class GerenciadorFinalUnico : MonoBehaviour
{
    [Header("Objetos Visuais de Cada Final (Na mesma cena)")]
    public GameObject grupoFinalBom;
    public GameObject grupoFinalMedio;
    public GameObject grupoFinalRuim;

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
        // Garante que todos comecem desligados
        if (grupoFinalBom != null) grupoFinalBom.SetActive(false);
        if (grupoFinalMedio != null) grupoFinalMedio.SetActive(false);
        if (grupoFinalRuim != null) grupoFinalRuim.SetActive(false);

        // Fallback caso teste a cena diretamente sem passar pelas fases
        if (CommunityManager.instance == null)
        {
            if (grupoFinalMedio != null) grupoFinalMedio.SetActive(true);
            TocarMusica(musicaFinalMedio);
            return;
        }

        CommunityManager cm = CommunityManager.instance;
        float media = cm.MediaComunidade();
        bool temBarraCritica = cm.AlimentacaoCritica() || cm.SaudeCritica() || cm.FelicidadeCritica();

        // 1. FINAL RUIM: Média < 40 ou qualquer barra zerada
        if (media < 40f || cm.alimentacao <= 0f || cm.saude <= 0f || cm.felicidade <= 0f)
        {
            if (grupoFinalRuim != null) grupoFinalRuim.SetActive(true);
            TocarMusica(musicaFinalRuim);
        }
        // 2. FINAL BOM: Média >= 70 e nenhuma barra em estado crítico
        else if (media >= 70f && !temBarraCritica)
        {
            if (grupoFinalBom != null) grupoFinalBom.SetActive(true);
            TocarMusica(musicaFinalBom);
        }
        // 3. FINAL MÉDIO: Casos intermediários
        else
        {
            if (grupoFinalMedio != null) grupoFinalMedio.SetActive(true);
            TocarMusica(musicaFinalMedio);
        }
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