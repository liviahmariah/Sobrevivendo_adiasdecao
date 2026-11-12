using UnityEngine;
using System.Collections;

public class EnfermeiraAgressiva : MonoBehaviour
{
    [Header("Alvo")]
    public Transform sandy;

    [Header("Percepção")]
    public float distanciaPercepcao = 5f;

    [Header("Ataque")]
    public GameObject prefabProjetil;
    public Transform pontoLancamento;
    public float intervaloAtaques = 3f;

    [Tooltip("Caso NÃO use Animation Events na Unity, o projétil será instanciado após esse atraso.")]
    public float atrasoDisparoFallback = 0.25f;

    [Header("Componentes Visuais")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    [Header("Orientação Base")]
    public bool comecaViradaParaDireita = true;

    [Header("Referências de Efeito")]
    public EnergiaSandy energiaSandy;
    public OndulacaoCamera efeitoOndulacao;

    private float proximoAtaque;
    private bool viradaParaDireita;
    private bool projetilDisparadoNesteAtaque = false;

    // Trigger de ataque otimizado para o Animator
    private static readonly int HashAtacar = Animator.StringToHash("Atacar");

    private void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (sandy == null)
        {
            GameObject objetoPlayer = GameObject.FindGameObjectWithTag("Player");
            if (objetoPlayer != null)
                sandy = objetoPlayer.transform;
            else
                Debug.LogWarning("EnfermeiraAgressiva: Sandy (Player) não encontrada na cena!");
        }

        viradaParaDireita = comecaViradaParaDireita;
        AtualizarOrientacao();
    }

    private void Update()
    {
        if (FaseManager.Instance == null || !FaseManager.Instance.PodeColetar())
            return;

        if (sandy == null || prefabProjetil == null || pontoLancamento == null)
            return;

        float distancia = Vector2.Distance(transform.position, sandy.position);

        // Só reage quando a Sandy entra na distância de percepção
        if (distancia > distanciaPercepcao)
            return;

        // Orientação visual em direção à Sandy (Esquerda/Direita)
        Vector2 diferenca = (Vector2)sandy.position - (Vector2)transform.position;
        viradaParaDireita = diferenca.x >= 0f;
        AtualizarOrientacao();

        // Intervalo entre os disparos
        if (Time.time < proximoAtaque)
            return;

        proximoAtaque = Time.time + intervaloAtaques;
        IniciarAtaque();
    }

    private void IniciarAtaque()
    {
        projetilDisparadoNesteAtaque = false;

        // 1. Aciona o gatilho da animação no Animator
        if (animator != null)
        {
            animator.SetTrigger(HashAtacar);
        }

        // 2. Garante o disparo mesmo se a animação não tiver Animation Event configurado
        StartCoroutine(RotinaFallbackDisparo());
    }

    private IEnumerator RotinaFallbackDisparo()
    {
        yield return new WaitForSeconds(atrasoDisparoFallback);

        if (!projetilDisparadoNesteAtaque)
        {
            InstanciarProjetilMedicamento();
        }
    }

    // ========================================================================
    // MÉTODO DE DISPARO (Pode ser chamado por ANIMATION EVENT ou via FALLBACK)
    // ========================================================================
    public void InstanciarProjetilMedicamento()
    {
        if (projetilDisparadoNesteAtaque || prefabProjetil == null || puntoLancamentoInvalido())
            return;

        projetilDisparadoNesteAtaque = true;

        Vector2 direcao = ((Vector2)sandy.position - (Vector2)pontoLancamento.position).normalized;

        GameObject objetoProjetil = Instantiate(
            prefabProjetil,
            pontoLancamento.position,
            Quaternion.identity
        );

        ProjetilMedicamento projetil = objetoProjetil.GetComponent<ProjetilMedicamento>();

        if (projetil != null)
        {
            projetil.Configurar(direcao, energiaSandy, efeitoOndulacao);
            Debug.Log("HOSPITAL | Enfermeira disparou projétil de medicamento!");
        }
        else
        {
            Debug.LogError("O prefab do projétil não possui o componente ProjetilMedicamento!");
            Destroy(objetoProjetil);
        }
    }

    private bool puntoLancamentoInvalido()
    {
        return pontoLancamento == null || sandy == null;
    }

    private void AtualizarOrientacao()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = comecaViradaParaDireita ? !viradaParaDireita : viradaParaDireita;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaPercepcao);
    }
}