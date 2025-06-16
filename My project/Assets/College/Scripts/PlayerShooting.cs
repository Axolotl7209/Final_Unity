using System.Collections;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Настройки стрельбы")]
    [Min(0.1f)] public float fireRate = 0.5f;
    [Min(1)] public int damagePerShot = 25;
    [Min(1f)] public float range = 100f;
    [Min(0.1f)] public float shotWidth = 0.1f;
    public LayerMask shootableMask;

    [Header("Эффекты")]
    public ParticleSystem muzzleFlash;
    public GameObject hitEffect;
    public Camera playerCamera;
    public LineRenderer bulletTrail;
    public float trailDuration = 0.1f;

    private float nextFireTime;
    private AudioSource gunAudio;

    private void Start()
    {
        gunAudio = GetComponent<AudioSource>();
        if (bulletTrail != null) bulletTrail.enabled = false;
    }

    private void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        if (muzzleFlash != null) muzzleFlash.Play();
        if (gunAudio != null) gunAudio.Play();

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 endPoint = playerCamera.transform.position + playerCamera.transform.forward * range;

        RaycastHit hit;
        if (Physics.SphereCast(ray, shotWidth, out hit, range, shootableMask))
        {
            endPoint = hit.point;

            // Проверяем, что попали во врага
            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damagePerShot);
                    Debug.Log($"Enemy hit! Damage: {damagePerShot}");
                }
            }

            if (hitEffect != null)
            {
                GameObject effect = Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(effect, 2f);
            }
        }

        StartCoroutine(ShowBulletTrail(endPoint));
    }

    private IEnumerator ShowBulletTrail(Vector3 endPoint)
    {
        if (bulletTrail == null) yield break;

        bulletTrail.enabled = true;
        bulletTrail.SetPosition(0, playerCamera.transform.position);
        bulletTrail.SetPosition(1, endPoint);

        yield return new WaitForSeconds(trailDuration);

        bulletTrail.enabled = false;
    }
}