using UnityEngine;
using System.Collections; 

[RequireComponent(typeof(Collider2D))]
public class JumpscareTrigger : MonoBehaviour
{
    [Header("Jumpscare Settings")]
    public GameObject normalDisplayObject;
    public GameObject jumpscareObject;
    public float scareDelay = 2.0f;
    public float scareDuration = 0.5f;

    [Header("Impact Settings")]
    public AudioClip scareSound;
    private AudioSource audioSource;

    [Header("Scare Sound Volume")]
    [Range(0f, 1f)]
    public float scareVolume = 1f;   // ⭐ 깜놀 사운드 볼륨 추가

    public float maxScaleMultiplier = 1.2f;
    public float popDuration = 0.05f;

    [Header("Interaction Settings")]
    public string playerTag = "Player";

    [Header("Jumpscare Damage")]
    public int damageAmount = 1;

    [Header("UI & Menu Control")]
    public GameObject pauseMenuCanvas;
    private bool wasPauseMenuVisibleBeforeScare = false;

    // ⭐ 긴장용 사운드
    [Header("Suspense Sound (E 누른 직후 나오는 긴장음)")]
    public AudioClip suspenseClip;
    public float suspenseVolume = 1f;
    private AudioSource suspenseSource;

    // ⭐ BGM 복원용
    private float bgmOriginalVolume = 1f;

    private bool playerIsNear = false;
    private bool hasBeenTriggered = false;
    private Vector3 originalScale;
    private GameObject playerReference;


    private void Awake()
    {
        // scareSound 재생용 AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.ignoreListenerPause = false;

        // 긴장음 재생용 AudioSource
        suspenseSource = gameObject.AddComponent<AudioSource>();
        suspenseSource.playOnAwake = false;
        suspenseSource.ignoreListenerPause = false;
        suspenseSource.loop = true;
    }

    private void Start()
    {
        if (jumpscareObject != null)
        {
            jumpscareObject.SetActive(false);
            originalScale = jumpscareObject.transform.localScale;
        }

        if (normalDisplayObject != null)
            normalDisplayObject.SetActive(false);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Update()
    {
        if (playerIsNear && !hasBeenTriggered && Input.GetKeyDown(KeyCode.E))
            StartCoroutine(DoDelayedJumpscare());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerIsNear = true;
            playerReference = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            playerIsNear = false;
    }

    private IEnumerator DoDelayedJumpscare()
    {
        hasBeenTriggered = true;

        // PauseMenu 비활성화
        if (pauseMenuCanvas != null && pauseMenuCanvas.activeSelf)
        {
            wasPauseMenuVisibleBeforeScare = true;
            pauseMenuCanvas.SetActive(false);
        }

        // BGM 페이드 아웃
        if (BGMManager.Instance != null)
        {
            bgmOriginalVolume = BGMManager.Instance.CurrentVolume;
            BGMManager.Instance.FadeTo(0f, 0.4f);
        }

        // 긴장 사운드 재생
        if (suspenseClip != null)
        {
            suspenseSource.clip = suspenseClip;
            suspenseSource.volume = suspenseVolume;
            suspenseSource.Play();
        }

        Time.timeScale = 0f;

        if (normalDisplayObject != null)
            normalDisplayObject.SetActive(true);

        yield return new WaitForSecondsRealtime(scareDelay);

        // 긴장음 정지
        suspenseSource.Stop();

        // 데미지 적용
        if (playerReference != null)
        {
            var hp = playerReference.GetComponent<PlayerHealth>();
            if (hp != null)
                hp.TakeDamage(damageAmount);
        }

        if (normalDisplayObject != null)
            normalDisplayObject.SetActive(false);

        // ⭐ 깜놀 사운드 (볼륨 적용됨)
        if (audioSource != null && scareSound != null)
            audioSource.PlayOneShot(scareSound, scareVolume);

        // =============================
        // 🔥 팝 애니메이션 + 그림 유지 시간 = 사운드 길이에 맞추기
        // =============================
        jumpscareObject.SetActive(true);

        float t = 0f;
        Vector3 targetScale = originalScale * maxScaleMultiplier;

        // 1) 팝 애니메이션 (처음 짧게 확대)
        while (t < popDuration)
        {
            t += Time.unscaledDeltaTime;
            jumpscareObject.transform.localScale =
                Vector3.Lerp(originalScale, targetScale, t / popDuration);
            yield return null;
        }

        // 2) 그림을 띄워둘 총 시간 계산
        //    scareSound가 있으면 그 길이에 맞춰주고,
        //    없으면 기존 scareDuration 사용
        float totalVisualDuration = scareDuration;
        if (scareSound != null)
            totalVisualDuration = Mathf.Max(scareDuration, scareSound.length);

        // 팝 애니메이션(popDuration) 동안은 이미 기다렸으니까 남은 시간만 더 대기
        float remain = totalVisualDuration - popDuration;
        if (remain > 0f)
            yield return new WaitForSecondsRealtime(remain);

        // =============================
        // 🔥 이제 그림 끄기 + 게임 재개
        // =============================
        jumpscareObject.SetActive(false);
        jumpscareObject.transform.localScale = originalScale;

        Time.timeScale = 1f;

        // BGM 복원
        if (BGMManager.Instance != null)
            BGMManager.Instance.FadeTo(bgmOriginalVolume, 0.5f);

        // PauseMenu 복원
        if (wasPauseMenuVisibleBeforeScare && pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(true);
            wasPauseMenuVisibleBeforeScare = false;
        }

        // 🗑️ 마지막에 오브젝트 제거
        Destroy(gameObject);
    }
}
