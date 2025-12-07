using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ShadowMonster : MonoBehaviour
{
    public Transform player;          // 플레이어 Transform
    public Light2D playerLight;       // 플레이어 Light2D
    public float approachSpeed = 0.8f; // 천천히 다가오는 속도
    public float stopDistance = 3f;    // 플레이어 근처에서 멈춤
    public float appearRadius = 5f;    // 이 반경 이하일 때 나타남
    public float hideDistance = 12f;   // 플레이어와 너무 멀면 사라짐

    private SpriteRenderer sr;
    private bool isActive = false;     // 괴물이 등장 상태인지

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = false; // 처음엔 안 보이게
    }

    void Update()
    {
        if (player == null || playerLight == null) return;

        float currentLight = playerLight.pointLightOuterRadius;
        float distance = Vector2.Distance(transform.position, player.position);

        // 빛이 약해졌을 때만 등장
        if (currentLight <= appearRadius)
        {
            if (!isActive)
            {
                sr.enabled = true;
                isActive = true;
            }

            MoveTowardPlayer(distance);
        }
        else
        {
            // 빛이 강하면 사라짐
            sr.enabled = false;
            isActive = false;
        }
    }

    void MoveTowardPlayer(float distance)
    {
        if (distance > stopDistance)
        {
            // 방향 계산
            Vector2 dir = (player.position - transform.position).normalized;
            transform.position += (Vector3)dir * approachSpeed * Time.deltaTime;

            // 멀리 있을 때 투명 → 가까워질수록 뚜렷
            float fade = Mathf.InverseLerp(hideDistance, stopDistance, distance);
            sr.color = new Color(0, 0, 0, 1 - fade);
        }
        else
        {
            // 일정 거리 안에서는 멈추고 서 있음 (게임오버 직전 느낌)
            sr.color = new Color(0, 0, 0, 1f);
        }
    }
}
