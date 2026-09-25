using UnityEngine;

public class MolhoChavesTutorial : MonoBehaviour
{
    [Header("Configuração das chaves")]
    [SerializeField] private int quantidadeTotal = 8;

    [Header("Prefab da chave")]
    [SerializeField] private GameObject chavePrefab;

    [Header("Local onde a chave aparece")]
    [SerializeField] private Transform pontoColeta;

    [Header("Referência da Sandy")]
    [SerializeField] private Transform sandy;

    [Header("Distância necessária para latir")]
    [SerializeField] private float distanciaMaxima = 3f;

    // Quantas chaves o molho já derrubou
    private int chavesDerrubadas = 0;

    // Existe uma chave esperando ser coletada?
    private bool chaveNoChao = false;

    // Sandy já coletou a chave, mas ainda não usou?
    private bool chaveEmUso = false;


    // =====================================================
    // RECEBER LATIDO
    // =====================================================

    public void ReceberLatido()
    {
        Debug.Log("Molho recebeu o latido.");

        if (sandy == null)
        {
            Debug.LogError("Sandy não foi configurada!");
            return;
        }

        if (chavePrefab == null)
        {
            Debug.LogError("Prefab da chave não foi configurado!");
            return;
        }

        if (pontoColeta == null)
        {
            Debug.LogError("Ponto de coleta não foi configurado!");
            return;
        }

        // -------------------------------------------------
        // JÁ LIBEROU AS 8 CHAVES
        // -------------------------------------------------

        if (chavesDerrubadas >= quantidadeTotal)
        {
            Debug.Log(
                "O molho já liberou todas as "
                + quantidadeTotal
                + " chaves."
            );

            return;
        }

        // -------------------------------------------------
        // AINDA EXISTE UMA CHAVE NO CHÃO
        // -------------------------------------------------

        if (chaveNoChao)
        {
            Debug.Log(
                "A chave atual ainda está no chão. "
                + "Sandy precisa coletá-la primeiro."
            );

            return;
        }

        // -------------------------------------------------
        // SANDY PEGOU A CHAVE, MAS AINDA NÃO USOU
        // -------------------------------------------------

        if (chaveEmUso)
        {
            Debug.Log(
                "Sandy ainda possui uma chave. "
                + "Use a chave para abrir uma gaiola antes de pegar outra."
            );

            return;
        }

        // -------------------------------------------------
        // VERIFICAR DISTÂNCIA
        // -------------------------------------------------

        float distancia = Vector2.Distance(
            sandy.position,
            transform.position
        );

        if (distancia > distanciaMaxima)
        {
            Debug.Log(
                "Sandy está longe demais do molho."
            );

            return;
        }

        // -------------------------------------------------
        // PODE DERRUBAR
        // -------------------------------------------------

        DerrubarChave();
    }


    // =====================================================
    // DERRUBAR CHAVE
    // =====================================================

    private void DerrubarChave()
    {
        GameObject novaChave = Instantiate(
            chavePrefab,
            pontoColeta.position,
            Quaternion.identity
        );

        ChaveComumTutorial chave =
            novaChave.GetComponent<ChaveComumTutorial>();

        if (chave == null)
        {
            Debug.LogError(
                "O prefab precisa ter o componente " +
                "ChaveComumTutorial."
            );

            Destroy(novaChave);
            return;
        }

        chave.Configurar(this);

        chaveNoChao = true;

        chavesDerrubadas++;

        Debug.Log(
            "Chave comum derrubada: "
            + chavesDerrubadas
            + "/"
            + quantidadeTotal
        );
    }


    // =====================================================
    // SANDY COLETOU A CHAVE
    // =====================================================

    public void RegistrarColeta()
    {
        if (!chaveNoChao)
        {
            Debug.LogWarning(
                "Nenhuma chave está aguardando coleta."
            );

            return;
        }

        chaveNoChao = false;

        // Sandy agora possui uma chave.
        // Ela precisa usá-la antes de pegar outra.
        chaveEmUso = true;

        if (TutorialChavesManager.instance != null)
        {
            TutorialChavesManager.instance.ColetarChaveComum();
        }

        Debug.Log(
            "Sandy coletou a chave. "
            + "Ela precisa usá-la antes de pegar outra."
        );
    }


    // =====================================================
    // CHAVE FOI USADA EM UMA GAIOLA
    // =====================================================

    public void RegistrarUsoDaChave()
    {
        if (!chaveEmUso)
        {
            Debug.LogWarning(
                "Não havia uma chave do molho em uso."
            );

            return;
        }

        chaveEmUso = false;

        Debug.Log(
            "A chave foi usada. "
            + "O molho pode liberar a próxima."
        );
    }


    // =====================================================
    // INFORMAÇÕES
    // =====================================================

    public bool TodasAsChavesForamLiberadas()
    {
        return chavesDerrubadas >= quantidadeTotal;
    }

    public bool SandyPossuiChave()
    {
        return chaveEmUso;
    }
}