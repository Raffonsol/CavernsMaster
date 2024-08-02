using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Vector3 target;
    private bool going = false;
    private Vector3 firingPoint;

    private float lifetime = 0;
    // Start is called before the first frame update
    public void Go()
    {
        // transform.position += transform.right * -0.6f;
        firingPoint = transform.position;
        FixRotation();
        going = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!going) return;
        Run();
        Destroy();
    }
    private void Run () {
        transform.position += transform.up * 7.5f * Time.deltaTime;

       
    }
    private void FixRotation()
    {

        Vector2 moveDirection = (Vector2)target - (Vector2)transform.position;
        moveDirection.Normalize();
        float targetAngle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler (0, 0, targetAngle + 180);
    }   
    private void Destroy() {
        if (going && Vector3.Distance(firingPoint, target) < 0.5f) {
            Destroy(this.gameObject);
        }
        lifetime+=Time.deltaTime;
        if (lifetime > 3) Destroy(this.gameObject);
    }
}
