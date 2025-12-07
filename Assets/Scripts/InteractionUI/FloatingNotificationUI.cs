using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 화면 중앙에 고정되어 잠시 표시되었다가 사라지거나, 유지되는 시스템 알림 메시지를 관리합니다. (싱글톤)
/// </summary>
public class FloatingNotificationUI : MonoBehaviour
{
    // 💡 싱글톤 인스턴스
    public static FloatingNotificationUI Instance;

    // 💡 메시지를 표시할 TextMeshPro 컴포넌트
    private TextMeshProUGUI notificationText;
    // 💡 UI 오브젝트 자체
    private GameObject notificationObject;
    
    // 💡 메시지가 화면에 머무를 시간 (초) (자동 숨김 시 사용)
    public float displayDuration = 3.0f; 
    
    // 💡 현재 실행 중인 숨김 코루틴 참조 (수동 종료를 위해)
    private Coroutine currentHideCoroutine;

    void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null)
        {
            Instance = this;
            notificationText = GetComponent<TextMeshProUGUI>();
            notificationObject = gameObject;

            if (notificationText == null)
            {
                Debug.LogError("[FloatingNotificationUI] TextMeshProUGUI 컴포넌트를 찾을 수 없습니다.");
            }
            
            // 초기에는 UI를 비활성화합니다.
            notificationObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 지정된 메시지를 화면에 표시하고, 일정 시간이 지나면 자동으로 사라지게 합니다.
    /// </summary>
    /// <param name="message">표시할 메시지</param>
    /// <param name="autoHide">메시지를 displayDuration 후에 자동으로 숨길지 여부입니다. (기본값: true)</param>
    public void ShowNotification(string message, bool autoHide = true)
    {
        if (notificationObject == null || notificationText == null) return;

        // 💡 현재 실행 중인 숨김 코루틴이 있다면 중지 (새 메시지가 떴을 때 이전 타이머 초기화)
        if (currentHideCoroutine != null)
        {
            StopCoroutine(currentHideCoroutine);
            currentHideCoroutine = null;
        }

        notificationText.text = message;
        notificationObject.SetActive(true);

        // 💡 autoHide가 true일 때만 숨김 코루틴을 시작합니다.
        if (autoHide)
        {
            currentHideCoroutine = StartCoroutine(HideAfterDelay(displayDuration));
        }
    }
    
    /// <summary>
    /// 알림을 즉시 숨기고 타이머를 중지합니다. (외부 호출용, ExitDoorController에서 사용됨)
    /// </summary>
    public void HideNotification()
    {
        // 코루틴이 있다면 먼저 중지합니다.
        if (currentHideCoroutine != null)
        {
            StopCoroutine(currentHideCoroutine);
            currentHideCoroutine = null;
        }
        
        if (notificationObject != null && notificationObject.activeSelf)
        {
            notificationObject.SetActive(false);
        }
    }


    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 메시지 숨기기
        notificationObject.SetActive(false);
        currentHideCoroutine = null; // 코루틴 완료 후 참조 해제
    }
}