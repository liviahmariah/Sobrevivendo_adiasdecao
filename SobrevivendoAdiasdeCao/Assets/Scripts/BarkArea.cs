using UnityEngine;

public class BarkArea : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 2f);

            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("NPC"))
                {
                    hit.GetComponent<NPC>().DropItem();
                }
            }
        }
    }
}