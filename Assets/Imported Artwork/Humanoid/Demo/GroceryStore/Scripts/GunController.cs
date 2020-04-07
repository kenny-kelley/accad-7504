using System.Collections;
using UnityEngine;
using Passer;

[ExecuteInEditMode]
public class GunController : MonoBehaviour {

    protected HandTarget grabbingHandTarget;
    public Light nozzleFlash;
    public AudioSource fireAudio;
    public InteractionPointer interactionPointer;

    protected virtual void Awake() {
        if (nozzleFlash != null)
            nozzleFlash.enabled = false;
    }

    public virtual void Fire() {
        StartCoroutine(NozzleFlash());

        if (fireAudio != null)
            fireAudio.Play();

        Shoot();
    }

    protected IEnumerator NozzleFlash() {
        if (nozzleFlash == null)
            yield return null;

        nozzleFlash.enabled = true;
        yield return new WaitForSeconds(0.1F);
        nozzleFlash.enabled = false;
    }

    protected virtual void Shoot() {
        if (interactionPointer == null)
            return;

        GameObject target = interactionPointer.objectInFocus;
        if (target == null)
            return;

        Rigidbody targetRigidbody = target.GetComponent<Rigidbody>();
        if (targetRigidbody == null)
            return;

        Vector3 hitPoint = interactionPointer.focusPointObj.transform.position;
        Vector3 forceDirection = interactionPointer.transform.forward;
        targetRigidbody.AddForceAtPosition(forceDirection * 10, hitPoint, ForceMode.Impulse);
    }

    public void OnGrabbed(HandTarget handTarget) {
        grabbingHandTarget = handTarget;

        HumanoidControl humanoid = handTarget.humanoid;
        if (humanoid == null)
            return;

        if (interactionPointer != null)
            interactionPointer.active = true;

        ControllerInput controllerInput = humanoid.GetComponent<ControllerInput>();
        if (controllerInput == null)
            return;

        if (handTarget.isLeft)
            controllerInput.leftTrigger1Input.SetMethod(Fire, InputEvent.EventType.Start);
        else
            controllerInput.rightTrigger1Input.SetMethod(Fire, InputEvent.EventType.Start);

    }

    public void OnLetGo() {
        if (grabbingHandTarget == null)
            return;

        HumanoidControl humanoid = grabbingHandTarget.humanoid;
        if (humanoid == null)
            return;

        if (interactionPointer != null)
            interactionPointer.active = false;

        ControllerInput controllerInput = humanoid.GetComponent<ControllerInput>();
        if (controllerInput == null)
            return;

        if (grabbingHandTarget.isLeft)
            controllerInput.leftTrigger1Input.SetMethod((InputEvent.FloatMethod)null, InputEvent.EventType.None);
        else
            controllerInput.rightTrigger1Input.SetMethod((InputEvent.FloatMethod)null, InputEvent.EventType.None);
    }
}
