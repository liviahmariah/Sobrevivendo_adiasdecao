using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [Header("Fontes de Áudio (Dois canais para Crossfade)")]
    public AudioSource audioA;
    public AudioSource audioB;

    [Header("Músicas do Jogo")]
    public AudioClip musicaMenu;
    public AudioClip musicaCutscene;
    public AudioClip musicaTutorial;

    [Header("Configurações de Transição")]
    [Min(0f)]
    public float tempoCrossfade = 2f;

    private AudioSource audioAtual;
    private AudioSource proximoAudio;
    private Coroutine rotinaTransiction;

    private void Awake()
    {
        // Garante Singleton sem duplicar ao voltar de cenas
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            enabled = false;
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        PrepararAudioSource(audioA);
        PrepararAudioSource(audioB);

        audioAtual = audioA;
        proximoAudio = audioB;
    }

    private void Start()
    {
        if (instance != this) return;

        // Registra nos canais globais do AudioManager se ele existir
        if (AudioManager.Instance != null)
        {
            if (audioA != null) AudioManager.Instance.RegisterMusicSource(audioA);
            if (audioB != null) AudioManager.Instance.RegisterMusicSource(audioB);
        }

        // Toca a música do menu ao iniciar o jogo
        if (musicaMenu != null)
        {
            audioAtual.clip = musicaMenu;
            audioAtual.volume = 1f;
            audioAtual.Play();
        }
    }

    private void PrepararAudioSource(AudioSource fonte)
    {
        if (fonte == null) return;
        fonte.playOnAwake = false;
        fonte.loop = true;
        fonte.volume = 0f;
    }

    // =====================================================
    // MÉTODOS DE TRANSIÇÃO SUAVE
    // =====================================================

    // Chamado para ir para a Cutscene (Intro1)
    public void IrParaIntro()
    {
        TocarMusicaSuave(musicaCutscene);
        SceneManager.LoadScene("Intro1");
    }

    // Chamado pelo IntroManager ao trocar da Intro1 para Intro2 (Música continua)
    public void IrParaIntro2(string nomeProximaCena)
    {
        SceneManager.LoadScene(nomeProximaCena);
    }

    // Chamado pelo IntroManager ao terminar a Cutscene e ir para o Tutorial/Jogo
    public void PararMusicaCutsceneEEntrarNoJogo(string nomeCenaJogo)
    {
        if (nomeCenaJogo.Contains("Tutorial"))
        {
            TocarMusicaSuave(musicaTutorial);
        }
        else
        {
            TocarMusicaSuave(null); // Faz fade out até o silêncio se não tiver música
        }

        SceneManager.LoadScene(nomeCenaJogo);
    }

    // Método genérico de Crossfade
    public void TocarMusicaSuave(AudioClip novaMusica)
    {
        if (audioAtual != null && audioAtual.isPlaying && audioAtual.clip == novaMusica)
        {
            return; // Se já é a mesma música, continua tocando sem reiniciar
        }

        if (rotinaTransiction != null)
        {
            StopCoroutine(rotinaTransiction);
        }

        rotinaTransiction = StartCoroutine(ExecutarCrossfade(novaMusica));
    }

    private IEnumerator ExecutarCrossfade(AudioClip novaMusica)
    {
        // Se houver uma nova música, prepara o canal secundário
        if (novaMusica != null)
        {
            proximoAudio.Stop();
            proximoAudio.clip = novaMusica;
            proximoAudio.volume = 0f;
            proximoAudio.Play();
        }

        float volumeInicial = audioAtual != null ? audioAtual.volume : 0f;
        float tempo = 0f;

        while (tempo < tempoCrossfade)
        {
            tempo += Time.unscaledDeltaTime;
            float progresso = Mathf.Clamp01(tempo / tempoCrossfade);

            // Fade Out na atual
            if (audioAtual != null)
            {
                audioAtual.volume = Mathf.Lerp(volumeInicial, 0f, progresso);
            }

            // Fade In na próxima
            if (novaMusica != null && proximoAudio != null)
            {
                proximoAudio.volume = progresso;
            }

            yield return null;
        }

        // Finaliza o áudio antigo
        if (audioAtual != null)
        {
            audioAtual.Stop();
            audioAtual.volume = 0f;
        }

        if (novaMusica != null && proximoAudio != null)
        {
            proximoAudio.volume = 1f;
        }

        // Inverte as referências dos canais (o próximo vira o atual)
        AudioSource temp = audioAtual;
        audioAtual = proximoAudio;
        proximoAudio = temp;

        rotinaTransiction = null;
    }
}