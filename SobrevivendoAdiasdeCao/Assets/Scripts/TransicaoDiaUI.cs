
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TransicaoDiaUI : MonoBehaviour
{
    [Header("Imagens")]
    public Image imagemSol;
    public Image imagemLua;
    public Image sobreposicaoEscura;

    [Header("Painel de informações")]
    public CanvasGroup grupoInformacoes;
    public TMP_Text textoTitulo;
    public TMP_Text textoResumo;

    [Header("Tempos dos fades")]
    public float duracaoFade = 2f;

    [Header("Tempo de exibição")]
    public float tempoTelaNoite = 4f;
    public float tempoTelaAmanha = 4f;

    [Header("Escurecimento noturno")]
    [Range(0f, 1f)]
    public float opacidadeNoite = 0.65f;

    private Coroutine animacaoAtual;
    private bool executando;

    private Color corSol;
    private Color corLua;
    private Color corSobreposicao;

    private void Awake()
    {
        if (imagemSol != null)
            corSol = imagemSol.color;

        if (imagemLua != null)
            corLua = imagemLua.color;

        if (sobreposicaoEscura != null)
            corSobreposicao = sobreposicaoEscura.color;

        PrepararTelaInicial();
    }

    private void PrepararTelaInicial()
    {
        DefinirAlphaImagem(imagemSol, 0f);
        DefinirAlphaImagem(imagemLua, 0f);
        DefinirAlphaEscurecimento(0f);

        if (grupoInformacoes != null)
            grupoInformacoes.alpha = 0f;

        if (imagemSol != null)
            imagemSol.gameObject.SetActive(false);

        if (imagemLua != null)
            imagemLua.gameObject.SetActive(false);

        if (grupoInformacoes != null)
            grupoInformacoes.gameObject.SetActive(false);
    }

    public void IniciarTransicao(Action aoFinalizar = null)
    {
        if (executando)
        {
            Debug.LogWarning(
                "TransicaoDiaUI: uma transição já está em andamento."
            );
            return;
        }

        // Ativar antes de iniciar a Coroutine.
        gameObject.SetActive(true);

        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError(
                "TransicaoDiaUI: o objeto ou algum pai está inativo."
            );
            return;
        }

        animacaoAtual = StartCoroutine(
            ExecutarTransicao(aoFinalizar)
        );
    }

    private IEnumerator ExecutarTransicao(Action aoFinalizar)
    {
        executando = true;

        int diaEncerrado = DayManager.Instance.diaAtual;
        int proximoDia = diaEncerrado + 1;

        float mediaDia = DayManager.Instance.ObterMediaDoDia(
            diaEncerrado
        );

        // Atualiza os textos antes de exibi-los.
        MostrarInformacoes(
            "A noite chegou",
            CriarResumo(
                diaEncerrado,
                mediaDia
            )
        );

        // -----------------------------
        // 1. A NOITE CHEGOU
        // -----------------------------

        if (imagemLua != null)
        {
            imagemLua.gameObject.SetActive(true);
            DefinirAlphaImagem(imagemLua, 0f);

            yield return FadeImagem(
                imagemLua, 0f, 1f, duracaoFade
            );
        }

        yield return FadeEscurecimento(
            0f, opacidadeNoite, duracaoFade
        );

        yield return FadeInformacoes(0f, 1f, duracaoFade);

        yield return new WaitForSecondsRealtime(tempoTelaNoite);

        // -----------------------------
        // 2. DESAPARECE A NOITE
        // -----------------------------

        yield return FadeInformacoes(1f, 0f, duracaoFade);

        if (imagemLua != null)
        {
            yield return FadeImagem(
                imagemLua, 1f, 0f, duracaoFade
            );

            imagemLua.gameObject.SetActive(false);
        }

        // -----------------------------
        // 3. NOVO AMANHECER
        // -----------------------------

        DefinirAlphaImagem(imagemSol, 0f);

        if (imagemSol != null)
            imagemSol.gameObject.SetActive(true);

        DefinirAlphaEscurecimento(opacidadeNoite);

        MostrarInformacoes(
            "Dia " + proximoDia + " começou",
            CriarResumoComunidade()
        );

        yield return FadeEscurecimento(
            opacidadeNoite, 0f, duracaoFade
        );

        if (imagemSol != null)
        {
            yield return FadeImagem(
                imagemSol, 0f, 1f, duracaoFade
            );
        }

        yield return FadeInformacoes(0f, 1f, duracaoFade);

        yield return new WaitForSecondsRealtime(tempoTelaAmanha);

        // -----------------------------
        // 4. ENCERRA A TRANSIÇÃO
        // -----------------------------

        yield return FadeInformacoes(1f, 0f, duracaoFade);

        if (imagemSol != null)
        {
            yield return FadeImagem(
                imagemSol, 1f, 0f, duracaoFade
            );

            imagemSol.gameObject.SetActive(false);
        }

        yield return null;

        if (grupoInformacoes != null)
            grupoInformacoes.gameObject.SetActive(false);

        DefinirAlphaEscurecimento(0f);

        executando = false;
        animacaoAtual = null;

        // Só agora o DayManager pode iniciar o novo dia.
        aoFinalizar?.Invoke();
    }

    private void MostrarInformacoes(string titulo, string resumo)
    {
        if (grupoInformacoes != null)
        {
            grupoInformacoes.gameObject.SetActive(true);
            grupoInformacoes.alpha = 0f;
        }

        if (textoTitulo != null)
            textoTitulo.text = titulo;

        if (textoResumo != null)
            textoResumo.text = resumo;
    }

    private string CriarResumo(int dia, float media)
    {
        if (CommunityManager.instance == null)
        {
            return "Dia " + dia +
                   "\nMédia da comunidade: " + media.ToString("F1");
        }

        CommunityManager comunidade = CommunityManager.instance;

        return "Resumo do dia " + dia +
               "\nMédia da comunidade: " + media.ToString("F1") +
               "\nAlimentação: " + comunidade.alimentacao.ToString("F1") +
               "\nSaúde: " + comunidade.saude.ToString("F1") +
               "\nFelicidade: " + comunidade.felicidade.ToString("F1");
    }

    private string CriarResumoComunidade()
    {
        if (CommunityManager.instance == null)
            return "Prepare-se para mais um dia.";

        CommunityManager comunidade = CommunityManager.instance;

        return "Como a comunidade está?" +
               "\nAlimentação: " + comunidade.alimentacao.ToString("F1") +
               "\nSaúde: " + comunidade.saude.ToString("F1") +
               "\nFelicidade: " + comunidade.felicidade.ToString("F1");
    }

    private IEnumerator FadeInformacoes(
        float inicio, float fim, float duracao)
    {
        if (grupoInformacoes == null)
            yield break;

        yield return AnimarAlpha(
            inicio, fim, duracao,
            valor => grupoInformacoes.alpha = valor
        );
    }

    private IEnumerator FadeImagem(
        Image imagem, float inicio, float fim, float duracao)
    {
        if (imagem == null)
            yield break;

        Color corOriginal = imagem == imagemSol
            ? corSol
            : corLua;

        yield return AnimarAlpha(
            inicio, fim, duracao,
            valor =>
            {
                Color cor = corOriginal;
                cor.a = corOriginal.a * valor;
                imagem.color = cor;
            }
        );
    }

    private IEnumerator FadeEscurecimento(
        float inicio, float fim, float duracao)
    {
        if (sobreposicaoEscura == null)
            yield break;

        yield return AnimarAlpha(
            inicio, fim, duracao,
            DefinirAlphaEscurecimento
        );
    }

    private IEnumerator AnimarAlpha(
        float inicio,
        float fim,
        float duracao,
        Action<float> atualizar)
    {
        if (duracao <= 0f)
        {
            atualizar(fim);
            yield break;
        }

        float tempo = 0f;

        atualizar(inicio);

        while (tempo < duracao)
        {
            tempo += Time.unscaledDeltaTime;

            float progresso = Mathf.Clamp01(tempo / duracao);
            float suave = Mathf.SmoothStep(0f, 1f, progresso);

            atualizar(Mathf.Lerp(inicio, fim, suave));

            yield return null;
        }

        atualizar(fim);
    }

    private void DefinirAlphaImagem(Image imagem, float alpha)
    {
        if (imagem == null)
            return;

        Color cor = imagem == imagemSol
            ? corSol
            : corLua;

        cor.a = alpha;
        imagem.color = cor;
    }

    private void DefinirAlphaEscurecimento(float alpha)
    {
        if (sobreposicaoEscura == null)
            return;

        Color cor = corSobreposicao;
        cor.a = alpha;
        sobreposicaoEscura.color = cor;
    }
}
