using UnityEngine;
using UnityEngine.SceneManagement;   // ← 🔥 추가: SceneManager 쓰려고
                                     // (씬 이름 비교할 때 필요)

public class HealthPotionItem : MonoBehaviour
{
    [Header("회복 설정")]
    public int HealAmount = 1;

    [Header("UI 설정")]
    public GameObject floatingTextPrefab; 
    public string fullHealthMessage = "체력이 이미 가득 찼습니다!";

    [Header("캔버스 설정")]
    public Canvas targetCanvas;

    [Header("튜토리얼 대사 연결 (Lv_00_2 전용)")]
    public DialogueSO itemDialogue;   // ← 🔥 Inspector에서 SO 드래그해서 넣기

    [Header("상호작용 메시지 옵션")]
    public bool showInteractMessage = true;

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TryHealPlayer();
        }
    }

    // 🔥 아이템이 실제로 획득된 뒤 실행되는 처리
    private void Collect()
    {
        // 퀘스트 진행도 1 증가
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddProgress("COLLECT_ITEMS", 1);
        }

        // 아이템 파괴
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (showInteractMessage && FloatingNotificationUI.Instance != null)
                FloatingNotificationUI.Instance.ShowNotification("E키를 눌러 열기", false);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (showInteractMessage && FloatingNotificationUI.Instance != null)
                FloatingNotificationUI.Instance.HideNotification();
        }
    }

    private void TryHealPlayer()
    {
        Transform playerRoot = FindObjectOfType<PlayerHealth>()?.transform.root; 
        if (playerRoot == null)
        {
            ShowFloatingMessage(this.transform.position, "플레이어를 찾을 수 없습니다!");
            return;
        }

        PlayerHealth healthControl = playerRoot.GetComponentInChildren<PlayerHealth>();

        if (healthControl != null)
        {
            if (healthControl.IsHealthFull())
            {
                ShowFloatingMessage(this.transform.position, fullHealthMessage);
                return;
            }

            // ✅ 체력 회복 적용
            healthControl.Heal(HealAmount);

            // ✅ 회복 텍스트
            ShowFloatingMessage(this.transform.position, $"+{HealAmount:F0} HP 회복!");

            // ✅ Lv_00_2에서만 대사 실행
            if (SceneManager.GetActiveScene().name == "Lv_00_2")
            {
                if (itemDialogue != null && DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartDialogue(itemDialogue);
                }
                else
                {
                    Debug.LogWarning("itemDialogue 또는 DialogueManager가 비어있음");
                }
            }

            // ✅ 실제 아이템 획득 처리 (퀘스트 진행 + 파괴)
            Collect();
        }
        else
        {
            ShowFloatingMessage(this.transform.position, "체력 스크립트 오류!");
        }
    }

    private void ShowFloatingMessage(Vector3 position, string message)
    {
        if (targetCanvas == null)
            targetCanvas = FindObjectOfType<Canvas>();

        Camera cam = Camera.main;

        if (floatingTextPrefab != null && targetCanvas != null && cam != null)
        {
            Vector2 screenPoint = cam.WorldToScreenPoint(position);
            Vector2 localPoint;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetCanvas.GetComponent<RectTransform>(),
                screenPoint,
                targetCanvas.worldCamera,
                out localPoint
            );

            GameObject messageInstance = Instantiate(floatingTextPrefab, targetCanvas.transform);

            RectTransform rectTransform = messageInstance.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                float heightOffset = -40f;
                localPoint.y += heightOffset;

                rectTransform.localPosition = localPoint;
                rectTransform.localScale = Vector3.one;
            }

            FloatingMessage floatingScript = messageInstance.GetComponent<FloatingMessage>();
            if (floatingScript != null)
                floatingScript.SetMessage(message);
        }
        else
        {
            Debug.LogError("🚨 UI 생성 실패: FloatingTextPrefab / targetCanvas / MainCamera 중 하나가 Null입니다.");
        }
    }
}
