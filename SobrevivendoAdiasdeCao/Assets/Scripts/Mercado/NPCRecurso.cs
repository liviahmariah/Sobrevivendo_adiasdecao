
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

    // Chamado pelo PlayerMovement quando Sandy late na direção do NPC.
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

        // NPC agressivo causa dano, mas também deixa o item cair.
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

        Vector3 posicaoSolta;

        if (movimento != null &&
            movimento.DirecaoAtual != Vector2.zero)
        {
            Vector3 basePosicao = pontoDeSolta != null
                ? pontoDeSolta.position
                : transform.position;

            Vector2 direcao = movimento.DirecaoAtual.normalized;

            posicaoSolta = basePosicao +
                (Vector3)(direcao * distanciaAFrente);
        }
        else
        {
            posicaoSolta = pontoDeSolta != null
                ? pontoDeSolta.position
                : transform.position +
                  Vector3.right * distanciaAFrente;
        }

        GameObject recurso = Instantiate(
            recursoPrefab,
            posicaoSolta,
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

        Debug.Log(
            "NPC soltou o recurso: " + recurso.name,
            recurso
        );
    }
}
