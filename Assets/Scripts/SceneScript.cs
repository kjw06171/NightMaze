using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // 씬 이름으로 로드
    public void LoadSceneByName(string sceneName)
    {
        Time.timeScale = 1f;  // 게임 시간이 흐르도록 설정
        SceneManager.LoadScene(sceneName);
    }
    
    // 씬 인덱스로 로드
    public void LoadSceneByIndex(int sceneIndex)
    {
        Time.timeScale = 1f;  // 게임 시간이 흐르도록 설정
        SceneManager.LoadScene(sceneIndex);
    }
    
    // 현재 씬 재시작
    public void RestartCurrentScene()
    {
        Time.timeScale = 1f;  // 게임 시간이 흐르도록 설정
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    
    // 게임 종료
    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
