using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float originalSpeed = 5f; // 💡 [수정] 인스펙터에서 설정할 원래 속도
    private float currentSpeed;      // 💡 [추가] 실제 이동에 사용되는 속도 (감속 적용)
    
    private Rigidbody2D rb;
    private Vector2 input;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 💡 [수정] 초기 속도를 원래 속도로 설정합니다.
        currentSpeed = originalSpeed; 
    }

    void Update()
    {
        // 입력은 그대로 유지
        input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    void FixedUpdate()
    {
        // 💡 [수정] currentSpeed를 사용하여 이동합니다.
        rb.MovePosition(rb.position + input * currentSpeed * Time.fixedDeltaTime);
    }

    // ----------------------------------------------------
    // 💡 [추가] 거미줄 감속 로직 (WebSlowdown.cs에서 호출)
    // ----------------------------------------------------
    
    /// <summary>
    /// 플레이어의 이동 속도를 지정된 비율(factor)로 감속시킵니다.
    /// </summary>
    /// <param name="factor">감속 비율 (예: 0.5f는 50% 속도로 감속)</param>
    public void ApplySlowdown(float factor)
    {
        // 현재 속도를 원래 속도의 지정된 비율로 설정
        currentSpeed = originalSpeed * factor;
        Debug.Log($"속도 감속! 현재 속도: {currentSpeed}");
    }

    /// <summary>
    /// 플레이어의 이동 속도를 원래 속도로 복원합니다.
    /// </summary>
    public void RemoveSlowdown()
    {
        // 속도를 원래 속도로 복원
        currentSpeed = originalSpeed;
        Debug.Log($"속도 복원: {currentSpeed}");
    }
}