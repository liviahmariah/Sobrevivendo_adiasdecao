using UnityEngine;

public class SandyAudio : MonoBehaviour
{
    [Header("Áudios")]
    public AudioClip somRespiracao;
    public AudioClip somPata;
    public AudioClip somPulo;

    [Header("Volumes")]
    [Range(0f, 1f)]
    public float volumeRespiracao = 0.25f;

    [Range(0f, 1f)]
    public float volumePata = 0.55f;

    [Range(0f, 1f)]
    public float volumePulo = 0.7f;

    [Header("Passos")]
    public float intervaloPassos = 0.25f;

    private AudioSource audioRespiracao;
    private AudioSource audioPassos;
    private AudioSource audioPulo;

    private float contadorPassos;

    private Rigidbody2D rb;

    private bool estavaNoChao = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // =========================
        // RESPIRAÇÃO
        // =========================

        audioRespiracao = gameObject.AddComponent<AudioSource>();

        audioRespiracao.clip = somRespiracao;
        audioRespiracao.loop = true;
        audioRespiracao.playOnAwake = false;
        audioRespiracao.volume = volumeRespiracao;

        if (somRespiracao != null)
        {
            audioRespiracao.Play();
        }

        // =========================
        // PATAS
        // =========================

        audioPassos = gameObject.AddComponent<AudioSource>();

        audioPassos.clip = somPata;
        audioPassos.loop = false;
        audioPassos.playOnAwake = false;
        audioPassos.volume = volumePata;

        // =========================
        // PULO
        // =========================

        audioPulo = gameObject.AddComponent<AudioSource>();

        audioPulo.clip = somPulo;
        audioPulo.loop = false;
        audioPulo.playOnAwake = false;
        audioPulo.volume = volumePulo;
    }

    void Update()
    {
        if (rb == null)
            return;

        // =========================
        // MOVIMENTO
        // =========================

        float velocidadeX = Mathf.Abs(rb.linearVelocity.x);

        bool estaCorrendo = velocidadeX > 0.1f;

        if (estaCorrendo)
        {
            contadorPassos -= Time.deltaTime;

            if (contadorPassos <= 0f)
            {
                TocarPasso();
                contadorPassos = intervaloPassos;
            }
        }
        else
        {
            contadorPassos = 0f;
        }

        // =========================
        // PULO
        // =========================

        bool estaNoAr = Mathf.Abs(rb.linearVelocity.y) > 0.1f;

        if (estavaNoChao && estaNoAr)
        {
            TocarPulo();
        }

        estavaNoChao = !estaNoAr;
    }

    void TocarPasso()
    {
        if (somPata != null)
        {
            audioPassos.PlayOneShot(somPata);
        }
    }

    void TocarPulo()
    {
        if (somPulo != null)
        {
            audioPulo.PlayOneShot(somPulo);
        }
    }
}