using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ConfetiUIManager : MonoBehaviour
{
    public static ConfetiUIManager instance;

    [Header("Configurações dos Confetes")]
    [Tooltip("Quantidade de pedaços de confete a gerar.")]
    [SerializeField] private int quantidadeConfetes = 60;

    [Tooltip("Área horizontal da tela de onde os confetes caem.")]
    [SerializeField] private float larguraEmissao = 1200f;

    [Tooltip("Velocidade mínima e máxima de queda.")]
    [SerializeField] private float velocidadeMin = 300f;
    [SerializeField] private float velocidadeMax = 600f;

    [Tooltip("Cores sorteadas para os confetes.")]
    [SerializeField]
    private Color[] coresConfeti = new Color[]
    {
        Color.red, Color.yellow, Color.cyan, Color.green, Color.magenta, new Color(1f, 0.5f, 0f)
    };

    private List<GameObject> confetesAtivos = new List<GameObject>();
    private bool gerando = false;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void SoltarConfetes()
    {
        LimparConfetes();
        StartCoroutine(GerarESimularConfetes());
    }

    public void LimparConfetes()
    {
        StopAllCoroutines();
        gerando = false;

        foreach (var c in confetesAtivos)
        {
            if (c != null) Destroy(c);
        }
        confetesAtivos.Clear();
    }

    private IEnumerator GerarESimularConfetes()
    {
        gerando = true;

        for (int i = 0; i < quantidadeConfetes; i++)
        {
            if (!gerando) yield break;

            CriarPedacoConfete();
            yield return new WaitForSecondsRealtime(0.02f); // Gera gradualmente
        }
    }

    private void CriarPedacoConfete()
    {
        // Cria um novo UI GameObject
        GameObject pedaco = new GameObject("ConfetiUI", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        pedaco.transform.SetParent(transform, false); // Coloca dentro da UI atual

        // Garante que fique na frente de tudo no mesmo Canvas
        pedaco.transform.SetAsLastSibling();

        RectTransform rect = pedaco.GetComponent<RectTransform>();
        Image img = pedaco.GetComponent<Image>();

        // Cor e tamanho aleatório
        img.color = coresConfeti[Random.Range(0, coresConfeti.Length)];
        float tamanho = Random.Range(10f, 22f);
        rect.sizeDelta = new Vector2(tamanho, tamanho * Random.Range(0.6f, 1.2f));

        // Posição inicial no topo da tela
        float xAleatorio = Random.Range(-larguraEmissao / 2f, larguraEmissao / 2f);
        rect.anchoredPosition = new Vector2(xAleatorio, 600f);

        confetesAtivos.Add(pedaco);

        // Dispara o movimento individual do confete
        StartCoroutine(MoverConfete(rect, pedaco));
    }

    private IEnumerator MoverConfete(RectTransform rect, GameObject obj)
    {
        float velQueda = Random.Range(velocidadeMin, velocidadeMax);
        float velRotacao = Random.Range(-300f, 300f);
        float velBalanco = Random.Range(2f, 6f);
        float amplitudeBalanco = Random.Range(20f, 50f);
        float tempo = 0f;

        Vector2 posInicial = rect.anchoredPosition;

        while (rect != null && rect.anchoredPosition.y > -700f)
        {
            tempo += Time.unscaledDeltaTime;

            // Queda vertical + balanço horizontal (efeito folhas caindo)
            float novoY = posInicial.y - (velQueda * tempo);
            float novoX = posInicial.x + Mathf.Sin(tempo * velBalanco) * amplitudeBalanco;

            rect.anchoredPosition = new Vector2(novoX, novoY);
            rect.Rotate(0, 0, velRotacao * Time.unscaledDeltaTime);

            yield return null;
        }

        if (obj != null)
        {
            confetesAtivos.Remove(obj);
            Destroy(obj);
        }
    }
}