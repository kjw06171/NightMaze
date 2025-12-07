using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    [Header("Restart Button Script 연결")]
    public Restart restartButton;

    // 📌 게임오버 UI에서 호출됨
    public void OnGameOver()
    {
        // 🔇 퍼즈 메뉴처럼 모든 게임 사운드 정지
        AudioListener.pause = true;

        // 다른 게임오버 처리 필요 시 여기에 추가 가능
        Debug.Log("🎮 Game Over: 모든 게임 사운드 정지됨");
    }

    // 📌 UI Button → OnClick() 에서 이 함수만 호출하면 됨!
    public void RestartGameCall()
    {
        if (restartButton != null)
        {
            // 재시작 전 사운드 정상화
            AudioListener.pause = false;

            restartButton.RestartGame();
        }
        else
        {
            Debug.LogError("🚨 GameOverManager: restartButton이 Inspector에서 연결되지 않았습니다!");
        }
    }
}
