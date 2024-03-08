using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    // 아이템의 종류를 열거형으로 정의합니다. 각각의 아이템 타입을 나타냅니다.
    public enum Type { Ammo, Coin, Grenade, Heart, Weapon };
    public Type type; // 아이템의 타입을 저장하는 변수
    public int value; // 아이템의 값 혹은 효과의 크기를 저장하는 변수

    Rigidbody rigid;
    SphereCollider sphereCollider;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    // Update는 매 프레임마다 호출됩니다.
    void Update()
    {
        // 아이템을 회전시키는 코드입니다. Vector3.up을 기준으로 매 초 10도씩 회전합니다.
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
