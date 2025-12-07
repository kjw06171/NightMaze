using UnityEngine;
using UnityEngine.SceneManagement;

public class MansionDoorController : MonoBehaviour
{
    [Header("씬 이동 설정")]
    public string nextSceneName = "NextScene";
    public float loadDelay = 0.3f;

    [Header("퀘스트 설정")]
    public string doorQuestID = "MANSION_KEY";
    public string prerequisiteID = "CANDLE_TOGGLE";

    [Header("사운드 설정")]
    public AudioSource audioSource;

    [Tooltip("문 열림 효과음")]
    public AudioClip doorOpenSound;

    [Tooltip("문 잠김 효과음")]
    public AudioClip doorLockedSound;

    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    // =============================================
    // 🔥 문 앞 자동 대화 기능 (선택 사항)
    // =============================================
    [Header("문 앞 대화 설정 (선택 사항)")]
    public DialogueSO doorDialogue;         // 넣지 않으면 자동 대화 없음
    public bool playDialogueOnEnter = true; // 켜고 끌 수 있음
    private bool dialoguePlayed = false;    // 1회 제한

    private bool playerInRange = false;


    void Update()
    {
        if (!playerInRange) return;

        // 🔥 대화창 켜져 있으면 문 상호작용 완전 차단
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsActive())
            return;

        if (QuestManager.Instance == null)
        {
            Debug.LogError("🚨 QuestManager.Instance가 null입니다!");
            return;
        }

        var ui = FloatingNotificationUI.Instance;
        bool prereqCleared = QuestManager.Instance.IsQuestDone(prerequisiteID);

        // 🔒 선행 퀘스트 미완료 상태
        if (!prereqCleared)
        {
            if (ui != null)
                ui.ShowNotification("[잠김] 선행 퀘스트를 완수하세요.", false);

            // E 입력 시 "잠김" 소리
            if (Input.GetKeyDown(KeyCode.E))
                PlayLockedSound();

            return;
        }

        // 🔓 문 열 수 있는 상태
        if (ui != null)
            ui.ShowNotification("E 키를 눌러 문 열기", false);

        if (Input.GetKeyDown(KeyCode.E))
        {
            PlayOpenSound();

            QuestManager.Instance.CompleteQuest(doorQuestID);

            if (ui != null)
                ui.HideNotification();

            Invoke(nameof(LoadScene), loadDelay);
        }
    }

    // ==========================================================
    // 🔥 문 앞 자동 대화 실행 (옵션)
    // ==========================================================
    private void TryPlayDoorDialogue()
    {
        if (dialoguePlayed) return;                   // 이미 실행됨
        if (!playDialogueOnEnter) return;             // 기능 꺼져 있음
        if (doorDialogue == null) return;             // 대화 데이터 없음
        if (DialogueManager.Instance == null) return; // DialogueManager 없음

        // ⭐ 선행 퀘스트 미완료 상태라면 대사 실행하지 않음
        if (!string.IsNullOrEmpty(prerequisiteID))
        {
            if (QuestManager.Instance == null) return;

            bool prereqCleared = QuestManager.Instance.IsQuestDone(prerequisiteID);
            if (!prereqCleared)
                return;
        }

        // 선행 퀘 완료된 경우에만 대사 시작
        DialogueManager.Instance.StartDialogue(doorDialogue, null);
        dialoguePlayed = true;
    }

    // ==========================================================
    // 🔊 사운드
    // ==========================================================
    private void PlayOpenSound()
    {
        if (audioSource != null && doorOpenSound != null)
            audioSource.PlayOneShot(doorOpenSound, sfxVolume);
    }

    private void PlayLockedSound()
    {
        if (audioSource != null && doorLockedSound != null)
            audioSource.PlayOneShot(doorLockedSound, sfxVolume);
    }

    // ==========================================================
    void LoadScene()
    {
        if (FadeManager.Instance != null)
            FadeManager.Instance.FadeToScene(nextSceneName);
        else
            SceneManager.LoadScene(nextSceneName);
    }

    // ==========================================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            // 🔥 콜라이더 진입 시 자동 대화 (선행 퀘 완료 상태에서만)
            TryPlayDoorDialogue();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (FloatingNotificationUI.Instance != null)
                FloatingNotificationUI.Instance.HideNotification();
        }
    }
}
