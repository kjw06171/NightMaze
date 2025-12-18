using UnityEngine;
using TMPro; // 👈 TextMeshPro를 사용하기 위해 꼭 필요합니다!

public class Checkpoint : MonoBehaviour
{
    [Header("1. 저장 안내 UI 설정")]
    public GameObject guideUI;          // UI 껍데기 (켜고 끌 대상)
    public TextMeshProUGUI uiText;      // 👈 실제 글자가 적히는 컴포넌트
    
    [TextArea] // 인스펙터에서 줄바꿈 가능하게 입력창을 넓혀줍니다.
    public string guideMessage = "E키를 눌러 저장"; // 👈 인스펙터에서 수정할 텍스트 내용

    private bool isPlayerInRange = false;
    private PlayerHealth playerRef;

    void Start()
    {
        if (guideUI != null) guideUI.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerRef = other.GetComponent<PlayerHealth>();

            // ⭐ UI 켜기 전에 텍스트 내용 갈아끼우기
            if (uiText != null)
            {
                uiText.text = guideMessage; // 인스펙터에 적은 내용으로 변경!
            }

            if (guideUI != null) guideUI.SetActive(true);
            
            // 디버그 로그도 해당 메시지로 띄우면 헷갈리지 않음
            Debug.Log($"🚩 체크포인트 발견! ({guideMessage})");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerRef = null;

            if (guideUI != null) guideUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (playerRef != null)
            {
                playerRef.UpdateRespawnPosition(transform.position);
                Debug.Log("💾 체크포인트 획득 완료! 위치 저장됨.");

                if (guideUI != null) guideUI.SetActive(false);
                
                gameObject.SetActive(false); 
            }
        }
    }
}