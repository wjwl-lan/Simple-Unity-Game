using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 屏幕正上方 Boss 血条。拖 Slider + Boss 的 EnemyHealth 即可。
/// </summary>
public class BossHealthBar : MonoBehaviour
{
    [Header("Boss 目标")]
    public EnemyHealth boss;

    [Header("UI")]
    public Slider slider;
    public Image fill;
    public Text nameText;

    private void Start()
    {
        if (boss != null)
        {
            boss.HealthChanged += UpdateBar;
            UpdateBar(boss.currentHealth, boss.maxHealth);
        }
    }

    private void UpdateBar(float current, float max)
    {
        if (slider != null)
            slider.value = current / max;
        if (fill != null)
            fill.color = current / max < 0.3f ? Color.red : Color.white;
    }

    private void OnDestroy()
    {
        if (boss != null)
            boss.HealthChanged -= UpdateBar;
    }
}
