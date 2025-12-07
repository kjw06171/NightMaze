using UnityEngine;
using UnityEngine.SceneManagement;
using Pathfinding;

public enum RatState { Idle, Chase, Return }

public class RatAI : MonoBehaviour
{
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
    private Vector2 lastPos;

    void Start()
    {
        aiPath = GetComponent<AIPath>();
        setter = GetComponent<AIDestinationSetter>();
        animator = GetComponent<Animator>();

        startPosition = transform.position;

        // 플레이어 찾기
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerHealth = playerObj.GetComponent<PlayerHealth>();

        lastPos = transform.position;
        setter.target = null; // Idle default
    }

    void Update()
    {
        // ⭐ 무리 전체 공격 종료 시 모든 쥐는 즉시 Return 상태로 전환
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


    // 쥐끼리 겹치지 않게 Separation Force
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


    // Idle 상태
    void HandleIdle()
    {
        if (SceneManager.GetActiveScene().name != "Lv_02")
            return;

        if (RatAI.keyBCollected && !hasChasedOnce && !RatAI.hasAttackedOnce)
        {
            hasChasedOnce = true;
            state = RatState.Chase;
            setter.target = player;
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


    // Chase
    void HandleChase()
    {
        if (!playerInsideRoom)
        {
            state = RatState.Return;
            setter.target = null;
            return;
        }
    }

    // Return
    void HandleReturn()
    {
        setter.target = null;
        aiPath.destination = startPosition;

        if (Vector2.Distance(transform.position, startPosition) < 0.25f)
            state = RatState.Idle;
    }

    // ⭐ Trigger 방식 데미지 처리
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player"))
            return;

        // ⭐ 쥐가 추격 중일 때만 공격 가능
        if (state != RatState.Chase)
            return;

        // ⭐ 무리가 이미 한 번 공격했으면 추가 공격 없음
        if (RatAI.hasAttackedOnce)
            return;

        // 최초 1회 공격
        if (playerHealth != null)
            playerHealth.TakeDamage(1);

        RatAI.hasAttackedOnce = true;

        // 공격 후 즉시 귀환
        state = RatState.Return;
        setter.target = null;
    }



    // 애니메이션 처리
    private void UpdateAnimation()
    {
        Vector2 movement = aiPath.velocity;
        bool isMoving = movement.sqrMagnitude > 0.001f;

        animator.SetBool("isMoving", isMoving);

        if (!isMoving)
            return;

        // 🔥 1) 주요 방향 저장 로직 (부드러운 방향 유지)
        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
        {
            // x 방향이 더 크면 x 방향 유지
            lastMajorDir = new Vector2(Mathf.Sign(movement.x), 0);
        }
        else
        {
            // y 방향이 더 크면 y 방향 유지
            lastMajorDir = new Vector2(0, Mathf.Sign(movement.y));
        }

        // 🔥 2) 애니메이션 결정은 "lastMajorDir" 기준으로만!
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
