using Passer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// HumanoidButton is based on the standard Button class
public class HumanoidButton : Button {

    // The Event taking a HumanoidControl parameter
    [System.Serializable]
    public class HumanoidEvent : UnityEvent<HumanoidControl> { }

    // The onClick event which replaces the standard onClick event
    // This version takes an HumanoidControl parameter
    // The standard does not take a parameter
    public new HumanoidEvent onClick = new HumanoidEvent();

    private void Press(BaseEventData eventData) {
        if (!IsActive() || !IsInteractable())
            return;

        // Get the originator GameObject who clicked the button
        GameObject originator = eventData.currentInputModule.gameObject;
        if (originator == null) {
            Debug.LogError("Could not find the originator for this button click");
            return;
        }

        // Get the humanoid on the originator
        // and check if it exists
        HumanoidControl humanoid = originator.GetComponent<HumanoidControl>();
        if (humanoid == null) {
            Debug.LogError("Could not find the humanoid for this button click");
            return;
        }

        // Call the button click function with the humanoid as parameter
        onClick.Invoke(humanoid);
    }

    // This function is called when the button is clicked
    public override void OnPointerClick(PointerEventData eventData) {
        base.OnPointerClick(eventData);

        Press(eventData);
    }

    // This function is called when the button is activated with the default button
    // (not supported by Humanoid Control, but added for completeness)
    public override void OnSubmit(BaseEventData eventData) {
        base.OnSubmit(eventData);

        Press(eventData);
    }
}
