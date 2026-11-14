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

    [Header("Áudio")]
    public AudioSource audioSource;
    public AudioClip somAtaque;
    [Range(0f, 1f)]
    public float volumeSomAtaque = 1f;
    [Tooltip("Varia levemente o tom do som a cada ataque para ficar mais natural.")]
    public bool variarTom = true;

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

    private static readonly int HashAtacar = Animator.StringToHash("Atacar");

    private void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Cria um AudioSource automaticamente caso o objeto ainda não tenha um
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

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

        if (distancia > distanciaPercepcao)
            return;

        Vector2 diferenca = (Vector2)sandy.position - (Vector2)transform.position;
        viradaParaDireita = diferenca.x >= 0f;
        AtualizarOrientacao();

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

        // 2. Toca o som do ataque
        TocarSomAtaque();

        // 3. Garante o disparo mesmo se a animação não tiver Animation Event configurado
        StartCoroutine(RotinaFallbackDisparo());
    }

    private void TocarSomAtaque()
    {
        if (audioSource == null || somAtaque == null)
            return;

        audioSource.pitch = variarTom ? Random.Range(0.92f, 1.08f) : 1f;
        audioSource.PlayOneShot(somAtaque, volumeSomAtaque);
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