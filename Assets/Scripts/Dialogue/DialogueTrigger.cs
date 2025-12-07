using UnityEngine;

/// <summary>
/// 플레이어가 콜라이더 영역에 진입했을 때 대화를 시작하는 컴포넌트입니다.
/// (2D 전용으로 수정됨)
/// </summary>
public class    r : MonoBehaviour
{
    [Header("대화 설정")]
    [Tooltip("이 오브젝트와 연결된 대화 ScriptableObject (DialogueSO)를 연결하세요.")]
    [SerializeField] 
    private DialogueSO dialogueData;

    [Tooltip("대화가 한 번 시작된 후 다시 트리거되지 않게 할지 설정합니다.")]
    public bool triggerOnce = true;

    // 💡 대화가 이미 트리거되었는지 추적하는 플래그
    private bool hasBeenTriggered = false;

    // 💡 현재 대화가 진행 중인지 확인 (선택 사항: 플레이어 제어 스크립트에서 확인할 수 있습니다)
    private bool isDialogueActive = false;

    // ----------------------------------------------------
    // 💡 필수 컴포넌트 확인: 콜라이더와 리지드바디 (2D 전용)
    // ----------------------------------------------------
    private void OnValidate()
    {
        // 2D Collider 컴포넌트가 있는지 확인
        Collider2D col2D = GetComponent<Collider2D>();
        if (col2D == null)
        {
            Debug.LogError($"[DialogueTrigger] 오브젝트 ({gameObject.name})에는 Collider2D 컴포넌트가 필요합니다!");
        }
        else if (!col2D.isTrigger)
        {
            Debug.LogWarning($"[DialogueTrigger] 오브젝트 ({gameObject.name})의 Collider2D는 Is Trigger가 활성화되어야 합니다.");
        }

        // Rigidbody2D 컴포넌트가 있는지 확인
        if (GetComponent<Rigidbody2D>() == null)
        {
            Debug.LogWarning($"[DialogueTrigger] 오브젝트 ({gameObject.name})에는 물리 충돌 감지를 위해 Rigidbody2D 컴포넌트가 필요합니다. Is Kinematic을 설정할 수 있습니다.");
        }
    }
    
    /// <summary>
    /// 다른 콜라이더가 트리거 영역에 진입했을 때 호출됩니다. (2D 전용)
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleTrigger(other.gameObject);
    }
    
    // 💡 3D 충돌 감지 함수 (OnTriggerEnter)는 2D 전용 요청에 따라 제거되었습니다.

    private void HandleTrigger(GameObject other)
    {
        // 1. 플레이어 태그가 맞는지 확인
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // 2. 대화가 이미 진행 중인지, 한 번만 트리거되도록 설정했는지 확인
        if (isDialogueActive || (triggerOnce && hasBeenTriggered))
        {
            return;
        }
        
        // 3. DialogueManager의 유효성 및 데이터 확인
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("[DialogueTrigger] DialogueManager 인스턴스를 찾을 수 없습니다!");
            return;
        }

        if (dialogueData == null)
        {
            Debug.LogWarning($"[DialogueTrigger] 오브젝트 ({gameObject.name})에 연결된 DialogueSO 데이터가 없습니다!");
            return;
        }

        // 4. 대화 시작
        StartDialogueSequence();
    }

    /// <summary>
    /// 대화를 시작하고 상태를 업데이트합니다.
    /// </summary>
    private void StartDialogueSequence()
    {
        Debug.Log($"대화 시작: {dialogueData.name} by {gameObject.name}");
        
        // 💡 대화 시작 상태 설정
        isDialogueActive = true;
        hasBeenTriggered = true; // 대화 시작 시점부터 이미 트리거된 것으로 간주
        
        // 여기에 DialogueManager의 실제 StartDialogue 호출 로직을 넣습니다.
        DialogueManager.Instance.StartDialogue(dialogueData); 
        
        // NOTE: 실제 프로젝트에서는 DialogueManager의 OnDialogueEnd 이벤트에 
        // DialogueTrigger의 OnDialogueEndCallback 함수를 구독하여 isDialogueActive = false;를 설정해야 합니다.
    }

    // 대화가 끝났을 때 DialogueManager에 의해 호출되어야 하는 콜백 함수
    public void OnDialogueEndCallback()
    {
        isDialogueActive = false;
        Debug.Log($"대화 종료: {dialogueData.name}");
        
        // NOTE: 만약 이 오브젝트가 대화 후 바로 파괴되어야 한다면 여기에 Destroy(gameObject);를 추가합니다.
    }
}