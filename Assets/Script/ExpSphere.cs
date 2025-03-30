using UnityEngine;

public class ExpSphere : MonoBehaviour
{
    public int expValue = 5; // ���������� �����, ������� ��� �����

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerAttack playerAttack = other.GetComponent<PlayerAttack>();
            if (playerAttack != null)
            {
                playerAttack.AddExperience(expValue);
                Destroy(gameObject); // ���������� ����� ����� �������
            }
        }
    }
}