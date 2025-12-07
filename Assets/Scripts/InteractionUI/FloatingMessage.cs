using UnityEngine;
using TMPro;

/// <summary>
/// 짧은 메시지를 화면에 표시하고 일정 시간 후 스스로 파괴되는 스크립트입니다.
/// (이동 없이 제자리에 고정됩니다.)
/// </summary>
public class FloatingMessage : MonoBehaviour
{
    [Header("메시지 설정")]
    public float destroyTime = 1.5f; // 메시지가 완전히 사라지는 시간
    public float moveSpeed = 0.5f;   // 💡 [제거] 더 이상 사용하지 않습니다.
    
    private TextMeshProUGUI tmpText;
    private float startTime; // 메시지가 생성된 시간을 기록
    private bool isInitialized = false; // 초기화 플래그

    void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        
        if (tmpText == null)
        {
            Debug.LogError("🚨 FloatingMessage: TextMeshProUGUI 컴포넌트를 찾을 수 없습니다. 프리팹 설정을 확인하세요.");
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        if (!isInitialized) return; // 초기화되지 않았으면 로직 건너뛰기

        // 1. 💡 [수정] 이동 로직을 제거했습니다. 메시지는 제자리에 고정됩니다.
        
        // 2. 파괴 및 투명도 감소 로직
        float timeElapsed = Time.time - startTime;
        float fadeRatio = timeElapsed / destroyTime;
        
        // 시간이 지남에 따라 투명도 감소 (서서히 사라지는 효과)
        if (tmpText != null)
        {
            float alpha = 1f - fadeRatio; // 1 (불투명) -> 0 (투명)
            // 💡 alpha 값이 0 미만으로 떨어지는 것을 방지
            alpha = Mathf.Max(0f, alpha); 
            tmpText.color = new Color(tmpText.color.r, tmpText.color.g, tmpText.color.b, alpha);
        }

        // 3. 파괴 조건 검사
        if (timeElapsed >= destroyTime)
        {
            Destroy(gameObject); // 시간이 다 되면 확실하게 파괴
        }
    }

    /// <summary>
    /// 메시지 내용을 설정하고, 타이머를 시작합니다.
    /// </summary>
    /// <param name="message">표시할 텍스트</param>
    public void SetMessage(string message)
    {
        if (tmpText == null)
        {
             tmpText = GetComponent<TextMeshProUGUI>();
        }
        if (tmpText != null)
        {
            tmpText.text = message;
            // 초기 색상 알파값을 1로 보장
            tmpText.color = new Color(tmpText.color.r, tmpText.color.g, tmpText.color.b, 1f);
        }
        
        // 시작 시간을 기록하고 초기화 완료
        startTime = Time.time;
        isInitialized = true;
    }

    /// <summary>
    /// 메시지의 색상을 설정합니다.
    /// </summary>
    /// <param name="color">설정할 색상</param>
    public void SetColor(Color color)
    {
        if (tmpText != null)
        {
            tmpText.color = color; // 텍스트 색상을 설정합니다.
        }
        else
        {
            Debug.LogError("🚨 텍스트 컴포넌트를 찾을 수 없습니다!");
        }
    }
}
