using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Header("가이드 메시지 (범위 진입 시)")]
    public bool useGuideMessage = true;

    [TextArea]
    public string guideMessage = "E키를 눌러 저장";

    [Header("저장 완료 메시지")]
    public string savedMessage = "저장되었습니다!";

    [Header("Floating Message 설정 (저장 완료용)")]
    public GameObject floatingTextPrefab;
    public Canvas targetCanvas;

    [Header("입력키")]
    public KeyCode interactionKey = KeyCode.E;

    [Header("사운드 설정")]
    public AudioClip saveSound;

    [Range(0f, 1f)]
    public float saveSoundVolume = 1f;

    [Tooltip("사운드 재생용 AudioSource (없으면 자동 생성)")]
    public AudioSource audioSource;

    [Header("아이템 설명 대사 (Lv_00_2 전용)")]
    public DialogueSO itemDialogue;

    private bool isPlayerInRange = false;
    private bool guideVisible = false;
    private PlayerHealth playerRef;

    // =========================
    // Start
    // =========================
    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.spatialBlend = 0f; // 2D 사운드
        audioSource.playOnAwake = false;
    }

    // =========================
    // Trigger
    // =========================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInRange = true;
        playerRef = other.GetComponent<PlayerHealth>();

        if (useGuideMessage && FloatingNotificationUI.Instance != null)
        {
            FloatingNotificationUI.Instance.ShowNotification(guideMessage, false);
            guideVisible = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInRange = false;
        playerRef = null;

        if (guideVisible && FloatingNotificationUI.Instance != null)
        {
            FloatingNotificationUI.Instance.HideNotification();
            guideVisible = false;
        }
    }

    // =========================
    // Update
    // =========================
    private void Update()
    {
        if (!isPlayerInRange || playerRef == null)
            return;

        if (Input.GetKeyDown(interactionKey))
        {
            // 1️⃣ 체크포인트 저장
            playerRef.UpdateRespawnPosition(transform.position);

            // 🔊 저장 사운드
            if (audioSource != null && saveSound != null)
                audioSource.PlayOneShot(saveSound, saveSoundVolume);

            // 2️⃣ 가이드 메시지 끄기
            if (guideVisible && FloatingNotificationUI.Instance != null)
            {
                FloatingNotificationUI.Instance.HideNotification();
                guideVisible = false;
            }

            // 3️⃣ 저장 완료 플로팅 메시지
            ShowFloatingMessage(transform.position, savedMessage);

            // 4️⃣ COLLECT_ITEMS 퀘스트 진행도 +1
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.AddProgress("COLLECT_ITEMS", 1);
            }

            // ⭐ 5️⃣ Lv_00_2 전용 아이템 설명 대사
            if (SceneManager.GetActiveScene().name == "Lv_00_2")
            {
                if (itemDialogue != null && DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartDialogue(itemDialogue);
                }
            }

            // 6️⃣ 1회용 체크포인트
            gameObject.SetActive(false);
        }
    }

    // =================================================
    // FloatingMessage 출력
    // =================================================
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

            GameObject messageInstance =
                Instantiate(floatingTextPrefab, targetCanvas.transform);

            RectTransform rectTransform = messageInstance.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                localPoint.y -= 40f;
                rectTransform.localPosition = localPoint;
                rectTransform.localScale = Vector3.one;
            }

            FloatingMessage floatingScript =
                messageInstance.GetComponent<FloatingMessage>();

            if (floatingScript != null)
                floatingScript.SetMessage(message);
        }
    }
}
