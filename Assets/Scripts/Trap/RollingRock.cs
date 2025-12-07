using UnityEngine;

public class RollingRock2D : MonoBehaviour
{
    // =========================================================
    // 유니티 인스펙터에서 설정할 변수들
    // =========================================================
    public Transform startPoint;  // 시작 지점
    public Transform endPoint;    // 끝 지점
    public float speed = 5f;      // 이동 속도

    private Collider2D rockCollider;
    private bool isMoving = false;

    // =========================================================
    // 🔊 사운드 설정
    // =========================================================
    [Header("돌 트랩 사운드 설정 🎵")]
    public AudioSource audioSource;     // 돌 소리를 재생할 AudioSource
    public AudioClip rollingSound;      // 돌이 굴러가기 시작할 때 재생할 소리
    [Range(0f, 1f)]
    public float rollingVolume = 1f;     // 사운드 재생 볼륨


    void Awake()
    {
        rockCollider = GetComponent<Collider2D>();

        if (rockCollider != null)
        {
            rockCollider.enabled = true;
        }
    }

    // =========================================================
    // 초기 위치로 이동시키고 돌을 숨기는 함수
    // =========================================================
    public void InitializePosition()
    {
        transform.position = startPoint.position;
        gameObject.SetActive(false);

        if (rockCollider != null)
            rockCollider.isTrigger = false;
    }

    // =========================================================
    // 트랩 발동 함수
    // =========================================================
    public void ActivateTrap()
    {
        gameObject.SetActive(true);
        isMoving = true;

        // 트리거 활성화
        if (rockCollider != null)
            rockCollider.isTrigger = true;

        // 🔊 소리 재생
        PlayRollingSound();

        Debug.Log("트랩 발동! 돌이 움직이기 시작하며 트리거가 활성화되었습니다.");
    }

    // =========================================================
    // 🔊 굴러가기 시작할 때 사운드 재생
    // =========================================================
    private void PlayRollingSound()
    {
        if (audioSource != null && rollingSound != null)
        {
            audioSource.PlayOneShot(rollingSound, rollingVolume);
        }
    }

    // =========================================================
    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                endPoint.position,
                speed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, endPoint.position) < 0.01f)
            {
                StopMovement();
            }
        }
    }

    // =========================================================
    // 플레이어와 충돌하면 데미지
    // =========================================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isMoving && other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
                Debug.Log("⚠️ 돌 트랩이 플레이어에게 1 데미지를 입혔습니다.");
            }
        }
    }

    // =========================================================
    // 이동 종료
    // =========================================================
    private void StopMovement()
    {
        isMoving = false;

        if (rockCollider != null)
            rockCollider.isTrigger = false;

        Debug.Log("돌이 끝 지점에 도착하여 멈추고 트리거가 비활성화되었습니다.");
    }
}
