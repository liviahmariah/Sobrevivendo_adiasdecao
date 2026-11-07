using UnityEngine;
using System.Collections;

public class DukeVisualController : MonoBehaviour
{
    [Header("Animação e Áudio do Duke")]
    [SerializeField] private Animator animatorDuke;
    [SerializeField] private AudioSource audioSourceDuke;
    [SerializeField] private AudioClip somLatidoDuke;
    [SerializeField] private string triggerLatido = "Latir";

    [Header("Sprites e Aparição (Fade In)")]
    [SerializeField] private GameObject dukeVisual;
    [SerializeField] private float duracaoAparicaoDuke = 0.5f;

    private SpriteRenderer[] spritesDuke;
    private Color[] coresOriginaisDuke;
    private bool dukeJaApareceu = false;

    private void Awake()
    {
        if (dukeVisual == null)
            dukeVisual = gameObject;

        spritesDuke = dukeVisual.GetComponentsInChildren<SpriteRenderer>(true);
        coresOriginaisDuke = new Color[spritesDuke.Length];

        for (int i = 0; i < spritesDuke.Length; i++)
        {
            coresOriginaisDuke[i] = spritesDuke[i].color;
            spritesDuke[i].enabled = false;
        }
    }

    // =====================================================
    // TOCAR ANIMAÇÃO E SOM DO LATIDO
    // =====================================================
    public void TocarLatido()
    {
        if (animatorDuke != null)
        {
            animatorDuke.ResetTrigger(triggerLatido);
            animatorDuke.SetTrigger(triggerLatido);
        }

        if (audioSourceDuke != null && somLatidoDuke != null)
        {
            audioSourceDuke.PlayOneShot(somLatidoDuke);
        }
    }

    // =====================================================
    // ANIMAÇÃO DE APARECER (FADE IN)
    // =====================================================
    public void GarantaAparicao(MonoBehaviour runner)
    {
        if (!dukeJaApareceu)
        {
            runner.StartCoroutine(AnimarAparicaoDuke());
        }
    }

    private IEnumerator AnimarAparicaoDuke()
    {
        if (spritesDuke == null || spritesDuke.Length == 0)
            yield break;

        dukeJaApareceu = true;

        for (int i = 0; i < spritesDuke.Length; i++)
        {
            if (spritesDuke[i] == null) continue;
            spritesDuke[i].enabled = true;
            Color cor = coresOriginaisDuke[i];
            cor.a = 0f;
            spritesDuke[i].color = cor;
        }

        float duracao = Mathf.Max(0.01f, duracaoAparicaoDuke);
        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float progresso = Mathf.Clamp01(tempo / duracao);

            for (int i = 0; i < spritesDuke.Length; i++)
            {
                if (spritesDuke[i] == null) continue;
                Color cor = coresOriginaisDuke[i];
                cor.a = Mathf.Lerp(0f, coresOriginaisDuke[i].a, progresso);
                spritesDuke[i].color = cor;
            }

            yield return null;
        }

        for (int i = 0; i < spritesDuke.Length; i++)
        {
            if (spritesDuke[i] != null)
                spritesDuke[i].color = coresOriginaisDuke[i];
        }
    }
}