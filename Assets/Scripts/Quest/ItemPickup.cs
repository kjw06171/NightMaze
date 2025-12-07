using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [Header("아이템 정보")]
    public string itemID = "KEY_A";

    [Header("대화 데이터 연결")]
    [SerializeField]
    private DialogueSO dialogueData;

    [Header("스토리 UI (선택)")]
    public GameObject storyUIPanel;

    [Header("상호작용 UI")]
    public bool useNotificationUI = true;
    public string interactionMessage = "E키를 눌러 획득";

    [Header("선행 퀘스트 설정")]
    public string requiredQuestID = "";
    public string lockedMessage = "[잠김] 선행 퀘스트를 완료하세요";

    [Header("입력키")]
    public KeyCode interactionKey = KeyCode.E;

    [Header("사운드 설정")]
    public AudioClip pickupSound;
    public AudioSource audioSource;
    [Range(0f, 1f)] public float pickupVolume = 1f;

    [Header("선행 퀘스트 미완료 사운드")]
    public AudioClip lockedSound;
    [Range(0f, 1f)] public float lockedVolume = 1f;


    [Header("스토리 패널 BGM 설정")]
    public float storyLoweredVolume = 0.25f;   // 스토리 중 유지할 볼륨
    public float storyFadeOutTime = 0.8f;      // 스토리 패널 열릴 때 BGM 감소 속도
    public float storyFadeInTime = 0.8f; 

    private bool playerInRange = false;
    private bool isInteractable = false;

    void Start()
    {
        isInteractable = true;

        // 이미 가진 CANDLE은 제거
        if (itemID == "CANDLE" && GameState.HasCandle)
        {
            isInteractable = false;
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        if (!playerInRange || !isInteractable)
            return;

        bool prereqCleared = IsPrerequisiteCleared();

        if (!prereqCleared)
        {
            if (Input.GetKeyDown(interactionKey))
                PlayLockedSound();
            return;
        }

        if (Input.GetKeyDown(interactionKey))
        {
            bool isDialogueActive =
                (DialogueManager.Instance != null && DialogueManager.Instance.IsActive());

            if (!isDialogueActive)
                PickUp();
        }
    }

    private bool IsPrerequisiteCleared()
    {
        if (string.IsNullOrEmpty(requiredQuestID)) return true;
        if (QuestManager.Instance == null) return true;
        return QuestManager.Instance.IsQuestDone(requiredQuestID);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || !isInteractable)
            return;

        playerInRange = true;

        if (!useNotificationUI || FloatingNotificationUI.Instance == null)
            return;

        if (!IsPrerequisiteCleared())
            FloatingNotificationUI.Instance.ShowNotification(lockedMessage, false);
        else
            FloatingNotificationUI.Instance.ShowNotification(interactionMessage, false);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (useNotificationUI && FloatingNotificationUI.Instance != null)
            FloatingNotificationUI.Instance.HideNotification();
    }

    // ============================
    // ------- 아이템 획득 ---------
    // ============================
    private void PickUp()
    {
        isInteractable = false;

        if (pickupSound != null && audioSource != null)
            audioSource.PlayOneShot(pickupSound, pickupVolume);

        StartCoroutine(ReduceTypingVolumeTemporarily());

        if (useNotificationUI && FloatingNotificationUI.Instance != null)
            FloatingNotificationUI.Instance.HideNotification();

        // KEY 상태 업데이트
        if (itemID == "CANDLE") GameState.HasCandle = true;
        else if (itemID == "KEY_A") GameState.HasKeyA = true;
        else if (itemID == "KEY_B") { GameState.HasKeyB = true; RatAI.keyBCollected = true; }
        else if (itemID == "KEY_C") GameState.HasKeyC = true;

        // ----------------------------
        // 스토리 UI + BGM Fade
        // ----------------------------
        if (storyUIPanel != null)
        {
            StoryUIFader fader = storyUIPanel.GetComponent<StoryUIFader>();
            if (fader != null)
            {
                // ⭐ 원래 볼륨 저장
                float originalVolume = (BGMManager.Instance != null)
                    ? BGMManager.Instance.CurrentVolume
                    : 1f;

                // ⭐ 스토리 패널 시작 → BGM 줄이기 (속도 & 목표 볼륨 모두 조절 가능)
                if (BGMManager.Instance != null)
                    BGMManager.Instance.FadeTo(storyLoweredVolume, storyFadeOutTime);

                // ⭐ 스토리 UI 실행
                fader.Play(() =>
                {
                    // ⭐ 스토리 끝 → 원래 볼륨으로 복구 (속도 조절 가능)
                    if (BGMManager.Instance != null)
                        BGMManager.Instance.FadeTo(originalVolume, storyFadeInTime);

                    StartItemDialogue();
                });

                return;
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
        float loweredVolume = originalVolume * 0.4f;

        typing.volume = loweredVolume;

        yield return new WaitForSecondsRealtime(0.3f);

        typing.volume = originalVolume;
    }

    private void StartItemDialogue()
    {
        if (DialogueManager.Instance == null)
        {
            OnDialogueEnd();
            return;
        }

        if (dialogueData != null)
            DialogueManager.Instance.StartDialogue(dialogueData, OnDialogueEnd);
        else
            OnDialogueEnd();
    }

    private void OnDialogueEnd()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.CompleteQuest(itemID);

        Destroy(gameObject);
    }

    private void PlayLockedSound()
    {
        if (PauseMenu.isGamePaused)
            return;

        if (audioSource != null && lockedSound != null)
            audioSource.PlayOneShot(lockedSound, lockedVolume);
    }
}
