
using UnityEngine;
using TMPro;

public class TutorialTimer : MonoBehaviour
{
    [Header("Configuração inicial")]
    public float tempoInicial = 20f;

    [Header("Interface")]
    public TextMeshProUGUI textoEstado;
    public TextMeshProUGUI textoTimer;

    [Header("Piscada do timer")]
    [Tooltip("Começa a piscar quando restarem estes segundos.")]
    public float inicioPiscada = 10f;

    [Tooltip("Intervalo entre piscadas antes dos últimos 5 segundos.")]
    public float intervaloPiscada = 0.5f;

    [Tooltip("Intervalo entre piscadas nos últimos 5 segundos.")]
    public float intervaloPiscadaUrgente = 0.15f;

    [Header("Som da contagem")]
    public AudioSource audioSourceBip;
    public AudioClip somBip;

    [Range(0f, 1f)]
    public float volumeMinimo = 0.15f;

    [Range(0f, 1f)]
    public float volumeMaximo = 1f;

    [Tooltip("A partir deste tempo, o bip começa a aumentar de volume.")]
    public float tempoParaVolumeMaximo = 10f;

    private float tempoAtual;

    private bool timerAtivo = false;
    private bool tempoAcabou = false;
    private bool modoTimerChefe = false;

    private int ultimoSegundoExibido = -1;
    private bool possuiSegundoAnterior = false;

    private float tempoParaEfeito = 0f;
    private bool efeitosAtivos = false;

    private float proximaPiscada = 0f;
    private bool textoVisivel = true;

    private Color corOriginal = Color.white;
    private bool corOriginalSalva = false;

    public bool TempoAcabou
    {
        get { return tempoAcabou; }
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (textoTimer != null)
        {
            corOriginal = textoTimer.color;
            corOriginalSalva = true;
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // Atualiza a contagem regressiva inicial.
        if (timerAtivo)
        {
            tempoAtual -= Time.deltaTime;

            if (tempoAtual <= 0f)
            {
                tempoAtual = 0f;

                timerAtivo = false;
                tempoAcabou = true;

                AtualizarTimerUI(tempoAtual);

                Debug.Log(
                    "Intervalo terminou. A carrocinha voltou!"
                );

                if (TutorialManager.instance != null)
                {
                    TutorialManager.instance.IntervaloTerminou();
                }
            }
            else
            {
                AtualizarTimerUI(tempoAtual);
            }
        }

        // A piscada continua funcionando também quando
        // a contagem é controlada pela carrocinha.
        AtualizarPiscada();
    }


    // =========================================================
    // TIMER INICIAL
    // =========================================================

    public void IniciarTimer()
    {
        tempoAtual = tempoInicial;

        timerAtivo = true;
        tempoAcabou = false;
        modoTimerChefe = false;

        ReiniciarEfeitos();

        if (textoEstado != null)
        {
            textoEstado.text = "CHEFE RETORNA EM";
        }

        AtualizarTimerUI(tempoAtual);

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
        efeitosAtivos = false;

        RestaurarTexto();
    }


    // =========================================================
    // TIMER DA CARROCINHA
    // =========================================================

    public void MostrarTimerChefe(
        string mensagem,
        float tempo
    )
    {
        timerAtivo = false;

        // Detecta a troca do timer inicial para o timer
        // controlado pelo ciclo da carrocinha.
        if (!modoTimerChefe)
        {
            modoTimerChefe = true;
            ReiniciarEfeitos();
        }

        if (textoEstado != null)
        {
            textoEstado.text = mensagem;
        }

        AtualizarTimerUI(tempo);
    }


    // =========================================================
    // ATUALIZAR NÚMERO, SOM E TEMPO DOS EFEITOS
    // =========================================================

    private void AtualizarTimerUI(float tempo)
    {
        if (textoTimer == null)
            return;

        tempo = Mathf.Max(0f, tempo);

        int segundos = Mathf.Max(
            0,
            Mathf.CeilToInt(tempo)
        );

        // Formato fixo: MM:SS.
        int minutos = segundos / 60;
        int segundosRestantes = segundos % 60;

        textoTimer.text =
            minutos.ToString("00") +
            ":" +
            segundosRestantes.ToString("00");

        // Atualiza o som somente quando muda o número.
        if (
            !possuiSegundoAnterior ||
            segundos != ultimoSegundoExibido
        )
        {
            if (
                possuiSegundoAnterior &&
                segundos < ultimoSegundoExibido
            )
            {
                TocarBip(tempo);
            }

            ultimoSegundoExibido = segundos;
            possuiSegundoAnterior = true;
        }

        // Atualiza os efeitos visuais.
        tempoParaEfeito = tempo;
        efeitosAtivos = tempo > 0f;

        if (!efeitosAtivos)
        {
            RestaurarTexto();
        }
    }


    // =========================================================
    // SOM PROGRESSIVO
    // =========================================================

    private void TocarBip(float tempoRestante)
    {
        if (audioSourceBip == null || somBip == null)
            return;

        float duracaoVolume = Mathf.Max(
            0.01f,
            tempoParaVolumeMaximo
        );

        // O volume aumenta durante a aproximação do fim.
        float progresso = 1f - Mathf.Clamp01(
            tempoRestante / duracaoVolume
        );

        float volume = Mathf.Lerp(
            volumeMinimo,
            volumeMaximo,
            progresso
        );

        audioSourceBip.PlayOneShot(
            somBip,
            volume
        );
    }


    // =========================================================
    // PISCADA DO TIMER
    // =========================================================

    private void AtualizarPiscada()
    {
        if (textoTimer == null)
            return;

        if (!efeitosAtivos || tempoParaEfeito > inicioPiscada)
        {
            RestaurarTexto();
            return;
        }

        // A piscada acelera nos últimos 5 segundos.
        float intervalo = tempoParaEfeito <= 5f
            ? intervaloPiscadaUrgente
            : intervaloPiscada;

        intervalo = Mathf.Max(0.05f, intervalo);

        proximaPiscada -= Time.deltaTime;

        if (proximaPiscada <= 0f)
        {
            textoVisivel = !textoVisivel;
            proximaPiscada = intervalo;

            Color cor = corOriginalSalva
                ? corOriginal
                : textoTimer.color;

            cor.a = textoVisivel ? 1f : 0f;

            textoTimer.color = cor;
        }
    }


    // =========================================================
    // REINICIAR EFEITOS
    // =========================================================

    private void ReiniciarEfeitos()
    {
        ultimoSegundoExibido = -1;
        possuiSegundoAnterior = false;

        tempoParaEfeito = 0f;
        efeitosAtivos = false;

        proximaPiscada = 0f;
        textoVisivel = true;

        RestaurarTexto();
    }


    // =========================================================
    // RESTAURAR APARÊNCIA ORIGINAL
    // =========================================================

    private void RestaurarTexto()
    {
        if (textoTimer == null)
            return;

        if (corOriginalSalva)
        {
            textoTimer.color = corOriginal;
        }

        textoVisivel = true;
        proximaPiscada = 0f;
    }
}
