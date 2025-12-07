using UnityEngine;
using System; // Action 사용을 위해 추가
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 상호작용(E 키)으로 아이템 획득 + 대화 실행 + 스토리 UI 표시 + 선행퀘스트 체크
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [Header("아이템 정보")]
    public string itemID = "KEY_A";

    [Header("대화 데이터 연결")]
    [SerializeField]
    private DialogueSO dialogueData;

    [Header("스토리 UI (선택)")]
    public GameObject storyUIPanel;           // ← Story UI 패널 (없어도 OK)

    [Header("상호작용 알림 설정")]
    public bool useNotificationUI = true;
    public string interactionMessage = "E키를 눌러 획득";

    [Header("선행 퀘스트 설정")]
    public string requiredQuestID = "";       // 빈 값이면 선행퀘 없음
    public string lockedMessage = "[잠김] 선행 퀘스트를 완료하세요";

    [Header("입력키")]
    public KeyCode interactionKey = KeyCode.E;

    [Header("사운드 설정")]
    public AudioClip pickupSound;
    public AudioSource audioSource;
    [Range(0f, 1f)]
    public float pickupVolume = 1f;

    [Header("선행 퀘스트 미완료 사운드")]
    public AudioClip lockedSound;
    [Range(0f, 1f)]
    public float lockedVolume = 1f;

    private bool playerInRange = false;
    private bool isInteractable = false; // 기본값 false로 설정하여 Start에서 true로 활성화하는 것이 안전

    // ==========================================================
    // 초기 설정
    // ==========================================================
    void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
            Debug.LogWarning($"[ItemPickup] 콜라이더가 Trigger가 아닙니다: {gameObject.name}");

        isInteractable = true; // Start 시 상호작용 가능하도록 설정

        // 이미 CANDLE 먹었으면 제거 (영구 획득 아이템)
        if (itemID == "CANDLE" && GameState.HasCandle)
        {
            isInteractable = false;
            Destroy(gameObject);
            return;
        }
        
        // ⭐ 참고: KEY_A, B, C는 스테이지 재탕을 위해 여기서 영구 제거하지 않습니다.
        // 대신 스테이지 전환 시 GameState.HasKeyX 변수를 외부(GameManager 등)에서 초기화해야 합니다.
    }

    // ==========================================================
    // Update – 아이템 획득 처리
    // ==========================================================
    void Update()
    {
        if (!playerInRange || !isInteractable)
            return;

        bool prereqCleared = IsPrerequisiteCleared();

        // 선행 퀘스트 미완료 상태
        if (!prereqCleared)
        {
            // E를 눌렀을 때만 잠김 사운드 재생
            if (Input.GetKeyDown(interactionKey))
            {
                PlayLockedSound();
            }
            return;
        }

        // 선행 퀘스트 완료 상태에서만 실제 픽업 처리
        if (Input.GetKeyDown(interactionKey))
        {
            bool isDialogueActive =
                (DialogueManager.Instance != null && DialogueManager.Instance.IsActive());

            if (!isDialogueActive)
                PickUp();
        }
    }

    // ==========================================================
    // 선행 퀘스트 완료 여부 체크
    // ==========================================================
    private bool IsPrerequisiteCleared()
    {
        if (string.IsNullOrEmpty(requiredQuestID))
            return true;

        if (QuestManager.Instance == null)
            return true;

        return QuestManager.Instance.IsQuestDone(requiredQuestID);
    }

    // ==========================================================
    // 플레이어 트리거 진입
    // ==========================================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || !isInteractable)
            return;

        playerInRange = true;

        if (!useNotificationUI || FloatingNotificationUI.Instance == null)
            return;

        // 🔒 잠김 상태 UI 표시
        if (!IsPrerequisiteCleared())
            FloatingNotificationUI.Instance.ShowNotification(lockedMessage, false);
        else
            FloatingNotificationUI.Instance.ShowNotification(interactionMessage, false);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        if (useNotificationUI && FloatingNotificationUI.Instance != null)
            FloatingNotificationUI.Instance.HideNotification();
    }

    // ==========================================================
    // PickUp – 아이템 획득
    // ==========================================================
    private void PickUp()
    {
        isInteractable = false;

        // ⭐ 아이템 획득 사운드 재생
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound, pickupVolume);
        }

        // 🔉 타이핑 소리 잠깐 낮추기
        StartCoroutine(ReduceTypingVolumeTemporarily());

        if (useNotificationUI && FloatingNotificationUI.Instance != null)
        {
            FloatingNotificationUI.Instance.HideNotification();
        }

        // 🔥 CANDLE 상태 업데이트 (영구 아이템)
        if (itemID == "CANDLE")
        {
            GameState.HasCandle = true;
        }
        else if (itemID == "KEY_A")
        {
            GameState.HasKeyA = true;
        }
        else if (itemID == "KEY_B")
        {
            GameState.HasKeyB = true;
            RatAI.keyBCollected = true;
        }
        else if (itemID == "KEY_C")
        {
            GameState.HasKeyC = true;
        }

        // 🔥 스토리 UI
        if (storyUIPanel != null)
        {
            StoryUIFader fader = storyUIPanel.GetComponent<StoryUIFader>();

            if (fader != null)
            {
                fader.Play(() =>
                {
                    StartItemDialogue();
                });

                return;
            }
            else
            {
                storyUIPanel.SetActive(true);
            }
        }

        StartItemDialogue();
    }

    private IEnumerator ReduceTypingVolumeTemporarily()
    {
        if (DialogueManager.Instance == null || DialogueManager.Instance.typingAudioSource == null)
            yield break;

        AudioSource typing = DialogueManager.Instance.typingAudioSource;

        float originalVolume = typing.volume;
        float loweredVolume = originalVolume * 0.4f;  // 🔉 40%로 감소

        typing.volume = loweredVolume;

        yield return new WaitForSecondsRealtime(0.3f);  // ⭐ 0.3초 유지

        typing.volume = originalVolume; // 🔊 원래대로 복구
    }

    private void StartItemDialogue()
    {
        // DialogueManager가 없으면 즉시 종료 처리
        if (DialogueManager.Instance == null)
        {
            OnDialogueEnd();
            return;
        }

        // 대화 실행
        if (dialogueData != null)
        {
            DialogueManager.Instance.StartDialogue(dialogueData, OnDialogueEnd);
        }
        else
        {
            OnDialogueEnd();
        }
    }

    // ==========================================================
    // 대화 종료 후 콜백
    // ==========================================================
    private void OnDialogueEnd()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.CompleteQuest(itemID);

        Destroy(gameObject);
    }

    // ==========================================================
    // 🔊 선행 퀘스트 미완료 사운드
    // ==========================================================
    private void PlayLockedSound()
    {
        // 퍼즈 메뉴 열려 있을 땐 재생 안 되게
        if (PauseMenu.isGamePaused)
            return;

        if (audioSource != null && lockedSound != null)
        {
            audioSource.PlayOneShot(lockedSound, lockedVolume);
        }
    }
}
