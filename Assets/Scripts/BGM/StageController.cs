using UnityEngine;

public class StageController : MonoBehaviour
{
    // 유니티 인스펙터 창에서 드래그하여 할당할 BGM 파일
    public AudioClip stageBGM; 
    public float bgmVolume = 0.8f; // 이 스테이지의 희망 볼륨

    void Start()
    {
        // 씬이 로드되고 스테이지가 시작될 때 BGMManager를 통해 BGM을 재생
        if (BGMManager.Instance != null && stageBGM != null)
        {
            // BGM을 부드럽게 페이드 인하며 시작
            BGMManager.Instance.FadeIn(stageBGM, 0f, bgmVolume);
            
            // 만약 페이드 없이 즉시 재생하려면 아래 코드를 사용
            // BGMManager.Instance.PlayBGM(stageBGM, bgmVolume);
        }
    }
    
    // 스테이지가 끝날 때 (예: 다음 씬으로 전환) BGM을 페이드 아웃
    public void GoToNextStage()
    {
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.FadeOutAndStop();
            // 이후 씬 로딩 코드를 넣습니다.
            // SceneManager.LoadScene("NextScene"); 
        }
    }
}