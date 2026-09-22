
using UnityEngine;
using TMPro;

public class TutorialTimer : MonoBehaviour
{
    [Header("Configuração")]
    public float tempoInicial = 20f;

    [Header("UI")]
    public TextMeshProUGUI textoTimer;

    private float tempoAtual;

    private bool timerAtivo = false;

    public bool TempoAcabou { get; private set; }

    void Start()
    {
        tempoAtual = 0f;

        TempoAcabou = false;

        AtualizarUI();

        // O timer não começa automaticamente.
        // O TutorialManager vai iniciar.
    }

    void Update()
    {
        if (!timerAtivo)
            return;

        tempoAtual -= Time.deltaTime;

        if (tempoAtual <= 0f)
        {
            tempoAtual = 0f;

            timerAtivo = false;

            TempoAcabou = true;

            AtualizarUI();

            QuandoTempoAcabar();

            return;
        }

        AtualizarUI();
    }

    // =====================================================
    // INICIAR
    // =====================================================

    public void IniciarTimer()
    {
        tempoAtual = tempoInicial;

        TempoAcabou = false;

        timerAtivo = true;

        AtualizarUI();

        Debug.Log("⏱️ O chefe está fora! 20 segundos restantes.");
    }

    // =====================================================
    // PARAR
    // =====================================================

    public void PararTimer()
    {
        timerAtivo = false;
    }

    // =====================================================
    // REINICIAR
    // =====================================================

    public void ReiniciarTimer()
    {
        PararTimer();

        tempoAtual = 0f;

        TempoAcabou = false;

        AtualizarUI();
    }

    // =====================================================
    // UI
    // =====================================================

    void AtualizarUI()
    {
        if (textoTimer == null)
            return;

        int segundos = Mathf.CeilToInt(tempoAtual);

        textoTimer.text = segundos.ToString();
    }

    // =====================================================
    // TEMPO ACABOU
    // =====================================================

    void QuandoTempoAcabar()
    {
        Debug.Log("🚨 O CHEFE VOLTOU!");

        if (TutorialManager.instance != null)
        {
            TutorialManager.instance.IntervaloTerminou();
        }
    }
}