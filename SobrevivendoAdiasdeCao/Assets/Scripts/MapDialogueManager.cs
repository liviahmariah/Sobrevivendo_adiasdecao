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

    [Header("Fade de Encerramento (Dia 15)")]
    [Tooltip("Arraste aqui o CanvasGroup da imagem preta de fade (Alpha inicial = 0)")]
    public CanvasGroup painelFadeSaida;
    public float duracaoFadeSaida = 2f;

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
    private bool encerrandoCampanha = false;

    private Coroutine rotinaApresentacao;
    private Coroutine rotinaEventos;

    private const string CHAVE_APRESENTACAO_REALIZADA = "ApresentacaoComunidadeRealizada";

    [Header("Falas da Introdução Inicial da Sandy")]
    [TextArea(2, 5)]
    public string[] falasBoasVindas =
    {
        "Que bom ver você de volta sã e salva, Sandy!",
        "Você foi incrível conseguindo escapar e libertar nossos amigos lá atrás.",
        "Agora, este é o nosso Ponto de Encontro. É daqui que cuidamos da nossa grande família canina ao longo de 15 dias!",
        "Dê uma olhada nas barras ali em cima: elas mostram a Alimentação, a Felicidade e a Saúde da comunidade.",
        "Fique atenta: quanto mais baixa estiver uma barra, menor será o tempo disponível naquela missão!",
        "Em cada local, colete o máximo de itens bons antes do tempo acabar e fuja para a saída antes que a Carrocinha chegue.",
        "Cuidado com itens ruins ou estragados, pois eles descontam pontos, e fique longe dos vigias!",
        "Se algum vigia agarrar você, lata bastante pressionando Z para assustá-lo e escapar!",
        "No Mercado, buscamos comida e água para matar a fome da matilha.",
        "Na Praça, recolhemos brinquedos e itens de diversão para levantar o ânimo de todos.",
        "E no Hospital, pegamos medicamentos importantes para curar quem está doentinho.",
        "Se a Carrocinha pegar você, perdemos tudo o que foi coletado na missão. Escolha no mapa por onde começar!"
    };

    [Header("Falas do Último Dia (Dia 15)")]
    [TextArea(2, 5)]
    public string[] falasEncerramentoBom =
    {
        "Chegamos ao décimo quinto dia, Sandy! Olhe só ao nosso redor...",
        "Graças à sua coragem e dedicação, toda a nossa matilha está forte, saudável e feliz!",
        "Você provou que juntos podemos superar qualquer desafio nas ruas. Vamos ver o resultado da nossa jornada!"
    };

    [TextArea(2, 5)]
    public string[] falasEncerramentoMedio =
    {
        "Chegamos ao décimo quinto dia, Sandy! Foi uma jornada cheia de desafios.",
        "Enfrentamos dias difíceis, mas graças ao seu esforço conseguimos manter a comunidade unida.",
        "Vamos ver como nossa família canina se saiu nesses quinze dias!"
    };

    [TextArea(2, 5)]
    public string[] falasEncerramentoRuim =
    {
        "Chegamos ao décimo quinto dia, Sandy... Os tempos foram muito duros para todos nós.",
        "A fome e o cansaço pesaram na matilha, mas você nunca desistiu de lutar por nós.",
        "Vamos ver o que restou da nossa jornada nesses quinze dias."
    };

    private void Start()
    {
        ConfigurarDuke();
        ConfigurarBalao();

        if (painelDialogo != null)
            painelDialogo.SetActive(false);

        if (DayManager.Instance != null && DayManager.Instance.EhUltimoDia())
        {
            ExibirDukeImediatamente();
            IniciarDiscursoFinalDia15();
            return;
        }

        StartCoroutine(AguardarIntroEIniciar());
    }

    private IEnumerator AguardarIntroEIniciar()
    {
        while (Time.timeScale == 0f)
            yield return null;

        yield return new WaitForSeconds(0.4f);

        if (encerrandoCampanha)
            yield break;

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

    // =====================================================
    // ENCERRAMENTO DA CAMPANHA NO DIA 15 + FADE FINAL
    // =====================================================
    public void IniciarDiscursoFinalDia15()
    {
        if (encerrandoCampanha)
            return;

        encerrandoCampanha = true;

        if (rotinaApresentacao != null) StopCoroutine(rotinaApresentacao);
        if (rotinaEventos != null) StopCoroutine(rotinaEventos);

        StartCoroutine(RotinaDiscursoEFadeFinal());
    }

    private IEnumerator RotinaDiscursoEFadeFinal()
    {
        while (Time.timeScale == 0f)
            yield return null;

        yield return new WaitForSeconds(0.5f);

        string[] falasFinais = ObterFalasDoDesfecho();

        for (int i = 0; i < falasFinais.Length; i++)
        {
            yield return StartCoroutine(MostrarFalaAnimada(falasFinais[i]));
            yield return new WaitForSeconds(tempoMensagem);
            yield return StartCoroutine(EsconderDialogoAnimado());

            if (i < falasFinais.Length - 1)
                yield return new WaitForSeconds(tempoEntreFalase);
        }

        if (painelFadeSaida != null)
        {
            painelFadeSaida.gameObject.SetActive(true);
            painelFadeSaida.blocksRaycasts = true;

            float tempo = 0f;
            float alphaInicial = painelFadeSaida.alpha;

            while (tempo < duracaoFadeSaida)
            {
                tempo += Time.unscaledDeltaTime;
                painelFadeSaida.alpha = Mathf.Lerp(alphaInicial, 1f, tempo / duracaoFadeSaida);
                yield return null;
            }

            painelFadeSaida.alpha = 1f;
        }

        if (DayManager.Instance != null)
            DayManager.Instance.IrParaCenaFinal();
    }

    private string[] ObterFalasDoDesfecho()
    {
        if (DayManager.Instance == null)
            return falasEncerramentoMedio;

        switch (DayManager.Instance.resultadoFinal)
        {
            case "Final Bom":
                return falasEncerramentoBom;
            case "Final Ruim":
                return falasEncerramentoRuim;
            default:
                return falasEncerramentoMedio;
        }
    }

    // =====================================================
    // APRESENTAÇÃO INICIAL E DIÁLOGOS ANIMADOS
    // =====================================================
    private IEnumerator IniciarApresentacao()
    {
        for (int i = 0; i < falasBoasVindas.Length; i++)
        {
            if (encerrandoCampanha) yield break;

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

        if (!encerrandoCampanha)
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

    private IEnumerator SistemaEventos()
    {
        yield return new WaitForSeconds(5f);

        while (!encerrandoCampanha)
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

            yield return new WaitForSeconds(tempoEntreEventos);
        }
    }

    private string EscolherDicaDinamica()
    {
        CommunityManager c = CommunityManager.instance;
        List<string> dicas = new List<string>();

        if (c != null && c.alimentacao < 25)
        {
            dicas.Add("Sandy, a comida está acabando! No Mercado você coleta ração e água para alimentar a matilha!");
            dicas.Add("A barriguinha dos filhotes está roncando... Corra para o Mercado e pegue apenas os alimentos bons!");
            dicas.Add("Sem comida, a comunidade fica fraca. O Mercado é nossa melhor opção agora!");
        }
        else if (c != null && c.alimentacao < 50)
        {
            dicas.Add("Os estoques de ração estão baixos — lembre-se que quanto menor a barra, menor o tempo no Mercado!");
            dicas.Add("Seria bom planejar uma busca de alimentos no Mercado em breve.");
        }

        if (c != null && c.felicidade < 25)
        {
            dicas.Add("O pessoal está muito desanimado... Na Praça você coleta brinquedos e diversão para animar todos!");
            dicas.Add("Precisamos renovar a alegria da matilha! Visite a Praça e fuja do vigia antes do tempo acabar.");
            dicas.Add("O clima na comunidade está meio triste... Que tal irmos até a Praça?");
        }
        else if (c != null && c.felicidade < 50)
        {
            dicas.Add("O ânimo da matilha podia estar melhor. Uma passada na Praça ajuda a subir a felicidade!");
        }

        if (c != null && c.saude < 25)
        {
            dicas.Add("Atenção, Sandy! A saúde da comunidade está crítica! Precisamos buscar remédios bons no Hospital!");
            dicas.Add("Temos cãezinhos doentes! Vá ao Hospital e evite os medicamentos estragados.");
        }
        else if (c != null && c.saude < 50)
        {
            dicas.Add("A saúde da matilha precisa de cuidado. No Hospital você coleta medicamentos para proteger todos!");
        }

        if (dicas.Count == 0)
        {
            dicas.Add("No Mercado, colete ração e água para aumentar a Alimentação — mas evite comidas estragadas!");
            dicas.Add("Na Praça, recolha itens recreativos para subir a Felicidade da comunidade, tomando cuidado com o vigia!");
            dicas.Add("No Hospital, pegue apenas os medicamentos bons para aumentar a Saúde da matilha!");
            dicas.Add("Fique de olho no cronômetro de cada fase: quando o tempo acaba, a Carrocinha aparece para capturar você!");
            dicas.Add("Se algum vigia agarrar você na fase, aperte Z várias vezes para latir, assustá-lo e escapar!");
            dicas.Add("Lembre-se: se a Carrocinha pegar a Sandy, todos os itens coletados naquela missão são perdidos!");
            dicas.Add("Quanto mais cheia estiver a barra de uma necessidade, mais tempo você terá para explorar o local!");
            dicas.Add("Cada dia no mapa permite uma nova missão. Equilibre Alimentação, Felicidade e Saúde até o Dia 15!");
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
    }
}