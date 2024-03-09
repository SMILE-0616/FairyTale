using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �÷��̾� ��Ʈ�ѷ� Ŭ����
public class Player : MonoBehaviour
{
    public float speed; // �÷��̾��� �̵� �ӵ�
    public GameObject[] weapons; // ��� ������ ���� ���
    public bool[] hasWeapons; // �� ������ ���� ����

    public int ammo; // ���� ź�� ��
    public int coin; // ���� ���� ��
    public int health; // ���� ü��
    public int score;
    
    public GameObject[] grenades;
    public int hasGrenades; // ���� ������ ����ź ��

    public GameObject grenadeObj;

    public Camera followCamera;
    public GameManager manager;

    public int maxAmmo; // �ִ� ź�� ��
    public int maxCoin; // �ִ� ���� ��
    public int maxHealth; // �ִ� ü��
    public int maxHasGrenades; // ���� ������ �ִ� ����ź ��

    float hAxis; // ���� �Է� ��
    float vAxis; // ���� �Է� ��

    // �پ��� �׼��� ���� �Է� �÷���
    bool rDown; // �޸��� ��ư�� ���ȴ��� ����
    bool jDown; // ���� ��ư�� ���ȴ��� ����
    bool fDown; // ���� ��ư�� ���ȴ��� ����
    bool gDown; // ����ź ����
    bool reDown; // ���� ���ε�
    bool iDown; // ��ȣ�ۿ� ��ư�� ���ȴ��� ����
    bool sDown1; // ���� ��ü ��ư 1�� ���ȴ��� ����
    bool sDown2; // ���� ��ü ��ư 2�� ���ȴ��� ����

    // ĳ������ ���¸� ��Ÿ���� �÷���
    bool isJump; // ���� ������ ����
    bool isDodge; // ȸ�� ������ ����
    bool isSwap; // ���� ��ü ������ ����
    bool isFireReady = true; // �߻� �غ� �Ǿ����� ����
    bool isReload;
    bool isDamage;
    bool isDead;

    Vector3 moveVec; // �̵� ���� ����
    Vector3 dodgeVec; // ȸ�� ���� ����

    Rigidbody rigid; // ������ٵ� ������Ʈ
    Animator anim; // �ִϸ����� ������Ʈ
    MeshRenderer[] meshs;

    GameObject nearObject; // ������ �ִ� ��ü
    public Weapon equipWeapon; // ���� ������ ����
    int equipWeaponIndex = -1; // ���� ������ ���� �ε���

    float fireDelay; // �߻� ���� �ð�

    void Awake()
    {
        rigid = GetComponent<Rigidbody>(); // ������ٵ� ������Ʈ ��������
        anim = GetComponentInChildren<Animator>(); // �ִϸ����� ������Ʈ ��������
        meshs = GetComponentsInChildren<MeshRenderer>();

        PlayerPrefs.SetInt("MaxScore", 100000);
    }

        // �� �����Ӹ��� ȣ��Ǵ� ������Ʈ �޼���
        void Update()
    {
        GetInput(); // �Է� �ޱ�
        Move(); // �̵� ó��
        Turn(); // ȸ�� ó��
        Jump(); // ���� ó��
        Grenade();
        Attack(); // ���� ó��
        Reload();
        Dodge(); // ȸ�� ó��
        Swap(); // ���� ��ü ó��
        Interaction(); // ��ȣ�ۿ� ó��
    }

    void GetInput()
    {
        // �Է� ���� ���� �̵� ���� ����
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
        // �̵� ���� ���
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        // ȸ�� ���̸� ȸ�� �������� �̵�
        if (isDodge)
            moveVec = dodgeVec;

        // ���� ��ü ���̸� �̵����� ����
        if (isSwap || isReload || !isFireReady || isDead)
            moveVec = Vector3.zero;

        // ���� �̵� ó��
        transform.position += moveVec * (rDown ? 2f : 1f) * speed * Time.deltaTime;

        // �ִϸ��̼� ó��
        anim.SetBool("isWalk", moveVec != Vector3.zero);
        anim.SetBool("isRun", rDown);
    }

    void Turn()
    {
        // 1. Ű���忡 ���� ȸ��
        transform.LookAt(transform.position + moveVec);

        // 2. ���콺�� ���� ȸ��
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
        // ���� ���� Ȯ�� �� ���� ����
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
        if (equipWeapon == null) // ���Ⱑ ���� ���� ����
            return;

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if (fDown && isFireReady && !isSwap && !isDodge && !isDead)
        {
            equipWeapon.Use(); // ���� ���
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

                GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation); // grenadeObj ������ ����Ͽ� ���ο� ����ź ����
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
        // ������ �Ѿ��� ������ �ִ� �Ѿ� ���� ������ �������� �Ѿ� = ������ �Ѿ�, ������ �������� �Ѿ� = ������ �ִ� �Ѿ�
        equipWeapon.curAmmo = reAmmo; // ���� �Ѿ��� ���� = �������� �Ѿ� ����
        ammo -= reAmmo; // �������Ѹ�ŭ �Ѿ��� ��ü �Ѿ˿��� ����
        isReload = false;
    }

    void Dodge()
    {
        // ȸ�� ���� Ȯ�� �� ȸ�� ����
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap && !isDead)
        {
            dodgeVec = moveVec;
            speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.5f); // ���� �ð� �� ȸ�� ���� ����
        }
    }

    void DodgeOut()
    {
        // ȸ�� ���� ���� ó��
        speed *= 0.5f;
        isDodge = false;
    }

    void Swap()
    {
        // ���� ��ü ���� Ȯ��
        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0))
            return;
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
            return;

        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;

        // ���� ��ü ����
        if ((sDown1 || sDown2) && !isJump && !isDodge && !isDead)
        {
            if (equipWeapon != null)
                equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            anim.SetTrigger("doSwap");

            isSwap = true;

            Invoke("SwapOut", 0.4f); // ���� �ð� �� ���� ��ü ���� ����
        }
    }

    void SwapOut()
    {
        // ���� ��ü ���� ���� ó��
        isSwap = false;
    }

    void Interaction()
    {
        // ��ȣ�ۿ� ���� Ȯ�� �� ����
        if (iDown && nearObject != null && !isJump && !isDodge && !isDead)
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject); // ���� ȹ��
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
        // �ٴڿ� ������ ���� ���� ����
        if (collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // ���� �����ۿ� ����� �� ó��
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
            Destroy(other.gameObject); // ������ ȹ�� �� ����
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
        // ���� ��ó�� ���� �� �ش� ���⸦ ������ �ִ� ��ü�� ����
        if (other.tag == "Weapon" || other.tag == "Shop")
            nearObject = other.gameObject;
    }

    void OnTriggerExit(Collider other)
    {
        // ���⿡�� �־����� ������ �ִ� ��ü ����
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
