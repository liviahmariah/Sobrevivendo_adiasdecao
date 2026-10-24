
using System.Collections;
using UnityEngine;

public class NPCRecurso : MonoBehaviour
{
    [Header("Recurso que o NPC está segurando")]
    public GameObject recursoPrefab;

    [Header("Ponto onde o recurso será solto")]
    public Transform pontoDeSolta;

    [Header("Interação")]
    public float distanciaInteracao = 1.5f;

    [Header("Posição do recurso")]
    public float distanciaAFrente = 1f;

    [Header("Animação do NPC")]
    public Animator animatorNPC;

    [Tooltip("Nome do Trigger no Animator.")]
    public string triggerSoltarRecurso = "SoltarRecurso";

    [Header("Animação do recurso")]
    public float duracaoLancamento = 0.35f;
    public float alturaLancamento = 0.6f;
    public float escalaInicial = 0.65f;

    private bool recursoSolto;
    private Transform player;
    private NPCMovimentoMercado movimento;
    private NPCAgressivoMercado agressividade;

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
    }

    private void Start()
    {
        GameObject objetoPlayer =
            GameObject.FindGameObjectWithTag("Player");

        if (objetoPlayer != null)
            player = objetoPlayer.transform;
        else
            Debug.LogWarning(
                "NPCRecurso: nenhum objeto com a tag Player foi encontrado.",
                this
            );
    }

    // Chamado pelo PlayerMovement quando Sandy late.
    public void ReceberLatido()
    {
        if (recursoSolto || player == null)
            return;

        if (MercadoManager.Instance == null ||
            !MercadoManager.Instance.PodeColetar())
            return;

        float distancia = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distancia > distanciaInteracao)
            return;

        // NPC agressivo causa dano e também solta o recurso.
        if (agressividade != null && agressividade.eAgressivo)
        {
            EnergiaSandy energia =
                player.GetComponentInParent<EnergiaSandy>();

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

        if (recursoPrefab == null)
        {
            Debug.LogWarning(
                "NPCRecurso: configure o prefab do recurso.",
                this
            );
            return;
        }

        // Anima o NPC, se o Animator estiver configurado.
        if (animatorNPC != null &&
            !string.IsNullOrEmpty(triggerSoltarRecurso))
        {
            animatorNPC.SetTrigger(triggerSoltarRecurso);
        }

        Vector3 posicaoInicial = pontoDeSolta != null
            ? pontoDeSolta.position
            : transform.position;

        Vector3 posicaoFinal;

        if (movimento != null &&
            movimento.DirecaoAtual != Vector2.zero)
        {
            Vector2 direcao = movimento.DirecaoAtual.normalized;

            posicaoFinal = posicaoInicial +
                (Vector3)(direcao * distanciaAFrente);
        }
        else
        {
            posicaoFinal = posicaoInicial +
                Vector3.right * distanciaAFrente;
        }

        // Cria o recurso no ponto em que o NPC o segura.
        GameObject recurso = Instantiate(
            recursoPrefab,
            posicaoInicial,
            Quaternion.identity
        );

        RecursoColetavel coletavel =
            recurso.GetComponent<RecursoColetavel>();

        if (coletavel == null)
        {
            Debug.LogWarning(
                "O prefab solto não possui RecursoColetavel.",
                recurso
            );
        }

        recursoSolto = true;

        StartCoroutine(
            AnimarRecurso(recurso, posicaoInicial, posicaoFinal)
        );

        Debug.Log(
            "NPC soltou o recurso com animação: " + recurso.name,
            recurso
        );
    }

    private IEnumerator AnimarRecurso(
        GameObject recurso,
        Vector3 inicio,
        Vector3 destino)
    {
        if (recurso == null)
            yield break;

        Transform visual = recurso.transform;
        Vector3 escalaFinal = visual.localScale;

        // Evita que a física interfira no movimento animado.
        Rigidbody2D rb = recurso.GetComponent<Rigidbody2D>();
        bool tinhaFisicaAtiva = rb != null && rb.simulated;

        if (tinhaFisicaAtiva)
            rb.simulated = false;

        visual.localScale = escalaFinal * escalaInicial;

        float duracao = Mathf.Max(0.01f, duracaoLancamento);
        float tempo = 0f;

        while (tempo < duracao)
        {
            if (recurso == null)
                yield break;

            tempo += Time.deltaTime;

            float t = Mathf.Clamp01(tempo / duracao);

            // Deslocamento em arco: sobe e depois cai.
            Vector3 posicao = Vector3.Lerp(inicio, destino, t);

            posicao.y += Mathf.Sin(t * Mathf.PI) * alturaLancamento;

            visual.position = posicao;

            // Cresce até o tamanho normal com uma entrada suave.
            float escalaT = Mathf.SmoothStep(0f, 1f, t);

            visual.localScale = Vector3.Lerp(
                escalaFinal * escalaInicial,
                escalaFinal,
                escalaT
            );

            yield return null;
        }

        if (recurso == null)
            yield break;

        visual.position = destino;
        visual.localScale = escalaFinal;

        // Devolve o controle à física após o lançamento.
        if (rb != null)
        {
            if (tinhaFisicaAtiva)
                rb.simulated = true;

            rb.linearVelocity = Vector2.zero;
        }
    }
}
