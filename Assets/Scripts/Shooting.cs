using UnityEngine;
using System;
using System.Collections;
using Valve.VR;

public class Shooting : MonoBehaviour
{
    public string fireAction = "GrabPinch";
    public int fireDelay = 1000;
    public int maxAmmo = 10;
    public float explosionForce = 50.0f;
    public string tagFilter = "Target";
    public AudioClip fire;
    public AudioClip noAmmo;

    protected SteamVR_Behaviour_Pose controller;
    private LineRenderer line;
    private DateTime lastFired;
    private AudioSource sound;
    private bool pickedUp;
    private int ammo;

    protected virtual void FireHapticFeedback(bool hasAmmo)
    {
        float duration = 0.2f;
        int frequency = 20;
        float strength = 0.75f;

        if (hasAmmo == false)
        {
            duration = 0.1f;
            frequency = 1;
            strength = 1f;
        }
        
        SteamVR_Actions.default_Haptic[controller.inputSource].Execute(0, duration, frequency, strength);
        //
        // YOUR HAPTIC FEEDBACK CODE GOES HERE
        //
    }
    
    void Start ()
    {
        sound = GetComponent<AudioSource>();
        line = GetComponent<LineRenderer>();
        lastFired = DateTime.Now;
        pickedUp = false;
        ammo = maxAmmo;
	}

    public void PickUp()
    {
        controller = GetComponentInParent<SteamVR_Behaviour_Pose>();
        pickedUp = true;
    }

    public void PutDown()
    {
        pickedUp = false;
        line.enabled = false;
    }

    void Update ()
    {
        if(pickedUp)
        {
            Ray ray = new Ray(transform.position, transform.TransformDirection(Vector3.forward));
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                GameObject target = hitInfo.collider.gameObject;
                line.SetPosition(0, ray.origin);
                line.SetPosition(1, hitInfo.point);
                line.enabled = true;

                bool pressDown = SteamVR_Input.GetStateDown(fireAction, controller.inputSource);

                TimeSpan elapsed = DateTime.Now - lastFired;
                if (pressDown && elapsed > TimeSpan.FromMilliseconds(fireDelay))
                {
                    if (ammo > 0)
                    {
                        lastFired = DateTime.Now;
                        sound.PlayOneShot(fire);
                        FireHapticFeedback(true);
                        ammo--;

                        if (target.tag == tagFilter)
                        {
                            Rigidbody rb = target.GetComponent<Rigidbody>();
                            rb.AddExplosionForce(explosionForce, hitInfo.point, 1.0f);
                        }
                    }
                    else
                    {
                        sound.PlayOneShot(noAmmo);
                        FireHapticFeedback(false);
                    }
                }
            }
            else
            {
                line.enabled = false;
            }
        }
    }

    void OnTriggerEnter(Collider c)
    {
        if(c.gameObject.tag == "Ammo")
        {
            ammo = maxAmmo;
        }
    }
}
