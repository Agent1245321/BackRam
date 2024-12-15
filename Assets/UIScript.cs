using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIScript : MonoBehaviour
{
    public PlayerScript playerScript;
    public TextMeshProUGUI velocityText;
    public TextMeshProUGUI jumpChargeText;
    public TextMeshProUGUI slideChargeText;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        velocityText.text = $"Velocity:{Mathf.Round(playerScript.zVelocity * 100f) / 100f}";
        jumpChargeText.text = $"Jump Charge:{Mathf.Round(playerScript.jumpCharge * 100f) / 100f} / {Mathf.Round(playerScript.maxJump * 100f) / 100f}";//jump charge / max jump
        slideChargeText.text = $"Slide Charge:{Mathf.Round(playerScript.slideCharge * 100f) / 100f} / {Mathf.Round(playerScript.slideLength * 100f) / 100f}";//slide charge / slide length
    }
}
