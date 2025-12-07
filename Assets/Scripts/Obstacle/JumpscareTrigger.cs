using UnityEngine;
using System.Collections; 



/// <summary>
/// 플레이어의 'E' 키 상호작용을 감지하고, 지정된 시간 지연 후 점프 스케어 효과를 실행합니다.
/// 💥 [주요 기능]
/// 1. 'E' 상호작용 감지 후 게임 시간 정지 (Time.timeScale = 0).
/// 2. 깜놀 애니메이션 중 Pause Menu 억제.
/// 3. 일정 지연 후 깜놀 그림 표시, 사운드 재생 및 데미지 적용.
/// 4. 깜놀 종료 후 게임 시간 재개 및 오브젝트 파괴.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class JumpscareTrigger : MonoBehaviour
{
    [Header("Jumpscare Settings")]
    [Tooltip("상호작용 후 깜놀 전 잠시 표시되는 평범한 오브젝트")]
    public GameObject normalDisplayObject; 
    [Tooltip("실제 깜놀 그림 (최종적으로 크게 팝업되는 오브젝트)")]
    public GameObject jumpscareObject; 
    [Tooltip("긴장 조성 지연 시간 (초). 이 시간 동안 화면이 멈춰있습니다.")]
    public float scareDelay = 2.0f; 
    [Tooltip("깜놀 그림이 화면에 표시되는 시간 (초).")]
    public float scareDuration = 0.5f; 
    
    [Header("Impact Settings")]
    [Tooltip("깜놀 시 재생될 사운드 클립")]
    public AudioClip scareSound;
    private AudioSource audioSource;
    
    [Tooltip("깜놀 시 원래 크기 대비 몇 배로 커질지")]
    public float maxScaleMultiplier = 1.2f; 
    [Tooltip("팝업 애니메이션에 걸리는 짧은 시간")]
    public float popDuration = 0.05f; 
    
    [Header("Interaction Settings")]
    [Tooltip("플레이어 오브젝트의 태그")]
    public string playerTag = "Player"; 
    [Tooltip("UI에 표시될 상호작용 힌트 메시지 (주석 처리됨)")]
    public string interactionHintMessage = "E 키를 눌러 확인";
    
    // ⭐ 데미지 설정: 이 트리거가 플레이어에게 입힐 데미지 양
    [Header("Jumpscare Damage")]
    public int damageAmount = 1; 

    // ⭐ [새로운 필드] 일시정지 메뉴 억제
    [Header("UI & Menu Control")]
    [Tooltip("게임 내 일시정지 메뉴의 Canvas 또는 Root GameObject를 연결하세요. 깜놀 중에는 비활성화됩니다.")]
    public GameObject pauseMenuCanvas;
    
    // 💡 상태 변수
    private bool playerIsNear = false;
    private bool hasBeenTriggered = false; 
    private Vector3 originalScale; 
    private GameObject playerReference; // 💥 플레이어 오브젝트 참조
    // 💡 일시정지 메뉴가 깜놀 이전에 활성화되어 있었는지 저장
    private bool wasPauseMenuVisibleBeforeScare = false;


    private void Awake()
    {
        // AudioSource 설정
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.ignoreListenerPause = true; // 시간 정지 시에도 재생되도록 설정
        }
    }

    private void Start()
    {
        // 초기 상태 설정
        if (jumpscareObject != null)
        {
            jumpscareObject.SetActive(false);
            originalScale = jumpscareObject.transform.localScale;
        }
        else
        {
            Debug.LogError("[JumpscareTrigger] 🚨 Jumpscare Object가 할당되지 않아 스크립트를 비활성화합니다.");
            enabled = false;
            return;
        }

        if (normalDisplayObject != null)
        {
            normalDisplayObject.SetActive(false); 
        }

        // 콜라이더 설정 확인
        Collider2D col = GetComponent<Collider2D>();
        if (col == null || !col.isTrigger)
        {
             if (col != null) col.isTrigger = true;
             else { Debug.LogError("[JumpscareTrigger] 🚨 Collider2D 컴포넌트가 없습니다."); enabled = false; }
        }
    }
    
    private void Update()
    {
        if (playerIsNear && !hasBeenTriggered && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(DoDelayedJumpscare());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerIsNear = true;
            playerReference = other.gameObject; // 플레이어 참조 저장
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerIsNear = false;
        }
    }

    /// <summary>
    /// 깜놀 발동 코루틴 (시간 정지/재개 사용)
    /// </summary>
    private IEnumerator DoDelayedJumpscare()
    {
        hasBeenTriggered = true;

        // 💡 [Pause Menu 억제 시작]
        if (pauseMenuCanvas != null)
        {
            // 메뉴가 현재 활성화되어 있다면 상태를 저장하고 비활성화합니다.
            if (pauseMenuCanvas.activeSelf)
            {
                wasPauseMenuVisibleBeforeScare = true;
                pauseMenuCanvas.SetActive(false);
                Debug.Log("[JumpscareTrigger] Pause Menu Canvas를 비활성화했습니다. (깜놀 중 억제)");
            }
            else
            {
                wasPauseMenuVisibleBeforeScare = false;
            }
        }
        
        // 1. 🎮 [시간 정지]
        Time.timeScale = 0f;
        Debug.Log("[JumpscareTrigger] ⏸️ 게임 시간 정지.");
        
        // 2. 🔥 [평범한 그림 활성화]
        if (normalDisplayObject != null)
        {
            normalDisplayObject.SetActive(true);
        }
        
        // 3. 🔥 [긴장 조성 지연] (Time.timeScale=0이므로 Realtime 대기)
        yield return new WaitForSecondsRealtime(scareDelay);


        // --- 💥 깜놀 순간 (Jumpscare Moment) ---

        // 4. 🔥 [데미지 적용]
        if (playerReference != null)
        {
            var healthComponent = playerReference.GetComponent<PlayerHealth>(); 
            if (healthComponent != null)
            {
                healthComponent.TakeDamage(damageAmount); 
                Debug.Log($"[JumpscareTrigger] 💔 플레이어에게 {damageAmount} 데미지 적용 완료.");
            }
            else
            {
                Debug.LogError("[JumpscareTrigger] ⚠️ PlayerHealth 컴포넌트를 찾을 수 없습니다. 데미지 적용 실패.");
            }
        }
        
        // 5. 🔥 [평범한 그림 비활성화]
        if (normalDisplayObject != null)
        {
            normalDisplayObject.SetActive(false);
        }
        
        // 6. 🎧 [사운드 재생]
        if (audioSource != null && scareSound != null)
        {
            audioSource.PlayOneShot(scareSound); 
        }

        // 7. 🔥 [깜놀 그림 활성화 및 팝 애니메이션] (Time.unscaledDeltaTime 사용)
        jumpscareObject.SetActive(true);

        float timer = 0f;
        Vector3 targetScale = originalScale * maxScaleMultiplier;
        
        while (timer < popDuration)
        {
            timer += Time.unscaledDeltaTime; 
            float t = Mathf.Clamp01(timer / popDuration);
            jumpscareObject.transform.localScale = Vector3.Lerp(originalScale, targetScale, t); 
            yield return null;
        }
        jumpscareObject.transform.localScale = targetScale;
        
        // 8. 🔥 [깜놀 지속 시간 대기] (Realtime 사용)
        float timeRemaining = scareDuration - popDuration;
        if (timeRemaining > 0)
        {
            yield return new WaitForSecondsRealtime(timeRemaining);
        }

        // 9. 🔥 [깜놀 그림 비활성화]
        jumpscareObject.SetActive(false);
        jumpscareObject.transform.localScale = originalScale;
        
        // 10. 🎮 [시간 재개]
        Time.timeScale = 1f;
        Debug.Log("[JumpscareTrigger] ▶️ 게임 시간 재개.");
        
        // 💡 [Pause Menu 상태 복원]
        // 깜놀 이전에 메뉴가 활성화되어 있었다면 다시 활성화합니다.
        if (pauseMenuCanvas != null && wasPauseMenuVisibleBeforeScare)
        {
            pauseMenuCanvas.SetActive(true);
            wasPauseMenuVisibleBeforeScare = false; // 상태 초기화
            Debug.Log("[JumpscareTrigger] Pause Menu Canvas를 다시 활성화했습니다. (이전 상태 복원)");
        }

        // 11. 🗑️ [오브젝트 파괴]
        Debug.Log("[JumpscareTrigger] 🗑️ 트리거 오브젝트 파괴.");
        Destroy(gameObject);
    }
}