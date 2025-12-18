using UnityEngine;

/// <summary>
/// 플레이어가 콜라이더 영역에 진입했을 때 대화를 시작하는 컴포넌트입니다.
/// 선행 퀘스트 조건 추가 버전.
/// </summary>
public class DialogueTrigger : MonoBehaviour
{
    [Header("대화 설정")]
    [SerializeField] 
    private DialogueSO dialogueData;

    [Tooltip("대화가 한 번 시작된 후 다시 트리거되지 않게 할지 설정합니다.")]
    public bool triggerOnce = true;

    [Header("선행 퀘스트 설정")]
    [Tooltip("이 대화가 실행되기 전에 완료되어야 하는 퀘스트 ID (없으면 빈칸)")]
    public string prerequisiteQuestID = "";

    [Tooltip("선행 퀘스트 미완료 시 띄울 문구 (FloatingNotificationUI 사용)")]
    public string prerequisiteNotMetMessage = "선행 퀘스트를 완료하세요.";

    private bool hasBeenTriggered = false;
    private bool isDialogueActive = false;

    private void OnValidate()
    {
        Collider2D col2D = GetComponent<Collider2D>();
        if (col2D == null)
        {
            Debug.LogError($"[DialogueTrigger] 오브젝트 ({gameObject.name})에는 Collider2D 컴포넌트가 필요합니다!");
        }
        else if (!col2D.isTrigger)
        {
            Debug.LogWarning($"[DialogueTrigger] 오브젝트 ({gameObject.name})의 Collider2D는 Is Trigger가 활성화되어야 합니다.");
        }

        if (GetComponent<Rigidbody2D>() == null)
        {
            Debug.LogWarning($"[DialogueTrigger] 오브젝트 ({gameObject.name})에는 물리 충돌 감지를 위해 Rigidbody2D 컴포넌트가 필요합니다.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleTrigger(other.gameObject);
    }

    private void HandleTrigger(GameObject other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (isDialogueActive || (triggerOnce && hasBeenTriggered))
            return;

        // 🔥 1) 선행 퀘스트 체크 추가
        if (!IsPrerequisiteCleared())
        {
            ShowPrerequisiteMessage();
            return;
        }

        // 🔥 2) DialogueManager 유효성 확인
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("[DialogueTrigger] DialogueManager 인스턴스를 찾을 수 없습니다!");
            return;
        }

        if (dialogueData == null)
        {
            Debug.LogWarning($"[DialogueTrigger] 오브젝트 ({gameObject.name})에 DialogueSO가 없습니다!");
            return;
        }

        StartDialogueSequence();
    }

    // ==============================================================
    // 🔥 선행 퀘스트 충족 여부 확인
    // ==============================================================
    private bool IsPrerequisiteCleared()
    {
        // 선행 퀘스트 ID가 없으면 바로 통과
        if (string.IsNullOrEmpty(prerequisiteQuestID))
            return true;

        // QuestManager가 없으면 강제 통과 (에러 방지)
        if (QuestManager.Instance == null)
            return true;

        return QuestManager.Instance.IsQuestDone(prerequisiteQuestID);
    }

    // 🔥 선행 퀘스트 미완료 메시지 표시
    private void ShowPrerequisiteMessage()
    {
        if (FloatingNotificationUI.Instance != null)
        {
            FloatingNotificationUI.Instance.ShowNotification(prerequisiteNotMetMessage, false);
        }
        Debug.Log($"[DialogueTrigger] 선행 퀘스트({prerequisiteQuestID})가 완료되지 않아 대화가 차단됨.");
    }

    // ==============================================================

    private void StartDialogueSequence()
    {
        Debug.Log($"대화 시작: {dialogueData.name} by {gameObject.name}");

        isDialogueActive = true;
        hasBeenTriggered = true;

        DialogueManager.Instance.StartDialogue(dialogueData, OnDialogueEndCallback);
    }

    public void OnDialogueEndCallback()
    {
        isDialogueActive = false;
        Debug.Log($"대화 종료: {dialogueData.name}");
    }
}
