using UnityEngine;

public class MolhoChavesTutorial : MonoBehaviour
{
    [Header("Configuração das chaves")]
    [SerializeField] private int quantidadeTotal = 8;
    [SerializeField] private int chavesColetadas = 0;

    [Header("Prefab da chave")]
    [SerializeField] private GameObject chavePrefab;

    [Header("Local onde a chave aparece")]
    [SerializeField] private Transform pontoColeta;

    [Header("Referência da Sandy")]
    [SerializeField] private Transform sandy;

    [Header("Distância necessária para latir")]
    [SerializeField] private float distanciaMaxima = 3f;

    private bool chaveNoChao = false;

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

        if (chavesColetadas >= quantidadeTotal)
        {
            Debug.Log("Todas as chaves já foram coletadas.");
            return;
        }

        if (chaveNoChao)
        {
            Debug.Log("Colete a chave atual antes de derrubar outra.");
            return;
        }

        float distancia = Vector2.Distance(
            sandy.position,
            transform.position
        );

        if (distancia > distanciaMaxima)
        {
            Debug.Log("Sandy está longe demais do molho.");
            return;
        }

        DerrubarChave();
    }

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

        Debug.Log("Uma chave foi derrubada.");
    }

    public void RegistrarColeta()
    {
        if (!chaveNoChao)
        {
            Debug.LogWarning("Nenhuma chave está aguardando coleta.");
            return;
        }

        chaveNoChao = false;

        if (TutorialChavesManager.instance != null)
        {
            TutorialChavesManager.instance.ColetarChaveComum();
        }

        Debug.Log("Chave comum coletada!");
    }

    public int ObterChavesColetadas()
    {
        return chavesColetadas;
    }

    public bool TodasAsChavesForamColetadas()
    {
        return chavesColetadas >= quantidadeTotal;
    }
}