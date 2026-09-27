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

    // =====================================================
    // START
    // =====================================================

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

        if (fundoBarra != null)
        {
            fundoBarra.SetActive(true);
        }

        if (barraProgresso != null)
        {
            barraProgresso.gameObject.SetActive(true);
            barraProgresso.fillAmount = 0f;
        }
    }

    // =====================================================
    // UPDATE
    // =====================================================

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

        // -------------------------------------------------
        // SANDY ESTÁ LONGE
        // -------------------------------------------------

        if (!sandyPerto)
        {
            pressionando = false;
            EsconderInteracao();
            return;
        }

        // -------------------------------------------------
        // SANDY ESTÁ PERTO
        // -------------------------------------------------

        MostrarInteracao();

        // Começou a segurar o botão
        if (Input.GetMouseButtonDown(0))
        {
            pressionando = true;
            MostrarBarra();
        }

        // Continua segurando
        if (Input.GetMouseButton(0) && pressionando)
        {
            AumentarProgresso();
        }

        // Soltou o botão
        if (Input.GetMouseButtonUp(0))
        {
            pressionando = false;
        }

        // Se não estiver segurando, a barra diminui
        if (!pressionando)
        {
            DiminuirProgresso();
        }
    }

    // =====================================================
    // MOSTRAR INTERAÇÃO
    // =====================================================

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

    // =====================================================
    // ESCONDER INTERAÇÃO
    // =====================================================

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

    // =====================================================
    // MOSTRAR BARRA
    // =====================================================

    private void MostrarBarra()
    {
        if (fundoBarra != null)
        {
            fundoBarra.SetActive(true);
        }

        if (barraProgresso != null)
        {
            barraProgresso.gameObject.SetActive(true);
            barraProgresso.fillAmount = progresso;
        }
    }

    // =====================================================
    // AUMENTAR PROGRESSO
    // =====================================================

    private void AumentarProgresso()
    {
        if (TutorialChavesManager.instance == null)
        {
            Debug.LogError(
                "TutorialChavesManager não foi encontrado."
            );

            return;
        }

        // -------------------------------------------------
        // VERIFICA SE SANDY POSSUI UMA CHAVE
        // -------------------------------------------------

        if (!TutorialChavesManager.instance.TemChaveDisponivel())
        {
            if (textoInteracao != null)
            {
                textoInteracao.text = "VOCÊ NÃO TEM CHAVE";
            }

            return;
        }

        // -------------------------------------------------
        // AUMENTA A BARRA
        // -------------------------------------------------

        progresso += Time.deltaTime / tempoParaAbrir;

        progresso = Mathf.Clamp01(progresso);

        if (barraProgresso != null)
        {
            barraProgresso.fillAmount = progresso;
        }

        // -------------------------------------------------
        // COMPLETOU
        // -------------------------------------------------

        if (progresso >= 1f)
        {
            TentarAbrirGaiola();
        }
    }

    // =====================================================
    // DIMINUIR PROGRESSO
    // =====================================================

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

    // =====================================================
    // TENTAR ABRIR
    // =====================================================

    private void TentarAbrirGaiola()
    {
        if (gaiolaAberta)
        {
            return;
        }

        bool abriu =
            TutorialChavesManager.instance
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

    // =====================================================
    // ABRIR GAIOLA
    // =====================================================

    private void AbrirGaiola()
    {
        gaiolaAberta = true;
        pressionando = false;

        // -------------------------------------------------
        // ESCONDE INTERAÇÃO
        // -------------------------------------------------

        if (painelInteracao != null)
        {
            painelInteracao.SetActive(false);
        }

        // -------------------------------------------------
        // ZERA A BARRA
        // -------------------------------------------------

        if (barraProgresso != null)
        {
            barraProgresso.fillAmount = 0f;
        }

        // -------------------------------------------------
        // ABRE AS GRADES
        // -------------------------------------------------

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

        // -------------------------------------------------
        // LIBERTA O CÃO
        // -------------------------------------------------

        if (cao != null)
        {
            cao.SetActive(true);
        }

        Debug.Log(
            "Gaiola aberta! Cão libertado."
        );

        // =================================================
        // VERIFICAÇÃO DE VITÓRIA
        // =================================================

        if (TutorialManager.instance != null)
        {
            TutorialManager.instance.VerificarVitoria();
        }
    }
}