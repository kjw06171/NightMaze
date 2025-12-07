using UnityEngine;
using Pathfinding;
using System.Collections; 

public class EnemyDadChase : MonoBehaviour
{
    // A* Pathfinding 컴포넌트
    public Seeker seeker;
    public AIPath aiPath;

    [Header("Target & Light")]
    public Transform Player;
    public LightControl LightControlScript;
    public MonsterPatrol_Random patrolScript; 

    [Header("Movement & State")]
    public float ChaseSpeed = 3f;
    public float FleeSpeed = 2f;
    public float ChaseDistance = 5f; // 플레이어 추격 시작 거리
    public float FleeDistance = 10f; // 도망갈 때 플레이어로부터 멀어지려는 거리

    private bool isChasing = false;
    private bool isFleeing = false;
    
    void Start()
    {
        if (aiPath == null) aiPath = GetComponent<AIPath>();
        if (seeker == null) seeker = GetComponent<Seeker>();
        if (patrolScript == null) patrolScript = GetComponent<MonsterPatrol_Random>();

        if (aiPath != null) aiPath.enabled = false; 
    }

    void Update()
    {
        if (Player == null || LightControlScript == null || aiPath == null || patrolScript == null) 
            return;

        bool isLightActive = LightControlScript.IsLightOn;
        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);
        
        // ----------------------------------------------------
        // 1. 상태 판정 및 전환 (Flee → Chase → Patrol 순으로 우선순위 결정)
        // ----------------------------------------------------
        
        bool shouldFlee = isLightActive && distanceToPlayer < ChaseDistance;
        bool shouldChase = !isLightActive && distanceToPlayer < ChaseDistance;
        
        if (shouldFlee)
        {
            // Flee (가장 높은 우선순위)
            if (isChasing) StopChasing();
            if (patrolScript.IsPatrolling) patrolScript.StopPatrolling();
            if (!isFleeing) StartFleeing();
        }
        else if (shouldChase)
        {
            // Chase
            if (isFleeing) StopFleeing();
            if (patrolScript.IsPatrolling) patrolScript.StopPatrolling();
            
            if (!isChasing) 
            {
                PlayerDetected(true); 
                StartChasing();
            }
        }
        else // 추격/도망 조건이 해제되었을 때 → 순찰로 복귀
        {
            if (isChasing || isFleeing)
            {
                PlayerDetected(false); 
                
                // 이전 상태 정리 (AIPath 끄기 포함)
                if (isChasing) StopChasing(); 
                if (isFleeing) StopFleeing(); 
                
                // 완벽 복귀 로직
                if (!patrolScript.IsPatrolling)
                {
                    patrolScript.StartPatrolling(); 
                }
            }
            else
            {
                // 아무 상태도 아니면 순찰 유지/시작
                 if (!patrolScript.IsPatrolling)
                {
                    patrolScript.StartPatrolling(); 
                }
            }
        }

        // ----------------------------------------------------
        // 2. 이동 처리 (기존 로직 유지)
        // ----------------------------------------------------
        
        if (isFleeing)
        {
            HandleFleeMovement();
        }
        else if (isChasing)
        {
            HandleChaseMovement();
        }
    }
    
    public void PlayerDetected(bool detected)
    {
        if (detected)
            Debug.Log("플레이어 감지됨 → 상태 전환 준비");
        else
            Debug.Log("플레이어 사라짐 → 순찰 복귀");
    }

    private void HandleFleeMovement()
    {
        Vector3 directionToPlayer = Player.position - transform.position;
        Vector3 fleeDirection = -directionToPlayer.normalized;
        Vector3 targetPosition = transform.position + fleeDirection * FleeDistance;

        NNConstraint constraint = NNConstraint.None;
        NNInfo nearestNodeInfo = AstarPath.active.GetNearest(targetPosition, constraint);
        Vector3 nearestValidTarget = nearestNodeInfo.position;

        aiPath.destination = nearestValidTarget;

        if (!aiPath.enabled) aiPath.enabled = true;
        aiPath.maxSpeed = FleeSpeed;
    }

    private void HandleChaseMovement()
    {
        if (!aiPath.enabled) aiPath.enabled = true;
        aiPath.target = Player; 
        aiPath.maxSpeed = ChaseSpeed;
    }

    void StartChasing()
    {
        isChasing = true;
        aiPath.target = Player;
        aiPath.enabled = true;
        aiPath.maxSpeed = ChaseSpeed;
        Debug.Log("추격 시작!");
    }

    // ----------------------------------------------
    // ⭐ [완료 상태 보강] StopChasing 함수
    // ----------------------------------------------
    void StopChasing()
    {
        isChasing = false;
        
        // Flee 상태가 아니라면 AIPath를 끄고 경로를 취소합니다.
        if (!isFleeing) 
        {
            aiPath.target = null; 
            aiPath.enabled = false;
            seeker.CancelCurrentPathRequest();
        }

        Debug.Log("추격 중지!");
    }
    
    // ----------------------------------------------
    // ⭐ [보강] StartFleeing 함수: aiPath.target 명시적 null 설정
    // ----------------------------------------------
    void StartFleeing()
    {
        isFleeing = true;
        
        // ⭐ 추가: 도망은 destination을 사용하므로, target을 확실히 null로 만듭니다.
        aiPath.target = null; 

        aiPath.enabled = true;
        aiPath.maxSpeed = FleeSpeed;
        Debug.Log("불 감지! 도망 시작!");
    }

    // ----------------------------------------------
    // ⭐ [완료 상태 보강] StopFleeing 함수
    // ----------------------------------------------
    void StopFleeing()
    {
        isFleeing = false;
        
        // Chase 상태가 아니라면 AIPath를 끄고 경로를 취소합니다.
        if (!isChasing) 
        {
            // ⭐ 추가: target을 명시적으로 null로 설정하여 cleanup을 완성합니다.
            aiPath.target = null; 
            
            aiPath.enabled = false;
            seeker.CancelCurrentPathRequest();
        }
        
        Debug.Log("도망 중지!");
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 충돌 처리 (필요 시 확장)
    }
}