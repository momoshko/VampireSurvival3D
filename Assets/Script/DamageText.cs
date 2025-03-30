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
        // ��������, ��� Canvas ���������� ������� ������
        if (canvas.worldCamera == null)
        {
            canvas.worldCamera = Camera.main;
        }
    }

    public void Setup(int damage, Vector3 position)
    {
        textMesh.text = damage.ToString();
        position.z = 0; // ������������� Z = 0, ����� ����� ��� � ��������� ������
        transform.position = position;
        Debug.Log($"Damage text positioned at: {transform.position}");
        Destroy(gameObject, 1f);
    }
}