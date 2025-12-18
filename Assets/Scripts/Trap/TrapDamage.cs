using UnityEngine;
using UnityEngine.SceneManagement;

public class TrapDamage : MonoBehaviour
{
    [Header("데미지 설정")]
    public int damageAmount = 1;
    public float damageCooldown = 1f;

    [Header("튜토리얼 대화 연결")]
    public DialogueSO trapTutorialDialogue;

    [Header("튜토리얼 퀘스트 ID")]
    public string trapQuestID = "TRAP_TUTORIAL";

    [Header("튜토리얼 종료 퀘스트 ID")]
    public string tutorialEndQuestID = "TUTORIAL_END";

    private float lastDamageTime = -999f;
    private bool tutorialTriggered = false;

    [Header("트랩 사운드")]
    public AudioSource trapAudioSource;
    public AudioClip trapSoundClip;
    [Range(0f, 1f)]
    public float trapVolume = 1f;
    public bool ignoreListenerPause = true;

    private void Awake()
    {
        if (trapAudioSource != null)
            trapAudioSource.ignoreListenerPause = ignoreListenerPause;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerFeet")) return;

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null) return;

        if (Time.time - lastDamageTime < damageCooldown) return;

        playerHealth.TakeDamage(damageAmount);
        lastDamageTime = Time.time;

        PlayTrapSound();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerFeet")) return;
        if (SceneManager.GetActiveScene().name != "Lv_00_2") return;
        if (tutorialTriggered) return;

        // =========================
        // 1️⃣ 첫 번째 함정 (조건 없음)
        // =========================
        if (!GameState.TrapTutorialSeen)
        {
            TriggerTrapTutorial();
            GameState.TrapTutorialSeen = true;
            tutorialTriggered = true;
            return;
        }

        // =========================
        // 2️⃣ 이후 함정 (COLLECT_ITEMS 완료 필요)
        // =========================
        if (QuestManager.Instance == null ||
            !QuestManager.Instance.IsQuestDone("COLLECT_ITEMS"))
        {
            return;
        }

        TriggerTrapTutorial();

        // ⭐ 튜토리얼 종료 퀘스트 완료 (여기서만!)
        QuestManager.Instance.CompleteQuest(tutorialEndQuestID);
        Debug.Log("🏁 TUTORIAL_END 퀘스트 완료!");

        tutorialTriggered = true;
    }

    // ---------------------------------------------------------
    // 🔥 트랩 튜토리얼 공통 처리
    // ---------------------------------------------------------
    private void TriggerTrapTutorial()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest(trapQuestID);
            Debug.Log("🎉 TRAP_TUTORIAL 퀘스트 완료!");
        }

        if (DialogueManager.Instance != null && trapTutorialDialogue != null)
        {
            DialogueManager.Instance.StartDialogue(trapTutorialDialogue);
            Debug.Log("💬 함정 튜토리얼 대화 시작");
        }
    }

    // ---------------------------------------------------------
    // 🔊 트랩 사운드
    // ---------------------------------------------------------
    private void PlayTrapSound()
    {
        if (trapAudioSource != null && trapSoundClip != null)
        {
            trapAudioSource.volume = trapVolume;
            trapAudioSource.PlayOneShot(trapSoundClip);
        }
    }
}
