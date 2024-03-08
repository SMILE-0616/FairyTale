using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    // �������� ������ ���������� �����մϴ�. ������ ������ Ÿ���� ��Ÿ���ϴ�.
    public enum Type { Ammo, Coin, Grenade, Heart, Weapon };
    public Type type; // �������� Ÿ���� �����ϴ� ����
    public int value; // �������� �� Ȥ�� ȿ���� ũ�⸦ �����ϴ� ����

    Rigidbody rigid;
    SphereCollider sphereCollider;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    // Update�� �� �����Ӹ��� ȣ��˴ϴ�.
    void Update()
    {
        // �������� ȸ����Ű�� �ڵ��Դϴ�. Vector3.up�� �������� �� �� 10���� ȸ���մϴ�.
        transform.Rotate(Vector3.up * 10 * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            rigid.isKinematic = true;
            sphereCollider.enabled = false;
        }
    }
}
