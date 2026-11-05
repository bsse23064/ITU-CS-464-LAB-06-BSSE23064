using UnityEngine;
using TMPro;

// "Floor is lava" display. Sits on the same object as the lava Hazard and follows its cycle:
//   collider off + no warning  = safe, counting down to the warning
//   warning object on          = lava incoming, floor flashes
//   collider on                = lava, floor is deadly
// Shows a countdown in a TextMeshPro text and tints the floor to match.
[RequireComponent(typeof(Hazard), typeof(Collider))]
public class LavaTimer : MonoBehaviour
{
    [SerializeField] Renderer floor;                  // the floor's Mesh Renderer
    [SerializeField] TMP_Text timerText;
    [SerializeField] Color warnColor = new Color(1f, 0.6f, 0.1f);
    [SerializeField] Color lavaColor = new Color(1f, 0.15f, 0.05f);

    enum Phase { Safe, Warning, Lava }

    Hazard hazard;
    Collider zone;
    GameObject warning;
    Phase phase;
    float phaseStart;
    Color safeColor;

    void Awake()
    {
        hazard = GetComponent<Hazard>();
        zone = GetComponent<Collider>();
        warning = hazard.Warning;
    }

    void Start()
    {
        if (floor != null) safeColor = floor.material.color;   // remember the normal floor colour
        phaseStart = Time.time;
    }

    void Update()
    {
        Phase now = zone.enabled ? Phase.Lava
                  : (warning != null && warning.activeSelf) ? Phase.Warning
                  : Phase.Safe;
        if (now != phase) { phase = now; phaseStart = Time.time; }

        float duration = phase == Phase.Safe ? hazard.RestTime
                       : phase == Phase.Warning ? hazard.WarnTime
                       : hazard.StrikeTime;
        float left = Mathf.Max(0f, duration - (Time.time - phaseStart));
        string t = left.ToString("0.0");

        switch (phase)
        {
            case Phase.Safe:
                Show($"Lava in {t}", Color.white, safeColor);
                break;
            case Phase.Warning:
                Show($"LAVA INCOMING!  {t}", warnColor,
                     Color.Lerp(safeColor, warnColor, Mathf.PingPong(Time.time * 6f, 1f)));   // flashing
                break;
            case Phase.Lava:
                Show($"LAVA! Get on a platform  {t}", lavaColor, lavaColor);
                break;
        }
    }

    void Show(string message, Color textColor, Color floorColor)
    {
        if (timerText != null) { timerText.text = message; timerText.color = textColor; }
        if (floor != null) floor.material.color = floorColor;
    }
}