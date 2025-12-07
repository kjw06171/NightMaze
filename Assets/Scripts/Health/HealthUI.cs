using UnityEngine;
using UnityEngine.UI; 

public class HealthUI : MonoBehaviour
{
    [Header("UI 연결")]
    // 💡 인스펙터에서 PlayerHealth 스크립트가 붙은 오브젝트를 연결해야 합니다.
    public PlayerHealth playerHealthScript; 
    // 💡 인스펙터에서 하트 이미지(Heart1, Heart2 등)를 순서대로 연결해야 합니다.
    public Image[] heartImages;             
    
    [Header("하트 스프라이트")]
    // 💡 인스펙터에서 가득 찬 하트와 빈 하트 스프라이트를 연결해야 합니다.
    public Sprite fullHeartSprite;          
    public Sprite emptyHeartSprite;         

    void Start()
    {
        if (playerHealthScript == null)
        {
            Debug.LogError("PlayerHealth 스크립트가 연결되지 않았습니다! Inspector에서 Player 오브젝트를 연결해 주세요.");
            return;
        }

        // 💡 PlayerHealth 이벤트 구독 시작: 목숨이 바뀔 때마다 UpdateHealthDisplay 함수 호출
        playerHealthScript.OnHealthChanged += UpdateHealthDisplay;
        
        // 초기 UI 상태 설정
        UpdateHealthDisplay(playerHealthScript.maxHealth, playerHealthScript.maxHealth); 
    }

    /// <summary>
    /// PlayerHealth 스크립트로부터 이벤트가 발생하면 호출됩니다.
    /// </summary>
    private void UpdateHealthDisplay(int currentHealth, int maxHealth)
    {
        // 🚨 하트 이미지 개수가 최대 목숨 개수와 일치하는지 확인
        if (heartImages.Length != maxHealth)
        {
            Debug.LogError("하트 이미지 개수가 최대 목숨 개수(" + maxHealth + ")와 일치하지 않습니다!");
            return;
        }

        // 목숨 개수에 따라 하트 이미지 업데이트
        for (int i = 0; i < maxHealth; i++)
        {
            // 스프라이트 본연의 색상을 유지하기 위해 색상을 흰색으로 초기화
            heartImages[i].color = Color.white; 
            
            if (i < currentHealth)
            {
                // 현재 목숨 개수보다 작은 인덱스는 가득 찬 하트
                heartImages[i].sprite = fullHeartSprite;
            }
            else
            {
                // 나머지 인덱스는 빈 하트
                heartImages[i].sprite = emptyHeartSprite;
            }
        }
    }
    
    void OnDestroy()
    {
        // 메모리 누수 방지를 위해 이벤트 구독 해제
        if (playerHealthScript != null)
        {
            playerHealthScript.OnHealthChanged -= UpdateHealthDisplay;
        }
    }
}