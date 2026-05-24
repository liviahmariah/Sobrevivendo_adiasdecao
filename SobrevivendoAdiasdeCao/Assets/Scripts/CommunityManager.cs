using UnityEngine;
using UnityEngine.UI;

public class CommunityManager : MonoBehaviour
{
    [Header("Status da Comunidade")]
    public float fome = 50f;
    public float felicidade = 50f;
    public float seguranca = 50f;

    [Header("Velocidade (por segundo)")]
    public float fomeRate = 2f;
    public float felicidadeRate = -1f;
    public float segurancaRate = -0.5f;

    [Header("Barras")]
    public Slider barraFome;
    public Slider barraFelicidade;
    public Slider barraSeguranca;

    void Start()
    {
        AtualizarUI();
    }

    void Update()
    {
        // Atualização contínua
        fome += fomeRate * Time.deltaTime;
        felicidade += felicidadeRate * Time.deltaTime;
        seguranca += segurancaRate * Time.deltaTime;

        // Limites
        fome = Mathf.Clamp(fome, 0, 100);
        felicidade = Mathf.Clamp(felicidade, 0, 100);
        seguranca = Mathf.Clamp(seguranca, 0, 100);

        AtualizarUI();
    }

    void AtualizarUI()
    {
        if (barraFome != null)
            barraFome.value = fome;

        if (barraFelicidade != null)
            barraFelicidade.value = felicidade;

        if (barraSeguranca != null)
            barraSeguranca.value = seguranca;
    }
}