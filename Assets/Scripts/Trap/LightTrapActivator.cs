using UnityEngine;

public class LightTrapActivator2D : MonoBehaviour
{
    public RollingRock2D rollingRock;

    private bool playerInRange = false; 
    private bool trapActivated = false; 

    // ========================================
    // 🔊 트랩 발동 사운드 추가
    // ========================================
    [Header("트랩 발동 사운드 설정 🎵")]
    public AudioSource audioSource;      // 재생용 AudioSource
    public AudioClip trapSound;          // 트랩 발동 사운드
    [Range(0f, 1f)]
    public float trapVolume = 1f;        // 볼륨 조절

    void Start()
    {
        if (rollingRock != null)
        {
            rollingRock.InitializePosition();
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !trapActivated)
        {
            ActivateTrap();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("E 키를 눌러 트랩을 발동할 수 있습니다.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("플레이어가 범위를 벗어났습니다.");
        }
    }

    // ========================================
    // 🔥 트랩 발동
    // ========================================
    private void ActivateTrap()
    {
        if (rollingRock != null)
        {
            // 🔊 트랩 발동 사운드 재생
            PlayTrapSound();

            // 돌 굴리기 시작
            rollingRock.ActivateTrap();

            trapActivated = true; 
            Debug.Log("트랩 발동! 돌이 굴러갑니다.");

            // 발동 오브젝트 제거
            Destroy(gameObject);
            Debug.Log("빛 오브젝트가 사라졌습니다.");
        }
    }

    // ========================================
    // 🔊 사운드 재생 함수
    // ========================================
    private void PlayTrapSound()
    {
        if (audioSource != null && trapSound != null)
        {
            audioSource.PlayOneShot(trapSound, trapVolume);
        }
    }
}
