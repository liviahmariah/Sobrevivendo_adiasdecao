
using UnityEngine;
using System.Collections;

public class MolhoChavesTutorial : MonoBehaviour
{
    [Header("Configuração das chaves")]
    [SerializeField] private int quantidadeTotal = 8;

    [Header("Prefab da chave liberada")]
    [SerializeField] private GameObject chavePrefab;

    [Header("Local onde a chave aparece")]
    [SerializeField] private Transform pontoColeta;

    [Header("Referência da Sandy")]
    [SerializeField] private Transform sandy;

    [Header("Distância necessária para latir")]
    [SerializeField] private float distanciaMaxima = 3f;

    [Header("Visual do molho")]
    [SerializeField] private SpriteRenderer spriteMolho;

    [Tooltip("Arraste aqui o filho visual do molho, não o objeto principal.")]
    [SerializeField] private Transform visualBalanco;

    [Tooltip("Índice 0 = molho vazio; índice 8 = molho com 8 chaves.")]
    [SerializeField] private Sprite[] spritesPorChavesRestantes;

    [Header("Animação de balanço")]
    [SerializeField] private float duracaoBalanco = 0.35f;

    [SerializeField] private float anguloBalanco = 18f;

    [SerializeField] private float velocidadeBalanco = 35f;

    private int chavesDerrubadas = 0;

    private bool chaveNoChao = false;
    private bool chaveEmUso = false;

    private Coroutine rotinaBalanco;
    private Quaternion rotacaoOriginal;

    private void Start()
    {
        if (visualBalanco == null && spriteMolho != null)
            visualBalanco = spriteMolho.transform;

        if (visualBalanco != null)
            rotacaoOriginal = visualBalanco.localRotation;

        AtualizarVisualMolho();
    }

    // =====================================================
    // RECEBER LATIDO
    // =====================================================

    public void ReceberLatido()
    {
        // O molho reage a cada chamada de latido.
        AnimarBalanco();

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

        if (chavesDerrubadas >= quantidadeTotal)
        {
            Debug.Log("O molho já liberou todas as chaves.");
            return;
        }

        if (chaveNoChao)
        {
            Debug.Log("Sandy precisa coletar a chave no chão.");
            return;
        }

        if (chaveEmUso)
        {
            Debug.Log("Sandy precisa usar a chave na gaiola.");
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

    // =====================================================
    // ANIMAÇÃO DO MOLHO
    // =====================================================

    private void AnimarBalanco()
    {
        if (visualBalanco == null)
            return;

        if (rotinaBalanco != null)
            StopCoroutine(rotinaBalanco);

        // Garante que a animação sempre comece da mesma posição.
        visualBalanco.localRotation = rotacaoOriginal;

        rotinaBalanco = StartCoroutine(ExecutarBalanco());
    }

    private IEnumerator ExecutarBalanco()
    {
        float tempo = 0f;

        while (tempo < duracaoBalanco)
        {
            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(
                tempo / duracaoBalanco
            );

            // Começa forte e termina suavemente.
            float intensidade = 1f - progresso;

            float angulo = Mathf.Sin(
                tempo * velocidadeBalanco
            ) * anguloBalanco * intensidade;

            visualBalanco.localRotation =
                rotacaoOriginal *
                Quaternion.Euler(0f, 0f, angulo);

            yield return null;
        }

        visualBalanco.localRotation = rotacaoOriginal;
        rotinaBalanco = null;
    }

    // =====================================================
    // ATUALIZAR APARÊNCIA DO MOLHO
    // =====================================================

    private void AtualizarVisualMolho()
    {
        if (spriteMolho == null ||
            spritesPorChavesRestantes == null)
            return;

        int restantes = Mathf.Clamp(
            quantidadeTotal - chavesDerrubadas,
            0,
            spritesPorChavesRestantes.Length - 1
        );

        Sprite novoSprite = spritesPorChavesRestantes[restantes];

        if (novoSprite != null)
            spriteMolho.sprite = novoSprite;
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
                "O prefab precisa ter ChaveComumTutorial."
            );

            Destroy(novaChave);
            return;
        }

        chave.Configurar(this);

        chaveNoChao = true;
        chavesDerrubadas++;

        // Cada chave liberada muda a aparência do molho.
        AtualizarVisualMolho();

        Debug.Log(
            "Chave liberada: " + chavesDerrubadas +
            "/" + quantidadeTotal
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
        chaveEmUso = true;

        if (TutorialChavesManager.instance != null)
        {
            TutorialChavesManager.instance.ColetarChaveComum();
        }

        Debug.Log(
            "Sandy coletou a chave e precisa usá-la."
        );
    }

    // =====================================================
    // CHAVE USADA EM UMA GAIOLA
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
            "Chave usada. O molho pode liberar outra."
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

    private void OnDisable()
    {
        if (rotinaBalanco != null)
        {
            StopCoroutine(rotinaBalanco);
            rotinaBalanco = null;
        }

        if (visualBalanco != null)
            visualBalanco.localRotation = rotacaoOriginal;
    }
}
