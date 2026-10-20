
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EnergiaSandy : MonoBehaviour
{
    [Header("Energia")]
    public int energiaMaxima = 3;
    public int energiaAtual;

    [Header("Comportamento ao zerar")]
    public bool derrotarAoZerarEnergia = false;

    [Header("Imagens de Energia")]
    [Tooltip("Adicione as 3 imagens na ordem: esquerda, meio e direita.")]
    public Image[] imagensEnergia = new Image[3];

    [Tooltip("Duração da animação de descarga.")]
    public float duracaoDescarga = 0.7f;

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
    private Coroutine rotinaDescarga;

    private void Start()
    {
        energiaMaxima = Mathf.Max(1, energiaMaxima);
        energiaAtual = energiaMaxima;

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (spriteSandy == null)
            spriteSandy = GetComponentInChildren<SpriteRenderer>();

        if (spriteSandy != null)
            corOriginal = spriteSandy.color;

        podePerderEnergia = true;

        AtualizarImagensEnergia();
        AtualizarVelocidade();

        if (painelAviso != null)
            painelAviso.SetActive(false);
    }

    public void PerderEnergia()
    {
        if (!podePerderEnergia || energiaAtual <= 0)
            return;

        // Guarda qual carga será descarregada.
        int indiceCarga = energiaAtual - 1;

        energiaAtual = Mathf.Max(0, energiaAtual - 1);

        // Atualiza imediatamente a energia lógica.
        AtualizarVelocidade();
        ReproduzirEfeitoDano();

        // Anima a carga perdida.
        if (rotinaDescarga != null)
            StopCoroutine(rotinaDescarga);

        if (indiceCarga >= 0 &&
            indiceCarga < imagensEnergia.Length &&
            imagensEnergia[indiceCarga] != null)
        {
            rotinaDescarga = StartCoroutine(
                AnimarDescarga(imagensEnergia[indiceCarga])
            );
        }

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

    private IEnumerator AnimarDescarga(Image imagem)
    {
        if (imagem == null)
            yield break;

        float preenchimentoInicial = imagem.fillAmount;
        float tempo = 0f;

        while (tempo < duracaoDescarga)
        {
            tempo += Time.deltaTime;

            float progresso = duracaoDescarga > 0f
                ? Mathf.Clamp01(tempo / duracaoDescarga)
                : 1f;

            // Suaviza a descarga.
            float suavizado = Mathf.SmoothStep(0f, 1f, progresso);

            imagem.fillAmount = Mathf.Lerp(
                preenchimentoInicial,
                0f,
                suavizado
            );

            yield return null;
        }

        imagem.fillAmount = 0f;
        rotinaDescarga = null;
    }

    private void AtualizarImagensEnergia()
    {
        if (imagensEnergia == null)
            return;

        for (int i = 0; i < imagensEnergia.Length; i++)
        {
            if (imagensEnergia[i] == null)
                continue;

            imagensEnergia[i].fillAmount =
                i < energiaAtual ? 1f : 0f;
        }
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

    private void AtualizarVelocidade()
    {
        if (!usarPenalidadeVelocidade || playerMovement == null)
            return;

        float proporcao = (float)energiaAtual / energiaMaxima;
        float multiplicador;

        if (proporcao >= 1f)
            multiplicador = 1f;
        else if (proporcao >= 0.66f)
            multiplicador = 0.9f;
        else if (proporcao >= 0.33f)
            multiplicador = 0.7f;
        else
            multiplicador = 0.5f;

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
            Debug.LogWarning(
                "O ponto de retorno não foi configurado."
            );
            return;
        }

        transform.position = pontoRetorno.position;

        Debug.Log("Sandy foi colocada de volta na gaiola.");
    }

    private void FalharTutorial()
    {
        podePerderEnergia = false;

        Debug.Log(
            "Sandy ficou sem energia! Tutorial finalizado."
        );

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
        CancelInvoke(nameof(EsconderAviso));

        if (rotinaDano != null)
        {
            StopCoroutine(rotinaDano);
            rotinaDano = null;
        }

        if (rotinaDescarga != null)
        {
            StopCoroutine(rotinaDescarga);
            rotinaDescarga = null;
        }

        if (spriteSandy != null)
            spriteSandy.color = corOriginal;
    }
}
