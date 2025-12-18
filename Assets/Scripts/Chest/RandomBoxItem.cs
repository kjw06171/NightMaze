using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RandomBoxItem : MonoBehaviour
{
    private enum EffectType { Health, Light }

    private struct RandomEffect
    {
        public EffectType type;
        public string message;
        public float value;
        public Color color;
    }

    [Header("UI 설정")]
    public GameObject floatingTextPrefab;

    [Header("캔버스 설정")]
    public Canvas targetCanvas;

    [Header("아이템 설명 대사 (Lv_00_2 전용)")]
    public DialogueSO itemDialogue;

    [Header("상호작용 메시지 옵션")]
    public bool showInteractMessage = true;

    // 🔊 랜덤박스 사운드
    [Header("랜덤박스 사운드 설정 🔊")]
    public AudioSource boxAudioSource;
    public AudioClip boxClip;
    [Range(0f, 1f)]
    public float boxVolume = 1f;

    private bool playerInRange = false;

    // ⭐ 핵심 1: 상자 1회성 락
    private bool isOpened = false;

    private List<RandomEffect> possibleEffects;

    void Awake()
    {
        possibleEffects = new List<RandomEffect>
        {
            new RandomEffect { type = EffectType.Health, message = "+1 HP 회복", value = 1f, color = Color.green },
            new RandomEffect { type = EffectType.Health, message = "-1 HP 피해", value = -1f, color = Color.red },

            new RandomEffect { type = EffectType.Light, message = "빛 15% 감소", value = -0.15f, color = new Color(0.8f, 0.5f, 0f) },
            new RandomEffect { type = EffectType.Light, message = "빛 50% 감소!", value = -0.50f, color = Color.red },
            new RandomEffect { type = EffectType.Light, message = "빛 모두 소멸!", value = -1.00f, color = Color.magenta },

            new RandomEffect { type = EffectType.Light, message = "빛 15% 증가", value = 0.15f, color = Color.yellow },
            new RandomEffect { type = EffectType.Light, message = "빛 30% 증가!", value = 0.30f, color = Color.yellow },
            new RandomEffect { type = EffectType.Light, message = "빛 완충!", value = 1.00f, color = Color.cyan }
        };
    }

    void Update()
    {
        if (!playerInRange) return;
        if (isOpened) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            OpenRandomBox();
        }
    }

    // =========================================================
    // 🔥 Trigger 처리 (콜라이더 여러 개 대응)
    // =========================================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isOpened) return;

        playerInRange = true;

        if (showInteractMessage && FloatingNotificationUI.Instance != null)
            FloatingNotificationUI.Instance.ShowNotification("E키를 눌러 획득", false);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isOpened) return;

        playerInRange = false;

        if (showInteractMessage && FloatingNotificationUI.Instance != null)
            FloatingNotificationUI.Instance.HideNotification();
    }

    // =========================================================
    // 🔥 상자 열기 (E 입력 시 1회만 실행)
    // =========================================================
    private void OpenRandomBox()
    {
        if (isOpened) return;
        isOpened = true;   // ⭐ 여기서 락

        Transform playerRoot = FindObjectOfType<PlayerHealth>()?.transform.root;
        if (playerRoot == null)
        {
            Debug.LogError("플레이어를 찾을 수 없습니다.");
            return;
        }

        PlayerHealth healthControl = playerRoot.GetComponentInChildren<PlayerHealth>();
        LightControl lightControl = playerRoot.GetComponentInChildren<LightControl>();

        // 효과 선택
        RandomEffect selectedEffect;

        if (SceneManager.GetActiveScene().name == "Lv_00_2")
        {
            var lightNegativeEffects = possibleEffects.FindAll(e =>
                e.type == EffectType.Light && e.value < 0f
            );

            selectedEffect = lightNegativeEffects[Random.Range(0, lightNegativeEffects.Count)];
        }
        else
        {
            selectedEffect = possibleEffects[Random.Range(0, possibleEffects.Count)];
        }

        Debug.Log($"📦 랜덤 상자 → {selectedEffect.message}");

        // =================================================
        // 🔥 효과 적용
        // =================================================
        if (selectedEffect.type == EffectType.Health && healthControl != null)
        {
            int value = (int)selectedEffect.value;

            if (value < 0)
            {
                // 데미지는 TakeDamage로만
                healthControl.TakeDamage(Mathf.Abs(value));
            }
            else
            {
                healthControl.Heal(value);
            }
        }
        else if (selectedEffect.type == EffectType.Light && lightControl != null)
        {
            lightControl.RestoreLight(selectedEffect.value);
        }

        // 메시지
        ShowFloatingMessage(transform.position, selectedEffect.message, selectedEffect.color);

        // 퀘스트
        if (QuestManager.Instance != null)
            QuestManager.Instance.AddProgress("COLLECT_ITEMS", 1);

        // Lv_00_2 전용 대사
        if (SceneManager.GetActiveScene().name == "Lv_00_2")
        {
            if (itemDialogue != null && DialogueManager.Instance != null)
                DialogueManager.Instance.StartDialogue(itemDialogue);
        }

        // 사운드
        PlayBoxSound();

        // UI 정리
        if (FloatingNotificationUI.Instance != null)
            FloatingNotificationUI.Instance.HideNotification();

        // 삭제
        Destroy(gameObject);
    }

    // =========================================================
    // 🔊 사운드
    // =========================================================
    private void PlayBoxSound()
    {
        if (PauseMenu.isGamePaused) return;

        if (boxAudioSource != null && boxClip != null)
        {
            boxAudioSource.volume = boxVolume;
            boxAudioSource.PlayOneShot(boxClip);
        }
    }

    // =========================================================
    // 🔥 플로팅 메시지
    // =========================================================
    private void ShowFloatingMessage(Vector3 position, string message, Color color)
    {
        if (targetCanvas == null)
            targetCanvas = FindObjectOfType<Canvas>();

        if (floatingTextPrefab == null || targetCanvas == null || Camera.main == null)
            return;

        Vector2 screenPoint = Camera.main.WorldToScreenPoint(position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetCanvas.GetComponent<RectTransform>(),
            screenPoint,
            targetCanvas.worldCamera,
            out Vector2 localPoint
        );

        GameObject messageObj = Instantiate(floatingTextPrefab, targetCanvas.transform);
        RectTransform rect = messageObj.GetComponent<RectTransform>();

        localPoint.y -= 40f;
        rect.localPosition = localPoint;
        rect.localScale = Vector3.one;

        FloatingMessage fm = messageObj.GetComponent<FloatingMessage>();
        if (fm != null)
        {
            fm.SetMessage(message);
            fm.SetColor(color);
        }
    }
}
