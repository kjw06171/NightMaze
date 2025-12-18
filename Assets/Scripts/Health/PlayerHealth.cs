using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("생명력 설정")]
    public int maxHealth = 3;
    private int currentHealth;

    // ⭐ 추가: 방금 리스폰 여부
    public bool IsJustRespawned { get; private set; }

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

        // GameOverManager 자동 찾기
        if (gameOverManager == null)
            gameOverManager = FindObjectOfType<GameOverManager>();
    }

    // 🚩 체크포인트에서 호출
    public void UpdateRespawnPosition(Vector3 newPosition)
    {
        respawnPosition = newPosition;
        Debug.Log("🚩 체크포인트 저장 완료: " + newPosition);
    }

    // ⚔️ 데미지
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

    // 💀 사망
    private void Die()
    {
        Debug.Log("💀 사망! 게임 정지 및 UI 호출");

        if (gameOverManager != null)
        {
            gameOverManager.OnGameOver();
        }
        else
        {
            Time.timeScale = 0;
            Debug.LogError("GameOverManager가 연결되지 않았습니다!");
        }
    }

    // 🔄 부활 처리 (풀피 유지)
    public void Respawn()
    {
        StopAllCoroutines();
        SetPlayerColor(Color.white);

        if (damageAudioSource != null)
            damageAudioSource.Stop();

        // ⭐ 빛 즉시 풀 충전
        LightControl light = FindObjectOfType<LightControl>();
        if (light != null)
        {
            light.ForceFullRecharge();
        }

        // ⭐ 풀피 리스폰 (기존 기능 유지)
        currentHealth = maxHealth;
        IsJustRespawned = true; // ⭐ 추가

        SaveSharedHealth();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (playerMove != null)
            playerMove.Teleport(respawnPosition);
        else
            transform.position = respawnPosition;

        PlayHealSound();

        // ⭐ 일정 시간 후 리스폰 상태 해제
        StartCoroutine(ClearRespawnFlag());
    }

    // ⭐ 추가: 리스폰 상태 자동 해제
    private IEnumerator ClearRespawnFlag()
    {
        yield return new WaitForSeconds(3f);
        IsJustRespawned = false;
    }

    // ❤️ 회복
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

    // 🔊 사운드
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

    // ✨ 깜빡임 효과
    private IEnumerator HitFlash()
    {
        if (renderers == null) yield break;

        for (int i = 0; i < flashCount; i++)
        {
            SetPlayerColor(new Color(1f, 0.3f, 0.3f));
            yield return new WaitForSeconds(flashDuration);

            SetPlayerColor(Color.white);
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
