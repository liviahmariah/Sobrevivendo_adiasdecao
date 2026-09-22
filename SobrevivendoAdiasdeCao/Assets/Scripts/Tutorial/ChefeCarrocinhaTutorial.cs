
using UnityEngine;

public class ChefeCarrocinhaTutorial : MonoBehaviour
{
    public enum EstadoChefe
    {
        Fora,
        Patrulhando,
        Perseguindo,
        Assustado
    }

    [Header("Estado atual")]
    public EstadoChefe estadoAtual = EstadoChefe.Fora;

    public int chavesColetadas = 0;
    public int quantidadeNecessaria = 8;

    [Header("Movimento")]
    public float velocidadePatrulha = 2f;
    public float velocidadePerseguicao = 4f;

    [Header("Perseguição")]
    public Transform sandy;
    public float distanciaVisao = 7f;
    public float distanciaPerdaVisao = 10f;

    [Header("Pontos da patrulha")]
    public Transform[] pontosPatrulha;
    public float distanciaDoPonto = 0.2f;

    [Header("Referências")]
    public EnergiaSandy energiaSandy;
    public Transform pontoInicioRonda;
    public Transform pontoRetornoChefe;

    private int pontoAtual = 0;
    private bool chefeAtivo = false;
    private bool podeCapturar = true;

    [Header("Chave final")]
    public ChaveFinalTutorial chaveFinal;

    void Start()
    {
        estadoAtual = EstadoChefe.Fora;
        chefeAtivo = false;
    }

    void Update()
    {
        if (!chefeAtivo)
            return;

        if (sandy == null)
            return;

        float distancia = Vector2.Distance(transform.position, sandy.position);

        switch (estadoAtual)
        {
            case EstadoChefe.Patrulhando:
                VerificarSandy(distancia);
                ExecutarPatrulha();
                break;

            case EstadoChefe.Perseguindo:
                ExecutarPerseguicao(distancia);
                break;

            case EstadoChefe.Assustado:
                // O chefe fica parado enquanto está assustado.
                break;
        }
    }

    public void AtualizarChavesColetadas(int quantidade)
    {
        chavesColetadas = quantidade;

        Debug.Log("Chaves coletadas: " + chavesColetadas);
    }
    // =====================================================
    // ATIVAR A CARROCINHA
    // =====================================================

    public void LiberarChefe()
    {
        chefeAtivo = true;
        podeCapturar = true;

        estadoAtual = EstadoChefe.Patrulhando;
        pontoAtual = 0;

        if (pontoInicioRonda != null)
        {
            transform.position = pontoInicioRonda.position;
        }

        Debug.Log("Carrocinha iniciou a patrulha.");
    }

    // =====================================================
    // PATRULHA
    // =====================================================

    void ExecutarPatrulha()
    {
        if (pontosPatrulha == null || pontosPatrulha.Length == 0)
            return;

        Transform destino = pontosPatrulha[pontoAtual];

        MoverAte(destino.position, velocidadePatrulha);

        float distancia = Vector2.Distance(
            transform.position,
            destino.position
        );

        if (distancia <= distanciaDoPonto)
        {
            pontoAtual++;

            if (pontoAtual >= pontosPatrulha.Length)
            {
                pontoAtual = 0;
            }
        }
    }

    // =====================================================
    // DETECTAR SANDY
    // =====================================================

    void VerificarSandy(float distancia)
    {
        if (distancia <= distanciaVisao)
        {
            estadoAtual = EstadoChefe.Perseguindo;

            Debug.Log("A carrocinha encontrou Sandy!");
        }
    }

    // =====================================================
    // PERSEGUIÇÃO
    // =====================================================

    void ExecutarPerseguicao(float distancia)
    {
        if (distancia > distanciaPerdaVisao)
        {
            estadoAtual = EstadoChefe.Patrulhando;

            Debug.Log("Sandy escapou da visão. Retornando à patrulha.");

            return;
        }

        MoverAte(sandy.position, velocidadePerseguicao);
    }

    // =====================================================
    // MOVIMENTO
    // =====================================================

    void MoverAte(Vector3 destino, float velocidade)
    {
        Vector3 novaPosicao = Vector2.MoveTowards(
            transform.position,
            destino,
            velocidade * Time.deltaTime
        );

        transform.position = novaPosicao;

        // Vira o sprite na direção do movimento.
        if (destino.x > transform.position.x)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }
        else if (destino.x < transform.position.x)
        {
            transform.localScale = new Vector3(
                -Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }

    // =====================================================
    // CAPTURAR SANDY
    // =====================================================

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (!chefeAtivo)
            return;

        if (!podeCapturar)
            return;

        if (outro.CompareTag("Player"))
        {
            CapturarSandy();
        }
    }

    public void CapturarSandy()
    {
        if (!chefeAtivo || !podeCapturar)
            return;

        podeCapturar = false;

        Debug.Log("Sandy foi capturada!");

        if (energiaSandy != null)
        {
            energiaSandy.PerderEnergia();
        }
        else
        {
            Debug.LogWarning("EnergiaSandy não foi configurada.");
        }

        Invoke(nameof(PermitirNovaCaptura), 1.5f);
    }

    void PermitirNovaCaptura()
    {
        podeCapturar = true;
    }

    // =====================================================
    // ASSUSTAR O CHEFE
    // =====================================================

    public void AssustarChefe()
    {
        if (!chefeAtivo)
            return;

        if (sandy == null)
            return;

        float distancia = Vector2.Distance(
            transform.position,
            sandy.position
        );

        if (distancia > distanciaVisao)
        {
            Debug.Log("Sandy latiu, mas está longe demais.");
            return;
        }

        estadoAtual = EstadoChefe.Assustado;

        Debug.Log("Sandy assustou a carrocinha!");

        if (chavesColetadas < quantidadeNecessaria)
        {
            Debug.Log(
                "Ainda faltam chaves! Coletadas: "
                + chavesColetadas
                + "/"
                + quantidadeNecessaria
            );

            return;
        }

        if (chaveFinal != null)
        {
            chaveFinal.SoltarChave(transform.position);
        }
        else
        {
            Debug.LogWarning("Configure a chave final.");
        }

        CancelInvoke(nameof(VoltarDaAssustado));
        Invoke(nameof(VoltarDaAssustado), 3f);
    }

    // =====================================================
    // DESATIVAR
    // =====================================================

    public void DesativarChefe()
    {
        chefeAtivo = false;
        estadoAtual = EstadoChefe.Fora;

        CancelInvoke();

        if (pontoRetornoChefe != null)
        {
            transform.position = pontoRetornoChefe.position;
        }

        Debug.Log("A carrocinha saiu do cenário.");
    }

    // =====================================================
    // VOLTAR À PATRULHA APÓS O SUSTO
    // =====================================================

    void VoltarDaAssustado()
    {
        if (!chefeAtivo)
            return;

        estadoAtual = EstadoChefe.Patrulhando;

        Debug.Log("A carrocinha voltou à patrulha.");
    }
}