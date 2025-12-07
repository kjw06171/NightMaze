using UnityEngine;
using UnityEngine.SceneManagement; // 💡 씬 관리 기능을 사용하기 위해 추가

public class ExitDoorController : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("퀘스트 완료 후 이동할 다음 씬의 이름 (Build Settings에 추가되어야 함)")]
    public string nextSceneName = "GameOverScene"; // 인스펙터에서 설정 필요

    // 💡 문이 열렸을 때 시각적으로 표시할 메시지
    private string lockedMessage = "E를 눌러 상호작용 (모든 열쇠 필요)";
    private string unlockedMessage = "E를 눌러 탈출!";

    private bool isPlayerNearby = false;
    private bool isDoorOpen = false;
    
    // 💡 문 오브젝트의 SpriteRenderer와 Collider2D를 참조합니다.
    private SpriteRenderer doorRenderer;
    private Collider2D doorCollider;

    void Awake()
    {
        // 스크립트가 붙은 오브젝트에서 SpriteRenderer와 Collider2D를 가져옵니다.
        doorRenderer = GetComponent<SpriteRenderer>();
        doorCollider = GetComponent<Collider2D>();

        if (doorCollider == null || doorRenderer == null)
        {
            // Debug.LogWarning 대신 Debug.LogError를 사용하여 문제를 강조합니다.
            Debug.LogError("🚨 ExitDoorController: SpriteRenderer 또는 Collider2D를 찾을 수 없습니다. 문 오브젝트에 컴포넌트가 있는지 확인하세요. 이 스크립트는 이 컴포넌트들이 필요합니다.");
        }

        // 씬 이름 미설정 시 경고
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("⚠️ ExitDoorController: 다음 씬 이름(nextSceneName)이 설정되지 않았습니다. 씬 이동이 불가능합니다.");
        }
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            TryExit();
        }
    }
    
    // 💡 퀘스트 완료 여부에 따라 문을 열거나 메시지를 표시합니다.
    private void TryExit()
    {
        if (isDoorOpen) return;
        
        // UI를 사용하는 방식이므로, FloatingNotificationUI가 없다면 안전하게 종료합니다.
        if (FloatingNotificationUI.Instance == null)
        {
             Debug.Log("🚨 FloatingNotificationUI가 씬에 없습니다. 문 상호작용 UI를 표시할 수 없습니다.");
             return;
        }

        // 퀘스트 관리자의 완료 상태를 확인합니다.
        if (QuestManager.Instance != null && QuestManager.Instance.IsQuestCompleted)
        {
            // 퀘스트 완료: 문을 엽니다. (오브젝트 유지)
            OpenDoor();
            
            // 💡 [핵심: 씬 이동] 문이 열리면 다음 씬으로 이동합니다.
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                Debug.Log($"[ExitDoor - TryExit] 🎉 탈출 성공! 씬 {nextSceneName}으로 이동합니다.");
                // 씬 로드
                SceneManager.LoadScene(nextSceneName); 
            }
            else
            {
                Debug.LogWarning("[ExitDoor - TryExit] ⚠️ nextSceneName이 설정되지 않아 씬 이동을 건너뛰고 문만 열립니다.");
            }
        }
        else
        {
            // 퀘스트 미완료: 사용자에게 알립니다.
            Debug.Log($"[ExitDoor - TryExit] 🔐 아직 모든 열쇠를 모으지 못했습니다.");
            FloatingNotificationUI.Instance.ShowNotification($"잠김: {lockedMessage}");
        }
    }
    
    private void OpenDoor()
    {
        isDoorOpen = true;
        
        // 💡 [수정됨]: 사용자의 요청에 따라, 문이 열려도 오브젝트의 시각적 요소(SpriteRenderer)나 충돌체(Collider2D)를
        // 💡 비활성화하지 않고 유지합니다. 씬 이동이 즉시 발생합니다.
        
        // if (doorRenderer != null) doorRenderer.enabled = false; // 시각적 비활성화 로직 주석 처리
        // if (doorCollider != null) doorCollider.enabled = false; // 충돌체 비활성화 로직 주석 처리
        
        // 💡 문이 열리면 상호작용 UI는 숨깁니다.
        if (FloatingNotificationUI.Instance != null)
        {
            FloatingNotificationUI.Instance.HideNotification();
        }
        
        Debug.Log("🎉 문이 열렸습니다! 씬 이동을 준비합니다.");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isDoorOpen)
        {
            isPlayerNearby = true;
            
            string messageToShow;
            
            // 퀘스트 완료 여부를 확인하여 적절한 메시지를 설정
            if (QuestManager.Instance != null && QuestManager.Instance.IsQuestCompleted)
            {
                messageToShow = unlockedMessage;
            }
            else
            {
                messageToShow = lockedMessage;
            }
            
            // 💡 FloatingNotificationUI를 사용하여 화면에 고정된 상호작용 문구를 표시합니다.
            if (FloatingNotificationUI.Instance != null)
            {
                FloatingNotificationUI.Instance.ShowNotification(messageToShow, false);
            }
            Debug.Log($"[ExitDoor - Enter] 상호작용 문구 표시: {messageToShow}");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isDoorOpen)
        {
            isPlayerNearby = false;
            
            // 💡 플레이어가 벗어날 때 FloatingNotificationUI를 수동으로 숨깁니다.
            if (FloatingNotificationUI.Instance != null)
            {
                FloatingNotificationUI.Instance.HideNotification();
            }
            Debug.Log("[ExitDoor - Exit] 근처에서 벗어남. 상호작용 종료.");
        }
    }
}