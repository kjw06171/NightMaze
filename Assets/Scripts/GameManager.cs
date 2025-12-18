using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI 참조")]
    public GameObject titleScreenPanel;

    public static bool IsGameOver = false;
    public static bool IsTitleScreenActive = false;

    void OnEnable()
    {
        // 씬 로드될 때 자동 초기화
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // =========================================================
    // 🔥 씬 로드 시 KEY 상태 자동 초기화
    // =========================================================
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetKeyStates();
    }

    void ResetKeyStates()
    {
        GameState.HasKeyA = false;
        GameState.HasKeyB = false;
        GameState.HasKeyC = false;
        GameState.HasCandle = false;

        // 쥐 AI 관련 static 값도 초기화
        RatAI.keyBCollected = false;
        RatAI.hasAttackedOnce = false;

        Debug.Log("🔄 GameState 초기화 완료");
    }

    // =========================================================
    // 🔥 기본 GameManager 기능
    // =========================================================
    void Start()
    {
        if (titleScreenPanel != null)
            ShowTitleScreen();
    }

    void ShowTitleScreen()
    {
        if (titleScreenPanel != null)
            titleScreenPanel.SetActive(true);

        Time.timeScale = 0f;  // 게임을 멈추기 위해 Time.timeScale = 0
        IsTitleScreenActive = true;
    }

    public void StartGame()
    {
        if (titleScreenPanel != null)
            titleScreenPanel.SetActive(false);

        // 게임 시작 시 Time.timeScale을 1로 설정 (기존 코드 기능을 그대로 유지)
        Time.timeScale = 1f;

        // 게임 상태 초기화 (StartGame()에서만 호출)
        ResetKeyStates();

        // 타이틀 화면이 비활성화되면 게임 진행 가능
        IsTitleScreenActive = false;
    }

    public void QuitGame()
    {
        Debug.Log("게임 종료 요청됨");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
