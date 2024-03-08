using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // ���� ������ ��Ÿ���� ������ ����
    public enum Type { Melee, Range }; // ���� ����� ���Ÿ� ���� ����
    public Type type; // ���� ������ ����
    public int damage; // ������ ���ݷ�
    public float rate; // ������ ���� �ӵ�
    public int maxAmmo;
    public int curAmmo;

    public BoxCollider meleeArea; // ���� ���� ����
    public TrailRenderer trailEffect; // ���� ��� �� ȿ�� (��: �� �ֵθ� ���� ����)
    public Transform bulletPos;
    public GameObject bullet;

    public void Use()
    {
        if (type == Type.Melee) // ���� ������ ������ ���
        {
            StopCoroutine("Swing"); // Ȥ�� ���� ���� "Swing" �ڷ�ƾ�� �ִٸ� �ߴ�
            StartCoroutine("Swing"); // "Swing" �ڷ�ƾ ����
        }
        else if(type == Type.Range && curAmmo > 0)
        {
            curAmmo--;
            StartCoroutine("Shot");
        }
    }

    IEnumerator Swing() // ���� ���� �ֵθ��� ������ ���� �ڷ�ƾ
    {
        // 1. ���� �غ�
        yield return new WaitForSeconds(0.1f); // 0.1�� ���
        meleeArea.enabled = true; // ���� ���� ���� Ȱ��ȭ
        trailEffect.enabled = true; // ���� ���� ȿ�� Ȱ��ȭ

        // 2. ���� ���� ����
        yield return new WaitForSeconds(0.3f); // 0.3�� ���� ���� ����
        meleeArea.enabled = false; // ���� ���� ���� ��Ȱ��ȭ

        // 3. ���� ������
        yield return new WaitForSeconds(0.3f); // �߰� 0.3�� ��
        trailEffect.enabled = false; // ���� ���� ȿ�� ��Ȱ��ȭ
    }

    IEnumerator Shot()
    {
        // 1. �Ѿ� �߻�
        GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 15;
        yield return null; // �� ������ ��
    }

    // Use() ���� ��ƾ -> Swing() ���� ��ƾ -> Use() ���� ��ƾ���� ����
    // Use() ���� ��ƾ�� Swing() �ڷ�ƾ�� ���������� �۵� (Co-op)

    void Start()
    {
        // �ʱ�ȭ �ڵ尡 �ʿ��� ��� ���⿡ �ۼ�
    }

    // Update is called once per frame
    void Update()
    {
        // �� �����Ӹ��� ����� �ڵ尡 �ʿ��� ��� ���⿡ �ۼ�
    }
}
