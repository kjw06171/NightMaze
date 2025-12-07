using UnityEngine;

public class MonsterChaseSensor : MonoBehaviour
{
    public EnemyDadChase enemy;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            enemy.PlayerDetected(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            enemy.PlayerDetected(false);
    }
}
