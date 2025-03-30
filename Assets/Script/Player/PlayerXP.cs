using UnityEngine;

public class PlayerXP : MonoBehaviour
{
    public int xp = 0;
    public PlayerAttack playerAttack; // ������ �� ������ �����

    void Start()
    {
        playerAttack = GetComponent<PlayerAttack>(); // �������� ���������
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("XPOrb"))
        {
            xp += 1;
            Destroy(other.gameObject);
            Debug.Log("XP: " + xp);

            // ���������: ������ 5 ����� ����������� �������� �����
            if (xp % 5 == 0)
            {
                playerAttack.attackRate -= 0.05f; // ��������� �������� ��������
                Debug.Log("Attack Rate: " + playerAttack.attackRate);
            }
        }
    }
}