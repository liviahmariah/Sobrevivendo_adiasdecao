using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DayVisualManager : MonoBehaviour
{
    [Header("Overlay do Ambiente")]
    public Image overlayAmbiente;

    [Header("Horário")]
    public TextMeshProUGUI textoHorario;

    [Header("Configuração do Dia")]
    public float horaInicio = 6f;
    public float horaFim = 22f;

    [Header("Intensidade da Noite")]
    [Range(0f, 1f)]
    public float intensidadeNoite = 0.65f;

    [Header("Entardecer")]
    public float inicioEntardecer = 16f;
    public float fimEntardecer = 19f;

    [Header("Noite")]
    public float inicioNoite = 19f;

    [Header("Manhã")]
    public float inicioManha = 6f;
    public float fimManha = 8f;

    [Header("Transição entre Dias")]
    public Image overlayTransicao;

    public float duracaoFadeSaida = 0.8f;
    public float tempoTelaPreta = 0.8f;
    public float duracaoFadeEntrada = 1.2f;

    private int ultimoDia;
    private bool fazendoTransicao = false;

    private void Start()
    {
        if (DayManager.Instance != null)
        {
            ultimoDia = DayManager.Instance.diaAtual;
        }

        if (overlayTransicao != null)
        {
            Color cor = overlayTransicao.color;
            cor.a = 0f;
            overlayTransicao.color = cor;
        }

        AtualizarVisual();
    }

    private void Update()
    {
        if (DayManager.Instance == null)
            return;

        VerificarMudancaDeDia();

        if (!fazendoTransicao)
        {
            AtualizarVisual();
        }
    }

    private void VerificarMudancaDeDia()
    {
        int diaAtual = DayManager.Instance.diaAtual;

        if (diaAtual != ultimoDia)
        {
            ultimoDia = diaAtual;

            if (!fazendoTransicao)
            {
                StartCoroutine(TransicaoNovoDia());
            }
        }
    }

    private IEnumerator TransicaoNovoDia()
    {
        fazendoTransicao = true;

        // FADE OUT
        yield return StartCoroutine(
            AlterarAlpha(overlayTransicao, 0f, 1f, duracaoFadeSaida)
        );

        // Pequeno momento totalmente preto
        yield return new WaitForSeconds(tempoTelaPreta);

        // Atualiza imediatamente o visual para o novo horário
        AtualizarVisual();

        // FADE IN
        yield return StartCoroutine(
            AlterarAlpha(overlayTransicao, 1f, 0f, duracaoFadeEntrada)
        );

        fazendoTransicao = false;
    }

    private IEnumerator AlterarAlpha(
        Image imagem,
        float alphaInicial,
        float alphaFinal,
        float duracao
    )
    {
        if (imagem == null)
            yield break;

        float tempo = 0f;

        Color cor = imagem.color;
        cor.a = alphaInicial;
        imagem.color = cor;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;

            float porcentagem = tempo / duracao;

            // Suaviza a transição
            porcentagem = Mathf.SmoothStep(0f, 1f, porcentagem);

            cor = imagem.color;
            cor.a = Mathf.Lerp(alphaInicial, alphaFinal, porcentagem);
            imagem.color = cor;

            yield return null;
        }

        cor = imagem.color;
        cor.a = alphaFinal;
        imagem.color = cor;
    }

    private void AtualizarVisual()
    {
        if (DayManager.Instance == null)
            return;

        float tempoRestante = DayManager.Instance.ObterTempoRestante();
        float duracaoDia = DayManager.Instance.duracaoDia;

        // Converte o tempo restante para horário
        float progresso = 1f - (tempoRestante / duracaoDia);

        float horaAtual = Mathf.Lerp(
            horaInicio,
            horaFim,
            progresso
        );

        AtualizarHorario(horaAtual);
        AtualizarCorAmbiente(horaAtual);
    }

    private void AtualizarHorario(float hora)
    {
        if (textoHorario == null)
            return;

        int horas = Mathf.FloorToInt(hora);
        int minutos = Mathf.FloorToInt((hora - horas) * 60f);

        textoHorario.text = horas.ToString("00") + ":" + minutos.ToString("00");
    }

    private void AtualizarCorAmbiente(float hora)
    {
        if (overlayAmbiente == null)
            return;

        Color corFinal;
        float intensidade;

        // MANHÃ
        if (hora >= inicioManha && hora < fimManha)
        {
            float t = Mathf.InverseLerp(
                inicioManha,
                fimManha,
                hora
            );

            // Começa levemente alaranjado e vai ficando neutro
            corFinal = Color.Lerp(
                new Color(1f, 0.65f, 0.35f),
                Color.white,
                t
            );

            intensidade = Mathf.Lerp(0.18f, 0f, t);
        }

        // DIA
        else if (hora >= fimManha && hora < inicioEntardecer)
        {
            corFinal = Color.white;
            intensidade = 0f;
        }

        // ENTARDECER
        else if (hora >= inicioEntardecer && hora < fimEntardecer)
        {
            float t = Mathf.InverseLerp(
                inicioEntardecer,
                fimEntardecer,
                hora
            );

            // Dourado → laranja → vermelho suave
            corFinal = Color.Lerp(
                new Color(1f, 0.72f, 0.38f),
                new Color(0.95f, 0.40f, 0.20f),
                t
            );

            intensidade = Mathf.Lerp(0.15f, 0.35f, t);
        }

        // NOITE
        else if (hora >= inicioNoite)
        {
            float t = Mathf.InverseLerp(
                inicioNoite,
                horaFim,
                hora
            );

            // Laranja do fim da tarde → azul escuro
            corFinal = Color.Lerp(
                new Color(0.35f, 0.20f, 0.35f),
                new Color(0.05f, 0.08f, 0.22f),
                t
            );

            intensidade = Mathf.Lerp(
                0.35f,
                intensidadeNoite,
                t
            );
        }

        // Segurança
        else
        {
            corFinal = Color.white;
            intensidade = 0f;
        }

        corFinal.a = intensidade;

        overlayAmbiente.color = corFinal;
    }
}