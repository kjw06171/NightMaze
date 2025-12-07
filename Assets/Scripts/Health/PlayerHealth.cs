using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("생명력 설정")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("사망 UI")]
    public GameObject deathUI;

    [Header("피격 효과 설정")]
    public float flashDuration = 0.1f;
    public int flashCount = 2;
    private SpriteRenderer[] renderers;

    [Header("피격 사운드 설정 🔊")]
    public AudioSource damageAudioSource;   // 피격 사운드용
    public AudioClip damageClip;            // 피격 소리
    [Range(0f, 1f)]
    public float damageVolume = 1f;         // 볼륨 조절

    [Header("회복 사운드 설정 🔊")]
    public AudioSource healAudioSource;     // 회복 사운드용
    public AudioClip healClip;              // 회복 소리
    [Range(0f, 1f)]
    public float healVolume = 1f;

    public delegate void HealthChanged(int currentHealth, int maxHealth);
    public event HealthChanged OnHealthChanged;

    void Start()
    {
        // HP 불러오기
        string scene = SceneManager.GetActiveScene().name;

        if (scene == "Lv_00_1" || scene == "Lv_00_2")
        {
            currentHealth = GameState.SharedHealth <= 0
                ? maxHealth
                : GameState.SharedHealth;
        }
        else
        {
            currentHealth = maxHealth;
        }

        renderers = GetComponentsInChildren<SpriteRenderer>();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (deathUI != null)
            deathUI.SetActive(false);
    }

    // =========================================================
    // 데미지 처리
    // =========================================================
    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damageAmount);

        // HP 저장
        SaveSharedHealth();

        Debug.Log($"플레이어 데미지 → 남은 HP: {currentHealth}");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // 🔥 피격 사운드 재생 (Pause 중엔 재생 X)
        PlayDamageSound();

        StartCoroutine(HitFlash());

        if (currentHealth <= 0)
            Die();
    }

    // 🔊 피격 사운드
    private void PlayDamageSound()
    {
        if (PauseMenu.isGamePaused) return; // Pause 중 재생 금지

        if (damageAudioSource != null && damageClip != null)
        {
            damageAudioSource.volume = damageVolume;
            damageAudioSource.PlayOneShot(damageClip);
        }
    }

    // =========================================================
    // 회복 처리
    // =========================================================
    public void Heal(int amount)
    {
        if (amount > 0 && currentHealth >= maxHealth)
        {
            Debug.Log("최대 체력입니다.");
            return;
        }

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // HP 저장
        SaveSharedHealth();

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // 🔥 회복 사운드 재생 (Pause 중엔 재생 X)
        PlayHealSound();

        if (currentHealth <= 0)
            Die();
    }

    // 🔊 회복 사운드
    private void PlayHealSound()
    {
        if (PauseMenu.isGamePaused) return; // Pause 중 재생 금지

        if (healAudioSource != null && healClip != null)
        {
            healAudioSource.volume = healVolume;
            healAudioSource.PlayOneShot(healClip);
        }
    }

    public bool IsHealthFull()
    {
        return currentHealth >= maxHealth;
    }

    // =========================================================
    // 사망 처리
    // =========================================================
    private void Die()
    {
        Debug.Log("💀 플레이어 사망! 게임 오버");

        // 🔥 1) 모든 게임 사운드 정지 (PauseMenu처럼)
        AudioListener.pause = true;

        // 🔥 2) 게임 정지
        Time.timeScale = 0;

        // 🔥 3) GameManager에게 상태 전달 (PauseMenu에서 ESC 막기용)
        GameManager.IsGameOver = true;

        // 🔥 4) UI 표시
        if (deathUI != null)
            deathUI.SetActive(true);
    }


    // =========================================================
    // HP 공유 저장 함수
    // =========================================================
    private void SaveSharedHealth()
    {
        string scene = SceneManager.GetActiveScene().name;

        if (scene == "Lv_00_1" || scene == "Lv_00_2")
        {
            GameState.SharedHealth = currentHealth;
        }
    }

    // =========================================================
    // 피격 깜빡임 효과
    // =========================================================
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
            if (r != null)
                r.color = color;
        }
    }
}
