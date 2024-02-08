using UnityEngine;

public class WeaponProperty : MonoBehaviour
{

  public bool infiniteAmmo;

  public float fireDelay;
  public float reloadTime;
  public float fireTime;
  public float damage;
  public AudioClip fireSound;
  public AudioClip impactSound;
  public bool reloading;
  Animator anim;
  AudioSource audd;

  public int initMag;
  public int initBull;
  public int mag;
  public int bullet;

  public GameObject impact;
  public GameObject blood;

  void Awake()
  {
    mag = initMag;
    bullet = initBull;
    anim = GetComponent<Animator>();
    audd = GetComponent<AudioSource>();
  }

  void FixedUpdate()
  {
    Mathf.Max(fireTime -= Time.fixedDeltaTime,0);
    if (fireTime <= 0 && reloading)
    {
      reloading = false;
      if (bullet < initMag) {
        mag = bullet;
        bullet = 0;
        return;
      }
      bullet -= initMag - mag;
      mag += initMag - mag;
    }
  }

  public void PlayCustom(AudioClip c)
  {
    audd.PlayOneShot(c);
  }

  public void FireMotion() {
      audd.PlayOneShot(fireSound);
      anim.SetTrigger("fire");
  }

  public void Reload()
  {
    if (bullet <= 0 || fireTime > 0 || mag >= initMag)
      return;
    anim.SetTrigger("reload");
    reloading = true;
    fireTime = reloadTime;
  }

  public void Reset() {
    mag = initMag;
    bullet = initBull;
  }
}
