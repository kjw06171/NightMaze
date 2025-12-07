using UnityEngine;

public class WebSlowdown : MonoBehaviour
{
    [Tooltip("원래 속도의 몇 %로 감속할지 설정 (0.0: 정지 ~ 1.0: 감속 없음)")]
    public float SlowdownFactor = 0.3f;

    [Header("🎵 효과음 설정")]
    public AudioClip webEnterSound;       // 거미줄에 들어갈 때 사운드
    [Range(0f, 1f)]
    public float webSoundVolume = 1f;     // 볼륨 조절
    private AudioSource audioSource;

    private void Awake()
    {
        // AudioSource 자동 생성 또는 기존 거 가져오기
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;               // 자동 반복 방지
        audioSource.ignoreListenerPause = false; // TimeScale=0에서도 재생됨
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 감속 처리
            PlayerMove playerMove = other.GetComponent<PlayerMove>();
            if (playerMove != null)
            {
                playerMove.ApplySlowdown(SlowdownFactor);
            }

            // 🔊 사운드는 PlayOneShot → 멈출 수 없으므로 Play() 방식으로 변경
            if (webEnterSound != null)
            {
                audioSource.clip = webEnterSound;
                audioSource.volume = webSoundVolume;
                audioSource.Stop();    // 혹시 이전 재생 중이면 초기화
                audioSource.Play();    // 재생
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 감속 해제
            PlayerMove playerMove = other.GetComponent<PlayerMove>();
            if (playerMove != null)
            {
                playerMove.RemoveSlowdown();
            }

            // 🔇 범위 벗어나면 사운드 즉시 중단
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}
