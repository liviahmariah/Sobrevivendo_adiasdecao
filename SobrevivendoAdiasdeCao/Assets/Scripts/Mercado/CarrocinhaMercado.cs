using System.Collections;
using UnityEngine;

public class CarrocinhaMercado : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 3.5f;
    public bool virarParaSandy = true;
    public bool iniciarAoAtivarObjeto = true;

    [Header("Alvo e Captura")]
    public Transform player;
    [Tooltip("Pequeno atraso após encostar na Sandy para o toque ficar visível antes do painel abrir (0 = instantâneo).")]
    public float atrasoPainelDerrota = 0.25f;

    [Header("Animação")]
    public Animator animator;

    [Header("Altura do chão")]
    public float alturaDoChao;

    private Rigidbody2D rb;
    private Vector3 escalaInicial;

    private bool perseguindo = false;
    private bool capturou = false;
    private bool alturaInicializada = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        escalaInicial = transform.localScale;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void OnEnable()
    {
        if (iniciarAoAtivarObjeto)
        {
            Ativar();
        }
    }

    private void Start()
    {
        Inicializar();
    }

    private void Inicializar()
    {
        if (!alturaInicializada)
        {
            alturaDoChao = rb != null ? rb.position.y : transform.position.y;
            alturaInicializada = true;
        }

        if (player == null)
        {
            GameObject objetoPlayer = GameObject.FindGameObjectWithTag("Player");
            if (objetoPlayer != null)
                player = objetoPlayer.transform;
        }
    }

    private void FixedUpdate()
    {
        if (!perseguindo || capturou)
            return;

        if (player == null)
        {
            Inicializar();
            if (player == null) return;
        }

        PerseguirSandy();
    }

    public void Ativar()
    {
        gameObject.SetActive(true);
        Inicializar();

        perseguindo = true;
        capturou = false;

        AtualizarAnimacao(true, false);
    }

    public void Desativar()
    {
        perseguindo = false;
        AtualizarAnimacao(false, false);

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private void PerseguirSandy()
    {
        float posAtualX = rb != null ? rb.position.x : transform.position.x;
        float distanciaX = player.position.x - posAtualX;
        float direcaoX = Mathf.Abs(distanciaX) < 0.05f ? 0f : Mathf.Sign(distanciaX);

        AtualizarAnimacao(direcaoX != 0f, false);

        if (direcaoX != 0f && virarParaSandy)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(escalaInicial.x) * direcaoX,
                escalaInicial.y,
                escalaInicial.z
            );
        }

        float novoX = posAtualX + (direcaoX * velocidade * Time.fixedDeltaTime);
        Vector2 novaPosicao = new Vector2(novoX, alturaDoChao);

        if (rb != null)
            rb.MovePosition(novaPosicao);
        else
            transform.position = novaPosicao;
    }

    private void AtualizarAnimacao(bool correndo, bool agarrando)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return;

        animator.SetBool("EstaCorrendo", correndo);
        animator.SetBool("EstaParado", !correndo && !agarrando);
        animator.SetBool("EstaAndando", false);
        animator.SetBool("Agarrando", agarrando);
    }

    private void OnCollisionEnter2D(Collision2D collision) => VerificarColisao(collision.gameObject);
    private void OnTriggerEnter2D(Collider2D other) => VerificarColisao(other.gameObject);
    private void OnTriggerStay2D(Collider2D other) => VerificarColisao(other.gameObject);

    private void VerificarColisao(GameObject objeto)
    {
        if (!perseguindo || capturou)
            return;

        if (!objeto.CompareTag("Player") && objeto.GetComponentInParent<PlayerMovement>() == null)
            return;

        StartCoroutine(CapturarSandy());
    }

    private IEnumerator CapturarSandy()
    {
        if (capturou)
            yield break;

        capturou = true;
        perseguindo = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        AtualizarAnimacao(false, true);

        if (atrasoPainelDerrota > 0f)
            yield return new WaitForSecondsRealtime(atrasoPainelDerrota);

        if (PracaManager.Instance != null)
            PracaManager.Instance.Derrota();
        else if (HospitalManager.Instance != null)
            HospitalManager.Instance.Derrota();
        else if (MercadoManager.Instance != null)
            MercadoManager.Instance.Derrota();
        else if (FaseManager.Instance != null)
            FaseManager.Instance.Derrota();
    }
}