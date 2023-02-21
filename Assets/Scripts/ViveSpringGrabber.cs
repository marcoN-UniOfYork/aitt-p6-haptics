using UnityEngine;
using System.Collections;
using Valve.VR;

[RequireComponent(typeof(SpringJoint))]
[RequireComponent(typeof(SteamVR_Behaviour_Pose))]
[RequireComponent(typeof(Collider))]
public class ViveSpringGrabber : Grabber
{
    public string grabAction = "GrabPinch";
    private SpringJoint joint;

    private Coroutine vibration;
    
    new void Start ()
    {
        base.Start();
        joint = GetComponent<SpringJoint>();
    }
	
	protected override void Update ()
    {
        if (joint.connectedBody == null && target != null && SteamVR_Input.GetStateDown(grabAction, controller.inputSource))
        {
            joint.connectedBody = target.GetComponent<Rigidbody>();
        }
        else if (joint.connectedBody != null && SteamVR_Input.GetStateUp(grabAction, controller.inputSource))
        {
            joint.connectedBody = null;
        }

        if(joint.connectedBody != null)
        {
            if(vibration == null) vibration = StartCoroutine(ChestVibration());
            
        }
        
    }

    IEnumerator ChestVibration()
    {
        float lastHingeAngle = joint.connectedBody.gameObject.GetComponent<HingeJoint>().angle;
        float strength = 1f;
        float frequency = 1f;
        while (joint.connectedBody != null)
        {
            float angleDifference = (Mathf.Abs(lastHingeAngle - joint.connectedBody.gameObject.GetComponent<HingeJoint>().angle) > 1f) ? (Mathf.Abs(lastHingeAngle - joint.connectedBody.gameObject.GetComponent<HingeJoint>().angle)) : 0f;

            frequency = Mathf.Lerp(0, 1, angleDifference / 0.1f / 360f);
            strength = frequency;
            Debug.Log(angleDifference);
            SteamVR_Actions.default_Haptic[controller.inputSource].Execute(0, 0.1f, frequency, strength);
            lastHingeAngle = joint.connectedBody.gameObject.GetComponent<HingeJoint>().angle;
            yield return new WaitForSeconds(0.1f);
        }
        
        vibration = null;
    }
}
