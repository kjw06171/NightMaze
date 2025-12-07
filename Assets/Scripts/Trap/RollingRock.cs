using UnityEngine;

public class RollingRock2D : MonoBehaviour
{
    // 유니티 인스펙터에서 설정할 변수들
    public Transform startPoint;  // 시작 지점 (Empty GameObject)
    public Transform endPoint;    // 끝 지점 (Empty GameObject)
    public float speed = 5f;      // 이동 속도
    
    // 2D Collider 참조
    private Collider2D rockCollider;

    // 내부 상태 변수
    private bool isMoving = false; // 돌이 이동 중인지 여부

    void Awake()
    {
        // Collider2D 컴포넌트 가져오기
        rockCollider = GetComponent<Collider2D>();
        
        // 콜라이더 컴포넌트 자체는 활성화 상태로 둡니다.
        if (rockCollider != null)
        {
             rockCollider.enabled = true;
        }
    }

    // 이동 시작 지점으로 설정하는 초기화 함수
    public void InitializePosition()
    {
        transform.position = startPoint.position;
        gameObject.SetActive(false);
        
        // 초기에는 트리거 비활성화 상태 (트랩 발동 전)
        if (rockCollider != null)
        {
            rockCollider.isTrigger = false;
        }
    }

    // 트랩 발동 함수
    public void ActivateTrap()
    {
        gameObject.SetActive(true);
        isMoving = true;
        
        // [핵심] 돌이 움직이기 시작할 때 Is Trigger를 true로 설정 (트리거 켜짐)
        if (rockCollider != null)
        {
            rockCollider.isTrigger = true;
        }
        Debug.Log("트랩 발동! 돌이 움직이기 시작하며 트리거가 활성화되었습니다.");
    }

    void Update()
    {
        if (isMoving)
        {
            // 돌을 끝 지점을 향해 이동
            transform.position = Vector3.MoveTowards(
                transform.position,
                endPoint.position,
                speed * Time.deltaTime
            );

            // 끝 지점에 거의 도달했는지 확인
            if (Vector3.Distance(transform.position, endPoint.position) < 0.01f)
            {
                StopMovement();
            }
        }
    }
    
    // 💡 [추가된 로직] 돌이 플레이어와 충돌(트리거) 시 데미지 부여
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 돌이 움직이는 중이고 (isMoving), 충돌한 오브젝트가 "Player" 태그를 가지고 있다면
        if (isMoving && other.CompareTag("Player"))
        {
            // 플레이어 오브젝트에서 PlayerHealth 컴포넌트 가져오기
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // PlayerHealth의 TakeDamage 함수를 호출하여 1 데미지 부여
                playerHealth.TakeDamage(1); 
                Debug.Log("⚠️ 돌 트랩이 플레이어에게 1 데미지를 입혔습니다.");
            }
        }
    }

    // 이동 정지 함수
    private void StopMovement()
    {
        isMoving = false;
        
        // 이동 완료 시 Is Trigger를 false로 설정 (트리거 꺼짐)
        if (rockCollider != null)
        {
            rockCollider.isTrigger = false;
        }
        Debug.Log("돌이 끝 지점에 도착하여 멈추고 트리거가 비활성화되었습니다.");
    }
}