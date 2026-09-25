using UnityEngine;
using TMPro;

public class TutorialTimer : MonoBehaviour
{
    [Header("Configuração inicial")]
    public float tempoInicial = 20f;

    [Header("Interface")]
    public TextMeshProUGUI textoEstado;
    public TextMeshProUGUI textoTimer;

    private float tempoAtual;
    private bool timerAtivo = false;
    private bool tempoAcabou = false;

    public bool TempoAcabou
    {
        get { return tempoAcabou; }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!timerAtivo)
            return;

        tempoAtual -= Time.deltaTime;

        if (tempoAtual <= 0f)
        {
            tempoAtual = 0f;

            timerAtivo = false;
            tempoAcabou = true;

            AtualizarTimerUI();

            Debug.Log(
                "Intervalo terminou. " +
                "A carrocinha voltou!"
            );

            if (TutorialManager.instance != null)
            {
                TutorialManager.instance.IntervaloTerminou();
            }

            return;
        }

        AtualizarTimerUI();
    }


    // =========================================================
    // TIMER INICIAL
    // =========================================================

    public void IniciarTimer()
    {
        tempoAtual = tempoInicial;

        timerAtivo = true;
        tempoAcabou = false;

        if (textoEstado != null)
        {
            textoEstado.text = "CHEFE RETORNA EM";
        }

        AtualizarTimerUI();

        Debug.Log(
            "Timer inicial iniciado: " +
            tempoInicial +
            " segundos."
        );
    }


    // =========================================================
    // PARAR TIMER
    // =========================================================

    public void PararTimer()
    {
        timerAtivo = false;
    }


    // =========================================================
    // ATUALIZAR TIMER
    // =========================================================

    private void AtualizarTimerUI()
    {
        if (textoTimer == null)
            return;

        int segundos =
            Mathf.Max(
                0,
                Mathf.CeilToInt(tempoAtual)
            );

        int minutos =
            segundos / 60;

        int segundosRestantes =
            segundos % 60;

        textoTimer.text =
            minutos.ToString("00") +
            ":" +
            segundosRestantes.ToString("00");
    }


    // =========================================================
    // TIMER DA CARROCINHA
    // =========================================================

    public void MostrarTimerChefe(
        string mensagem,
        float tempo
    )
    {
        // O timer inicial deixa de controlar a contagem.

        timerAtivo = false;

        if (textoEstado != null)
        {
            textoEstado.text = mensagem;
        }

        int segundos =
            Mathf.Max(
                0,
                Mathf.CeilToInt(tempo)
            );

        int minutos =
            segundos / 60;

        int segundosRestantes =
            segundos % 60;

        if (textoTimer != null)
        {
            textoTimer.text =
                minutos.ToString("00") +
                ":" +
                segundosRestantes.ToString("00");
        }
    }
}