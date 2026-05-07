using UnityEngine;
using System.Collections;

public class NPCAtaque : MonoBehaviour
{
    public int dano = 1;

    private bool podeAtacar = true;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && podeAtacar)
        {
            PlayerHealth vida = collision.GetComponent<PlayerHealth>();

            if (vida != null)
            {
                vida.LevarDano(dano);
                StartCoroutine(CooldownAtaque());
            }
        }
    }

    IEnumerator CooldownAtaque()
    {
        podeAtacar = false;

        yield return new WaitForSeconds(1f);

        podeAtacar = true;
    }
}