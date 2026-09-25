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

    [Header("Chaves")]
    public int chavesColetadas = 0;
    public int quantidadeNecessaria = 8;

    [Header("Tempo da dinâmica")]
    public float tempoPatrulha = 20f;
    public float tempoFora = 20f;

    [Header("Movimento")]
    public float velocidadePatrulha = 2f;
    public float velocidadePerseguicao = 4f;
    public float velocidadeTransporte = 2f;

    [Header("Perseguição")]
    public Transform sandy;
    public float distanciaVisao = 7f;
    public float distanciaPerdaVisao = 10f;
    public float distanciaCaptura = 1.2f;

    [Header("Pontos da patrulha")]
    public Transform[] pontosPatrulha;
    public float distanciaDoPonto = 0.2f;

    [Header("Referências")]
    public EnergiaSandy energiaSandy;
    public Transform pontoInicioRonda;
    public Transform pontoRetornoChefe;

    [Header("Gaiola da Sandy")]
    public Transform pontoGaiolaSandy;
    public float raioAreaSegura = 2f;

    [Header("Transporte da Sandy")]
    public Transform pontoTransporteSandy;
    public float distanciaGaiola = 0.3f;

    [Header("Timer da interface")]
    public TutorialTimer tutorialTimer;

    [Header("Prefab da chave final")]
    public GameObject chaveFinalPrefab;

    private int pontoAtual = 0;

    private bool chefeAtivo = false;
    private bool podeCapturar = true;
    private bool transportandoSandy = false;

    private PlayerMovement movimentoSandy;
    private Transform paiOriginalSandy;

    private Coroutine cicloChefeCoroutine;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        estadoAtual = EstadoChefe.Fora;
        chefeAtivo = false;
        podeCapturar = true;
        transportandoSandy = false;

        if (sandy != null)
        {
            movimentoSandy =
                sandy.GetComponent<PlayerMovement>();
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!chefeAtivo)
            return;

        if (sandy == null)
            return;

        switch (estadoAtual)
        {
            // -------------------------------------------------
            // PATRULHA
            // -------------------------------------------------

            case EstadoChefe.Patrulhando:
                {
                    float distancia = Vector2.Distance(
                        transform.position,
                        sandy.position
                    );

                    VerificarSandy(distancia);

                    // Se VerificarSandy colocou o chefe
                    // em perseguição, não executa a patrulha
                    // neste frame.
                    if (estadoAtual != EstadoChefe.Perseguindo)
                    {
                        ExecutarPatrulha();
                    }

                    break;
                }


            // -------------------------------------------------
            // PERSEGUIÇÃO
            // -------------------------------------------------

            case EstadoChefe.Perseguindo:
                {
                    float distancia = Vector2.Distance(
                        transform.position,
                        sandy.position
                    );

                    ExecutarPerseguicao(distancia);

                    if (estadoAtual == EstadoChefe.Perseguindo)
                    {
                        VerificarCapturaPorDistancia();
                    }

                    break;
                }


            // -------------------------------------------------
            // ASSUSTADO
            // -------------------------------------------------

            case EstadoChefe.Assustado:
                break;


            // -------------------------------------------------
            // LEVANDO SANDY
            // -------------------------------------------------

            case EstadoChefe.LevandoSandy:
                ExecutarTransporte();
                break;
        }
    }


    // =========================================================
    // CHAVES
    // =========================================================

    public void AtualizarChavesColetadas(int quantidade)
    {
        chavesColetadas = quantidade;

        Debug.Log(
            "Chaves coletadas: " +
            chavesColetadas
        );
    }


    // =========================================================
    // LIBERAR CHEFE
    // =========================================================

    public void LiberarChefe()
    {
        // Se já existe um ciclo rodando,
        // não cria outro.
        if (cicloChefeCoroutine != null)
        {
            StopCoroutine(cicloChefeCoroutine);
        }

        chefeAtivo = true;
        podeCapturar = true;
        transportandoSandy = false;

        estadoAtual =
            EstadoChefe.Patrulhando;

        pontoAtual = 0;

        if (pontoInicioRonda != null)
        {
            transform.position =
                pontoInicioRonda.position;
        }

        Debug.Log(
            "A carrocinha voltou e iniciou " +
            "uma nova ronda de 20 segundos."
        );

        cicloChefeCoroutine =
            StartCoroutine(CicloDoChefe());
    }


    // =========================================================
    // CICLO:
    // 20s PATRULHA
    // 20s FORA
    // =========================================================

    private IEnumerator CicloDoChefe()
    {
        // -----------------------------------------------------
        // PATRULHA
        // -----------------------------------------------------

        float tempoRestante = tempoPatrulha;

        while (
            tempoRestante > 0f &&
            chefeAtivo
        )
        {
            AtualizarTimerUI(
                "CHEFE EM RONDA",
                tempoRestante
            );

            tempoRestante -= Time.deltaTime;

            yield return null;
        }


        // -----------------------------------------------------
        // SE SANDY AINDA ESTÁ SENDO TRANSPORTADA,
        // ESPERA O TRANSPORTE TERMINAR.
        //
        // O tempo NÃO é reiniciado.
        // -----------------------------------------------------

        while (transportandoSandy)
        {
            yield return null;
        }


        if (!chefeAtivo)
        {
            cicloChefeCoroutine = null;
            yield break;
        }


        // -----------------------------------------------------
        // TERMINOU A RONDA
        // -----------------------------------------------------

        chefeAtivo = false;
        podeCapturar = false;

        estadoAtual =
            EstadoChefe.Fora;

        if (pontoRetornoChefe != null)
        {
            transform.position =
                pontoRetornoChefe.position;
        }

        Debug.Log(
            "A ronda terminou. " +
            "A carrocinha saiu por " +
            tempoFora +
            " segundos."
        );


        // -----------------------------------------------------
        // 20 SEGUNDOS FORA
        // -----------------------------------------------------

        tempoRestante = tempoFora;

        while (tempoRestante > 0f)
        {
            AtualizarTimerUI(
                "CHEFE FORA",
                tempoRestante
            );

            tempoRestante -= Time.deltaTime;

            yield return null;
        }


        // -----------------------------------------------------
        // VOLTA
        // -----------------------------------------------------

        if (sandy == null)
        {
            cicloChefeCoroutine = null;
            yield break;
        }

        chefeAtivo = true;
        podeCapturar = true;
        transportandoSandy = false;

        estadoAtual =
            EstadoChefe.Patrulhando;

        pontoAtual = 0;

        if (pontoInicioRonda != null)
        {
            transform.position =
                pontoInicioRonda.position;
        }

        Debug.Log(
            "A carrocinha voltou. " +
            "Uma nova ronda de 20 segundos começou."
        );

        cicloChefeCoroutine = null;

        // Inicia novo ciclo.
        cicloChefeCoroutine =
            StartCoroutine(CicloDoChefe());
    }


    // =========================================================
    // TIMER DA UI
    // =========================================================

    private void AtualizarTimerUI(
        string mensagem,
        float tempo
    )
    {
        if (tutorialTimer != null)
        {
            tutorialTimer.MostrarTimerChefe(
                mensagem,
                tempo
            );
        }
    }


    // =========================================================
    // PATRULHA
    // =========================================================

    private void ExecutarPatrulha()
    {
        if (
            pontosPatrulha == null ||
            pontosPatrulha.Length == 0
        )
        {
            return;
        }

        if (
            pontoAtual < 0 ||
            pontoAtual >= pontosPatrulha.Length
        )
        {
            pontoAtual = 0;
        }

        Transform destino =
            pontosPatrulha[pontoAtual];

        if (destino == null)
        {
            return;
        }

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

            if (
                pontoAtual >=
                pontosPatrulha.Length
            )
            {
                pontoAtual = 0;
            }
        }
    }


    // =========================================================
    // VERIFICAR SANDY
    // =========================================================

    private void VerificarSandy(
        float distancia
    )
    {
        // Se Sandy está dentro da gaiola,
        // o chefe ignora completamente.
        if (SandyEstaNaAreaSegura())
        {
            return;
        }

        if (distancia <= distanciaVisao)
        {
            estadoAtual =
                EstadoChefe.Perseguindo;

            Debug.Log(
                "A carrocinha encontrou Sandy!"
            );
        }
    }


    // =========================================================
    // ÁREA SEGURA
    // =========================================================

    private bool SandyEstaNaAreaSegura()
    {
        if (sandy == null)
            return false;

        if (pontoGaiolaSandy == null)
            return false;

        float distancia = Vector2.Distance(
            sandy.position,
            pontoGaiolaSandy.position
        );

        return distancia <= raioAreaSegura;
    }


    // =========================================================
    // VERIFICAR CAPTURA
    // =========================================================

    private void VerificarCapturaPorDistancia()
    {
        if (!chefeAtivo)
            return;

        if (!podeCapturar)
            return;

        if (transportandoSandy)
            return;

        if (sandy == null)
            return;

        // Nunca captura Sandy dentro da gaiola.
        if (SandyEstaNaAreaSegura())
        {
            estadoAtual =
                EstadoChefe.Patrulhando;

            return;
        }

        if (
            estadoAtual !=
            EstadoChefe.Perseguindo
        )
        {
            return;
        }

        float distancia = Vector2.Distance(
            transform.position,
            sandy.position
        );

        if (distancia <= distanciaCaptura)
        {
            Debug.Log(
                "A carrocinha chegou perto de Sandy!"
            );

            CapturarSandy();
        }
    }


    // =========================================================
    // PERSEGUIÇÃO
    // =========================================================

    private void ExecutarPerseguicao(
        float distancia
    )
    {
        // Sandy entrou na área segura.
        if (SandyEstaNaAreaSegura())
        {
            estadoAtual =
                EstadoChefe.Patrulhando;

            Debug.Log(
                "Sandy voltou para a gaiola. " +
                "A carrocinha não pode capturá-la aqui."
            );

            return;
        }

        // Sandy fugiu.
        if (distancia > distanciaPerdaVisao)
        {
            estadoAtual =
                EstadoChefe.Patrulhando;

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


    // =========================================================
    // MOVIMENTO
    // =========================================================

    private void MoverAte(
        Vector3 destino,
        float velocidade
    )
    {
        Vector3 novaPosicao =
            Vector2.MoveTowards(
                transform.position,
                destino,
                velocidade * Time.deltaTime
            );

        transform.position =
            novaPosicao;

        if (
            destino.x >
            transform.position.x
        )
        {
            transform.localScale =
                new Vector3(
                    Mathf.Abs(
                        transform.localScale.x
                    ),
                    transform.localScale.y,
                    transform.localScale.z
                );
        }
        else if (
            destino.x <
            transform.position.x
        )
        {
            transform.localScale =
                new Vector3(
                    -Mathf.Abs(
                        transform.localScale.x
                    ),
                    transform.localScale.y,
                    transform.localScale.z
                );
        }
    }


    // =========================================================
    // COLISÃO
    // =========================================================

    private void OnTriggerEnter2D(
        Collider2D outro
    )
    {
        if (!chefeAtivo)
            return;

        if (!podeCapturar)
            return;

        if (transportandoSandy)
            return;

        if (
            estadoAtual !=
            EstadoChefe.Perseguindo &&
            estadoAtual !=
            EstadoChefe.Patrulhando
        )
        {
            return;
        }

        if (!outro.CompareTag("Player"))
            return;

        // Segurança extra:
        // nunca captura dentro da gaiola.
        if (SandyEstaNaAreaSegura())
        {
            Debug.Log(
                "Sandy está na área segura. " +
                "Captura cancelada."
            );

            return;
        }

        CapturarSandy();
    }


    // =========================================================
    // CAPTURAR SANDY
    // =========================================================

    public void CapturarSandy()
    {
        if (
            !chefeAtivo ||
            !podeCapturar ||
            transportandoSandy
        )
        {
            return;
        }

        // Segurança extra.
        if (SandyEstaNaAreaSegura())
        {
            Debug.Log(
                "Captura cancelada: " +
                "Sandy está na área segura."
            );

            return;
        }

        // IMPORTANTE:
        // NÃO paramos o CicloDoChefe aqui.
        //
        // O relógio da ronda continua correndo
        // normalmente durante a captura.

        podeCapturar = false;
        transportandoSandy = true;

        estadoAtual =
            EstadoChefe.LevandoSandy;

        Debug.Log(
            "Sandy foi capturada! " +
            "O tempo da ronda continua normalmente."
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


    // =========================================================
    // INICIAR TRANSPORTE
    // =========================================================

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
                sandy.GetComponent<
                    PlayerMovement
                >();
        }

        if (movimentoSandy != null)
        {
            movimentoSandy.enabled = false;
        }

        paiOriginalSandy =
            sandy.parent;

        if (pontoTransporteSandy != null)
        {
            sandy.SetParent(
                pontoTransporteSandy
            );

            sandy.localPosition =
                Vector3.zero;
        }
        else
        {
            sandy.SetParent(transform);

            sandy.localPosition =
                Vector3.zero;
        }

        Debug.Log(
            "Transporte da Sandy iniciado."
        );
    }


    // =========================================================
    // TRANSPORTE
    // =========================================================

    private void ExecutarTransporte()
    {
        if (pontoGaiolaSandy == null)
        {
            FinalizarTransporte();

            return;
        }

        float distancia =
            Vector2.Distance(
                transform.position,
                pontoGaiolaSandy.position
            );

        if (
            distancia <=
            distanciaGaiola
        )
        {
            FinalizarTransporte();

            return;
        }

        MoverAte(
            pontoGaiolaSandy.position,
            velocidadeTransporte
        );
    }


    // =========================================================
    // FINALIZAR TRANSPORTE
    // =========================================================

    private void FinalizarTransporte()
    {
        transportandoSandy = false;

        if (sandy != null)
        {
            sandy.SetParent(
                paiOriginalSandy
            );

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

        // A carrocinha NÃO sai.
        // Ela volta imediatamente para a patrulha.

        estadoAtual =
            EstadoChefe.Patrulhando;

        podeCapturar = true;

        Debug.Log(
            "Sandy chegou à gaiola. " +
            "A carrocinha voltou à patrulha " +
            "e o tempo da ronda continua."
        );
    }


    // =========================================================
    // ASSUSTAR CHEFE
    // =========================================================

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

        if (
            estadoAtual ==
            EstadoChefe.LevandoSandy
        )
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

        float distancia =
            Vector2.Distance(
                transform.position,
                sandy.position
            );

        if (
            distancia >
            distanciaVisao
        )
        {
            Debug.Log(
                "Sandy está longe demais para assustar."
            );

            return;
        }

        if (
            chavesColetadas <
            quantidadeNecessaria
        )
        {
            Debug.Log(
                "Ainda faltam chaves: " +
                chavesColetadas +
                "/" +
                quantidadeNecessaria
            );

            return;
        }

        if (
            estadoAtual ==
            EstadoChefe.Assustado
        )
        {
            return;
        }

        estadoAtual =
            EstadoChefe.Assustado;

        Debug.Log(
            "Sandy assustou a carrocinha!"
        );

        GameObject novaChave =
            Instantiate(
                chaveFinalPrefab,
                transform.position,
                Quaternion.identity
            );

        ChaveFinalTutorial chave =
            novaChave.GetComponent<
                ChaveFinalTutorial
            >();

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


    // =========================================================
    // DESATIVAR CHEFE
    // =========================================================

    public void DesativarChefe()
    {
        chefeAtivo = false;
        podeCapturar = false;

        estadoAtual =
            EstadoChefe.Fora;

        CancelInvoke();

        if (cicloChefeCoroutine != null)
        {
            StopCoroutine(
                cicloChefeCoroutine
            );

            cicloChefeCoroutine = null;
        }

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


    // =========================================================
    // VOLTAR DO ASSUSTADO
    // =========================================================

    private void VoltarDaAssustado()
    {
        if (!chefeAtivo)
            return;

        estadoAtual =
            EstadoChefe.Patrulhando;

        Debug.Log(
            "A carrocinha voltou à patrulha."
        );
    }
}