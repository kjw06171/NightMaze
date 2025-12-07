/// <summary>
/// 게임의 전반적인 상태를 관리하는 정적(Static) 클래스입니다.
/// 플레이어 인벤토리, 핵심 아이템 획득 여부 등을 전역적으로 추적하는 데 사용됩니다.
/// </summary>
public static class GameState
{
    // 💡 플레이어가 촛불을 획득했는지 여부를 추적하는 플래그입니다.
    // 기본값은 false이며, 획득해야만 true로 설정되어 빛 조절이 가능해집니다.
    public static bool HasCandle { get; set; } = false;

    // HP 공유 (이미 있음)
    public static int SharedHealth = 0;

    // 🔥 LightControl 공유 타이머 (0~duration 사이 값)
    public static float SharedLightTimer = 0f;

    // ==========================================================
    // ⭐ KEY_A, KEY_B, KEY_C 상태 변수 추가 (재탕을 위해 필요)
    // ==========================================================
    public static bool HasKeyA { get; set; } = false;
    public static bool HasKeyB { get; set; } = false;
    public static bool HasKeyC { get; set; } = false;


    // ==========================================================
    // ⭐ 스테이지 전환 시 열쇠 상태를 초기화하는 함수 추가
    // ==========================================================
    /// <summary>
    /// 스테이지 전환 시 KEY_A, B, C의 획득 상태를 초기화합니다.
    /// </summary>
    public static void ResetKeysForNewStage()
    {
        HasKeyA = false;
        HasKeyB = false;
        HasKeyC = false;
        // HasCandle처럼 영구적인 아이템은 초기화하지 않습니다.
    }
}