using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("생명력 설정")]
    public int maxHealth = 3;
    private int currentHealth;

    // 🔥 체크포인트 위치 저장용
    private Vector3 respawnPosition;

    // 연결할 컴포넌트들
    private PlayerMove playerMove;
    public GameOverManager gameOverManager; // Inspector에서 연결 필요

    [Header("피격 효과")]
    public float flashDuration = 0.1f;
    public int flashCount = 2;
    private SpriteRenderer[] renderers;

    [Header("사운드 설정")]
    public AudioSource damageAudioSource;
    public AudioClip damageClip;
    [Range(0f, 1f)] public float damageVolume = 1f;

    public AudioSource healAudioSource;
    public AudioClip healClip;
    [Range(0f, 1f)] public float healVolume = 1f;

    public delegate void HealthChanged(int currentHealth, int maxHealth);
    public event HealthChanged OnHealthChanged;

    void Start()
    {
        playerMove = GetComponent<PlayerMove>();
        renderers = GetComponentsInChildren<SpriteRenderer>();

        // 1. 시작 위치를 첫 체크포인트로 저장
        respawnPosition = transform.position;

        // 2. HP 불러오기 로직
        string scene = SceneManager.GetActiveScene().name;
        if (scene == "Lv_00_1" || scene == "Lv_00_2")
        {
            currentHealth = GameState.SharedHealth <= 0 ? maxHealth : GameState.SharedHealth;
        }
        else
        {
            currentHealth = maxHealth;
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // GameOverManager 자동 찾기 (연결 안 했을 경우 대비)
        if (gameOverManager == null)
            gameOverManager = FindObjectOfType<GameOverManager>();
    }

    // 🚩 체크포인트에서 호출하는 함수
    public void UpdateRespawnPosition(Vector3 newPosition)
    {
        respawnPosition = newPosition;
        Debug.Log("🚩 체크포인트 저장 완료: " + newPosition);
    }

    // ⚔️ 데미지 입는 함수
    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damageAmount);
        SaveSharedHealth();
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        PlayDamageSound();
        StartCoroutine(HitFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 💀 죽었을 때 (게임 멈추고 UI 띄움)
    private void Die()
    {
        Debug.Log("💀 사망! 게임 정지 및 UI 호출");
        
        // 매니저에게 "게임 오버 처리해줘"라고 요청
        if (gameOverManager != null)
        {
            gameOverManager.OnGameOver();
        }
        else
        {
            // 매니저가 없으면 비상용으로 그냥 시간만 멈춤
            Time.timeScale = 0;
            Debug.LogError("GameOverManager가 연결되지 않았습니다!");
        }
    }

    // 🔄 부활 처리 (UI 버튼이 누르면 GameOverManager가 이 함수를 실행)
    // 🔄 부활 처리 (완전 초기화 버전)
    public void Respawn()
    {
        Debug.Log("✨ 부활! 상태 완전 초기화");

        // [중요 1] 죽을 때 실행되던 모든 깜빡임(Coroutine) 강제 종료
        StopAllCoroutines(); 

        // [중요 2] 혹시 빨간색인 상태로 멈췄을 수 있으니, 강제로 원래 색(흰색)으로 복구
        SetPlayerColor(Color.white);

        // [중요 3] 죽을 때 지르던 비명 소리가 남아있다면 끊기
        if (damageAudioSource != null)
        {
            damageAudioSource.Stop();
        }

        // 1. 체력 100% 회복
        currentHealth = maxHealth;
        SaveSharedHealth();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // 2. 위치 이동
        if (playerMove != null)
        {
            playerMove.Teleport(respawnPosition);
        }
        else
        {
            transform.position = respawnPosition;
        }

        // 3. 이제 피격 효과(빨간불)는 내지 말고, 치유 소리만 한번 딱 재생
        // StartCoroutine(HitFlash()); // <--- 이거 삭제함 (이게 있으면 부활할 때 빨개짐)
        PlayHealSound(); 
    }

    // ❤️ 회복 함수 (삭제되어서 에러났던 부분 복구!)
    public void Heal(int amount)
    {
        if (amount > 0 && currentHealth >= maxHealth)
        {
            return;
        }

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        SaveSharedHealth();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        PlayHealSound();
    }

    // ❓ 체력이 꽉 찼는지 확인 (삭제되어서 에러났던 부분 복구!)
    public bool IsHealthFull()
    {
        return currentHealth >= maxHealth;
    }

    // 💾 체력 저장
    private void SaveSharedHealth()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (scene == "Lv_00_1" || scene == "Lv_00_2")
        {
            GameState.SharedHealth = currentHealth;
        }
    }

    // 🔊 사운드 재생
    private void PlayDamageSound()
    {
        if (damageAudioSource != null && damageClip != null)
        {
            damageAudioSource.volume = damageVolume;
            damageAudioSource.PlayOneShot(damageClip);
        }
    }

    private void PlayHealSound()
    {
        if (healAudioSource != null && healClip != null)
        {
            healAudioSource.volume = healVolume;
            healAudioSource.PlayOneShot(healClip);
        }
    }

    // ✨ 깜빡임 효과 (내용 복구!)
    private IEnumerator HitFlash()
    {
        if (renderers == null) yield break;

        for (int i = 0; i < flashCount; i++)
        {
            SetPlayerColor(new Color(1f, 0.3f, 0.3f)); // 빨강
            yield return new WaitForSeconds(flashDuration);

            SetPlayerColor(Color.white); // 원상복구
            yield return new WaitForSeconds(flashDuration);
        }
    }

    private void SetPlayerColor(Color color)
    {
        foreach (var r in renderers)
        {
            if (r != null) r.color = color;
        }
    }
}