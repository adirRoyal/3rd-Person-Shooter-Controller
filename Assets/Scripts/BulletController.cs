using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BulletController : MonoBehaviour
{

    // Reference to the bullet decal prefab
    [SerializeField] private GameObject bulletDecal;

    // Speed of the bullet
    private float speed = 50f;

    // Time after which the bullet will be destroyed automatically
    private float timeToDestroy = 3f;

    // Target position for the bullet
    public Vector3 target {  get; set; }

    // Flag to check if the bullet has hit a target
    public bool hit {  get; set; }

    private void OnEnable()
    {
        // Schedule destruction of the bullet after a certain time
        Destroy(gameObject, timeToDestroy);
    }

    // Update is called once per frame
    private void Update()
    {
        // Move the bullet towards the target position
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // Check if the bullet is very close to the target and hasn't hit yet
        if (!hit && Vector3.Distance(transform.position, target) < 0.01f)
        {
            Destroy(gameObject);
        }
    }

    // Called when the bullet collides with another object
    private void OnCollisionEnter(Collision collision)
    {
        // Get the point of contact from the collision
        ContactPoint contact = collision.GetContact(0);

        // Instantiate a bullet decal at the point of contact with a slight offset to avoid z-fighting
        Instantiate(bulletDecal, contact.point + contact.normal * 0.0001f, Quaternion.LookRotation(contact.normal));

        // Destroy the bullet on collision
        Destroy(gameObject);
    }
}
