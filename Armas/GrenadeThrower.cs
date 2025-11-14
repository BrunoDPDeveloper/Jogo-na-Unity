using UnityEngine;

public class GrenadeThrower : MonoBehaviour
{
    [Header("Explosion Prefab")]
    [SerializeField] private GameObject grenadePrefab;

    [Header("Grenade Settings")]
    [SerializeField] private KeyCode throwKey = KeyCode.G;
    [SerializeField] private Transform throwPosition;
    [SerializeField] private Vector3 throwDirection = new Vector3(0,1,0);

    [Header("Grenade Force")]
    [SerializeField] private float chargeTime = 0f;


    private bool isCharging = false;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float maxForce = 20f;
    public Camera mainCamera;

     void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(throwKey))
        {
            StartThrowing();
        }

        if(isCharging)
        {
            chargeThrow();
        }

        if(Input.GetKeyUp(throwKey)) 
            {
                ReleaseThrow();
            }

    }

    void StartThrowing()
    {
        isCharging = true;
        chargeTime = 0f;

        //Trajectory Line
    }

    void chargeThrow()
    {
        chargeTime += Time.deltaTime;

        //Trajectory Line velocity
    }

    void ReleaseThrow()
    {
        ThrowGrenade(Mathf.Min(chargeTime * throwForce, maxForce));
        isCharging = false;

        //Hide Line
    }

    void ThrowGrenade(float force)
    {
        Vector3 spawnPosition = throwPosition.position + mainCamera.transform.forward;

        GameObject grenade = Instantiate(grenadePrefab, spawnPosition, mainCamera.transform.rotation);

        Rigidbody rb = grenade.GetComponent<Rigidbody>();

        Vector3 finalThrowDirection = (mainCamera.transform.forward + throwDirection).normalized;
        rb.AddForce(finalThrowDirection * force, ForceMode.VelocityChange);
    }

}
