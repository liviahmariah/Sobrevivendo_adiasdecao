
using System.Collections;
using UnityEngine;
using TMPro;

public class ComentarioBrinquedoPracaUI : MonoBehaviour
{
    public static ComentarioBrinquedoPracaUI Instance { get; private set; }

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
    }

    public void MostrarComentario(
        BrinquedoPraca.TipoBrinquedo tipoBrinquedo,
        float valor,
        bool brinquedoRuim)
    {
        if (texto == null || canvas == null || sandy == null)
        {
            Debug.LogWarning(
                "ComentarioBrinquedoPracaUI: configure Texto, Canvas e Sandy.",
                this
            );
            return;
        }

        string nome = ObterNome(tipoBrinquedo);
        string comentario = ObterComentario(tipoBrinquedo, brinquedoRuim);

        float valorExibido = brinquedoRuim ? -valor : valor;

        string pontos = valorExibido > 0
            ? "+" + valorExibido.ToString("0.##")
            : valorExibido.ToString("0.##");

        texto.text = nome + " " + pontos + "\n" + comentario;

        texto.color = Color.black;

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

    private string ObterNome(BrinquedoPraca.TipoBrinquedo tipo)
    {
        switch (tipo)
        {
            case BrinquedoPraca.TipoBrinquedo.BolaDePraia:
                return "Bola de praia";

            case BrinquedoPraca.TipoBrinquedo.Urso:
                return "Urso";

            case BrinquedoPraca.TipoBrinquedo.Sandalia:
                return "Sandália";

            case BrinquedoPraca.TipoBrinquedo.Graveto:
                return "Graveto";

            case BrinquedoPraca.TipoBrinquedo.Pato:
                return "Pato";

            case BrinquedoPraca.TipoBrinquedo.Bolinha:
                return "Bolinha";

            case BrinquedoPraca.TipoBrinquedo.Galinha:
                return "Galinha";

            case BrinquedoPraca.TipoBrinquedo.Polvo:
                return "Polvo";

            case BrinquedoPraca.TipoBrinquedo.Disco:
                return "Disco";

            default:
                return "Brinquedo";
        }
    }

    private string ObterComentario(
        BrinquedoPraca.TipoBrinquedo tipo,
        bool ruim)
    {
        if (ruim)
            return "Esse brinquedo não está em boas condições!";

        switch (tipo)
        {
            case BrinquedoPraca.TipoBrinquedo.BolaDePraia:
                return "Uma diversão para toda a comunidade!";

            case BrinquedoPraca.TipoBrinquedo.Urso:
                return "Um companheiro fofinho para brincar!";

            case BrinquedoPraca.TipoBrinquedo.Sandalia:
                return "Até uma sandália pode virar brinquedo!";

            case BrinquedoPraca.TipoBrinquedo.Graveto:
                return "Simples, mas muito divertido!";

            case BrinquedoPraca.TipoBrinquedo.Pato:
                return "Um brinquedo divertido para os cães!";

            case BrinquedoPraca.TipoBrinquedo.Bolinha:
                return "Perfeita para correr e brincar!";

            case BrinquedoPraca.TipoBrinquedo.Galinha:
                return "Mais um brinquedo para animar a turma!";

            case BrinquedoPraca.TipoBrinquedo.Polvo:
                return "Um brinquedo cheio de diversão!";

            case BrinquedoPraca.TipoBrinquedo.Disco:
                return "Hora de brincar de buscar!";

            default:
                return "Brinquedo coletado!";
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
