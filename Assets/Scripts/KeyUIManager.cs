using UnityEngine;

public class KeyUIManager : MonoBehaviour
{
    [Header("UI 아이콘 오브젝트 연결 (획득 시 켜지는 이미지)")]
    public GameObject keyA_Icon; // 첫 번째 열쇠 아이콘
    public GameObject keyB_Icon; // 두 번째 열쇠 아이콘
    public GameObject keyC_Icon; // 세 번째 열쇠 아이콘
    public GameObject candle_Icon; // 양초 아이콘 (있다면)

    // 씬이 시작될 때마다 실행됨
    void Start()
    {
        UpdateKeyUI();
    }

    public void UpdateKeyUI()
    {
        // 1. 첫 번째 열쇠 확인
        if (GameState.HasKeyA)
        {
            if (keyA_Icon != null) keyA_Icon.SetActive(true);
        }

        // 2. 두 번째 열쇠 확인
        if (GameState.HasKeyB)
        {
            if (keyB_Icon != null) keyB_Icon.SetActive(true);
        }

        // 3. 세 번째 열쇠 확인
        if (GameState.HasKeyC)
        {
            if (keyC_Icon != null) keyC_Icon.SetActive(true);
        }

        // 4. 양초 확인
        if (GameState.HasCandle)
        {
            if (candle_Icon != null) candle_Icon.SetActive(true);
        }
    }
}