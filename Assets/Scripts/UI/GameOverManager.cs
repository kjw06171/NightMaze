using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    [Header("설정: 게임오버 UI 패널")]
    public GameObject gameOverPanel; 

    [Header("설정: 플레이어 스크립트")]
    public PlayerHealth playerHealth;

    // 🔥 [추가된 부분] 게임 시작할 때 UI를 강제로 끕니다.
    void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false); // 시작하자마자 안 보이게 끄기
        }
    }

    // 💀 플레이어가 죽었을 때 호출됨
    public void OnGameOver()
    {
        Debug.Log("🎮 Game Over UI 활성화");
        AudioListener.pause = true;
        Time.timeScale = 0;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // 죽으면 켜기
        }
    }

    // 🔄 UI 버튼(다시하기)에 연결할 함수
    public void RestartGameCall()
    {
        Debug.Log("🔄 다시하기 버튼 클릭됨");
        AudioListener.pause = false;
        Time.timeScale = 1;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false); // 다시 시작하면 끄기
        }

        if (playerHealth != null)
        {
            playerHealth.Respawn();
        }
    }
}