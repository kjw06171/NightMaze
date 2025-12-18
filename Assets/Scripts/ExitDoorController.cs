using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro 사용

public class ExitDoorController : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("퀘스트 완료 후 이동할 다음 씬의 이름 (Build Settings에 추가되어야 함)")]
    public string nextSceneName = "GameOverScene";

    private string lockedMessage = "E를 눌러 상호작용 (모든 열쇠 필요)";
    private string unlockedMessage = "E를 눌러 탈출!";

    private bool isPlayerNearby = false;
    private bool isDoorOpen = false; // 문이 열렸는지 확인하는 변수
    private bool hasPlayedOpenSound = false; // 소리가 이미 재생된 여부를 확인

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

    // 대화 SO 연결
    [Header("대화 데이터 (DialogueSO)")]
    public DialogueSO dialogueData; // 대화 데이터 연결

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

            // 대화창 표시
            ShowExitDialogue(() =>
            {
                OpenDoor();
                if (!string.IsNullOrEmpty(nextSceneName))
                    SceneManager.LoadScene(nextSceneName);
            });
            return;
        }

        // 🔒 열쇠 부족
        PlayLockedSound();  // E 눌렀을 때만 재생

        // 🔥 FloatingMessage 사용하여 메시지 표시
        ShowFloatingMessage(lockedMessage);
    }

    // =========================================================
    // 🔊 사운드 재생 함수 (E 입력 시에만 호출됨)
    // =========================================================
    private void PlayAvailableSound()
    {
        if (audioSource != null && interactAvailableSound != null && !hasPlayedOpenSound)
        {
            audioSource.PlayOneShot(interactAvailableSound, interactAvailableVolume);
            hasPlayedOpenSound = true; // 소리가 재생되었으므로 한 번만 재생되도록 설정
        }
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

    // =========================================================
    // 🔥 대화창 표시 (Exit 대화)
    // =========================================================
    private void ShowExitDialogue(System.Action onDialogueEnd)
    {
        // 대화창 표시
        if (DialogueManager.Instance != null && dialogueData != null)
        {
            DialogueManager.Instance.StartDialogue(dialogueData, () =>
            {
                // 대화가 끝난 후, 문을 열고 씬을 전환
                onDialogueEnd?.Invoke();
            });
        }
    }

    // =========================================================
    // 🔥 FloatingMessage UI 표시 (문과 관련된 메시지)
    // =========================================================
    private void ShowFloatingMessage(string message)
    {
        // 새로운 FloatingMessage 객체를 생성하여 메시지를 화면에 표시합니다.
        GameObject floatingMessageObj = new GameObject("FloatingMessage");
        TextMeshProUGUI messageText = floatingMessageObj.AddComponent<TextMeshProUGUI>();
        messageText.text = message;

        // FloatingMessage UI 설정
        RectTransform rectTransform = messageText.GetComponent<RectTransform>();
        rectTransform.SetParent(transform); // ExitDoorController 오브젝트 아래에 배치

        // 위치 설정 (오브젝트 아래로 위치하도록)
        rectTransform.anchoredPosition = new Vector2(0f, -50f); // 오브젝트 아래로 50px

        // FloatingMessage 생명 주기 설정 (일정 시간 후 삭제)
        Destroy(floatingMessageObj, 1.5f); // 1.5초 후에 메시지 제거
    }
}
