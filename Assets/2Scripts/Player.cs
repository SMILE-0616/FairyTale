using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 플레이어 컨트롤러 클래스
public class Player : MonoBehaviour
{
    public float speed; // 플레이어의 이동 속도
    public GameObject[] weapons; // 사용 가능한 무기 목록
    public bool[] hasWeapons; // 각 무기의 소유 여부

    public int ammo; // 현재 탄약 수
    public int coin; // 현재 코인 수
    public int health; // 현재 체력
    public int score;
    
    public GameObject[] grenades;
    public int hasGrenades; // 현재 소유한 수류탄 수

    public GameObject grenadeObj;

    public Camera followCamera;
    public GameManager manager;

    public int maxAmmo; // 최대 탄약 수
    public int maxCoin; // 최대 코인 수
    public int maxHealth; // 최대 체력
    public int maxHasGrenades; // 소유 가능한 최대 수류탄 수

    float hAxis; // 수평 입력 값
    float vAxis; // 수직 입력 값

    // 다양한 액션을 위한 입력 플래그
    bool rDown; // 달리기 버튼이 눌렸는지 여부
    bool jDown; // 점프 버튼이 눌렸는지 여부
    bool fDown; // 공격 버튼이 눌렸는지 여부
    bool gDown; // 수류탄 공격
    bool reDown; // 공격 리로드
    bool iDown; // 상호작용 버튼이 눌렸는지 여부
    bool sDown1; // 무기 교체 버튼 1이 눌렸는지 여부
    bool sDown2; // 무기 교체 버튼 2가 눌렸는지 여부

    // 캐릭터의 상태를 나타내는 플래그
    bool isJump; // 점프 중인지 여부
    bool isDodge; // 회피 중인지 여부
    bool isSwap; // 무기 교체 중인지 여부
    bool isFireReady = true; // 발사 준비가 되었는지 여부
    bool isReload;
    bool isDamage;
    bool isDead;

    Vector3 moveVec; // 이동 방향 벡터
    Vector3 dodgeVec; // 회피 방향 벡터

    Rigidbody rigid; // 리지드바디 컴포넌트
    Animator anim; // 애니메이터 컴포넌트
    MeshRenderer[] meshs;

    GameObject nearObject; // 가까이 있는 객체
    public Weapon equipWeapon; // 현재 장착된 무기
    int equipWeaponIndex = -1; // 현재 장착된 무기 인덱스

    float fireDelay; // 발사 지연 시간

    void Awake()
    {
        rigid = GetComponent<Rigidbody>(); // 리지드바디 컴포넌트 가져오기
        anim = GetComponentInChildren<Animator>(); // 애니메이터 컴포넌트 가져오기
        meshs = GetComponentsInChildren<MeshRenderer>();

        PlayerPrefs.SetInt("MaxScore", 100000);
    }

        // 매 프레임마다 호출되는 업데이트 메서드
        void Update()
    {
        GetInput(); // 입력 받기
        Move(); // 이동 처리
        Turn(); // 회전 처리
        Jump(); // 점프 처리
        Grenade();
        Attack(); // 공격 처리
        Reload();
        Dodge(); // 회피 처리
        Swap(); // 무기 교체 처리
        Interaction(); // 상호작용 처리
    }

    void GetInput()
    {
        // 입력 값을 통해 이동 방향 결정
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        rDown = Input.GetButton("Run");
        jDown = Input.GetButton("Jump");
        fDown = Input.GetButtonDown("Fire1");
        gDown = Input.GetButtonDown("Fire2");
        reDown = Input.GetButtonDown("Reload");
        iDown = Input.GetButton("Interaction");
        sDown1 = Input.GetButton("Swap1");
        sDown2 = Input.GetButton("Swap2");
    }

    void Move()
    {
        // 이동 벡터 계산
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        // 회피 중이면 회피 방향으로 이동
        if (isDodge)
            moveVec = dodgeVec;

        // 무기 교체 중이면 이동하지 않음
        if (isSwap || isReload || !isFireReady || isDead)
            moveVec = Vector3.zero;

        // 실제 이동 처리
        transform.position += moveVec * (rDown ? 2f : 1f) * speed * Time.deltaTime;

        // 애니메이션 처리
        anim.SetBool("isWalk", moveVec != Vector3.zero);
        anim.SetBool("isRun", rDown);
    }

    void Turn()
    {
        // 1. 키보드에 의한 회전
        transform.LookAt(transform.position + moveVec);

        // 2. 마우스에 의한 회전
        if(fDown && !isDead)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
            }
        }
    }

    void Jump()
    {
        // 점프 조건 확인 후 점프 실행
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap && !isDead)
        {
            rigid.AddForce(Vector3.up * 5, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Attack()
    {
        if (equipWeapon == null) // 무기가 있을 때만 실행
            return;

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if (fDown && isFireReady && !isSwap && !isDodge && !isDead)
        {
            equipWeapon.Use(); // 무기 사용
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }
    }

    void Grenade()
    {
        if (hasGrenades == 0)
            return;

        if(gDown && !isReload && !isSwap && !isDead)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 5;

                GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation); // grenadeObj 변수를 사용하여 새로운 수류탄 생성
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);

                hasGrenades--;
                grenades[hasGrenades].SetActive(false);
            }
        }
    }

    void Reload()
    {
        if (equipWeapon == null)
            return;

        if (equipWeapon.type == Weapon.Type.Melee)
            return;

        if (ammo == 0)
            return;

        if(reDown && isFireReady && !isSwap && !isDodge && !isDead)
        {
            anim.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 3f);
        }
    }
    void ReloadOut()
    {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        // 소지한 총알이 무기의 최대 총알 보다 적으면 재장전한 총알 = 소지한 총알, 많으면 재장전한 총알 = 무기의 최대 총알
        equipWeapon.curAmmo = reAmmo; // 현재 총알의 개수 = 재장전한 총알 개수
        ammo -= reAmmo; // 재장전한만큼 총알을 전체 총알에서 빼줌
        isReload = false;
    }

    void Dodge()
    {
        // 회피 조건 확인 후 회피 실행
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap && !isDead)
        {
            dodgeVec = moveVec;
            speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.5f); // 일정 시간 후 회피 상태 해제
        }
    }

    void DodgeOut()
    {
        // 회피 상태 해제 처리
        speed *= 0.5f;
        isDodge = false;
    }

    void Swap()
    {
        // 무기 교체 조건 확인
        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0))
            return;
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
            return;

        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;

        // 무기 교체 실행
        if ((sDown1 || sDown2) && !isJump && !isDodge && !isDead)
        {
            if (equipWeapon != null)
                equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            anim.SetTrigger("doSwap");

            isSwap = true;

            Invoke("SwapOut", 0.4f); // 일정 시간 후 무기 교체 상태 해제
        }
    }

    void SwapOut()
    {
        // 무기 교체 상태 해제 처리
        isSwap = false;
    }

    void Interaction()
    {
        // 상호작용 조건 확인 및 실행
        if (iDown && nearObject != null && !isJump && !isDodge && !isDead)
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject); // 무기 획득
            }
            else if (nearObject.tag == "Shop")
            {
                Shop shop = nearObject.GetComponent<Shop>();
                shop.Enter(this);
            }
        }
    }
    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero;
    }

    void FixedUpdate()
    {
        FreezeRotation();
    }
    void OnCollisionEnter(Collision collision)
    {
        // 바닥에 닿으면 점프 상태 해제
        if (collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 실제 아이템에 닿았을 때 처리
        if (other.tag == "Real Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxAmmo)
                        ammo = maxAmmo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                        coin = maxCoin;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth)
                        health = maxHealth;
                    break;
                case Item.Type.Grenade:
                    if (hasGrenades == maxHasGrenades)
                        return;
                    hasGrenades += item.value;
                    break;
            }
            Destroy(other.gameObject); // 아이템 획득 후 제거
        }
        else if (other.tag == "EnemyBullet")
        {
            if (!isDamage)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;

                bool isBossAtk = other.name == "Boss Melee Area";
                StartCoroutine(OnDamage(isBossAtk));
            }

            if (other.GetComponent<Rigidbody>() != null)
                Destroy(other.gameObject);
        }
    }

    IEnumerator OnDamage(bool isBossAtk)
    {
        isDamage = true;
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.yellow;
        }

        if (isBossAtk)
            rigid.AddForce(transform.forward * -25, ForceMode.Impulse);

        if (health <= 0 && !isDead)
            OnDie();

        yield return new WaitForSeconds(1f);

        isDamage = false;
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }

        if (isBossAtk)
            rigid.velocity = Vector3.zero;
    }

    void OnDie()
    {
        anim.SetTrigger("doDie");
        isDead = true;
        manager.GameOver();
    }

    void OnTriggerStay(Collider other)
    {
        // 무기 근처에 있을 때 해당 무기를 가까이 있는 객체로 설정
        if (other.tag == "Weapon" || other.tag == "Shop")
            nearObject = other.gameObject;
    }

    void OnTriggerExit(Collider other)
    {
        // 무기에서 멀어지면 가까이 있는 객체 해제
        if (other.tag == "Weapon")
            nearObject = null;
        else if (other.tag == "Shop")
        {
            Shop shop = nearObject.GetComponent<Shop>();
            shop.Exit();
            nearObject = null;
        }
    }
}
