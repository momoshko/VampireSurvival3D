using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Ссылка на игрока
    public float height = 10f; // Начальная высота камеры
    public float distance = 5f; // Расстояние назад от игрока
    public float zoomSpeed = 2f; // Скорость зума
    public float minHeight = 5f; // Минимальная высота
    public float maxHeight = 15f; // Максимальная высота

    void LateUpdate()
    {
        if (player != null) // Проверяем, существует ли игрок
        {
            // Позиция камеры: над игроком с небольшим отступом назад
            Vector3 targetPosition = player.position + Vector3.up * height - Vector3.forward * distance;
            transform.position = targetPosition;

            // Камера смотрит на игрока с небольшим наклоном
            transform.LookAt(player.position);
        }

        // Управление зумом колесом мыши
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        height -= scroll * zoomSpeed; // Уменьшаем/увеличиваем высоту
        height = Mathf.Clamp(height, minHeight, maxHeight); // Ограничиваем высоту
    }
}