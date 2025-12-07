using UnityEngine;

public class MonsterAttackSensor : MonoBehaviour
{
    public int damage = 1;
    public float attackCooldown = 1f; // 1초 쿨타임
    private float nextAttackTime = 0f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other); // Trigger ON일 때 필수
    }

    private void TryDamage(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (Time.time < nextAttackTime)
            return; // 쿨타임 중이면 공격 X

        nextAttackTime = Time.time + attackCooldown;

        PlayerHealth hp = other.GetComponent<PlayerHealth>();
        if (hp != null)
            hp.TakeDamage(damage);
    }
}
