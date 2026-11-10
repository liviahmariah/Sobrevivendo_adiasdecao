using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
    public float tempoMensagem = 3.5f;
    public float tempoEntreFalase = 0.4f;

    [Header("Sistema de Dicas Frequentes (Máx. 15 segundos)")]
    [Tooltip("Tempo de espera entre uma dica e outra (deve ser no máximo 15s).")]
    public float tempoEntreEventos = 10f;

    private bool mostrandoMensagem = false;
    private bool apresentacaoConcluida = false;

    private Coroutine rotinaApresentacao;
    private Coroutine rotinaEventos;

    private const string CHAVE_APRESENTACAO_REALIZADA = "ApresentacaoComunidadeRealizada";

    [Header("Falas da Introdução Inicial da Sandy")]
    [TextArea(2, 5)]
    public string[] falasBoasVindas =
    {
        "Que bom ver você de volta sã e salva, Sandy!",
        "Você foi incrível conseguindo escapar e libertar nossos amigos lá atrás.",
        "Agora, este é o nosso Ponto de Encontro. É daqui que cuidamos da nossa grande família canina!",
        "Dê uma olhada nessas barras ali em cima: elas mostram como a comunidade está se sentindo.",
        "Precisamos manter todos alimentados, alegres e saudáveis.",
        "Se a comida faltar, o Mercado é o melhor lugar para buscar suprimentos.",
        "Se o pessoal estiver desanimado, uma ida à Praça vai renovar a alegria de todos!",
        "E se alguém ficar doentinho, precisaremos buscar remédios lá no Hospital.",
        "Escolha para onde ir com sabedoria. Toda a comunidade confia nas suas patinhas, Sandy!",
        "Estamos juntos nessa! Quando estiver pronta, escolha um local no mapa."
    };

    private void Start()
    {
        ConfigurarDuke();
        ConfigurarBalao();

        if (painelDialogo != null)
            painelDialogo.SetActive(false);

        StartCoroutine(AguardarIntroEIniciar());
    }

    private IEnumerator AguardarIntroEIniciar()
    {
        // Aguarda a cena fazer o fade de entrada suave
        while (Time.timeScale == 0f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.4f);

        bool apresentacaoJaRealizada = PlayerPrefs.GetInt(CHAVE_APRESENTACAO_REALIZADA, 0) == 1;

        if (apresentacaoJaRealizada)
        {
            apresentacaoConcluida = true;
            ExibirDukeImediatamente();
            rotinaEventos = StartCoroutine(SistemaEventos());
        }
        else
        {
            rotinaApresentacao = StartCoroutine(IniciarApresentacao());
        }
    }

    private void ConfigurarDuke()
    {
        if (textoIdentificacaoDuke != null)
            textoIdentificacaoDuke.text = "DUCKT - LÍDER DA COMUNIDADE";

        if (dukeVisual != null)
        {
            spritesDuke = dukeVisual.GetComponentsInChildren<SpriteRenderer>(true);
            coresOriginaisDuke = new Color[spritesDuke.Length];

            for (int i = 0; i < spritesDuke.Length; i++)
            {
                if (spritesDuke[i] == null) continue;
                coresOriginaisDuke[i] = spritesDuke[i].color;
                spritesDuke[i].enabled = false;
            }
        }
    }

    private void ConfigurarBalao()
    {
        if (balaoAnimado == null) return;
        escalaOriginalBalao = balaoAnimado.localScale;
        posicaoOriginalBalao = balaoAnimado.anchoredPosition;
        balaoAnimado.localScale = Vector3.zero;
    }

    private void ExibirDukeImediatamente()
    {
        dukeJaApareceu = true;
        if (spritesDuke == null) return;

        for (int i = 0; i < spritesDuke.Length; i++)
        {
            if (spritesDuke[i] != null)
            {
                spritesDuke[i].enabled = true;
                spritesDuke[i].color = coresOriginaisDuke[i];
            }
        }
    }

    private IEnumerator IniciarApresentacao()
    {
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

        PlayerPrefs.SetInt(CHAVE_APRESENTACAO_REALIZADA, 1);
        PlayerPrefs.Save();

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
        if (spritesDuke == null || spritesDuke.Length == 0) yield break;

        dukeJaApareceu = true;

        for (int i = 0; i < spritesDuke.Length; i++)
        {
            if (spritesDuke[i] == null) continue;
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
                if (spritesDuke[i] == null) continue;
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
        if (balaoAnimado == null) yield break;

        Vector3 escondido = Vector3.zero;
        Vector3 aumentado = escalaOriginalBalao * escalaPop;
        Vector3 normal = escalaOriginalBalao;

        float tempo = 0f;

        while (tempo < tempoEntradaBalao)
        {
            tempo += Time.deltaTime;
            float progresso = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tempo / tempoEntradaBalao));
            balaoAnimado.localScale = Vector3.Lerp(escondido, aumentado, progresso);
            yield return null;
        }

        tempo = 0f;
        float duracaoPop = tempoEntradaBalao * 0.35f;

        while (tempo < duracaoPop)
        {
            tempo += Time.deltaTime;
            float progresso = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tempo / duracaoPop));
            balaoAnimado.localScale = Vector3.Lerp(aumentado, normal, progresso);
            yield return null;
        }

        balaoAnimado.localScale = escalaOriginalBalao;
        rotinaAnimacaoBalao = null;
    }

    private IEnumerator EsconderDialogoAnimado()
    {
        if (balaoAnimado == null)
        {
            if (painelDialogo != null) painelDialogo.SetActive(false);
            mostrandoMensagem = false;
            yield break;
        }

        Vector3 escalaInicial = balaoAnimado.localScale;
        float tempo = 0f;

        while (tempo < tempoSaidaBalao)
        {
            tempo += Time.deltaTime;
            float progresso = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tempo / tempoSaidaBalao));
            balaoAnimado.localScale = Vector3.Lerp(escalaInicial, Vector3.zero, progresso);
            yield return null;
        }

        balaoAnimado.localScale = Vector3.zero;
        balaoAnimado.anchoredPosition = posicaoOriginalBalao;

        if (painelDialogo != null) painelDialogo.SetActive(false);

        mostrandoMensagem = false;
    }

    private void Update()
    {
        if (balaoAnimado != null && painelDialogo != null && painelDialogo.activeSelf)
        {
            float movimento = Mathf.Sin(Time.time * velocidadeFlutuacao) * intensidadeFlutuacao;
            balaoAnimado.anchoredPosition = posicaoOriginalBalao + Vector2.up * movimento;
        }
    }

    // =====================================================
    // SISTEMA DE EVENTOS CONSTANTES (MÁXIMO 15 SEGUNDOS)
    // =====================================================
    private IEnumerator SistemaEventos()
    {
        // Primeira espera curta de 5 segundos após entrar no mapa
        yield return new WaitForSeconds(5f);

        while (true)
        {
            if (!mostrandoMensagem && apresentacaoConcluida)
            {
                string dica = EscolherDicaDinamica();

                if (!string.IsNullOrEmpty(dica))
                {
                    yield return StartCoroutine(MostrarFalaAnimada(dica));
                    yield return new WaitForSeconds(tempoMensagem);
                    yield return StartCoroutine(EsconderDialogoAnimado());
                }
            }

            // Garante que o intervalo entre o fim de uma fala e o início de outra fique em 10~12 segundos
            yield return new WaitForSeconds(tempoEntreEventos);
        }
    }

    private string EscolherDicaDinamica()
    {
        CommunityManager c = CommunityManager.instance;
        List<string> dicas = new List<string>();

        // 1. VERIFICAÇÃO DE ALIMENTAÇÃO
        if (c != null && c.alimentacao < 25)
        {
            dicas.Add("Sandy, a comida está acabando! Precisamos ir ao Mercado quanto antes!");
            dicas.Add("A barriguinha dos filhotes está roncando... Que tal dar um pulo no Mercado?");
            dicas.Add("Sem comida, a comunidade fica fraca. O Mercado é nossa melhor opção agora!");
        }
        else if (c != null && c.alimentacao < 50)
        {
            dicas.Add("Os estoques de ração estão ficando baixos.");
            dicas.Add("Seria bom planejar uma busca de alimentos em breve.");
        }

        // 2. VERIFICAÇÃO DE FELICIDADE / ÂNIMO
        if (c != null && c.felicidade < 25)
        {
            dicas.Add("O pessoal está muito desanimado... Uma visitinha à Praça faria milagres!");
            dicas.Add("Precisamos de brinquedos e carinho! A Praça é o lugar ideal para animar todos.");
            dicas.Add("O clima na comunidade está meio triste... Que tal irmos até a Praça?");
        }
        else if (c != null && c.felicidade < 50)
        {
            dicas.Add("O ânimo da matilha podia estar melhor. Fique de olho nisso!");
        }

        // 3. SE TUDO ESTIVER EQUILIBRADO (MENSAGENS VARIADAS DE COMPANHIAS E DICAS)
        if (dicas.Count == 0)
        {
            // Dicas gerais sobre locais
            dicas.Add("No Mercado você encontra alimentos e água para abastecer a comunidade!");
            dicas.Add("A Praça é ótima para encontrar itens recreativos e aumentar a felicidade dos cães.");
            dicas.Add("No Hospital conseguimos medicamentos importantes para cuidar de quem precisa.");

            // Frases motivacionais e de carinho da comunidade
            dicas.Add("Você é o orgulho da nossa matilha, Sandy!");
            dicas.Add("Lembre-se: explore com cuidado para não atrair a atenção da Carrocinha.");
            dicas.Add("Sempre que precisar, estou aqui para te dar um direcionamento.");
            dicas.Add("Qualquer missão que você escolher ajudará demais nossa comunidade!");
            dicas.Add("Dá uma olhada no mapa e escolha para onde quer ir primeiro!");
        }

        return dicas[Random.Range(0, dicas.Count)];
    }

    private void OnDisable()
    {
        if (rotinaApresentacao != null) StopCoroutine(rotinaApresentacao);
        if (rotinaEventos != null) StopCoroutine(rotinaEventos);
        if (rotinaAnimacaoBalao != null) StopCoroutine(rotinaAnimacaoBalao);
    }

    [ContextMenu("Resetar Apresentacao do Duke")]
    public void ResetarApresentacaoSalva()
    {
        PlayerPrefs.DeleteKey(CHAVE_APRESENTACAO_REALIZADA);
        PlayerPrefs.Save();
        Debug.Log("Apresentação inicial da Sandy resetada com sucesso!");
    }
}