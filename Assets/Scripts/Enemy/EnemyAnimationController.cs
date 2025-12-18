using UnityEngine;
using Pathfinding;

public class EnemyAnimationController : MonoBehaviour
{
    Animator anim;
    AIPath aiPath;

    // 마지막으로 확정된 좌우 방향
    float lastHorizontal = 1f; // 기본은 오른쪽

    void Awake()
    {
        anim = GetComponent<Animator>();
        aiPath = GetComponent<AIPath>();
    }

    void Update()
    {
        Vector2 v = aiPath.desiredVelocity;
        float speed = v.magnitude;

        anim.SetFloat("Speed", speed);

        if (speed < 0.05f)
        {
            // 멈췄을 때도 마지막 방향 유지
            anim.SetFloat("Horizontal", lastHorizontal);
            anim.SetFloat("Vertical", 0f);
            return;
        }

        Vector2 dir = v.normalized;

        // 🔥 핵심: dead zone
        if (Mathf.Abs(dir.x) > 0.2f)
        {
            lastHorizontal = Mathf.Sign(dir.x);
        }

        anim.SetFloat("Horizontal", lastHorizontal);
        anim.SetFloat("Vertical", 0f);
    }
}
