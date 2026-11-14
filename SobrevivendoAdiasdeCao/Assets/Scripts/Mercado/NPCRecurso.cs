using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCRecurso : MonoBehaviour
{
    [Header("Recursos que o NPC pode soltar")]
    [Tooltip("Cadastre aqui os prefabs que este NPC poderá sortear.")]
    public GameObject[] recursosDisponiveis;

    [Header("Ponto onde o recurso fica na mão e é solto")]
    public Transform pontoDeSolta;

    [Header("Visual do item na mão")]
    [Tooltip("Multiplica o tamanho do recurso enquanto está na mão do NPC (ex: 0.5 = metade do tamanho).")]
    [Range(0.05f, 2f)]
    public float escalaItemNaMao = 0.5f;

    [Header("Interação")]
    public float distanciaInteracao = 1.5f;

    [Header("Salto do recurso (Por cima da cabeça)")]
    [Tooltip("Marque para o item voar por cima da cabeça para trás do NPC. Desmarque para voar para frente.")]
    public bool jogarPorCimaParaTras = true;
    [Tooltip("Distância horizontal que o item percorre ao passar por cima da cabeça.")]
    public float distanciaHorizontalSalto = 1.6f;
    [Tooltip("Altura máxima do pico da parábola.")]
    public float alturaLancamento = 4.5f;
    [Tooltip("Altura final onde o item para/cai em relação ao centro do NPC (ex: 1.3 = altura da cabeça, 0 = chão).")]
    public float alturaFinalRelativa = 1.3f;
    public float duracaoLancamento = 0.65f;
    public float escalaInicial = 0.85f;
    public float velocidadeRotacaoNoAr = 360f;

    [Header("Trilha de Pontinhos (Trajetória)")]
    [Tooltip("Opcional: arraste um sprite de bolinha/quadradinho. Se deixar vazio, cria uma bolinha branca automaticamente!")]
    public Sprite spritePontinho;
    public Color corPontinho = new Color(1f, 1f, 1f, 0.9f);
    public float tamanhoPontinho = 0.14f;
    public float distanciaEntrePontinhos = 0.18f;
    public float tempoVidaPontinho = 0.85f;

    [Header("Animação do NPC")]
    public Animator animatorNPC;
    [Tooltip("Nome exato do Trigger no Animator.")]
    public string triggerSoltarRecurso = "Susto";
    [Tooltip("Tempo em segundos que o NPC fica parado na pose de susto antes de voltar a andar.")]
    public float tempoParadoNoSusto = 0.5f;

    private bool recursoSolto;
    private Transform player;
    private NPCMovimentoMercado movimento;
    private NPCAgressivoMercado agressividade;

    private GameObject recursoSorteado;
    private GameObject visualNaMao;
    private Vector3 posLocalMaoInicial;
    private bool maoInicializada;

    private static Sprite spriteCirculoPadrao;

    private void Awake()
    {
        movimento = GetComponent<NPCMovimentoMercado>();
        if (movimento == null)
            movimento = GetComponentInParent<NPCMovimentoMercado>();

        agressividade = GetComponent<NPCAgressivoMercado>();
        if (agressividade == null)
            agressividade = GetComponentInParent<NPCAgressivoMercado>();

        if (animatorNPC == null)
            animatorNPC = GetComponentInChildren<Animator>();

        if (pontoDeSolta != null)
        {
            posLocalMaoInicial = pontoDeSolta.localPosition;
            maoInicializada = true;
        }
    }

    private void Start()
    {
        GameObject objetoPlayer = GameObject.FindGameObjectWithTag("Player");

        if (objetoPlayer != null)
            player = objetoPlayer.transform;
        else
            Debug.LogWarning("NPCRecurso: nenhum objeto com a tag Player foi encontrado.", this);

        SortearEMostrarNaMao();
    }

    private void LateUpdate()
    {
        if (pontoDeSolta != null && movimento != null && movimento.DirecaoAtual != Vector2.zero)
        {
            if (!maoInicializada)
            {
                posLocalMaoInicial = pontoDeSolta.localPosition;
                maoInicializada = true;
            }

            float sinalX = movimento.DirecaoAtual.x < 0f ? -1f : 1f;
            pontoDeSolta.localPosition = new Vector3(
                Mathf.Abs(posLocalMaoInicial.x) * sinalX,
                posLocalMaoInicial.y,
                posLocalMaoInicial.z
            );
        }
    }

    private void SortearEMostrarNaMao()
    {
        if (recursosDisponiveis == null || recursosDisponiveis.Length == 0)
            return;

        List<GameObject> recursosValidos = new List<GameObject>();
        foreach (GameObject prefab in recursosDisponiveis)
        {
            if (prefab != null)
                recursosValidos.Add(prefab);
        }

        if (recursosValidos.Count == 0)
            return;

        recursoSorteado = recursosValidos[Random.Range(0, recursosValidos.Count)];

        SpriteRenderer srPrefab = recursoSorteado.GetComponentInChildren<SpriteRenderer>(true);
        if (srPrefab != null && srPrefab.sprite != null)
        {
            Transform pai = pontoDeSolta != null ? pontoDeSolta : transform;
            visualNaMao = new GameObject("VisualRecursoNaMao");
            visualNaMao.transform.SetParent(pai);
            visualNaMao.transform.localPosition = pontoDeSolta != null ? Vector3.zero : new Vector3(0.35f, 0.1f, 0f);

            Vector3 escalaPai = pai.lossyScale;
            Vector3 escalaDesejada = recursoSorteado.transform.localScale * escalaItemNaMao;

            visualNaMao.transform.localScale = new Vector3(
                escalaPai.x != 0f ? escalaDesejada.x / Mathf.Abs(escalaPai.x) : escalaDesejada.x,
                escalaPai.y != 0f ? escalaDesejada.y / Mathf.Abs(escalaPai.y) : escalaDesejada.y,
                1f
            );

            SpriteRenderer srMao = visualNaMao.AddComponent<SpriteRenderer>();
            srMao.sprite = srPrefab.sprite;
            srMao.color = srPrefab.color;
            srMao.sortingLayerID = srPrefab.sortingLayerID;
            srMao.sortingOrder = srPrefab.sortingOrder + 1;
        }
    }

    public void ReceberLatido()
    {
        if (recursoSolto || player == null)
            return;

        if (FaseManager.Instance == null || !FaseManager.Instance.PodeColetar())
            return;

        float distancia = Vector2.Distance(transform.position, player.position);

        if (distancia > distanciaInteracao)
            return;

        if (agressividade != null && agressividade.eAgressivo)
        {
            EnergiaSandy energia = player.GetComponentInParent<EnergiaSandy>();
            if (energia == null)
                energia = player.GetComponentInChildren<EnergiaSandy>();

            if (energia != null)
                energia.PerderEnergia();
        }

        SoltarRecurso();
    }

    private void SoltarRecurso()
    {
        if (recursoSolto)
            return;

        if (recursoSorteado == null)
        {
            Debug.LogWarning("NPCRecurso: nenhum recurso válido cadastrado no Inspector.", this);
            return;
        }

        recursoSolto = true;

        if (visualNaMao != null)
        {
            Destroy(visualNaMao);
        }

        DispararAnimacaoSusto();

        if (movimento != null)
        {
            movimento.PausarPorSusto(tempoParadoNoSusto);
        }

        Vector3 posicaoInicial = pontoDeSolta != null
            ? pontoDeSolta.position
            : transform.position;

        float direcaoAndandoX = 1f;
        if (movimento != null && movimento.DirecaoAtual != Vector2.zero)
        {
            direcaoAndandoX = Mathf.Sign(movimento.DirecaoAtual.x);
        }

        float direcaoSaltoX = jogarPorCimaParaTras ? -direcaoAndandoX : direcaoAndandoX;

        Vector3 posicaoFinal = new Vector3(
            posicaoInicial.x + (direcaoSaltoX * distanciaHorizontalSalto),
            transform.position.y + alturaFinalRelativa,
            posicaoInicial.z
        );

        GameObject recurso = Instantiate(
            recursoSorteado,
            posicaoInicial,
            Quaternion.identity
        );

        ItemSoltoPorLatido marcador = recurso.GetComponent<ItemSoltoPorLatido>();
        if (marcador == null)
            marcador = recurso.AddComponent<ItemSoltoPorLatido>();

        StartCoroutine(AnimarRecursoComTrilha(recurso, posicaoInicial, posicaoFinal, direcaoSaltoX));
    }

    private void DispararAnimacaoSusto()
    {
        if (animatorNPC == null)
            animatorNPC = GetComponentInChildren<Animator>();

        if (animatorNPC == null || animatorNPC.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"{name}: Animator ou AnimatorController não encontrado no NPC!", this);
            return;
        }

        string nomeLimpo = triggerSoltarRecurso.Trim();

        foreach (AnimatorControllerParameter param in animatorNPC.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger &&
                string.Equals(param.name, nomeLimpo, System.StringComparison.OrdinalIgnoreCase))
            {
                animatorNPC.ResetTrigger(param.name);
                animatorNPC.SetTrigger(param.name);
                return;
            }
        }

        Debug.LogWarning($"{name}: O Trigger '{nomeLimpo}' não existe no Animator Controller do NPC!", this);
    }

    private IEnumerator AnimarRecursoComTrilha(
        GameObject recurso,
        Vector3 inicio,
        Vector3 destino,
        float direcaoX)
    {
        if (recurso == null)
            yield break;

        Transform visual = recurso.transform;
        Vector3 escalaFinal = visual.localScale;

        Rigidbody2D rb = recurso.GetComponent<Rigidbody2D>();
        bool tinhaFisicaAtiva = rb != null && rb.simulated;

        if (tinhaFisicaAtiva)
            rb.simulated = false;

        visual.localScale = escalaFinal * escalaInicial;

        float duracao = Mathf.Max(0.05f, duracaoLancamento);
        float tempo = 0f;
        Vector3 ultimoPontinhoPos = inicio;

        CriarPontinho(inicio);

        while (tempo < duracao)
        {
            if (recurso == null)
                yield break;

            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / duracao);

            Vector3 posicao = Vector3.Lerp(inicio, destino, t);
            posicao.y += Mathf.Sin(t * Mathf.PI) * alturaLancamento;
            visual.position = posicao;

            visual.Rotate(0f, 0f, -velocidadeRotacaoNoAr * direcaoX * Time.deltaTime);

            float escalaT = Mathf.SmoothStep(0f, 1f, t);
            visual.localScale = Vector3.Lerp(escalaFinal * escalaInicial, escalaFinal, escalaT);

            if (Vector3.Distance(posicao, ultimoPontinhoPos) >= distanciaEntrePontinhos)
            {
                CriarPontinho(posicao);
                ultimoPontinhoPos = posicao;
            }

            yield return null;
        }

        if (recurso == null)
            yield break;

        visual.position = destino;
        visual.rotation = Quaternion.identity;
        visual.localScale = escalaFinal;

        if (rb != null)
        {
            if (tinhaFisicaAtiva)
                rb.simulated = true;

            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void CriarPontinho(Vector3 posicao)
    {
        GameObject pontinho = new GameObject("PontinhoTrajetoria");
        pontinho.transform.position = posicao;
        pontinho.transform.localScale = Vector3.one * tamanhoPontinho;

        SpriteRenderer sr = pontinho.AddComponent<SpriteRenderer>();
        sr.sprite = spritePontinho != null ? spritePontinho : ObterSpriteCirculoPadrao();
        sr.color = corPontinho;
        sr.sortingOrder = 10;

        Destroy(pontinho, tempoVidaPontinho);
    }

    private static Sprite ObterSpriteCirculoPadrao()
    {
        if (spriteCirculoPadrao != null)
            return spriteCirculoPadrao;

        int res = 24;
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        float centro = (res - 1) * 0.5f;
        float raio = res * 0.45f;

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(centro, centro));
                float alpha = Mathf.Clamp01(1f - ((dist - (raio - 1.5f)) / 1.5f));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.Apply();
        spriteCirculoPadrao = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 24f);
        return spriteCirculoPadrao;
    }
}