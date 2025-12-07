using UnityEngine;
using UnityEngine.SceneManagement; // 💡 씬 관리 기능을 사용하기 위해 추가

public class ExitDoorController : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("퀘스트 완료 후 이동할 다음 씬의 이름 (Build Settings에 추가되어야 함)")]
    public string nextSceneName = "GameOverScene";

    private string lockedMessage = "E를 눌러 상호작용 (모든 열쇠 필요)";
    private string unlockedMessage = "E를 눌러 탈출!";

    private bool isPlayerNearby = false;
    private bool isDoorOpen = false;

    private SpriteRenderer doorRenderer;
    private Collider2D doorCollider;

    // =========================================================
    // 🔊 오디오 설정
    // =========================================================
    [Header("Door Interaction Sounds 🎵")]

    [Tooltip("문을 열 수 있을 때(E 입력 시) 재생되는 소리")]
    public AudioClip interactAvailableSound;
    [Range(0f, 1f)]
    public float interactAvailableVolume = 1f;

    [Tooltip("열쇠 부족으로 문을 열 수 없을 때(E 입력 시) 재생되는 소리")]
    public AudioClip interactLockedSound;
    [Range(0f, 1f)]
    public float interactLockedVolume = 1f;

    [Tooltip("문 소리를 재생할 AudioSource")]
    public AudioSource audioSource;

    void Awake()
    {
        doorRenderer = GetComponent<SpriteRenderer>();
        doorCollider = GetComponent<Collider2D>();

        if (doorCollider == null || doorRenderer == null)
        {
            Debug.LogError("🚨 ExitDoorController: 필요한 SpriteRenderer 또는 Collider2D가 없습니다.");
        }

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("⚠️ 다음 씬 이름(nextSceneName)이 설정되지 않았습니다.");
        }
    }


    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
            HandleInteraction();
    }


    // =========================================================
    // 🔥 문 상호작용 처리 (E를 눌렀을 때만 실행됨)
    // =========================================================
    private void HandleInteraction()
    {
        if (isDoorOpen) return;

        bool questDone = (QuestManager.Instance != null && QuestManager.Instance.IsQuestCompleted);

        // 🔓 열쇠 모두 있음 → 문 열림 & 이동
        if (questDone)
        {
            PlayAvailableSound();  // E 눌렀을 때만 재생

            OpenDoor();

            if (!string.IsNullOrEmpty(nextSceneName))
                SceneManager.LoadScene(nextSceneName);

            return;
        }

        // 🔒 열쇠 부족
        PlayLockedSound();  // E 눌렀을 때만 재생

        if (FloatingNotificationUI.Instance != null)
            FloatingNotificationUI.Instance.ShowNotification($"잠김: {lockedMessage}");
    }


    // =========================================================
    // 🔊 사운드 재생 함수 (E 입력 시에만 호출됨)
    // =========================================================
    private void PlayAvailableSound()
    {
        if (audioSource != null && interactAvailableSound != null)
            audioSource.PlayOneShot(interactAvailableSound, interactAvailableVolume);
    }

    private void PlayLockedSound()
    {
        if (audioSource != null && interactLockedSound != null)
            audioSource.PlayOneShot(interactLockedSound, interactLockedVolume);
    }


    private void OpenDoor()
    {
        isDoorOpen = true;

        if (FloatingNotificationUI.Instance != null)
            FloatingNotificationUI.Instance.HideNotification();
    }


    // =========================================================
    // 🔥 범위 진입 / 이탈
    // =========================================================
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || isDoorOpen) return;

        isPlayerNearby = true;

        bool questDone = (QuestManager.Instance != null && QuestManager.Instance.IsQuestCompleted);
        string messageToShow = questDone ? unlockedMessage : lockedMessage;

        if (FloatingNotificationUI.Instance != null)
            FloatingNotificationUI.Instance.ShowNotification(messageToShow, false);
    }


    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || isDoorOpen) return;

        isPlayerNearby = false;

        if (FloatingNotificationUI.Instance != null)
            FloatingNotificationUI.Instance.HideNotification();
    }
}
