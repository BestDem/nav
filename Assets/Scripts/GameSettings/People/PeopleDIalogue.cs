using System;
using UnityEngine;
using UnityEngine.UI;

public class PeopleDIalogue : IPeople
{
    private PassengerDialogue dialogue;
    private Renderer[] bodyRenderers;
    private GUIStyle timerStyle;

    private void Start()
    {
        haveTicket = true;
        dialogue = GetComponent<PassengerDialogue>();
        bodyRenderers = GetComponentsInChildren<Renderer>();
        // Replace the flat world-space image, which disappears when seen edge-on.
        foreach (var image in GetComponentsInChildren<Image>(true)) image.enabled = false;
    }

    private void OnGUI()
    {
        if (dialogue == null || !dialogue.HasScenario || dialogue.IsConcluded ||
            DialogueUI.IsScenarioVisible || Time.timeScale <= 0) return;
        var camera = Camera.main;
        if (camera == null) return;
        var head = transform.position + Vector3.up * 1.8f;
        if (bodyRenderers != null && bodyRenderers.Length > 0)
        {
            var bounds = bodyRenderers[0].bounds;
            foreach (var body in bodyRenderers) bounds.Encapsulate(body.bounds);
            head = new Vector3(bounds.center.x, bounds.max.y + .2f, bounds.center.z);
        }
        var point = camera.WorldToScreenPoint(head);
        if (point.z <= 0 || point.z > 15 || point.x < 0 || point.x > Screen.width ||
            point.y < 0 || point.y > Screen.height) return;
        if (timerStyle == null)
        {
            timerStyle = new GUIStyle(GUI.skin.box) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
            timerStyle.normal.textColor = Color.white;
        }
        var scenario = dialogue.Scenario;
        string label = "Есть просьба";
        if (string.IsNullOrEmpty(scenario.decisionReceivedAt) && scenario.decisionTimeLimitSeconds > 0)
        {
            double remaining = scenario.decisionTimeLimitSeconds;
            if (DateTimeOffset.TryParse(scenario.decisionDeadlineAt, out var deadline))
                remaining = Math.Max(0, (deadline - DateTimeOffset.UtcNow).TotalSeconds);
            int seconds = (int)Math.Ceiling(remaining);
            label = $"Просьба · {seconds / 60:00}:{seconds % 60:00}";
        }
        var rect = new Rect(point.x - 85, Screen.height - point.y - 38, 170, 34);
        GUI.Box(rect, label, timerStyle);
        var oldColor = GUI.color;
        GUI.color = dialogue.TimerFraction < .25f ? new Color(1, .35f, .25f) : new Color(.2f, .7f, 1);
        GUI.DrawTexture(new Rect(rect.x + 4, rect.yMax - 4, (rect.width - 8) * dialogue.TimerFraction, 3), Texture2D.whiteTexture);
        GUI.color = oldColor;
    }

    public override void Use()
    {
        if (dialogue != null && dialogue.CanInteract) DialogueUI.OpenPassenger(dialogue);
    }
}
