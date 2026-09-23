using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GaiolaTutorial : MonoBehaviour
{
    [Header("Referências")]
    public Transform sandy;
    public Transform pontoInteracao;

    [Header("Elementos da gaiola")]
    public GameObject[] grades;
    public GameObject cao;

    [Header("Interface de interação")]
    public GameObject painelInteracao;
    public TextMeshProUGUI textoInteracao;

    [Header("Interface da barra")]
    public GameObject fundoBarra;
    public Image barraProgresso;

    [Header("Configuração")]
    public float distanciaInteracao = 2f;
    public float tempoParaAbrir = 1.5f;

    private float progresso = 0f;
    private bool gaiolaAberta = false;
    private bool sandyPerto = false;
    private bool pressionando = false;

    private void Start()
    {
        if (cao != null)
        {
            cao.SetActive(false);
        }

        if (painelInteracao != null)
        {
            painelInteracao.SetActive(false);
        }

        // O fundo permanece completo.
        if (fundoBarra != null)
        {
            fundoBarra.SetActive(true);
        }

        // O preenchimento começa vazio.
        if (barraProgresso != null)
        {
            barraProgresso.gameObject.SetActive(true);
            barraProgresso.fillAmount = 0f;
        }
    }

    private void Update()
    {
        if (gaiolaAberta || sandy == null)
        {
            return;
        }

        Vector3 origemInteracao = transform.position;

        if (pontoInteracao != null)
        {
            origemInteracao = pontoInteracao.position;
        }

        float distancia = Vector2.Distance(
            origemInteracao,
            sandy.position
        );

        sandyPerto = distancia <= distanciaInteracao;

        if (!sandyPerto)
        {
            pressionando = false;
            EsconderInteracao();
            return;
        }

        MostrarInteracao();

        // Começa a interação ao pressionar o botão esquerdo.
        if (Input.GetMouseButtonDown(0))
        {
            pressionando = true;
            MostrarBarra();
        }

        // Preenche a barra enquanto o botão estiver pressionado.
        if (Input.GetMouseButton(0) && pressionando)
        {
            AumentarProgresso();
        }

        // Para de preencher ao soltar o botão.
        if (Input.GetMouseButtonUp(0))
        {
            pressionando = false;
        }

        // Diminui o progresso quando o jogador não está pressionando.
        if (!pressionando)
        {
            DiminuirProgresso();
        }
    }

    private void MostrarInteracao()
    {
        if (painelInteracao != null &&
            !painelInteracao.activeSelf)
        {
            painelInteracao.SetActive(true);
        }

        if (textoInteracao != null)
        {
            textoInteracao.text = "SEGURE O CLIQUE";
        }
    }

    private void EsconderInteracao()
    {
        if (painelInteracao != null)
        {
            painelInteracao.SetActive(false);
        }

        progresso = 0f;
        pressionando = false;

        if (barraProgresso != null)
        {
            barraProgresso.fillAmount = 0f;
        }
    }

    private void MostrarBarra()
    {
        // O fundo continua completo.
        if (fundoBarra != null)
        {
            fundoBarra.SetActive(true);
        }

        // A barra começa no progresso atual.
        if (barraProgresso != null)
        {
            barraProgresso.gameObject.SetActive(true);
            barraProgresso.fillAmount = progresso;
        }
    }

    private void AumentarProgresso()
    {
        if (TutorialChavesManager.instance == null)
        {
            Debug.LogError(
                "TutorialChavesManager não foi encontrado."
            );

            return;
        }

        if (!TutorialChavesManager.instance.TemChaveDisponivel())
        {
            if (textoInteracao != null)
            {
                textoInteracao.text = "VOCÊ NÃO TEM CHAVE";
            }

            return;
        }

        progresso += Time.deltaTime / tempoParaAbrir;
        progresso = Mathf.Clamp01(progresso);

        if (barraProgresso != null)
        {
            barraProgresso.fillAmount = progresso;
        }

        if (progresso >= 1f)
        {
            TentarAbrirGaiola();
        }
    }

    private void DiminuirProgresso()
    {
        if (progresso <= 0f)
        {
            return;
        }

        progresso -= Time.deltaTime * 2f;
        progresso = Mathf.Clamp01(progresso);

        if (barraProgresso != null)
        {
            barraProgresso.fillAmount = progresso;
        }
    }

    private void TentarAbrirGaiola()
    {
        if (gaiolaAberta)
        {
            return;
        }

        bool abriu = TutorialChavesManager.instance
            .UsarChaveParaAbrirGaiola();

        if (!abriu)
        {
            progresso = 0f;

            if (barraProgresso != null)
            {
                barraProgresso.fillAmount = 0f;
            }

            return;
        }

        AbrirGaiola();
    }

    private void AbrirGaiola()
    {
        gaiolaAberta = true;
        pressionando = false;

        if (painelInteracao != null)
        {
            painelInteracao.SetActive(false);
        }

        if (barraProgresso != null)
        {
            barraProgresso.fillAmount = 0f;
        }

        if (grades != null)
        {
            foreach (GameObject grade in grades)
            {
                if (grade != null)
                {
                    grade.SetActive(false);
                }
            }
        }

        if (cao != null)
        {
            cao.SetActive(true);
        }

        Debug.Log("Gaiola aberta! Cão libertado.");
    }
}