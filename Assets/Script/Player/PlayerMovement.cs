using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public RectTransform joystickBackground; // Фон джойстика
    public RectTransform joystickHandle;     // Ручка джойстика
    private Vector2 joystickInput;           // Вектор ввода с джойстика
    private Vector2 touchStartPos;           // Начальная позиция касания
    private bool isDragging;                 // Флаг касания
    private int touchId = -1;                // ID касания для мультитач

    void Update()
    {
        // Управление с клавиатуры (для ПК)
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 keyboardMovement = new Vector3(moveX, 0, moveZ).normalized * speed * Time.deltaTime;

        // Управление с сенсорного экрана (для мобильных)
        Vector3 touchMovement = HandleTouchInput();

        // Комбинируем ввод: если есть сенсорный ввод, он приоритетен
        Vector3 finalMovement = touchMovement != Vector3.zero ? touchMovement : keyboardMovement;
        transform.Translate(finalMovement);
    }

    Vector3 HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                Vector2 touchPos = touch.position;

                if (touch.phase == TouchPhase.Began)
                {
                    // Проверяем, попал ли тач в область джойстика
                    if (RectTransformUtility.RectangleContainsScreenPoint(joystickBackground, touchPos))
                    {
                        isDragging = true;
                        touchStartPos = touchPos;
                        touchId = touch.fingerId;
                        joystickHandle.anchoredPosition = Vector2.zero; // Центрируем ручку
                    }
                }
                else if (touch.phase == TouchPhase.Moved && isDragging && touch.fingerId == touchId)
                {
                    // Вычисляем смещение
                    Vector2 delta = touchPos - touchStartPos;
                    float maxDistance = joystickBackground.sizeDelta.x / 2; // Максимальный радиус джойстика
                    joystickInput = Vector2.ClampMagnitude(delta / maxDistance, 1f); // Нормализуем ввод
                    joystickHandle.anchoredPosition = joystickInput * maxDistance; // Двигаем ручку
                }
                else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && touch.fingerId == touchId)
                {
                    isDragging = false;
                    joystickInput = Vector2.zero;
                    joystickHandle.anchoredPosition = Vector2.zero; // Сбрасываем ручку
                    touchId = -1;
                }
            }
        }

        // Преобразуем ввод джойстика в движение
        return new Vector3(joystickInput.x, 0, joystickInput.y).normalized * speed * Time.deltaTime;
    }
}