
using System.Collections;
using UnityEngine;
using TMPro;

public class ComentarioMedicamentoUI : MonoBehaviour
{
    public static ComentarioMedicamentoUI Instance { get; private set; }

    [Header("Texto flutuante")]
    [SerializeField] private TMP_Text texto;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Transform sandy;

    [Header("Animação")]
    [SerializeField] private float alturaInicial = 100f;
    [SerializeField] private float subida = 60f;
    [SerializeField] private float duracao = 1.5f;

    [Header("Sons")]
    [SerializeField] private AudioClip somComum;
    [SerializeField] private AudioClip somValioso;
    [SerializeField] private AudioClip somRuim;

    [Range(0f, 1f)]
    [SerializeField] private float volumeSom = 1f;

    private RectTransform textoRect;
    private Coroutine animacaoAtual;
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (texto != null)
        {
            textoRect = texto.GetComponent<RectTransform>();
            texto.gameObject.SetActive(false);
            texto.color = Color.black;
        }

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    public void MostrarComentario(
        string idMedicamento,
        float valor,
        bool medicamentoRuim,
        bool medicamentoValioso)
    {
        if (texto == null || canvas == null || sandy == null)
        {
            Debug.LogWarning(
                "ComentarioMedicamentoUI: configure Texto, Canvas e Sandy.",
                this
            );
            return;
        }

        string nome = ObterNome(idMedicamento);

        string comentario = ObterComentario(
            idMedicamento,
            medicamentoRuim
        );

        float valorExibido = medicamentoRuim ? -valor : valor;

        string pontos = valorExibido > 0
            ? "+" + valorExibido.ToString("0.##")
            : valorExibido.ToString("0.##");

        texto.text = nome + " " + pontos + "\n" + comentario;

        texto.color = Color.black;

        TocarSom(medicamentoRuim, medicamentoValioso);

        PosicionarTexto();

        texto.gameObject.SetActive(true);

        if (animacaoAtual != null)
            StopCoroutine(animacaoAtual);

        animacaoAtual = StartCoroutine(AnimarTexto());
    }

    private void TocarSom(bool ruim, bool valioso)
    {
        AudioClip clip = ruim
            ? somRuim
            : valioso ? somValioso : somComum;

        if (clip != null)
            audioSource.PlayOneShot(clip, volumeSom);
    }

    private void PosicionarTexto()
    {
        Camera camJogo = Camera.main;

        Camera camCanvas = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        Vector3 posicaoMundo = sandy.position + Vector3.up * 1.5f;

        Vector2 posicaoTela = RectTransformUtility.WorldToScreenPoint(
            camJogo,
            posicaoMundo
        );

        RectTransform canvasRect = canvas.transform as RectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            posicaoTela,
            camCanvas,
            out Vector2 posicaoLocal))
        {
            textoRect.anchoredPosition =
                posicaoLocal + new Vector2(0f, alturaInicial);
        }
    }

    private IEnumerator AnimarTexto()
    {
        Vector2 inicio = textoRect.anchoredPosition;
        Vector2 fim = inicio + new Vector2(0f, subida);

        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;

            float t = Mathf.Clamp01(tempo / duracao);

            textoRect.anchoredPosition = Vector2.Lerp(
                inicio,
                fim,
                t
            );

            Color cor = Color.black;
            cor.a = 1f - t;
            texto.color = cor;

            yield return null;
        }

        texto.gameObject.SetActive(false);
        texto.color = Color.black;

        animacaoAtual = null;
    }

    private string ObterNome(string id)
    {
        switch (Normalizar(id))
        {
            case "antibiotico":
                return "Antibiótico";

            case "analgesico":
                return "Analgésico";

            case "antisseptico":
                return "Antisséptico";

            case "vermifugo":
                return "Vermífugo";

            case "colirio":
                return "Colírio";

            case "kit_primeiros_socorros":
                return "Kit de primeiros socorros";

            default:
                return string.IsNullOrWhiteSpace(id)
                    ? "Medicamento"
                    : id;
        }
    }

    private string ObterComentario(string id, bool ruim)
    {
        if (ruim)
            return "Esse medicamento não é seguro!";

        switch (Normalizar(id))
        {
            case "antibiotico":
                return "Um reforço importante para a saúde!";

            case "analgesico":
                return "Vai ajudar a aliviar o desconforto!";

            case "antisseptico":
                return "Um cuidado a mais para a comunidade!";

            case "vermifugo":
                return "Um achado muito valioso!";

            case "colirio":
                return "Mais cuidado com a saúde dos cães!";

            case "kit_primeiros_socorros":
                return "Um kit completo! Excelente achado!";

            default:
                return "Medicamento coletado!";
        }
    }

    private string Normalizar(string id)
    {
        return string.IsNullOrWhiteSpace(id)
            ? ""
            : id.Trim().ToLowerInvariant().Replace("-", "_");
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
