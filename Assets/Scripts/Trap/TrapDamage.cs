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

    private float lastDamageTime = -999f;
    private bool tutorialTriggered = false;

    // 🔊 트랩 밟을 때 나는 소리
    [Header("트랩 사운드")]
    public AudioSource trapAudioSource;  // 붙여줄 오디오소스
    public AudioClip trapSoundClip;      // 재생할 소리
    [Range(0f, 1f)]
    public float trapVolume = 1f;        // 볼륨 조절 가능
    public bool ignoreListenerPause = true; // 퍼즈 때도 소리 나게 할건지

    private void Awake()
    {
        // 옵션 적용
        if (trapAudioSource != null)
        {
            trapAudioSource.ignoreListenerPause = ignoreListenerPause;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerFeet")) return;

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null) return;

        // 쿨타임 체크
        if (Time.time - lastDamageTime < damageCooldown) return;

        // 데미지 적용
        playerHealth.TakeDamage(damageAmount);
        lastDamageTime = Time.time;

        // 🔊 트랩 사운드 재생
        PlayTrapSound();

        Debug.Log("함정 데미지 적용됨 (입장 시)");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerFeet")) return;

        if (tutorialTriggered) return;

        if (SceneManager.GetActiveScene().name != "Lv_00_2") return;

        tutorialTriggered = true;

        // 퀘스트 완료
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest(trapQuestID);
            Debug.Log("🎉 TRAP_TUTORIAL 퀘스트 완료!");
        }

        // 대화 실행
        if (DialogueManager.Instance != null && trapTutorialDialogue != null)
        {
            DialogueManager.Instance.StartDialogue(trapTutorialDialogue);
            Debug.Log("💬 함정 튜토리얼 대화 시작 (트랩 벗어났을 때)");
        }
        else
        {
            Debug.LogWarning("⚠ trapTutorialDialogue 또는 DialogueManager가 설정되지 않음");
        }
    }

    // 🔊 소리 재생 함수
    private void PlayTrapSound()
    {
        if (trapAudioSource != null && trapSoundClip != null)
        {
            trapAudioSource.volume = trapVolume;
            trapAudioSource.PlayOneShot(trapSoundClip);
        }
    }
}
