using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneController : MonoBehaviour
{
    public Slider loadingBar;
    public string sceneName;
    
    [Header("속도 조절")]
    public float minLoadingTime = 3.0f; // 최소 2초 동안은 로딩바가 움직임

    private void Start()
    {
        Time.timeScale = 1.0f;
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        
        // 중요: 로딩이 끝나도 바로 씬을 넘기지 않음 (90%에서 대기)
        operation.allowSceneActivation = false; 

        float timer = 0.0f;

        while (!operation.isDone)
        {
            yield return null; // 1프레임 대기

            timer += Time.deltaTime;

            // 1. 진짜 로딩 진행률 (0 ~ 1)
            // 유니티는 0.9가 로딩 완료라 0.9로 나눠줍니다.
            float realProgress = operation.progress / 0.9f;
            
            // 2. 가짜 시간 진행률 (0 ~ 1)
            // 설정한 시간(2초) 동안 0에서 1로 증가
            float fakeProgress = timer / minLoadingTime;

            // 3. 둘 중 더 '느린' 진행률을 사용
            // 로딩이 빨라도 시간(2초)을 채워야 하고,
            // 시간이 2초가 지났어도 로딩이 안 끝났으면 기다립니다.
            float currentProgress = Mathf.Min(fakeProgress, realProgress);

            // 로딩바 채우기 (Lerp를 써서 더 부드럽게 움직임)
            loadingBar.value = Mathf.Lerp(loadingBar.value, currentProgress, Time.deltaTime * 5f);

            // 진짜 로딩도 끝났고(0.9 이상), 설정한 시간(2초)도 지났다면?
            if (operation.progress >= 0.9f && timer >= minLoadingTime)
            {
                // 마지막으로 꽉 채워주고
                loadingBar.value = 1.0f;
                // 씬 이동 허용!
                operation.allowSceneActivation = true;
            }
        }
    }
}