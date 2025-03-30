using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // ������ �� ������
    public float height = 10f; // ��������� ������ ������
    public float distance = 5f; // ���������� ����� �� ������
    public float zoomSpeed = 2f; // �������� ����
    public float minHeight = 5f; // ����������� ������
    public float maxHeight = 15f; // ������������ ������

    void LateUpdate()
    {
        if (player != null) // ���������, ���������� �� �����
        {
            // ������� ������: ��� ������� � ��������� �������� �����
            Vector3 targetPosition = player.position + Vector3.up * height - Vector3.forward * distance;
            transform.position = targetPosition;

            // ������ ������� �� ������ � ��������� ��������
            transform.LookAt(player.position);
        }

        // ���������� ����� ������� ����
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        height -= scroll * zoomSpeed; // ���������/����������� ������
        height = Mathf.Clamp(height, minHeight, maxHeight); // ������������ ������
    }
}