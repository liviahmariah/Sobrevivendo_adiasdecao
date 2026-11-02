
using UnityEngine;

public class ControleAgarramentoSandy : MonoBehaviour
{
    public bool estaAgarrada { get; private set; }

    private Rigidbody2D rb;
    private Collider2D colisor;
    private PlayerMovement movimento;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        colisor = GetComponent<Collider2D>();
        movimento = GetComponent<PlayerMovement>();
    }

    public void Prender()
    {
        if (estaAgarrada)
            return;

        estaAgarrada = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        if (colisor != null)
            colisor.enabled = false;

        if (movimento != null)
            movimento.enabled = false;
    }

    public void Soltar()
    {
        if (!estaAgarrada)
            return;

        estaAgarrada = false;

        if (rb != null)
            rb.simulated = true;

        if (colisor != null)
            colisor.enabled = true;

        if (movimento != null)
            movimento.enabled = true;
    }
}
