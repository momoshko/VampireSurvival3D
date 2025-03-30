using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public RectTransform joystickBackground; // ��� ���������
    public RectTransform joystickHandle;     // ����� ���������
    private Vector2 joystickInput;           // ������ ����� � ���������
    private Vector2 touchStartPos;           // ��������� ������� �������
    private bool isDragging;                 // ���� �������
    private int touchId = -1;                // ID ������� ��� ���������

    void Update()
    {
        // ���������� � ���������� (��� ��)
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 keyboardMovement = new Vector3(moveX, 0, moveZ).normalized * speed * Time.deltaTime;

        // ���������� � ���������� ������ (��� ���������)
        Vector3 touchMovement = HandleTouchInput();

        // ����������� ����: ���� ���� ��������� ����, �� �����������
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
                    // ���������, ����� �� ��� � ������� ���������
                    if (RectTransformUtility.RectangleContainsScreenPoint(joystickBackground, touchPos))
                    {
                        isDragging = true;
                        touchStartPos = touchPos;
                        touchId = touch.fingerId;
                        joystickHandle.anchoredPosition = Vector2.zero; // ���������� �����
                    }
                }
                else if (touch.phase == TouchPhase.Moved && isDragging && touch.fingerId == touchId)
                {
                    // ��������� ��������
                    Vector2 delta = touchPos - touchStartPos;
                    float maxDistance = joystickBackground.sizeDelta.x / 2; // ������������ ������ ���������
                    joystickInput = Vector2.ClampMagnitude(delta / maxDistance, 1f); // ����������� ����
                    joystickHandle.anchoredPosition = joystickInput * maxDistance; // ������� �����
                }
                else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && touch.fingerId == touchId)
                {
                    isDragging = false;
                    joystickInput = Vector2.zero;
                    joystickHandle.anchoredPosition = Vector2.zero; // ���������� �����
                    touchId = -1;
                }
            }
        }

        // ����������� ���� ��������� � ��������
        return new Vector3(joystickInput.x, 0, joystickInput.y).normalized * speed * Time.deltaTime;
    }
}