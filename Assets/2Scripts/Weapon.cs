using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // 무기 유형을 나타내는 열거형 정의
    public enum Type { Melee, Range }; // 근접 무기와 원거리 무기 유형
    public Type type; // 현재 무기의 유형
    public int damage; // 무기의 공격력
    public float rate; // 무기의 공격 속도
    public int maxAmmo;
    public int curAmmo;

    public BoxCollider meleeArea; // 근접 공격 범위
    public TrailRenderer trailEffect; // 무기 사용 시 효과 (예: 검 휘두를 때의 궤적)
    public Transform bulletPos;
    public GameObject bullet;

    public void Use()
    {
        if (type == Type.Melee) // 무기 유형이 근접일 경우
        {
            StopCoroutine("Swing"); // 혹시 실행 중인 "Swing" 코루틴이 있다면 중단
            StartCoroutine("Swing"); // "Swing" 코루틴 시작
        }
        else if(type == Type.Range && curAmmo > 0)
        {
            curAmmo--;
            StartCoroutine("Shot");
        }
    }

    IEnumerator Swing() // 근접 무기 휘두르기 동작을 위한 코루틴
    {
        // 1. 공격 준비
        yield return new WaitForSeconds(0.1f); // 0.1초 대기
        meleeArea.enabled = true; // 근접 공격 범위 활성화
        trailEffect.enabled = true; // 무기 궤적 효과 활성화

        // 2. 실제 공격 실행
        yield return new WaitForSeconds(0.3f); // 0.3초 동안 공격 유지
        meleeArea.enabled = false; // 근접 공격 범위 비활성화

        // 3. 공격 마무리
        yield return new WaitForSeconds(0.3f); // 추가 0.3초 후
        trailEffect.enabled = false; // 무기 궤적 효과 비활성화
    }

    IEnumerator Shot()
    {
        // 1. 총알 발사
        GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 15;
        yield return null; // 한 프레임 쉼
    }

    // Use() 메인 루틴 -> Swing() 서브 루틴 -> Use() 메인 루틴으로 복귀
    // Use() 메인 루틴과 Swing() 코루틴은 협력적으로 작동 (Co-op)

    void Start()
    {
        // 초기화 코드가 필요한 경우 여기에 작성
    }

    // Update is called once per frame
    void Update()
    {
        // 매 프레임마다 실행될 코드가 필요한 경우 여기에 작성
    }
}
