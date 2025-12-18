using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float originalSpeed = 5f;
    private float currentSpeed;  

    private Rigidbody2D rb;
    private Vector2 input;

    private Animator animator;  // 애니메이터 컴포넌트 추가
    private SpriteRenderer spriteRenderer;

    // 마지막 이동 방향을 저장할 변수
    private const string _lastHorizontal = "LastHorizontal";
    private const string _lastVertical = "LastVertical";

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();  // 애니메이터 컴포넌트
        spriteRenderer = GetComponent<SpriteRenderer>();  // 스프라이트 렌더러 컴포넌트
        currentSpeed = originalSpeed;
    }

    void Update()
    {
        // 입력받은 값에 따라 이동
        input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        // 애니메이션 변경 처리
        HandleAnimations();
    }

    void FixedUpdate()
    {
        // 물리 엔진을 사용하여 이동
        rb.MovePosition(rb.position + input * currentSpeed * Time.fixedDeltaTime);
    }

    // 애니메이션 업데이트 처리
    private void HandleAnimations()
    {
        // Horizontal, Vertical 값을 설정
        animator.SetFloat("Horizontal", input.x); // 왼쪽/오른쪽 이동
        animator.SetFloat("Vertical", input.y);   // 위/아래 이동

        // 이동 속도에 맞춰 Speed 값 설정
        animator.SetFloat("Speed", input.sqrMagnitude); // 이동 속도에 맞게 Speed 값 업데이트

        // 이동 방향에 따른 스프라이트 반전 처리
        if (input.x < 0)
        {
            spriteRenderer.flipX = true; // 왼쪽으로 이동 시 반전
        }
        else if (input.x > 0)
        {
            spriteRenderer.flipX = false; // 오른쪽으로 이동 시 반전하지 않음
        }

        // 이동 방향에 따른 마지막 값을 저장
        if (input != Vector2.zero)
        {
            // 마지막 이동 방향을 기록
            animator.SetFloat(_lastHorizontal, input.x);
            animator.SetFloat(_lastVertical, input.y);
        }
    }

    // 거미줄 감속 관련 함수
    public void ApplySlowdown(float factor)
    {
        currentSpeed = originalSpeed * factor;
    }

    public void RemoveSlowdown()
    {
        currentSpeed = originalSpeed;
    }

    // 텔레포트 (이동 위치 설정)
    public void Teleport(Vector3 targetPosition)
    {
        transform.position = targetPosition;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // 물리 힘 초기화
        }
    }
}
