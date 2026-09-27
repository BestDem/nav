using UnityEngine;

// Compatibility entry point for existing scene references. All passengers use the same UI.
public class VSMDialogueWindow : MonoBehaviour
{
    public static void Open(PassengerDialogue target) { DialogueUI.OpenPassenger(target); }
}
