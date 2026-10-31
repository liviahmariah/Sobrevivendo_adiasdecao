using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MapDialogueManager : MonoBehaviour
{
    [Header("Referências do Diálogo")]
    public GameObject painelDialogo;
    public TextMeshProUGUI textoDialogo;
    public TextMeshProUGUI textoIdentificacaoDuke;

    [Header("Aparição do Duke")]
    public GameObject dukeVisual;
    public float duracaoAparicaoDuke = 0.5f;

    private SpriteRenderer[] spritesDuke;
    private Color[] coresOriginaisDuke;
    private bool dukeJaApareceu = false;

    [Header("Animação do Balão")]
    public RectTransform balaoAnimado;
    public float tempoEntradaBalao = 0.2f;
    public float tempoSaidaBalao = 0.15f;
    public float escalaPop = 1.08f;
    public float velocidadeFlutuacao = 2f;
    public float intensidadeFlutuacao = 2f;

    private Vector3 escalaOriginalBalao;
    private Vector2 posicaoOriginalBalao;
    private Coroutine rotinaAnimacaoBalao;

    [Header("Som e Animação do Duke")]
    public AudioSource audioSourceDuke;
    public AudioClip somLatidoDuke;
    public Animator animatorDuke;

    [Header("Configuração do Texto")]
    public float velocidadeTexto = 0.035f;
    public float pausaVirgula = 0.06f;
    public float pausaPontuacao = 0.15f;
    public float tempoMensagem = 3f;
    public float tempoEntreFalase = 0.5f;

    [Header("Sistema de Mensagens")]
    public float tempoEntreEventos = 8f;

    private bool mostrandoMensagem = false;
    private bool apresentacaoConcluida = false;

    private Coroutine rotinaApresentacao;
    private Coroutine rotinaEventos;

    [Header("Falas de Boas-vindas")]
    [TextArea(2, 5)]
    public string[] falasBoasVindas =
    {
        "Olá, Sandy! Seja bem-vinda ao nosso ponto de organização!",
        "É daqui que vamos acompanhar a situação de toda a comunidade.",
        "Estas barras mostram como estão os cães. Acompanhe a alimentação e o ânimo para saber do que eles precisam!",
        "Quando a alimentação estiver baixa, precisaremos buscar comida.",
        "O Mercado é onde podemos conseguir alimentos para a comunidade.",
        "Na Praça, podemos encontrar recursos para melhorar o ânimo dos cães.",
        "E no Hospital, podemos conseguir medicamentos para cuidar dos que precisam.",
        "Escolha os locais com atenção, consiga os recursos e volte para ajudar todos!",
        "Contamos com você, Sandy! Vamos nessa!"
    };

    private void Start()
    {
        ConfigurarDuke();
        ConfigurarBalao();

        if (painelDialogo != null)
            painelDialogo.SetActive(false);

        rotinaApresentacao = StartCoroutine(IniciarApresentacao());
    }

    private void ConfigurarDuke()
    {
        if (textoIdentificacaoDuke != null)
            textoIdentificacaoDuke.text = "DUKE - LÍDER DA COMUNIDADE";

        if (dukeVisual != null)
        {
            spritesDuke = dukeVisual.GetComponentsInChildren<SpriteRenderer>(true);
            coresOriginaisDuke = new Color[spritesDuke.Length];

            for (int i = 0; i < spritesDuke.Length; i++)
            {
                if (spritesDuke[i] == null)
                    continue;

                coresOriginaisDuke[i] = spritesDuke[i].color;
                spritesDuke[i].enabled = false;
            }
        }
    }

    private void ConfigurarBalao()
    {
        if (balaoAnimado == null)
            return;

        escalaOriginalBalao = balaoAnimado.localScale;
        posicaoOriginalBalao = balaoAnimado.anchoredPosition;
        balaoAnimado.localScale = Vector3.zero;
    }

    private IEnumerator IniciarApresentacao()
    {
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < falasBoasVindas.Length; i++)
        {
            yield return StartCoroutine(MostrarFalaAnimada(falasBoasVindas[i]));

            yield return new WaitForSeconds(tempoMensagem);

            yield return StartCoroutine(EsconderDialogoAnimado());

            if (i < falasBoasVindas.Length - 1)
                yield return new WaitForSeconds(tempoEntreFalase);
        }

        apresentacaoConcluida = true;
        rotinaApresentacao = null;

        rotinaEventos = StartCoroutine(SistemaEventos());
    }

    private IEnumerator MostrarFalaAnimada(string mensagem)
    {
        mostrandoMensagem = true;

        if (painelDialogo != null)
            painelDialogo.SetActive(true);

        if (textoDialogo == null)
            yield break;

        textoDialogo.text = mensagem;
        textoDialogo.ForceMeshUpdate();
        textoDialogo.maxVisibleCharacters = 0;

        if (rotinaAnimacaoBalao != null)
            StopCoroutine(rotinaAnimacaoBalao);

        rotinaAnimacaoBalao = StartCoroutine(AnimarEntradaBalao());

        if (!dukeJaApareceu)
            yield return StartCoroutine(AnimarAparicaoDuke());

        TocarLatidoDuke();

        int quantidadeCaracteres = textoDialogo.textInfo.characterCount;

        for (int i = 0; i <= quantidadeCaracteres; i++)
        {
            textoDialogo.maxVisibleCharacters = i;

            if (i < mensagem.Length)
            {
                char caractere = mensagem[i];

                if (caractere == '.' || caractere == '!' || caractere == '?')
                    yield return new WaitForSeconds(pausaPontuacao);
                else if (caractere == ',')
                    yield return new WaitForSeconds(pausaVirgula);
                else
                    yield return new WaitForSeconds(velocidadeTexto);
            }
        }

        textoDialogo.maxVisibleCharacters = int.MaxValue;
    }

    private IEnumerator AnimarAparicaoDuke()
    {
        if (spritesDuke == null || spritesDuke.Length == 0)
            yield break;

        dukeJaApareceu = true;

        for (int i = 0; i < spritesDuke.Length; i++)
        {
            if (spritesDuke[i] == null)
                continue;

            spritesDuke[i].enabled = true;

            Color cor = coresOriginaisDuke[i];
            cor.a = 0f;
            spritesDuke[i].color = cor;
        }

        float duracao = Mathf.Max(0.01f, duracaoAparicaoDuke);
        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float progresso = Mathf.Clamp01(tempo / duracao);

            for (int i = 0; i < spritesDuke.Length; i++)
            {
                if (spritesDuke[i] == null)
                    continue;

                Color cor = coresOriginaisDuke[i];
                cor.a = Mathf.Lerp(0f, coresOriginaisDuke[i].a, progresso);
                spritesDuke[i].color = cor;
            }

            yield return null;
        }

        for (int i = 0; i < spritesDuke.Length; i++)
        {
            if (spritesDuke[i] != null)
                spritesDuke[i].color = coresOriginaisDuke[i];
        }
    }

    private void TocarLatidoDuke()
    {
        if (animatorDuke != null)
        {
            animatorDuke.ResetTrigger("Latir");
            animatorDuke.SetTrigger("Latir");
        }

        if (audioSourceDuke != null && somLatidoDuke != null)
            audioSourceDuke.PlayOneShot(somLatidoDuke);
    }

    private IEnumerator AnimarEntradaBalao()
    {
        if (balaoAnimado == null)
            yield break;

        Vector3 escondido = Vector3.zero;
        Vector3 aumentado = escalaOriginalBalao * escalaPop;
        Vector3 normal = escalaOriginalBalao;

        float tempo = 0f;

        while (tempo < tempoEntradaBalao)
        {
            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(tempo / tempoEntradaBalao);
            progresso = Mathf.SmoothStep(0f, 1f, progresso);

            balaoAnimado.localScale = Vector3.Lerp(
                escondido,
                aumentado,
                progresso
            );

            yield return null;
        }

        tempo = 0f;
        float duracaoPop = tempoEntradaBalao * 0.35f;

        while (tempo < duracaoPop)
        {
            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(tempo / duracaoPop);

            balaoAnimado.localScale = Vector3.Lerp(
                aumentado,
                normal,
                Mathf.SmoothStep(0f, 1f, progresso)
            );

            yield return null;
        }

        balaoAnimado.localScale = escalaOriginalBalao;
        rotinaAnimacaoBalao = null;
    }

    private IEnumerator EsconderDialogoAnimado()
    {
        if (balaoAnimado == null)
        {
            if (painelDialogo != null)
                painelDialogo.SetActive(false);

            mostrandoMensagem = false;
            yield break;
        }

        Vector3 escalaInicial = balaoAnimado.localScale;
        float tempo = 0f;

        while (tempo < tempoSaidaBalao)
        {
            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(tempo / tempoSaidaBalao);
            progresso = Mathf.SmoothStep(0f, 1f, progresso);

            balaoAnimado.localScale = Vector3.Lerp(
                escalaInicial,
                Vector3.zero,
                progresso
            );

            yield return null;
        }

        balaoAnimado.localScale = Vector3.zero;
        balaoAnimado.anchoredPosition = posicaoOriginalBalao;

        if (painelDialogo != null)
            painelDialogo.SetActive(false);

        mostrandoMensagem = false;
    }

    private void Update()
    {
        if (balaoAnimado != null &&
            painelDialogo != null &&
            painelDialogo.activeSelf)
        {
            float movimento =
                Mathf.Sin(Time.time * velocidadeFlutuacao) *
                intensidadeFlutuacao;

            balaoAnimado.anchoredPosition =
                posicaoOriginalBalao + Vector2.up * movimento;
        }
    }

    private IEnumerator SistemaEventos()
    {
        yield return new WaitForSeconds(tempoEntreEventos);

        while (true)
        {
            if (!mostrandoMensagem && apresentacaoConcluida)
            {
                string mensagem = EscolherMensagem();

                if (!string.IsNullOrEmpty(mensagem))
                {
                    yield return StartCoroutine(MostrarFalaAnimada(mensagem));

                    yield return new WaitForSeconds(tempoMensagem);

                    yield return StartCoroutine(EsconderDialogoAnimado());
                }
            }

            yield return new WaitForSeconds(tempoEntreEventos);
        }
    }

    private string EscolherMensagem()
    {
        CommunityManager c = CommunityManager.instance;

        if (c == null)
            return "Olá, Sandy!";

        List<string> mensagens = new List<string>();

        if (c.alimentacao < 20)
        {
            mensagens.Add("Essa não! A comunidade está precisando muito de comida!");
            mensagens.Add("Precisamos conseguir alimentos imediatamente!");
        }
        else if (c.alimentacao < 50)
        {
            mensagens.Add("A comida está acabando...");
        }

        if (c.felicidade < 20)
        {
            mensagens.Add("O ânimo da comunidade está muito baixo!");
            mensagens.Add("Precisamos animar esses cães!");
        }
        else if (c.felicidade < 50)
        {
            mensagens.Add("A comunidade está ficando desanimada.");
        }

        if (mensagens.Count == 0)
        {
            mensagens.Add("A comunidade está indo muito bem!");
            mensagens.Add("Continue ajudando todos!");
            mensagens.Add("Hoje parece um ótimo dia!");
        }

        return mensagens[Random.Range(0, mensagens.Count)];
    }

    private void OnDisable()
    {
        if (rotinaApresentacao != null)
            StopCoroutine(rotinaApresentacao);

        if (rotinaEventos != null)
            StopCoroutine(rotinaEventos);

        if (rotinaAnimacaoBalao != null)
            StopCoroutine(rotinaAnimacaoBalao);
    }
}