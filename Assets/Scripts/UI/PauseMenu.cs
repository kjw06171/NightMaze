using UnityEngine;
using UnityEngine.SceneManagement; 

/// <summary>
/// 게임 일시정지 메뉴를 관리하고 ESC 키 입력을 처리하는 스크립트입니다.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    // 유니티 인스펙터에서 연결할 일시정지 UI 패널 (지금 Panel 연결해놨지!)
    public GameObject pauseMenuUI; 

    // 현재 게임이 일시정지 상태인지 확인하는 변수
    public static bool isGamePaused = false; 

    // 🔊 버튼 클릭 사운드 전용 AudioSource (항상 활성인 오브젝트에 붙이기!)
    public AudioSource uiClickAudio;

    void Awake()
    {
        // 버튼 클릭 사운드는 게임 일시정지 여부와 상관없이 재생되게 설정
        if (uiClickAudio != null)
        {
            uiClickAudio.ignoreListenerPause  = true;  // 🔥 AudioListener.pause 무시
            uiClickAudio.ignoreListenerVolume = true;  // (선택) 리스너 볼륨도 무시
        }
    }

    void Start()
    {
        isGamePaused = false;

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        
        // 시작 시 AudioListener가 정지되지 않도록 확인
        AudioListener.pause = false;
    }

    void Update()
    {
        // 1) 스토리 UI 재생 중이면 ESC 완전 차단
        if (StoryUIFader.IsStoryPlaying)
            return;

        // 2) 대화창 활성 상태면 ESC 완전 차단
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsActive())
            return;

        // 🔥 3) 게임오버 UI 켜져 있으면 ESC 차단
        if (GameOverUIActive())
            return;

        // 4) 타이틀 화면이면 ESC 차단
        if (GameManager.IsTitleScreenActive)
        {
             GameManager.IsTitleScreenActive = false;
             Debug.Log("GameManager: 타이틀 화면 플래그를 튜토리얼 씬에서 해제했습니다.");
        }

        // 5) ESC 입력 처리
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGamePaused)
                Resume();
            else
                Pause();
        }
    }

    private bool GameOverUIActive()
    {
        // ⚠ 여기 이름만 네 프로젝트 UI 이름에 맞게 바꾸면 됨!
        var goPanel = GameObject.Find("GameOverPanel");

        return goPanel != null && goPanel.activeInHierarchy;
    }


    /// <summary>
    /// 버튼 클릭 사운드 재생 (UI에서 OnClick으로 호출)
    /// </summary>
    public void PlayClickSound()
    {
        if (uiClickAudio != null)
        {
            uiClickAudio.Play();
        }
    }

    /// <summary>
    /// 게임을 재개하고 UI를 숨깁니다.
    /// </summary>
    public void Resume()
    {
        // 🔊 게임 소리 재개
        AudioListener.pause = false; 

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        Time.timeScale = 1f;

        isGamePaused = false;
        Debug.Log("게임 재개");
    }

    /// <summary>
    /// 게임을 일시정지하고 UI를 보여줍니다.
    /// </summary>
    void Pause()
    {
        // 🔇 게임 내 소리 정지 (단, uiClickAudio는 ignoreListenerPause라 계속 들림)
        AudioListener.pause = true;
        
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        Time.timeScale = 0f;

        isGamePaused = true;
        Debug.Log("게임 일시정지");
    }

    public void SaveGame()
    {
        Debug.Log("게임 저장 기능을 실행합니다.");
    }

    public void QuitGame()
    {
        Debug.Log("게임을 종료합니다.");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
