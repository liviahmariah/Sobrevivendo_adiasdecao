
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EnergiaSandy : MonoBehaviour
{
    [Header("Energia")]
    public int energiaMaxima = 5;
    public int energiaAtual;

    [Header("Comportamento ao zerar")]
    public bool derrotarAoZerarEnergia = false;

    [Header("Barra de Energia")]
    public Image barraEnergia;

    [Header("Configuração da Gaiola")]
    public Transform pontoRetorno;

    [Header("Aviso ao Jogador")]
    public GameObject painelAviso;
    public TextMeshProUGUI textoAviso;
    public float duracaoAviso = 3f;

    [Header("Penalidade de Velocidade")]
    public bool usarPenalidadeVelocidade = false;
    public PlayerMovement playerMovement;

    [Header("Efeitos ao Receber Dano")]
    public AudioSource audioDano;
    public SpriteRenderer spriteSandy;
    public Color corDano = Color.red;
    public float duracaoEfeitoDano = 0.2f;

    private bool podePerderEnergia = true;
    private Color corOriginal;
    private Coroutine rotinaDano;

    private void Start()
    {
        energiaAtual = energiaMaxima;

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (spriteSandy == null)
            spriteSandy = GetComponentInChildren<SpriteRenderer>();

        if (spriteSandy != null)
            corOriginal = spriteSandy.color;

        podePerderEnergia = true;

        AtualizarBarra();

        if (usarPenalidadeVelocidade)
            AtualizarVelocidade();

        if (painelAviso != null)
            painelAviso.SetActive(false);
    }

    public void PerderEnergia()
    {
        if (!podePerderEnergia || energiaAtual <= 0)
            return;

        energiaAtual = Mathf.Max(0, energiaAtual - 1);

        AtualizarBarra();
        AtualizarVelocidade();
        ReproduzirEfeitoDano();

        Debug.Log("Energia perdida! Atual: " + energiaAtual);

        if (energiaAtual <= 0)
        {
            if (derrotarAoZerarEnergia)
            {
                FalharTutorial();
                return;
            }

            MostrarAviso("SANDY ESTÁ SEM ENERGIA!");
            return;
        }

        MostrarAviso("VOLTE PARA SUA GAIOLA!");
    }

    private void ReproduzirEfeitoDano()
    {
        if (audioDano != null)
            audioDano.Play();

        if (spriteSandy != null)
        {
            if (rotinaDano != null)
                StopCoroutine(rotinaDano);

            rotinaDano = StartCoroutine(EfeitoVisualDano());
        }
    }

    private IEnumerator EfeitoVisualDano()
    {
        spriteSandy.color = corDano;

        yield return new WaitForSeconds(duracaoEfeitoDano);

        if (spriteSandy != null)
            spriteSandy.color = corOriginal;

        rotinaDano = null;
    }

    private void AtualizarBarra()
    {
        if (barraEnergia != null)
        {
            barraEnergia.fillAmount = energiaMaxima > 0
                ? (float)energiaAtual / energiaMaxima
                : 0f;
        }
    }

    private void AtualizarVelocidade()
    {
        if (!usarPenalidadeVelocidade || playerMovement == null)
            return;

        float multiplicador;

        switch (energiaAtual)
        {
            case 5: multiplicador = 1f; break;
            case 4: multiplicador = 0.9f; break;
            case 3: multiplicador = 0.8f; break;
            case 2: multiplicador = 0.65f; break;
            case 1: multiplicador = 0.5f; break;
            default: multiplicador = 0.4f; break;
        }

        playerMovement.DefinirVelocidade(multiplicador);
    }

    private void MostrarAviso(string mensagem)
    {
        if (painelAviso != null)
            painelAviso.SetActive(true);

        if (textoAviso != null)
            textoAviso.text = mensagem;

        CancelInvoke(nameof(EsconderAviso));
        Invoke(nameof(EsconderAviso), duracaoAviso);
    }

    private void EsconderAviso()
    {
        if (painelAviso != null)
            painelAviso.SetActive(false);
    }

    public void RetornarParaGaiolaPorCaptura()
    {
        if (pontoRetorno == null)
        {
            Debug.LogWarning("O ponto de retorno não foi configurado.");
            return;
        }

        transform.position = pontoRetorno.position;

        Debug.Log("Sandy foi colocada de volta na gaiola.");
    }

    private void FalharTutorial()
    {
        podePerderEnergia = false;

        Debug.Log("Sandy ficou sem energia! Tutorial finalizado.");

        MostrarAviso("VOCÊ FICOU SEM ENERGIA!");

        if (TutorialManager.instance != null)
        {
            TutorialManager.instance.Derrota();
        }
        else
        {
            Debug.LogWarning(
                "TutorialManager não foi encontrado. " +
                "Não foi possível mostrar o painel de derrota."
            );
        }
    }

    private void OnDisable()
    {
        if (rotinaDano != null)
        {
            StopCoroutine(rotinaDano);
            rotinaDano = null;
        }

        if (spriteSandy != null)
            spriteSandy.color = corOriginal;
    }
}
