using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class PlayerScript : NetworkBehaviour
{
    public Camera cam;
    public Camera vCam;
    Transform camTran;
    CharacterController cc;
    public float downForce = 10;
    public float sens = 1;
    public float speed = 4;
    public float friction = 1.2f;
    public float airFriction = 1.05f;
    public float jumpSpeed = 5;
    public float gravity = 5;
    float mouseY;
    public Vector3 velocity;
    AudioSource aud;
    public AudioClip[] sounds;
    public Transform weapons;
    Animator gunAnim;
    bool zoom;
    bool queueFire;
    int queueChangeWeapon;
    float targetCrossHairSize;

[SyncVar]
    int curWeapon = 0;
    float curfloat;
    public float maxHealth = 100;
    public GameObject pm;

    public GameObject ui;
    public TMP_Text hpTxt;
    public TMP_Text hpMax;
    public TMP_Text bullTxt;
    public TMP_Text ammunitionTxt;
    public TMP_Text usernameText;
    public Slider hpSlid;
    public RectTransform crossHair;

    public GameObject corpse;
    public GameObject corpsePhys;

    public Animator pmAnim;

    WeaponProperty curWep;

    bool mLock = true;
    float xVel;
    float xVelLerp;

    Vector3 camPos;
    Vector3 offset;

    public GameObject ht;

    public Transform personThatKilledYou;

    bool jumped;

[SyncVar]
    public float health = 100;

[SyncVar]
    public string name = "siblotalle Text";

    public bool lg;

[SyncVar]
    public bool alive = true;

    public bool smooth = true;

[SyncVar]
    public int points;
[SyncVar]
    public int deaths;

    float walkThing;

    float interactTimer;
  [SyncVar]
    bool hasCorpse;

    bool throwQueue;

    float fireDelay;

    public GameObject interactableIndicator;
    float maxInteractTimer;
    public Image interactBar;
    float interactDelay;

    // Start is called before the first frame update
    void Start()
    {
        aud = GetComponent<AudioSource>();
        camTran = cam.transform;
        cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        health = maxHealth;
        if (isLocalPlayer)
        {
          CmdSetString(name, PlayerPrefs.GetString("Username"));
          name = PlayerPrefs.GetString("Username");
          pm.SetActive(false);
          ui.SetActive(true);
          offset = camTran.position - transform.position;
          Destroy(usernameText.gameObject);
        }
        else {
          Destroy(camTran.gameObject);
          usernameText.text = name;
          Transform[] allChildren = weapons.GetComponentsInChildren<Transform>();
          foreach (Transform child in allChildren)
              child.gameObject.layer = 0;
        }
        CmdChangeWeapon(0);
    }

        [Command]
    private void CmdDestroyObject(GameObject obj)
    {
        if(!obj) return;
        Destroy(obj);
        RpcDestroyObject(obj);
    }
    
    [ClientRpc]
    private void RpcDestroyObject(GameObject obj)
    {
        if(!obj) return;
        Destroy(obj);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isLocalPlayer || curWep == null)
          return;
        PlayerMove();
        fireDelay -= Time.fixedDeltaTime;
        if (queueChangeWeapon != -2) {
          CmdChangeWeapon(queueChangeWeapon);
          queueChangeWeapon = -2;
        }
        if (throwQueue) {
          throwQueue = false;
        }
        if (interactDelay >= 0)
          interactDelay -= Time.fixedDeltaTime;
        RaycastHit hi;
        interactableIndicator.SetActive(Physics.Raycast(cam.transform.position, cam.transform.forward, out hi, 5) && hi.transform.gameObject.tag == "Corpse");
        if (Input.GetKey(KeyCode.F) && Physics.Raycast(cam.transform.position, cam.transform.forward, out hi, 5) && interactDelay <= 0 && hi.transform.gameObject.tag == "Corpse") {
          if (!hasCorpse) {
            maxInteractTimer = 0.5f;
            interactTimer += Time.fixedDeltaTime;
            interactBar.fillAmount = interactTimer / maxInteractTimer;
            interactBar.enabled = true;
            if (interactTimer >= maxInteractTimer) {
              NetworkServer.Destroy(hi.transform.root.gameObject);
              hasCorpse = true;
              CmdSetBool(hasCorpse, true);
              interactDelay = 0.3f;
            }
          }
        } else {
          interactTimer = 0;
          interactBar.enabled = false;
        }
        if (hasCorpse && Input.GetKey(KeyCode.F) && interactDelay <= 0) {
          CmdSpawnRag(cam.transform.position + cam.transform.forward, cam.transform.forward * 150);
          hasCorpse = false;
          CmdSetBool(hasCorpse, false);
        }
        if (Input.GetKey(KeyCode.R))
          curWep.Reload();
        hpTxt.text = "" + health;
        hpMax.text = "" + maxHealth;
        if (curWep.infiniteAmmo) {
          bullTxt.text = "";
          ammunitionTxt.text = "";
        } else {
          bullTxt.text = "" + curWep.mag;
          ammunitionTxt.text =  "" + curWep.bullet;
        }
        pmAnim.SetFloat("Speed",cc.velocity.magnitude / 5);
        pmAnim.SetBool("isGrounded",cc.isGrounded);
        hpSlid.value = Mathf.Lerp(hpSlid.value, health / maxHealth, Time.deltaTime * 2);

        this.xVel = camTran.InverseTransformDirection(cc.velocity).x;
        xVelLerp = Mathf.Lerp(xVelLerp, xVel, Time.deltaTime * 5);

        if (Input.GetButtonDown("Cancel"))
          mLock = !mLock;

        if (mLock)
          Cursor.lockState = CursorLockMode.Locked;
        else
          Cursor.lockState = CursorLockMode.None;

        if (alive)
        {
          if (Input.GetKey(KeyCode.K))
            CmdDie();
          cam.fieldOfView = Mathf.Lerp(cam.fieldOfView,zoom ? 20 : 90,Time.fixedDeltaTime * 10);
          vCam.fieldOfView = Mathf.Lerp(vCam.fieldOfView,zoom ? 5: 54,Time.fixedDeltaTime * 10);
          if (mLock) {
            transform.eulerAngles += transform.up * (Input.GetAxis("Mouse X") * sens);
            mouseY += sens * Input.GetAxis("Mouse Y");
            mouseY = Mathf.Clamp(mouseY, -90, 90);
            camTran.localEulerAngles = new Vector3(-mouseY, 0, 0);
            if (Input.GetMouseButtonDown(1) && curWeapon == 0)
              zoom = !zoom;
            if (curWeapon != 0 && zoom)
              zoom = false;
            if (Input.GetButton("Fire1")) {
              Fire();
            }
          }
          if (smooth) {
          if (Vector3.Distance(camTran.position, transform.position + offset) > 2)
            camPos = Vector3.MoveTowards(transform.position + offset, camPos,2);
          else
            camPos = Vector3.Lerp(camPos,transform.position + offset, Time.fixedDeltaTime * 20);
          } else {
            camPos = transform.position + offset;
          }
        }
        else {
          cam.fieldOfView = 90;
          if (personThatKilledYou == null) {
            camPos = Vector3.Lerp(camPos, transform.position - transform.forward * 3 + transform.up * 1.5f, Time.fixedDeltaTime * 3);
            camTran.LookAt(transform.position + Vector3.up / 2);
          }
          else {
            camPos = Vector3.Lerp(camPos, personThatKilledYou.position - personThatKilledYou.forward * 3 + personThatKilledYou.up * 1.5f, Time.fixedDeltaTime * 3);
          camTran.LookAt(personThatKilledYou.position + Vector3.up / 2);
          }
        }
        camTran.eulerAngles = new Vector3(camTran.eulerAngles.x, camTran.eulerAngles.y, -xVelLerp / 1.4f);
        crossHair.eulerAngles = new Vector3(0,0, -xVelLerp);
        weapons.localPosition = Vector3.Lerp(weapons.localPosition, new Vector3((Mathf.Sin(walkThing * 3) / 48) * 2,Mathf.Sin(walkThing * 6) / 72,-Mathf.Sin(walkThing *2) / 20) - vCam.transform.InverseTransformPoint(vCam.transform.position + cc.velocity / 150), Time.fixedDeltaTime * 10);
        camTran.position = camPos;
        if (lg != cc.isGrounded) {
            if (!lg && cc.velocity.y < -5)
            {
              aud.PlayOneShot(sounds[Random.Range(0,1)]);
            }
            if (lg)
            {
              if (jumped)
                jumped = false;
              else
              velocity.y += downForce;
            }
              lg = cc.isGrounded;
        }
        if (cc.isGrounded)
          walkThing += cc.velocity.magnitude / 200;
    }

    [Command(requiresAuthority = false)]
    void CmdChangeWeapon(int value) {
      if (fireDelay > 0)
        return;
      int ba = value;
      if (ba >= 3)
        ba = 0;
      if (ba < 0)
        ba = 2;
      curWeapon = ba;
      GetWeapon(ba);
      SetWeaponActive(ba);
    }

    [ClientRpc]
    void SetWeaponActive(int ba) {
      for (int i = 0; i < weapons.childCount; i++) {
        if (i == ba)
          weapons.GetChild(i).gameObject.SetActive(true);
        else
          weapons.GetChild(i).gameObject.SetActive(false);
      }
    }

    [ClientRpc]
    void GetWeapon(int value) {
      curWep = weapons.GetChild(value).GetChild(0).GetComponent<WeaponProperty>();
    }

    [Command(requiresAuthority = false)]
    void CmdChangeLastPlayer(Transform tr, PlayerScript ps) {
      RpcChangeLastPlayer(tr,ps);
    }

    [ClientRpc]
    void RpcChangeLastPlayer(Transform tr, PlayerScript ps) {
      ps.personThatKilledYou = tr;
    }

    [Command]
    void CmdSetBool(bool a, bool b) {
      RpcSetBool(a,b);
    }

    [Command(requiresAuthority = false)]
    void CmdSetString(string a, string b) {
      RpcSetString(a,b);
    }

    [ClientRpc]
    void RpcSetBool(bool a, bool b) {
      a=b;
    }

    [ClientRpc]
    void RpcSetString(string a, string b) {
      a=b;
    }

    void Fire()
    {
      if (!isLocalPlayer || !alive || curWep.reloading || curWep.fireTime > 0 || (curWep.mag <= 0 && curWep.bullet <= 0 && curWep.infiniteAmmo) || queueFire)
        return;
      if (curWep.mag <= 0)
        curWep.Reload();
      else {
        fireDelay = 0.5f;
        FireWeapon(this);
      queueFire = true;
      StartCoroutine("SetFireReset");
        targetCrossHairSize += 20;
      RaycastHit hit;
      if (!Physics.Raycast(transform.position + offset, cam.transform.forward, out hit))
        return;
      if (!hit.transform.GetComponent<PlayerScript>()) {
        CmdSpawnImpact(curWep.impact.name, hit.point + hit.normal * 0.001f, Quaternion.LookRotation(hit.point + hit.normal * 0.001f, hit.point + hit.normal));
        return;
      }
      CmdRayVisual(hit.point, curWep.damage, this);
      CmdHealth(hit.transform.GetComponent<PlayerScript>(), hit.transform.GetComponent<PlayerScript>().health - curWep.damage); //this dohes't work
      if (hit.transform.GetComponent<PlayerScript>().health <= curWep.damage)
        points++;
      CmdChangeLastPlayer(transform, hit.transform.GetComponent<PlayerScript>());
      CmdSpawnImpact(curWep.blood.name, hit.point + hit.normal * 0.001f, Quaternion.LookRotation(hit.point + hit.normal * 0.001f, hit.point + hit.normal));
      }
    }

    [Command]
    void CmdSpawnRag(Vector3 pos, Vector3 velocity) {
      GameObject thrownBody = Instantiate(NetworkManager.singleton.spawnPrefabs[1], pos, transform.rotation);
      thrownBody.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Rigidbody>().velocity = velocity;
      NetworkServer.Spawn(thrownBody);
    }

    [Command]
    void CmdRayVisual(Vector3 pos, float damage, PlayerScript ps) {
      RpcRayVisual(pos, damage, ps);
    }

    IEnumerator SetFireReset() {
      yield return new WaitUntil(() => curWep.fireTime > 0);
      queueFire = false;
    }

    [ClientRpc]
    void RpcRayVisual(Vector3 pos, float damage, PlayerScript ps) {
      if (!isLocalPlayer)
        return;
      ps.aud.PlayOneShot(sounds[2]);
      HitTextFallScript htfs = Instantiate(ht, pos, Quaternion.identity).GetComponent<HitTextFallScript>();
      htfs.txt.text = "" + damage;
      htfs.camToFollow = ps.camTran;
    }

    [Command]
    void FireWeapon(PlayerScript ps) {
      if (!curWep.infiniteAmmo)
        curWep.mag--;
      curWep.fireTime = curWep.fireDelay;
      RpcFireWeapon(ps, curWep.mag);
      FireMotion(this);
    }

    [ClientRpc]
    void RpcFireWeapon(PlayerScript ps, int mag) {
      if (!curWep.infiniteAmmo)
        curWep.mag = mag;
      curWep.fireTime = curWep.fireDelay;
    }

    [Command(requiresAuthority = false)]
    public void CmdDie()
    {
      deaths++;
      alive = false;
      StartCoroutine("respawn");

      GameObject bodyClone = Instantiate(NetworkManager.singleton.spawnPrefabs[0], transform.position, transform.rotation);
      NetworkServer.Spawn(bodyClone);
    }

    [ClientRpc]
    public void FireMotion(PlayerScript ps) {
      ps.curWep.FireMotion();
    }

    [Command(requiresAuthority = false)]
    void CmdSpawnImpact(string obj, Vector3 pos, Quaternion rot) {
      RpcSpawnImpact(obj,pos,rot);
    }

    [ClientRpc]
    void RpcSpawnImpact(string obj, Vector3 pos, Quaternion rot) {
        Destroy(Instantiate(Resources.Load("impact/"+obj), pos, rot), 2);
    }

    IEnumerator respawn()
    {
      yield return new WaitForSeconds(5);
      Reset();
    }

    void Reset() {
      MoveTo(new Vector3(0, 50, 0));
      alive = true;
      health = maxHealth;
      velocity = Vector3.zero;
      foreach (Transform balls in weapons)
        balls.GetChild(0).GetComponent<WeaponProperty>().Reset();
    }

    [Server]
    public void MoveTo(Vector3 newPosition) { //call this on the server
        transform.position = newPosition; //so the player moves also in the server
        RpcMoveTo(newPosition);
    }

    [ClientRpc]
    void RpcMoveTo(Vector3 newPosition) {
      transform.position = newPosition; //this will run in all clients
    }

    [Command]
    public void CmdHealth(PlayerScript ps, float health)
    {
      ps.health = health;
      ps.personThatKilledYou = transform;
      RpcHealth(ps, health);
    }

    [ClientRpc]
    public void RpcHealth(PlayerScript ps, float health)
    {
      ps.health = health;
      ps.personThatKilledYou = transform;
    }

    void Update(){

      cc.enabled = alive;
      weapons.gameObject.SetActive(alive);
      if (!isLocalPlayer)
        pm.SetActive(alive);
      if (health <= 0 && alive)
        CmdDie();
      if (Input.GetKeyDown(KeyCode.Alpha1))
        queueChangeWeapon = 0;
      if (Input.GetKeyDown(KeyCode.Alpha2))
        queueChangeWeapon = 1;
      if (Input.GetKeyDown(KeyCode.Alpha3))
        queueChangeWeapon = 2;
      if (Input.GetKeyDown(KeyCode.F))
        throwQueue = true;
      if (Input.GetAxis("Mouse ScrollWheel") > 0)
        queueChangeWeapon = curWeapon + 1;
      if (Input.GetAxis("Mouse ScrollWheel") < 0)
        queueChangeWeapon = curWeapon - 1;
        targetCrossHairSize = Mathf.Lerp(targetCrossHairSize, 10, Time.deltaTime * 5);
        crossHair.sizeDelta = new Vector2(targetCrossHairSize, targetCrossHairSize);
    }

    void PlayerMove()
    {
      if (!alive)
        return;
        Vector2 move = Vector3.zero;
        move.x = Input.GetAxis("Horizontal") * speed;
        move.y = Input.GetAxis("Vertical") * speed;
        if (cc.isGrounded)
        {
          velocity.y = -downForce;
          velocity.x /= friction;
          velocity.z /= friction;
          if (Input.GetButton("Jump"))
          {
            velocity += transform.up * (jumpSpeed + downForce);
            move *= 1.2f;
            velocity = Vector3.ClampMagnitude(velocity, 30);
            jumped = true;
          } else {
          }
        }
        else
        {
          velocity.y -= gravity;
          move /= 10;
          velocity.x /= airFriction;
          velocity.z /= airFriction;
        }
        velocity += transform.forward * move.y + transform.right * move.x;
        cc.Move(velocity * Time.fixedDeltaTime);
    }

    void OnCollisionEnter(Collision cs) {
      if (!isLocalPlayer)
        return;
      if (cs.gameObject.GetComponent<BouncePad>()) {
        Debug.Log("afgds");
        velocity = new Vector3(0,cs.gameObject.GetComponent<BouncePad>().jumpPower,0);
        cc.Move(velocity * Time.fixedDeltaTime);
      }
    }
}
