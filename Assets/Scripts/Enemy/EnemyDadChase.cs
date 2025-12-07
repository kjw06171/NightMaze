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

    // 🔊 추격/배경 BGM 설정
    [Header("Chase BGM Settings")]
    [Tooltip("추격 중에만 별도의 BGM을 쓸지 여부")]
    public bool useChaseBGM = true;

    [Tooltip("추격 BGM을 재생할 AudioSource (적 오브젝트나 별도 오브젝트에 붙인 Source)")]
    public AudioSource chaseAudioSource;

    [Tooltip("추격 중에만 들릴 BGM 클립")]
    public AudioClip chaseBGMClip;

    [Tooltip("추격 중 Chase BGM 볼륨")]
    [Range(0f, 1f)]
    public float chaseBGMVolume = 1f;

    [Tooltip("배경/추격 BGM 볼륨 페이드 시간 (초)")]
    public float bgmFadeTime = 0.4f;

    // 내부 상태
    private bool isChasing = false;
    private bool isFleeing = false;

    // BGM 상태
    private bool isChaseBGMStarted = false;   // chaseAudioSource가 재생 시작된 적 있는지
    private bool stageVolumeCaptured = false; // 배경 BGM 원래 볼륨 저장 여부
    private float stageOriginalVolume = 1f;   // 추격 전 배경 BGM 볼륨

    // 코루틴용
    private Coroutine chaseBGMFadeRoutine;

    void Start()
    {
        if (aiPath == null) aiPath = GetComponent<AIPath>();
        if (seeker == null) seeker = GetComponent<Seeker>();
        if (patrolScript == null) patrolScript = GetComponent<MonsterPatrol_Random>();

        if (aiPath != null) aiPath.enabled = false; 

        // 🔊 Chase BGM AudioSource 초기 세팅
        if (useChaseBGM && chaseAudioSource != null && chaseBGMClip != null)
        {
            chaseAudioSource.clip = chaseBGMClip;
            chaseAudioSource.loop = true;
            chaseAudioSource.playOnAwake = false;
            chaseAudioSource.volume = 0f; // 처음엔 안 들리게
            chaseAudioSource.ignoreListenerPause = false; // 일시정지에도 계속 재생되게 할 거면

            // 재생만 미리 시작해두고 볼륨만 0으로 유지 → 나중에 올리면 "이어듣기" 가능
            chaseAudioSource.Play();
            isChaseBGMStarted = true;
        }
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
        // 2. 이동 처리
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

        // 🔊 BGM 스위칭: 배경 ↓, 추격 ↑
        if (useChaseBGM)
        {
            // 배경 BGM 원래 볼륨 저장 (처음 한 번만)
            if (!stageVolumeCaptured && BGMManager.Instance != null)
            {
                stageOriginalVolume = BGMManager.Instance.CurrentVolume;
                stageVolumeCaptured = true;
            }

            // 배경 BGM 볼륨 0으로 페이드
            if (BGMManager.Instance != null)
            {
                BGMManager.Instance.FadeTo(0f, bgmFadeTime);
            }

            // 추격 BGM 볼륨 페이드 인
            if (chaseAudioSource != null && chaseBGMClip != null)
            {
                if (!isChaseBGMStarted)
                {
                    // 혹시 재생 안 하고 있었으면 여기서 세팅 후 재생
                    chaseAudioSource.clip = chaseBGMClip;
                    chaseAudioSource.loop = true;
                    chaseAudioSource.volume = 0f;
                    chaseAudioSource.Play();
                    isChaseBGMStarted = true;
                }

                StartFadeChaseBGM(chaseAudioSource.volume, chaseBGMVolume);
            }
        }
    }

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

        // 🔊 BGM 스위칭: 추격 ↓, 배경 ↑
        if (useChaseBGM)
        {
            // 추격 BGM 볼륨 0으로
            if (chaseAudioSource != null && isChaseBGMStarted)
            {
                StartFadeChaseBGM(chaseAudioSource.volume, 0f);
            }

            // 배경 BGM 원래 볼륨으로 복귀
            if (BGMManager.Instance != null && stageVolumeCaptured)
            {
                BGMManager.Instance.FadeTo(stageOriginalVolume, bgmFadeTime);
            }
        }
    }
    
    void StartFleeing()
    {
        isFleeing = true;
        
        aiPath.target = null; 
        aiPath.enabled = true;
        aiPath.maxSpeed = FleeSpeed;
        Debug.Log("불 감지! 도망 시작!");
    }

    void StopFleeing()
    {
        isFleeing = false;
        
        if (!isChasing) 
        {
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

    // ------------------------------------------------------
    // 🔊 Chase BGM 페이드 코루틴
    // ------------------------------------------------------
    private void StartFadeChaseBGM(float from, float to)
    {
        if (chaseBGMFadeRoutine != null)
            StopCoroutine(chaseBGMFadeRoutine);

        chaseBGMFadeRoutine = StartCoroutine(FadeChaseBGMCoroutine(from, to, bgmFadeTime));
    }

    private IEnumerator FadeChaseBGMCoroutine(float from, float to, float duration)
    {
        if (chaseAudioSource == null || duration <= 0f)
        {
            if (chaseAudioSource != null)
                chaseAudioSource.volume = to;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.Clamp01(t / duration);
            chaseAudioSource.volume = Mathf.Lerp(from, to, lerp);
            yield return null;
        }

        chaseAudioSource.volume = to;
    }
}
