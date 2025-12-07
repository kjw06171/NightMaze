using UnityEngine;

public class WebSlowdown : MonoBehaviour
{
    // 💡 [수정] private const -> public float
    // public으로 변경하여 인스펙터에 노출되고, 실행 중에도 수정 가능합니다.
    [Tooltip("원래 속도의 몇 %로 감속할지 설정 (0.0: 정지 ~ 1.0: 감속 없음). 0.5는 50% 감속입니다.")]
    public float SlowdownFactor = 0.3f; // 기본값: 원래 속도의 50%로 감속
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 💡 태그 "Player" 확인
        if (other.CompareTag("Player"))
        {
            PlayerMove playerMove = other.GetComponent<PlayerMove>();
            if (playerMove != null)
            {
                // SlowdownFactor 값을 직접 전달
                playerMove.ApplySlowdown(SlowdownFactor);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMove playerMove = other.GetComponent<PlayerMove>();
            if (playerMove != null)
            {
                playerMove.RemoveSlowdown();
            }
        }
    }
}