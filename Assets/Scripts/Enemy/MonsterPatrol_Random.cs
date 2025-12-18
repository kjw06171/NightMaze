using UnityEngine;
using Pathfinding;
using System.Collections;

public class MonsterPatrol_Random : MonoBehaviour
{
    [Header("Patrol Area Settings")]
    // ▼ 여기가 핵심! 이제 숫자(Radius) 대신 콜라이더(영역)를 넣게 바뀝니다.
    public Collider2D patrolZone; 
    public float arrivalThreshold = 0.4f;
    public Transform player;

    private AIPath aiPath;
    private Seeker seeker;
    private Transform randomTarget;
    private bool patrolActive = false;
    private bool hasDestination = false;

    public bool IsPatrolling => patrolActive;

    void Start()
    {
        aiPath = GetComponent<AIPath>();
        seeker = GetComponent<Seeker>();

        // 만약 Inspector에서 깜빡하고 영역을 안 넣었으면 에러 메시지 띄움
        if (patrolZone == null) Debug.LogError("⛔ [중요] 몬스터에게 순찰할 구역(Patrol Zone)을 연결해주세요!");

        randomTarget = new GameObject("RandomPatrolTarget").transform;
        randomTarget.SetParent(transform.parent); 

        aiPath.endReachedDistance = arrivalThreshold;
        aiPath.enabled = false;
    }

    public void StartPatrolling()
    {
        if (patrolActive) return;
        patrolActive = true;
        
        aiPath.destination = transform.position; 
        aiPath.target = randomTarget; 
        aiPath.enabled = true;
        
        hasDestination = false;
        SetNewRandomDestination();
    }

    public void StopPatrolling()
    {
        patrolActive = false;
        hasDestination = false;
        CancelInvoke(nameof(SetNewRandomDestination));
        aiPath.enabled = false;
        seeker.CancelCurrentPathRequest();
        aiPath.target = null; 
    }

    void Update()
    {
        if (!patrolActive || !hasDestination) return;

        if (aiPath.reachedDestination)
        {
            hasDestination = false; 
            Invoke(nameof(SetNewRandomDestination), 0.2f); 
        }
    }

    void SetNewRandomDestination()
    {
        if (hasDestination) return; 
        if (!patrolActive) return;

        // 콜라이더 범위 안에서 랜덤 위치 뽑기
        Vector3? targetPos = GetRandomPointInCollider(patrolZone);

        if (targetPos == null)
        {
            // 영역을 못 찾았거나 설정이 안 되어있으면 잠시 후 재시도
            Invoke(nameof(SetNewRandomDestination), 0.5f);
            return;
        }

        Vector3 finalPos = targetPos.Value;
        NNInfo node = AstarPath.active.GetNearest(finalPos);
        
        if (node.node == null || !node.node.Walkable) 
        {
            Invoke(nameof(SetNewRandomDestination), 0.1f);
            return;
        }

        randomTarget.position = node.position;
        aiPath.target = randomTarget;
        aiPath.SearchPath(); 
        hasDestination = true;
    }

    Vector3? GetRandomPointInCollider(Collider2D collider)
    {
        if (collider == null) return null;

        Bounds bounds = collider.bounds;
        for (int i = 0; i < 10; i++)
        {
            float randX = Random.Range(bounds.min.x, bounds.max.x);
            float randY = Random.Range(bounds.min.y, bounds.max.y);
            Vector2 randomPoint = new Vector2(randX, randY);

            if (collider.OverlapPoint(randomPoint))
            {
                return new Vector3(randomPoint.x, randomPoint.y, 0);
            }
        }
        return null;
    }
}