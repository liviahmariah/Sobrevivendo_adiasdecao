using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Itens")]
    public int itensColetados = 0;
    public int meta = 10;

    [Header("Tempo")]
    public float tempo = 60f;

    [Header("UI")]
    public TextMeshProUGUI textoItens;
    public TextMeshProUGUI textoTempo;

    public GameObject painelVitoria;
    public GameObject painelDerrota;

    [Header("Boss")]
    public GameObject carrocinha;

    public bool jogoAcabou = false;
    private bool bossLiberado = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        painelVitoria.SetActive(false);
        painelDerrota.SetActive(false);

        if (carrocinha != null)
            carrocinha.SetActive(false);

        AtualizarUI();
    }

    void Update()
    {
        if (jogoAcabou) return;

        if (!bossLiberado)
        {
            tempo -= Time.deltaTime;

            if (tempo <= 0)
            {
                tempo = 0;
                LiberarBoss();
            }

            AtualizarTempoUI();
        }
    }

    void AtualizarTempoUI()
    {
        int minutos = Mathf.FloorToInt(tempo / 60);
        int segundos = Mathf.FloorToInt(tempo % 60);
        int milesimos = Mathf.FloorToInt((tempo * 100) % 100);

        textoTempo.text = string.Format(
            "{0:00}:{1:00}:{2:00}",
            minutos,
            segundos,
            milesimos
        );

        // ÚLTIMOS 30 SEGUNDOS
        if (tempo <= 30)
        {
            textoTempo.color = Color.red;
        }

        // ÚLTIMOS 10 SEGUNDOS
        if (tempo <= 10)
        {
            float piscar = Mathf.PingPong(Time.time * 5f, 1f);

            textoTempo.color = Color.Lerp(
                Color.white,
                Color.red,
                piscar
            );
        }
    }

    public void Coletar(int valor)
    {
        if (jogoAcabou) return;

        itensColetados += valor;

        if (itensColetados < 0)
            itensColetados = 0;

        AtualizarUI();

        if (itensColetados >= meta)
        {
            Vitoria();
        }
    }

    void AtualizarUI()
    {
        textoItens.text = "Itens: " + itensColetados + "/" + meta;
    }

    void LiberarBoss()
    {
        bossLiberado = true;

        if (carrocinha != null)
            carrocinha.SetActive(true);

        textoTempo.text = "FUJA!";
    }

    public void Derrota()
    {
        jogoAcabou = true;

        painelDerrota.SetActive(true);

        Time.timeScale = 0f;
    }

    void Vitoria()
    {
        jogoAcabou = true;

        painelVitoria.SetActive(true);

        Time.timeScale = 0f;
    }
}