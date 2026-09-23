using UnityEngine;
using System.Collections;

public class ChefeCarrocinhaTutorial : MonoBehaviour
{
    public enum EstadoChefe
    {
        Fora,
        Patrulhando,
        Perseguindo,
        Assustado,
        LevandoSandy
    }

    [Header("Estado atual")]
    public EstadoChefe estadoAtual = EstadoChefe.Fora;

    public int chavesColetadas = 0;
    public int quantidadeNecessaria = 8;

    [Header("Movimento")]
    public float velocidadePatrulha = 2f;
    public float velocidadePerseguicao = 4f;
    public float velocidadeTransporte = 2f;

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

    [Header("Transporte da Sandy")]
    public Transform pontoGaiolaSandy;
    public Transform pontoTransporteSandy;
    public float distanciaGaiola = 0.3f;

    [Header("Tempo de saída")]
    public float tempoFora = 20f;

    [Header("Prefab da chave final")]
    public GameObject chaveFinalPrefab;

    private int pontoAtual = 0;
    private bool chefeAtivo = false;
    private bool podeCapturar = true;
    private bool transportandoSandy = false;

    private PlayerMovement movimentoSandy;
    private Transform paiOriginalSandy;

    private void Start()
    {
        estadoAtual = EstadoChefe.Fora;
        chefeAtivo = false;

        if (sandy != null)
        {
            movimentoSandy =
                sandy.GetComponent<PlayerMovement>();
        }
    }

    private void Update()
    {
        if (!chefeAtivo)
            return;

        if (sandy == null)
            return;

        switch (estadoAtual)
        {
            case EstadoChefe.Patrulhando:
                {
                    float distancia = Vector2.Distance(
                        transform.position,
                        sandy.position
                    );

                    VerificarSandy(distancia);
                    ExecutarPatrulha();

                    break;
                }

            case EstadoChefe.Perseguindo:
                {
                    float distancia = Vector2.Distance(
                        transform.position,
                        sandy.position
                    );

                    ExecutarPerseguicao(distancia);

                    break;
                }

            case EstadoChefe.Assustado:
                // O chefe permanece parado.
                break;

            case EstadoChefe.LevandoSandy:
                ExecutarTransporte();

                break;
        }
    }

    // =====================================================
    // ATUALIZAR CHAVES
    // =====================================================

    public void AtualizarChavesColetadas(int quantidade)
    {
        chavesColetadas = quantidade;

        Debug.Log(
            "Chaves coletadas: " + chavesColetadas
        );
    }

    // =====================================================
    // ATIVAR A CARROCINHA
    // =====================================================

    public void LiberarChefe()
    {
        chefeAtivo = true;
        podeCapturar = true;
        transportandoSandy = false;

        estadoAtual = EstadoChefe.Patrulhando;
        pontoAtual = 0;

        if (pontoInicioRonda != null)
        {
            transform.position =
                pontoInicioRonda.position;
        }

        Debug.Log(
            "Carrocinha iniciou a patrulha."
        );
    }

    // =====================================================
    // PATRULHA
    // =====================================================

    private void ExecutarPatrulha()
    {
        if (pontosPatrulha == null ||
            pontosPatrulha.Length == 0)
        {
            return;
        }

        Transform destino =
            pontosPatrulha[pontoAtual];

        MoverAte(
            destino.position,
            velocidadePatrulha
        );

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

    private void VerificarSandy(float distancia)
    {
        if (distancia <= distanciaVisao)
        {
            estadoAtual = EstadoChefe.Perseguindo;

            Debug.Log(
                "A carrocinha encontrou Sandy!"
            );
        }
    }

    // =====================================================
    // PERSEGUIÇÃO
    // =====================================================

    private void ExecutarPerseguicao(float distancia)
    {
        if (distancia > distanciaPerdaVisao)
        {
            estadoAtual = EstadoChefe.Patrulhando;

            Debug.Log(
                "Sandy escapou da visão. " +
                "Retornando à patrulha."
            );

            return;
        }

        MoverAte(
            sandy.position,
            velocidadePerseguicao
        );
    }

    // =====================================================
    // MOVIMENTO
    // =====================================================

    private void MoverAte(
        Vector3 destino,
        float velocidade
    )
    {
        Vector3 novaPosicao = Vector2.MoveTowards(
            transform.position,
            destino,
            velocidade * Time.deltaTime
        );

        transform.position = novaPosicao;

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

        if (estadoAtual != EstadoChefe.Perseguindo &&
            estadoAtual != EstadoChefe.Patrulhando)
        {
            return;
        }

        if (outro.CompareTag("Player"))
        {
            CapturarSandy();
        }
    }

    public void CapturarSandy()
    {
        if (!chefeAtivo ||
            !podeCapturar ||
            transportandoSandy)
        {
            return;
        }

        podeCapturar = false;
        transportandoSandy = true;

        estadoAtual = EstadoChefe.LevandoSandy;

        Debug.Log(
            "Sandy foi capturada! " +
            "A carrocinha vai levá-la à gaiola."
        );

        if (energiaSandy != null)
        {
            energiaSandy.PerderEnergia();
        }
        else
        {
            Debug.LogWarning(
                "EnergiaSandy não foi configurada."
            );
        }

        IniciarTransporte();
    }

    // =====================================================
    // INICIAR TRANSPORTE
    // =====================================================

    private void IniciarTransporte()
    {
        if (pontoGaiolaSandy == null)
        {
            Debug.LogError(
                "Ponto Gaiola Sandy não foi configurado."
            );

            FinalizarTransporte();
            return;
        }

        if (sandy == null)
        {
            Debug.LogError(
                "Sandy não foi configurada."
            );

            FinalizarTransporte();
            return;
        }

        if (movimentoSandy == null)
        {
            movimentoSandy =
                sandy.GetComponent<PlayerMovement>();
        }

        // Impede o jogador de movimentar Sandy.
        if (movimentoSandy != null)
        {
            movimentoSandy.enabled = false;
        }

        paiOriginalSandy = sandy.parent;

        // O ponto de transporte é opcional.
        // Se existir, Sandy ficará nesse ponto do chefe.
        if (pontoTransporteSandy != null)
        {
            sandy.SetParent(pontoTransporteSandy);

            sandy.localPosition = Vector3.zero;
        }
        else
        {
            sandy.SetParent(transform);

            sandy.localPosition = Vector3.zero;
        }

        Debug.Log(
            "Transporte da Sandy iniciado."
        );
    }

    // =====================================================
    // TRANSPORTAR SANDY ATÉ A GAIOLA
    // =====================================================

    private void ExecutarTransporte()
    {
        if (pontoGaiolaSandy == null)
        {
            FinalizarTransporte();
            return;
        }

        float distancia = Vector2.Distance(
            transform.position,
            pontoGaiolaSandy.position
        );

        if (distancia <= distanciaGaiola)
        {
            FinalizarTransporte();
            return;
        }

        MoverAte(
            pontoGaiolaSandy.position,
            velocidadeTransporte
        );
    }

    // =====================================================
    // FINALIZAR TRANSPORTE
    // =====================================================

    private void FinalizarTransporte()
    {
        transportandoSandy = false;

        if (sandy != null)
        {
            // Retira Sandy da hierarquia do chefe.
            sandy.SetParent(paiOriginalSandy);

            // Coloca Sandy na posição da gaiola.
            if (pontoGaiolaSandy != null)
            {
                sandy.position =
                    pontoGaiolaSandy.position;
            }
        }

        if (movimentoSandy != null)
        {
            movimentoSandy.enabled = true;
        }

        Debug.Log(
            "Sandy chegou à gaiola. " +
            "A carrocinha vai sair do cenário."
        );

        estadoAtual = EstadoChefe.Fora;

        StartCoroutine(
            SairPorTempoDeterminado()
        );
    }

    // =====================================================
    // SAIR POR 20 SEGUNDOS
    // =====================================================

    private IEnumerator SairPorTempoDeterminado()
    {
        chefeAtivo = false;

        if (pontoRetornoChefe != null)
        {
            transform.position =
                pontoRetornoChefe.position;
        }

        Debug.Log(
            "A carrocinha saiu por " +
            tempoFora + " segundos."
        );

        yield return new WaitForSeconds(
            tempoFora
        );

        if (sandy == null)
            yield break;

        chefeAtivo = true;
        podeCapturar = true;

        estadoAtual = EstadoChefe.Patrulhando;
        pontoAtual = 0;

        if (pontoInicioRonda != null)
        {
            transform.position =
                pontoInicioRonda.position;
        }

        Debug.Log(
            "A carrocinha retornou à patrulha."
        );
    }

    // =====================================================
    // ASSUSTAR O CHEFE
    // =====================================================

    public void AssustarChefe()
    {
        Debug.Log(
            "Tentativa de assustar a carrocinha."
        );

        if (!chefeAtivo)
        {
            Debug.Log(
                "A carrocinha ainda não está ativa."
            );

            return;
        }

        if (estadoAtual == EstadoChefe.LevandoSandy)
        {
            return;
        }

        if (sandy == null)
        {
            Debug.LogError(
                "Sandy não foi configurada."
            );

            return;
        }

        if (chaveFinalPrefab == null)
        {
            Debug.LogError(
                "A chave final não foi configurada."
            );

            return;
        }

        float distancia = Vector2.Distance(
            transform.position,
            sandy.position
        );

        if (distancia > distanciaVisao)
        {
            Debug.Log(
                "Sandy está longe demais para assustar."
            );

            return;
        }

        if (chavesColetadas < quantidadeNecessaria)
        {
            Debug.Log(
                "Ainda faltam chaves: " +
                chavesColetadas + "/" +
                quantidadeNecessaria
            );

            return;
        }

        if (estadoAtual == EstadoChefe.Assustado)
        {
            Debug.Log(
                "A carrocinha já está assustada."
            );

            return;
        }

        estadoAtual = EstadoChefe.Assustado;

        Debug.Log(
            "Sandy assustou a carrocinha!"
        );

        GameObject novaChave = Instantiate(
            chaveFinalPrefab,
            transform.position,
            Quaternion.identity
        );

        ChaveFinalTutorial chave =
            novaChave.GetComponent<ChaveFinalTutorial>();

        if (chave != null)
        {
            chave.SoltarChave(
                transform.position
            );
        }
        else
        {
            Debug.LogError(
                "O prefab da chave final não possui " +
                "o script ChaveFinalTutorial."
            );

            Destroy(novaChave);
        }

        CancelInvoke(
            nameof(VoltarDaAssustado)
        );

        Invoke(
            nameof(VoltarDaAssustado),
            3f
        );
    }

    // =====================================================
    // DESATIVAR CHEFE
    // =====================================================

    public void DesativarChefe()
    {
        chefeAtivo = false;

        estadoAtual = EstadoChefe.Fora;

        CancelInvoke();

        StopAllCoroutines();

        if (pontoRetornoChefe != null)
        {
            transform.position =
                pontoRetornoChefe.position;
        }

        Debug.Log(
            "A carrocinha saiu do cenário."
        );
    }

    // =====================================================
    // VOLTAR À PATRULHA APÓS O SUSTO
    // =====================================================

    private void VoltarDaAssustado()
    {
        if (!chefeAtivo)
            return;

        estadoAtual = EstadoChefe.Patrulhando;

        Debug.Log(
            "A carrocinha voltou à patrulha."
        );
    }
}