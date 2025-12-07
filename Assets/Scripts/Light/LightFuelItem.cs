using UnityEngine;
using UnityEngine.SceneManagement;

public class LightFuelItem : MonoBehaviour
{
    [Header("회복 설정")]
    public float RestoreDurationAmount = 15f;

    [Header("UI 설정")]
    public GameObject floatingTextPrefab;
    public string fullFuelMessage = "횃불 게이지가 가득 찼습니다!";

    [Header("캔버스 설정")]
    public Canvas targetCanvas;

    [Header("스토리 UI (선택)")]
    public GameObject storyUIPanel;

    [Header("아이템 설명 대사 (Lv_00_2 전용)")]
    public DialogueSO itemDialogue;

    [Header("상호작용 메시지 옵션")]
    public bool showInteractMessage = true;

    private bool playerInRange = false;

    // 🔊 사운드 추가
    [Header("아이템 사운드 설정 🔊")]
    public AudioSource itemAudioSource;   // AudioSource
    public AudioClip itemClip;            // 재생할 소리
    [Range(0f, 1f)]
    public float itemVolume = 1f;         // 볼륨 조절

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TryRestoreLight();
        }
    }

    private void Collect()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.AddProgress("COLLECT_ITEMS", 1);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        if (showInteractMessage && FloatingNotificationUI.Instance != null)
            FloatingNotificationUI.Instance.ShowNotification("E키를 눌러 획득", false);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        if (showInteractMessage && FloatingNotificationUI.Instance != null)
            FloatingNotificationUI.Instance.HideNotification();
    }

    // ===================================================================
    // 🔥 아이템 사용 로직
    // ===================================================================
    private void TryRestoreLight()
    {
        Transform playerRoot = FindObjectOfType<PlayerHealth>()?.transform.root;
        if (playerRoot == null)
        {
            ShowFloatingMessage(transform.position, "플레이어 없음!");
            return;
        }

        LightControl lightControl = playerRoot.GetComponentInChildren<LightControl>();
        if (lightControl == null)
        {
            ShowFloatingMessage(transform.position, "LightControl 없음!");
            return;
        }

        // 이미 가득 차있는지 확인
        if (lightControl.IsFuelFull())
        {
            ShowFloatingMessage(transform.position, fullFuelMessage);
            return;
        }

        // 회복 계산
        float percent = RestoreDurationAmount / lightControl.duration;
        lightControl.RestoreLight(percent);

        ShowFloatingMessage(transform.position, $"+{RestoreDurationAmount:F0}초 만큼 연료 회복!");

        // 🔊 아이템 소리 재생 (Pause 중에는 재생 X)
        PlayItemSound();

        // Lv_00_2 → 스토리 → 대사 → Collect
        if (SceneManager.GetActiveScene().name == "Lv_00_2")
        {
            HandleStoryThenDialogue();
        }
        else
        {
            Collect();
        }
    }

    // ===================================================================
    // 🔊 아이템 사운드 재생
    // ===================================================================
    private void PlayItemSound()
    {
        if (PauseMenu.isGamePaused) return; // Pause 중 재생 금지

        if (itemAudioSource != null && itemClip != null)
        {
            itemAudioSource.volume = itemVolume;
            itemAudioSource.PlayOneShot(itemClip);
        }
    }

    // ===================================================================
    // 🔥 스토리 UI → 대사 → Collect()
    // ===================================================================
    private void HandleStoryThenDialogue()
    {
        if (FloatingNotificationUI.Instance != null)
            FloatingNotificationUI.Instance.HideNotification();

        if (storyUIPanel != null)
        {
            StoryUIFader fader = storyUIPanel.GetComponent<StoryUIFader>();

            if (fader != null)
            {
                fader.Play(() =>
                {
                    StartDialogueThenCollect();
                });
                return;
            }
        }

        StartDialogueThenCollect();
    }

    private void StartDialogueThenCollect()
    {
        if (itemDialogue != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(itemDialogue, Collect);
        }
        else
        {
            Collect();
        }
    }

    // ===================================================================
    // 🔥 FloatingText 표시
    // ===================================================================
    private void ShowFloatingMessage(Vector3 position, string message)
    {
        if (targetCanvas == null)
            targetCanvas = FindObjectOfType<Canvas>();

        if (floatingTextPrefab == null || targetCanvas == null || Camera.main == null)
            return;

        Vector2 screenPoint = Camera.main.WorldToScreenPoint(position);
        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetCanvas.GetComponent<RectTransform>(),
            screenPoint,
            targetCanvas.worldCamera,
            out localPoint
        );

        GameObject messageObj = Instantiate(floatingTextPrefab, targetCanvas.transform);
        RectTransform rect = messageObj.GetComponent<RectTransform>();

        if (rect != null)
        {
            localPoint.y -= 40f;
            rect.localPosition = localPoint;
            rect.localScale = Vector3.one;
        }

        FloatingMessage fm = messageObj.GetComponent<FloatingMessage>();
        if (fm != null)
            fm.SetMessage(message);
    }
}
