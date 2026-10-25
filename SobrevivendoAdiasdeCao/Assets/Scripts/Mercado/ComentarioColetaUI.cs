using System.Collections;
using UnityEngine;
using TMPro;

public class ComentarioColetaUI : MonoBehaviour
{
    public static ComentarioColetaUI Instance { get; private set; }

    [Header("Texto flutuante")]
    [SerializeField] private TMP_Text texto;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Transform sandy;

    [Header("Animação")]
    [SerializeField] private float alturaInicial = 100f;
    [SerializeField] private float subida = 60f;
    [SerializeField] private float duracao = 1.5f;

    private RectTransform textoRect;
    private Coroutine animacaoAtual;
    private Color corOriginal;

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
            corOriginal = texto.color;
            texto.gameObject.SetActive(false);
        }

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
    }

    public void MostrarComentario(string idComida, float valor, bool comidaRuim)
    {
        if (texto == null || canvas == null || sandy == null)
        {
            Debug.LogWarning(
                "ComentarioColetaUI: configure Texto, Canvas e Sandy no Inspector.",
                this
            );
            return;
        }

        string nome = ObterNome(idComida);
        string comentario = ObterComentario(idComida, comidaRuim);
        string pontos = valor > 0 ? "+" + valor.ToString("0.##") : valor.ToString("0.##");

        texto.text = nome + " " + pontos + "\n" + comentario;
        texto.color = corOriginal;

        PosicionarTexto();
        texto.gameObject.SetActive(true);

        if (animacaoAtual != null)
            StopCoroutine(animacaoAtual);

        animacaoAtual = StartCoroutine(AnimarTexto());
    }

    private void PosicionarTexto()
    {
        Camera camJogo = Camera.main;
        Camera camCanvas = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        Vector3 posicaoMundo = sandy.position + Vector3.up * 1.5f;
        Vector2 posicaoTela = RectTransformUtility.WorldToScreenPoint(
            camJogo, posicaoMundo
        );

        RectTransform canvasRect = canvas.transform as RectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, posicaoTela, camCanvas, out Vector2 posicaoLocal))
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

            textoRect.anchoredPosition = Vector2.Lerp(inicio, fim, t);

            Color cor = corOriginal;
            cor.a = 1f - t;
            texto.color = cor;

            yield return null;
        }

        texto.color = corOriginal;
        texto.gameObject.SetActive(false);
        animacaoAtual = null;
    }

    private string ObterNome(string id)
    {
        switch (Normalizar(id))
        {
            case "peixe": return "Peixe";
            case "coxa de frango":
            case "coxa_frango":
            case "coxadefrango": return "Coxa de frango";
            case "carne de porco":
            case "carne_de_porco":
            case "carnedeporco": return "Carne de porco";
            case "camarao":
            case "camarão": return "Camarão";
            case "maca":
            case "maçã": return "Maçã";
            case "laranja": return "Laranja";
            case "milho": return "Milho";
            case "tapioca": return "Tapioca";
            case "grude": return "Grude";
            case "pastel": return "Pastel";
            case "coxinha": return "Coxinha";
            case "queijo": return "Queijo";
            case "salame": return "Salame";
            default: return string.IsNullOrWhiteSpace(id) ? "Comida" : id;
        }
    }

    private string ObterComentario(string id, bool comidaRuim)
    {
        if (comidaRuim)
            return "Comida estragada!";

        switch (Normalizar(id))
        {
            case "peixe": return "Um achado raro!";
            case "camarao":
            case "camarão": return "Um achado raro!";
            case "coxa de frango":
            case "coxa_frango":
            case "coxadefrango":
            case "carne de porco":
            case "carne_de_porco":
            case "carnedeporco": return "Mais pontos para a comunidade!";
            case "pastel":
            case "coxinha":
            case "salame": return "Cuidado: nem toda comida humana é adequada para cães.";
            case "queijo": return "Alguns cães não toleram bem laticínios.";
            default: return "Recurso coletado!";
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