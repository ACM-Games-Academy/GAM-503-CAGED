using UnityEngine;

public class SpikeHazard : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!enabled) return;

        if (collision.CompareTag("Player"))
        {
            var playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null && !playerHealth.IsRespawning)
            {
                playerHealth.SpikeDeathRespawn();
            }
        }
    }
}
