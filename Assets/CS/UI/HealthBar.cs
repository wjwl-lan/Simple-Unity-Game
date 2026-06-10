using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 简易血条：挂在一个 Canvas 下的 Slider/Image 物体上，指定 target 的 EnemyHealth 或 PlayerHealth。
/// </summary>
public class HealthBar : MonoBehaviour
{
    [Header("目标")]
    public EnemyHealth enemyTarget;
    public PlayerHealth playerTarget;
    public Transform followTarget;

    [Header("UI")]
    public Slider slider;
    public Image fillImage;
    public Color fullColor = Color.green;
    public Color lowColor = Color.red;
    [Range(0, 1)] public float lowThreshold = 0.3f;

    [Header("世界空间偏移")]
    public Vector3 worldOffset = new Vector3(0, 2.5f, 0);

    private float maxHealth;

    private void Start()
    {
        // 确定血量来源
        if (enemyTarget != null)
        {
            maxHealth = enemyTarget.maxHealth;
            enemyTarget.HealthChanged += OnHealthChanged;
            UpdateBar(enemyTarget.currentHealth);
        }
        else if (playerTarget != null)
        {
            maxHealth = playerTarget.maxHealth;
            playerTarget.HealthChanged += OnHealthChanged;
            UpdateBar(playerTarget.currentHealth);
        }

        // 设为世界空间渲染
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.transform.localScale = Vector3.one * 0.01f;
        }
    }

    private void LateUpdate()
    {
        if (followTarget != null)
            transform.position = followTarget.position + worldOffset;
    }

    private void OnHealthChanged(float current, float max)
    {
        UpdateBar(current);
    }

    private void UpdateBar(float current)
    {
        if (slider != null)
            slider.value = current / maxHealth;

        if (fillImage != null)
            fillImage.color = Color.Lerp(lowColor, fullColor, current / maxHealth / lowThreshold);
    }

    private void OnDestroy()
    {
        if (enemyTarget != null)
            enemyTarget.HealthChanged -= OnHealthChanged;
        if (playerTarget != null)
            playerTarget.HealthChanged -= OnHealthChanged;
    }
}
