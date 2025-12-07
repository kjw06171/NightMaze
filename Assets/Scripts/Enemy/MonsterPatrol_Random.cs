using UnityEngine;
using Pathfinding;
using System.Collections;

public class MonsterPatrol_Random : MonoBehaviour
{
    [Header("Random Patrol Settings")]
    public float patrolRadius = 5f;
    public float arrivalThreshold = 0.4f;   // 더 작은 값 추천
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

        if (aiPath == null) Debug.LogError("AIPath component is missing on " + gameObject.name);
        if (seeker == null) Debug.LogError("Seeker component is missing on " + gameObject.name);

        randomTarget = new GameObject("RandomPatrolTarget").transform;
        // 몬스터 오브젝트의 자식으로 설정하여 Hierarchy를 정리합니다.
        randomTarget.SetParent(transform.parent); 

        // ⭐ AIPath의 도착 거리를 스크립트의 arrivalThreshold와 통일
        aiPath.endReachedDistance = arrivalThreshold;
        
        aiPath.enabled = false;
    }

    // --------------------------------------------------------
    // 외부 제어 함수: Chase/Flee 종료 후 호출됨
    // --------------------------------------------------------

    public void StartPatrolling()
    {
        if (patrolActive) return;

        patrolActive = true;
        
        // ⭐ [핵심 안정화] 이전 상태(Chase/Flee)의 경로 정보를 완전히 지웁니다.
        // 이를 통해 몬스터가 이전 목표 지점에서 멈칫거리는 현상을 방지합니다.
        aiPath.destination = transform.position; // 현재 위치로 경로 목표 초기화
        aiPath.target = randomTarget; // 타겟 트랜스폼 재설정

        aiPath.enabled = true;
        
        // hasDestination = false 상태로 시작하여 새 목표를 찾습니다.
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
        
        // ⭐ AIPath.target도 명시적으로 null 처리하여 cleanup을 완성합니다.
        aiPath.target = null; 
    }

    void Update()
    {
        // 순찰 중이 아니거나, 아직 목표가 설정되지 않았다면 return
        if (!patrolActive || !hasDestination)
            return;

        // ⭐ [변경] AIPath의 내장된 도착 판정 사용 (EndReachedDistance 활용)
        // A* Pathfinding이 경로 완료를 판단하는 로직을 사용합니다.
        if (aiPath.reachedDestination)
        {
            // 목표에 도착했으므로, 다음 목표를 찾기 전까지 hasDestination을 false로 설정
            hasDestination = false; 
            
            // 0.2초 딜레이 후 다음 목표 설정 (멈춤 효과)
            Invoke(nameof(SetNewRandomDestination), 0.2f); 
        }
    }

    void SetNewRandomDestination()
    {
        if (hasDestination) return; 
        if (!patrolActive) return;

        // 플레이어 위치를 중심으로 순찰할지, 현재 몬스터 위치를 중심으로 순찰할지 결정
        Vector3 basePos = (player != null) ? player.position : transform.position;

        Vector2 rand = Random.insideUnitCircle * patrolRadius;
        Vector3 randomPos = basePos + new Vector3(rand.x, rand.y, 0); 

        // A* Pathfinding Project를 사용하여 가장 가까운 유효한 노드를 찾음
        NNInfo node = AstarPath.active.GetNearest(randomPos);
        Vector3 finalPos = node.position;

        // ⭐ 유효성 검사 추가: 만약 유효한 노드를 찾지 못했거나 접근 불가능하다면 재시도
        if (node.node == null || !node.node.Walkable) 
        {
            Debug.LogWarning("랜덤 목표 위치가 유효하지 않습니다. 0.2초 후 재시도.");
            Invoke(nameof(SetNewRandomDestination), 0.2f);
            return;
        }

        // 새로운 목표 위치 설정 및 경로 탐색 시작
        randomTarget.position = finalPos;
        aiPath.target = randomTarget;

        aiPath.SearchPath(); 

        // 새로운 목표가 설정되었으므로 도착 판정 활성화
        hasDestination = true;
    }
}