using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환 기능 사용을 위해 필요

/// <summary>
/// Unity Timeline 재생이 완료되었을 때 다음 씬(레벨)으로 전환하는 스크립트입니다.
/// 이 스크립트는 컷씬 씬의 Game Object에 부착하고, Timeline의 Event Track에서 호출되어야 합니다.
/// </summary>
public class TimelineSceneChanger : MonoBehaviour
{
    // Inspector 창에서 다음 로드할 씬의 이름을 설정합니다.
    [Header("다음 레벨 씬 이름")]
    [Tooltip("빌드 설정(Build Settings)에 등록된 다음 씬의 정확한 이름을 입력하세요.")]
    [SerializeField]
    private string nextSceneName = "Level_2_Main"; 

    // 옵션: 로딩 중 화면을 페이드 아웃하는 시간 (원하면 사용)
    [Header("추가 설정")]
    [Tooltip("씬 전환 전 화면이 어두워지는 페이드 아웃 시간 (0이면 즉시 전환)")]
    [SerializeField]
    private float fadeOutTime = 0f; 

    /// <summary>
    /// Timeline 재생이 완료되었을 때 호출되는 공용 함수입니다.
    /// 이 함수를 Timeline의 Event Track에 연결해야 합니다.
    /// </summary>
    public void OnTimelineFinished()
    {
        Debug.Log($"[TimelineSceneChanger] 컷씬 종료 감지. 다음 씬 '{nextSceneName}' 로드 준비.");

        if (fadeOutTime > 0f)
        {
            // 페이드 아웃 효과가 필요하면 코루틴 등으로 구현 (예시에서는 단순화)
            // StartCoroutine(FadeOutAndLoad()); 
            
            // 여기서는 간단히 지연 후 로드합니다.
            Invoke(nameof(LoadNextScene), fadeOutTime);
        }
        else
        {
            // 즉시 다음 씬 로드
            LoadNextScene();
        }
    }

    /// <summary>
    /// 다음 씬을 실제로 로드하는 내부 함수입니다.
    /// </summary>
    private void LoadNextScene()
    {
        // 씬 전환 로직
        try
        {
            // SceneManager.LoadScene 함수를 사용하여 지정된 씬을 로드합니다.
            SceneManager.LoadScene(nextSceneName);
            Debug.Log($"[TimelineSceneChanger] 씬 로드 성공: '{nextSceneName}'");
        }
        catch (System.Exception e)
        {
            // 예외 발생 시 디버그 콘솔에 오류 메시지 출력
            Debug.LogError($"[TimelineSceneChanger] 씬 로드 실패: '{nextSceneName}' - 빌드 설정에 등록되어 있는지 확인하세요. 오류: {e.Message}");
        }
    }

    // 페이드 아웃과 같은 비동기 처리를 원한다면 이 부분에 코루틴을 구현할 수 있습니다.
    /*
    private System.Collections.IEnumerator FadeOutAndLoad()
    {
        // 화면을 어둡게 만드는 코드 (예: Canvas Group의 Alpha 값을 0에서 1로)
        float timer = 0f;
        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            // FadeOut 로직 실행
            yield return null;
        }

        LoadNextScene();
    }
    */
}