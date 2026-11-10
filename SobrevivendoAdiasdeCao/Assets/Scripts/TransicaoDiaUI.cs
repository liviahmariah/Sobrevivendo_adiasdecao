using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TransicaoDiaUI : MonoBehaviour
{
    [Header("Painel Principal (UI)")]
    [Tooltip("GameObject pai do painel de transição na UI.")]
    public GameObject painelTransicao;
    [Tooltip("Imagem escura de fundo para fazer o fade.")]
    public Image sobreposicaoEscura;

    [Header("Textos TMP")]
    public TMP_Text textoTitulo;
    public TMP_Text textoResumo;
    public TMP_Text textoProximoDia;

    [Header("Efeitos Sonoros")]
    public AudioSource audioSourceUI;
    public AudioClip somDigitacao;
    public AudioClip somNovoDia;

    [Header("Configurações de Tempo")]
    public float duracaoFade = 0.5f;
    public float velocidadeTexto = 0.035f;
    public float tempoExibicaoResumo = 2.5f;
    public float tempoExibicaoProximoDia = 2.0f;

    [Header("Títulos dos Dias (Dia 1 ao 15)")]
    public string[] titulosDosDias = new string[]
    {
        "Dia Inicial",
        "Primeira Etapa",
        "Novo Ciclo",
        "Avanço da Matilha",
        "Manhã Regular",
        "Atividades da Comunidade",
        "Nova Jornada",
        "Ponto Central",
        "Desenvolvimento",
        "Continuidade",
        "Nova Fase",
        "Manutenção",
        "Organização",
        "Etapa Final",
        "Encerramento"
    };

    private bool executando = false;

    private void Awake()
    {
        PrepararTelaInicial();
    }

    private void PrepararTelaInicial()
    {
        if (textoTitulo != null) textoTitulo.text = "";
        if (textoResumo != null) textoResumo.text = "";
        if (textoProximoDia != null) textoProximoDia.text = "";

        if (sobreposicaoEscura != null)
        {
            Color c = sobreposicaoEscura.color;
            c.a = 0f;
            sobreposicaoEscura.color = c;
        }

        if (painelTransicao != null)
            painelTransicao.SetActive(false);
    }

    public void IniciarTransicao(Action aoFinalizar = null)
    {
        if (executando) return;

        gameObject.SetActive(true);
        if (painelTransicao != null)
            painelTransicao.SetActive(true);

        StartCoroutine(ExecutarTransicaoSimplificada(aoFinalizar));
    }

    private IEnumerator ExecutarTransicaoSimplificada(Action aoFinalizar)
    {
        executando = true;

        // 1. CONGELA O JOGO E O ÁUDIO DO MAPA
        Time.timeScale = 0f;
        AudioListener.pause = true;

        // Fade-In da Imagem Escura de Fundo
        yield return StartCoroutine(FadeFundo(0f, 1f, duracaoFade));

        // ----------------------------------------------------
        // ETAPA 1: RESULTADOS DO DIA QUE ACABOU
        // ----------------------------------------------------
        int diaEncerrado = DayManager.Instance != null ? DayManager.Instance.diaAtual : 1;
        float mediaDia = DayManager.Instance != null ? DayManager.Instance.ObterMediaDoDia(diaEncerrado) : 100f;

        if (textoTitulo != null)
            textoTitulo.text = $"FIM DO DIA {diaEncerrado}";

        string resumoTexto = CriarTextoResumo(diaEncerrado, mediaDia);

        // Digita o resumo perfeitamente sincronizado
        yield return StartCoroutine(EfeitoDigitarTexto(textoResumo, resumoTexto));

        // Aguarda a leitura do resumo
        yield return new WaitForSecondsRealtime(tempoExibicaoResumo);

        // Limpa tela do resumo
        if (textoTitulo != null) textoTitulo.text = "";
        if (textoResumo != null) textoResumo.text = "";

        // ----------------------------------------------------
        // ETAPA 2: ANÚNCIO DO NOVO DIA (Dia X: Título)
        // ----------------------------------------------------
        int proximoDia = diaEncerrado + 1;
        string tituloDia = ObterTituloDoDia(proximoDia);
        string mensagemProximoDia = $"Dia {proximoDia}: {tituloDia}";

        if (audioSourceUI != null && somNovoDia != null)
            audioSourceUI.PlayOneShot(somNovoDia);

        yield return StartCoroutine(EfeitoDigitarTexto(textoProximoDia, mensagemProximoDia));

        yield return new WaitForSecondsRealtime(tempoExibicaoProximoDia);

        // ----------------------------------------------------
        // ETAPA 3: FIM DA TRANSIÇÃO E DESCONGELAMENTO
        // ----------------------------------------------------
        yield return StartCoroutine(FadeFundo(1f, 0f, duracaoFade));

        if (textoProximoDia != null) textoProximoDia.text = "";

        if (painelTransicao != null)
            painelTransicao.SetActive(false);

        // Descongela o jogo e o áudio da cena
        AudioListener.pause = false;
        Time.timeScale = 1f;

        executando = false;

        // Notifica o DayManager para avançar o dia
        aoFinalizar?.Invoke();

        gameObject.SetActive(false);
    }

    private string CriarTextoResumo(int dia, float media)
    {
        if (CommunityManager.instance == null)
            return $"Média da Comunidade: {media:F0}%";

        var c = CommunityManager.instance;

        return $"Alimentação: {c.alimentacao:F0}%\n" +
               $"Saúde: {c.saude:F0}%\n" +
               $"Felicidade: {c.felicidade:F0}%\n\n" +
               $"MÉDIA GERAL: {media:F0}%";
    }

    private string ObterTituloDoDia(int dia)
    {
        int index = dia - 1;

        if (index >= 0 && index < titulosDosDias.Length)
            return titulosDosDias[index];

        return "Uma Nova Jornada";
    }

    // =====================================================
    // EFEITO DE DIGITAÇÃO INSTANTÂNEO E 100% SINCRONIZADO
    // =====================================================
    private IEnumerator EfeitoDigitarTexto(TMP_Text componenteTexto, string textoCompleto)
    {
        if (componenteTexto == null) yield break;

        // Limpa o texto e garante que a opacidade esteja visível
        componenteTexto.text = "";
        componenteTexto.maxVisibleCharacters = int.MaxValue;

        for (int i = 0; i < textoCompleto.Length; i++)
        {
            char caractereAtual = textoCompleto[i];

            // Adiciona a letra/símbolo atual no componente TMP
            componenteTexto.text += caractereAtual;

            // Se for uma letra ou número, toca o som IMEDIATAMENTE no mesmo frame
            if (char.IsLetterOrDigit(caractereAtual))
            {
                if (audioSourceUI != null && somDigitacao != null)
                {
                    audioSourceUI.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                    audioSourceUI.PlayOneShot(somDigitacao, 0.4f);
                }
            }

            // Aguarda o tempo exato em tempo real (sem congelar pela pausa da cena)
            yield return new WaitForSecondsRealtime(velocidadeTexto);
        }
    }

    private IEnumerator FadeFundo(float inicio, float fim, float duracao)
    {
        if (sobreposicaoEscura == null) yield break;

        Color cor = sobreposicaoEscura.color;
        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.unscaledDeltaTime;
            float progresso = Mathf.Clamp01(tempo / duracao);
            cor.a = Mathf.Lerp(inicio, fim, Mathf.SmoothStep(0f, 1f, progresso));
            sobreposicaoEscura.color = cor;
            yield return null;
        }

        cor.a = fim;
        sobreposicaoEscura.color = cor;
    }
}