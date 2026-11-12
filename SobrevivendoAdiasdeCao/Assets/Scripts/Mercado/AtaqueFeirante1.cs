using UnityEngine;
using System.Collections;

public class AtaqueFeirante : MonoBehaviour
{
    [Header("Referências")]
    public SpriteRenderer spriteRenderer;
    public Transform pontoDisparo;
    public GameObject projetilPrefab;
    public Animator animator;

    [Header("Diálogo / Falas")]
    public BalaoFalaFeirante balaoFala;
    [Tooltip("Tempo que a fala fica visível durante o ataque.")]
    public float tempoExibicaoFala = 1.5f;

    [Header("Visão")]
    public float alcanceVisao = 7f;

    [Range(1f, 180f)]
    public float anguloVisao = 100f;

    public LayerMask obstaculos;

    [Header("Ataque à distância")]
    public float distanciaMinimaProjetil = 2.5f;
    public float intervaloProjetil = 2f;
    [Tooltip("Caso NÃO use Animation Events na animação, o projétil será lançado após este tempo em segundos.")]
    public float atrasoDisparoFallback = 0.25f;

    [Header("Ataque por proximidade")]
    public float distanciaAtaque = 1.5f;
    public float intervaloAtaque = 1.5f;

    [Header("Orientação Base")]
    public bool comecaViradoParaDireita = true;

    private Transform player;
    private float proximoProjetil;
    private float proximoAtaque;
    private bool viradoParaDireita;
    private Coroutine rotinaFala;
    private bool projetilDisparadoNesteAtaque = false;

    // Trigger único de ataque no Animator
    private static readonly int HashAtacar = Animator.StringToHash("Atacar");

    private void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
            animator = GetComponent<Animator>();

        GameObject objetoPlayer = GameObject.FindGameObjectWithTag("Player");

        if (objetoPlayer != null)
            player = objetoPlayer.transform;
        else
            Debug.LogWarning("AtaqueFeirante: Player não encontrado!");

        viradoParaDireita = comecaViradoParaDireita;
        AtualizarOrientacao();

        if (balaoFala != null)
            balaoFala.EsconderFala();
    }

    private void Update()
    {
        if (player == null)
            return;

        Vector2 diferenca = (Vector2)player.position - (Vector2)transform.position;
        float distancia = diferenca.magnitude;

        // Sandy fora do alcance ou com visão bloqueada
        if (distancia > alcanceVisao || distancia <= 0.01f || !SandyEstaNoCampoDeVisao(diferenca, distancia))
        {
            EsconderFalaSeNecessario();
            return;
        }

        // Vira para o lado da Sandy (Esquerda/Direita)
        viradoParaDireita = diferenca.x >= 0f;
        AtualizarOrientacao();

        // Escolhe o ataque conforme a distância
        if (distancia <= distanciaAtaque)
        {
            TentarAtacarPerto();
        }
        else if (distancia >= distanciaMinimaProjetil)
        {
            TentarAtacarLonge();
        }
    }

    private bool SandyEstaNoCampoDeVisao(Vector2 diferenca, float distancia)
    {
        Vector2 frente = viradoParaDireita ? Vector2.right : Vector2.left;
        float angulo = Vector2.Angle(frente, diferenca);

        if (angulo > anguloVisao * 0.5f)
            return false;

        RaycastHit2D obstaculo = Physics2D.Raycast(
            transform.position,
            diferenca.normalized,
            distancia,
            obstaculos
        );

        return obstaculo.collider == null;
    }

    private void AtualizarOrientacao()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = comecaViradoParaDireita ? !viradoParaDireita : viradoParaDireita;
        }
    }

    private void TentarAtacarPerto()
    {
        if (Time.time < proximoAtaque)
            return;

        proximoAtaque = Time.time + intervaloAtaque;

        // 1. Fala junto com o ataque
        ExibirFalaJuntoComAtaque();

        // 2. Animação
        if (animator != null)
            animator.SetTrigger(HashAtacar);

        // 3. Aplica o dano
        EnergiaSandy energia = player.GetComponentInParent<EnergiaSandy>();
        if (energia == null)
            energia = player.GetComponentInChildren<EnergiaSandy>();

        if (energia != null)
        {
            energia.PerderEnergia();
            Debug.Log("FEIRANTE ATACOU DE PERTO!");
        }
    }

    private void TentarAtacarLonge()
    {
        if (projetilPrefab == null || Time.time < proximoProjetil)
            return;

        proximoProjetil = Time.time + intervaloProjetil;
        projetilDisparadoNesteAtaque = false;

        // 1. Fala junto com o ataque
        ExibirFalaJuntoComAtaque();

        // 2. Animação de Ataque
        if (animator != null)
            animator.SetTrigger(HashAtacar);

        // 3. Fallback: Garante o lançamento mesmo se não houver Animation Event configurado
        StartCoroutine(RotinaFallbackProjetil());
    }

    private void ExibirFalaJuntoComAtaque()
    {
        if (balaoFala == null)
            return;

        if (rotinaFala != null)
            StopCoroutine(rotinaFala);

        rotinaFala = StartCoroutine(RotinaExibirFala());
    }

    private IEnumerator RotinaExibirFala()
    {
        balaoFala.MostrarFala();
        yield return new WaitForSeconds(tempoExibicaoFala);
        balaoFala.EsconderFala();
        rotinaFala = null;
    }

    private void EsconderFalaSeNecessario()
    {
        if (rotinaFala != null)
        {
            StopCoroutine(rotinaFala);
            rotinaFala = null;
        }

        if (balaoFala != null)
            balaoFala.EsconderFala();
    }

    private IEnumerator RotinaFallbackProjetil()
    {
        yield return new WaitForSeconds(atrasoDisparoFallback);

        // Se a animação já não disparou via Animation Event, o código lança agora
        if (!projetilDisparadoNesteAtaque)
        {
            DispararProjetilInstancia();
        }
    }

    // ========================================================================
    // MÉTODO DE DISPARO (Pode ser chamado por Animation Event OU pelo Fallback)
    // ========================================================================
    public void DispararProjetilInstancia()
    {
        if (projetilPrefab == null || player == null || projetilDisparadoNesteAtaque)
            return;

        projetilDisparadoNesteAtaque = true;

        Vector3 origem = pontoDisparo != null ? pontoDisparo.position : transform.position;
        GameObject projetil = Instantiate(projetilPrefab, origem, Quaternion.identity);

        ProjetilFeirante script = projetil.GetComponent<ProjetilFeirante>();
        if (script != null)
        {
            script.Configurar(player.position);
            Debug.Log("FEIRANTE LANÇOU PROJÉTIL COM SUCESSO!");
        }
        else
        {
            Debug.LogError("O Prefab do projétil NÃO possui o script 'ProjetilFeirante' anexado!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, alcanceVisao);

        Vector3 frente = comecaViradoParaDireita ? Vector3.right : Vector3.left;
        float metadeAngulo = anguloVisao * 0.5f;

        Vector3 limiteEsquerdo = Quaternion.Euler(0, 0, metadeAngulo) * frente;
        Vector3 limiteDireito = Quaternion.Euler(0, 0, -metadeAngulo) * frente;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, limiteEsquerdo * alcanceVisao);
        Gizmos.DrawRay(transform.position, limiteDireito * alcanceVisao);
    }

    private void OnDisable()
    {
        EsconderFalaSeNecessario();
    }
}