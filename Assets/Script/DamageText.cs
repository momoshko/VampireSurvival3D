using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private Canvas canvas;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        canvas = GetComponent<Canvas>();
        // Убедимся, что Canvas использует главную камеру
        if (canvas.worldCamera == null)
        {
            canvas.worldCamera = Camera.main;
        }
    }

    public void Setup(int damage, Vector3 position)
    {
        textMesh.text = damage.ToString();
        position.z = 0; // Устанавливаем Z = 0, чтобы текст был в плоскости игрока
        transform.position = position;
        Debug.Log($"Damage text positioned at: {transform.position}");
        Destroy(gameObject, 1f);
    }
}