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
    public float sfxVolume = 1f;   // 🔊 문 사운드 볼륨

    private bool playerInRange = false;

    void Update()
    {
        if (!playerInRange) return;

        if (QuestManager.Instance == null)
        {
            Debug.LogError("🚨 QuestManager.Instance가 null입니다!");
            return;
        }

        var ui = FloatingNotificationUI.Instance;
        bool prereqCleared = QuestManager.Instance.IsQuestDone(prerequisiteID);

        // 🔒 선행 퀘스트 미완료
        if (!prereqCleared)
        {
            if (ui != null)
                ui.ShowNotification("[잠김] 선행 퀘스트를 완수하세요.", false);

            // 🔊 문 잠김 효과음 (E 입력했을 때만 재생)
            if (Input.GetKeyDown(KeyCode.E))
                PlayLockedSound();

            return;
        }

        // 🔓 문을 열 수 있는 상태
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

    // ----------------------------------------------------------
    // 🔊 사운드 재생 함수
    // ----------------------------------------------------------
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

    // ----------------------------------------------------------
    void LoadScene()
    {
        if (FadeManager.Instance != null)
            FadeManager.Instance.FadeToScene(nextSceneName);
        else
        {
            Debug.LogWarning("⚠ FadeManager null → 즉시 씬 이동");
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
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
