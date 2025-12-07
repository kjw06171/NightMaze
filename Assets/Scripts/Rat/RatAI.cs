using UnityEngine;
using UnityEngine.SceneManagement;
using Pathfinding;

public enum RatState { Idle, Chase, Return }

public class RatAI : MonoBehaviour
{
    [Header("추격 사운드 설정 🔊")]
    public AudioSource chaseAudioSource;   // 사운드 재생용 AudioSource
    public AudioClip chaseClip;            // 추격 시작 효과음
    [Range(0f, 1f)]
    public float chaseVolume = 1f;         // 볼륨 조절

    // ⭐ 모든 쥐 중 1마리만 추격 사운드를 재생하게 하는 글로벌 플래그
    public static bool chaseSoundPlayedGlobal = false;

    // KEY_B 획득 여부 (전체 공유)
    public static bool keyBCollected = false;

    // 📌 무리 전체 공격 1회 제한 + 무리 전체 귀환
    public static bool hasAttackedOnce = false;

    public RatState state = RatState.Idle;

    [Header("참조")]
    public Transform player;
    private PlayerHealth playerHealth;
    private AIPath aiPath;
    private AIDestinationSetter setter;

    [Header("방 내부 판정")]
    public bool playerInsideRoom = false;

    [Header("쥐 시작 위치")]
    public Vector2 startPosition;

    [Header("KEY_B 추격 1회 제한")]
    public bool hasChasedOnce = false;

    // Idle 랜덤 움직임
    public float idleMoveRadius = 0.2f;
    public float idleMoveSpeed = 0.6f;
    private Vector2 idleTargetPos;
    private float idleChangeTime;
    private Vector2 lastMajorDir = Vector2.right;

    // 애니메이션
    private Animator animator;

    void Start()
    {
        aiPath = GetComponent<AIPath>();
        setter = GetComponent<AIDestinationSetter>();
        animator = GetComponent<Animator>();

        startPosition = transform.position;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerHealth = playerObj.GetComponent<PlayerHealth>();

        setter.target = null; // Idle default
    }

    void Update()
    {
        // ⭐ 무리 전체 공격이 이미 끝났으면 모두 Return
        if (RatAI.hasAttackedOnce && state != RatState.Return)
        {
            state = RatState.Return;
            setter.target = null;
        }

        ApplySeparationForce();

        switch (state)
        {
            case RatState.Idle:
                HandleIdle();
                break;

            case RatState.Chase:
                HandleChase();
                break;

            case RatState.Return:
                HandleReturn();
                break;
        }

        UpdateAnimation();
    }

    // ==========================================
    // 🔊 추격 사운드 재생 (전역 1회)
    // ==========================================
    private void PlayChaseSound()
    {
        if (chaseSoundPlayedGlobal) return;            // 이미 재생했으면 무시
        if (PauseMenu.isGamePaused) return;            // 퍼즈 중이면 금지
        if (chaseAudioSource == null || chaseClip == null) return;

        chaseSoundPlayedGlobal = true;

        chaseAudioSource.clip = chaseClip;
        chaseAudioSource.volume = chaseVolume;
        chaseAudioSource.loop = false;
        chaseAudioSource.Play();
    }

    // ==========================================
    // 🔊 추격 사운드 즉시 끊기
    // ==========================================
    private void StopChaseSound()
    {
        if (chaseAudioSource != null && chaseAudioSource.isPlaying)
            chaseAudioSource.Stop();
    }

    // ==========================================
    // Separation Force
    // ==========================================
    private void ApplySeparationForce()
    {
        float separationRadius = 0.6f;
        float forceStrength = 1.2f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, separationRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject != this.gameObject && hit.GetComponent<RatAI>())
            {
                Vector2 direction = (Vector2)(transform.position - hit.transform.position);
                transform.position += (Vector3)(direction.normalized * forceStrength * Time.deltaTime);
            }
        }
    }

    // ==========================================
    // Idle 상태
    // ==========================================
    void HandleIdle()
    {
        if (SceneManager.GetActiveScene().name != "Lv_02")
            return;

        if (RatAI.keyBCollected && !hasChasedOnce && !RatAI.hasAttackedOnce)
        {
            hasChasedOnce = true;
            state = RatState.Chase;
            setter.target = player;

            PlayChaseSound();     // 🔥 추격 시작 시 사운드 재생
            return;
        }

        IdleWiggleMovement();
    }

    private void IdleWiggleMovement()
    {
        if (Time.time > idleChangeTime)
        {
            idleChangeTime = Time.time + Random.Range(0.5f, 1.2f);
            Vector2 randomOffset = Random.insideUnitCircle * idleMoveRadius;
            idleTargetPos = startPosition + randomOffset;
        }

        transform.position = Vector2.MoveTowards(
            transform.position,
            idleTargetPos,
            idleMoveSpeed * Time.deltaTime
        );
    }

    // ==========================================
    // Chase 상태
    // ==========================================
    void HandleChase()
    {
        // ⭐ 방을 나가면 바로 Return + 소리 즉시 끊기
        if (!playerInsideRoom)
        {
            StopChaseSound();   // 🔥 여기서 반드시 끊어야 함
            state = RatState.Return;
            setter.target = null;
            return;
        }
    }

    // ==========================================
    // Return 상태
    // ==========================================
    void HandleReturn()
    {
        setter.target = null;

        aiPath.destination = startPosition;

        if (Vector2.Distance(transform.position, startPosition) < 0.25f)
            state = RatState.Idle;
    }

    // ==========================================
    // 플레이어 충돌 → 공격 + Return
    // ==========================================
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        if (state != RatState.Chase) return;

        if (RatAI.hasAttackedOnce) return;

        if (playerHealth != null)
            playerHealth.TakeDamage(1);

        RatAI.hasAttackedOnce = true;

        // 🔥 공격 성공하면 즉시 소리 끊기
        StopChaseSound();

        state = RatState.Return;
        setter.target = null;
    }

    // ==========================================
    // 애니메이션 처리
    // ==========================================
    private void UpdateAnimation()
    {
        Vector2 movement = aiPath.velocity;
        bool isMoving = movement.sqrMagnitude > 0.001f;

        animator.SetBool("isMoving", isMoving);

        if (!isMoving) return;

        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
        {
            lastMajorDir = new Vector2(Mathf.Sign(movement.x), 0);
        }
        else
        {
            lastMajorDir = new Vector2(0, Mathf.Sign(movement.y));
        }

        if (lastMajorDir.x > 0)
            animator.Play("WalkRightRat");
        else if (lastMajorDir.x < 0)
            animator.Play("WalkLeftRat");
        else if (lastMajorDir.y > 0)
            animator.Play("WalkUpRat");
        else if (lastMajorDir.y < 0)
            animator.Play("WalkDownRat");
    }
}
