using UnityEngine;
using UnityEngine.UI;

public class SpinCube : MonoBehaviour {

    /// <summary>Whether the cube should be spun.</summary>
    public bool spinning;

    /// <summary>Speed of the cube's rotation, in degrees per second.</summary>
    public float speed = 90;

    /// <summary>This camera's background color will cycle through the rainbow.</summary>
    public Camera cam;

    /// <summary>
    /// Used by UGUI toggles to set the cube spinning (or not spinning)
    /// </summary>
    /// <param name="button">The toggle to get the cube's new spinning state from.</param>
    public void SetSpinning(Toggle button) {
        spinning = button.isOn;
    }

    /// <summary>
    /// Spins the cube, if spinning is true, and cycles the camera's background color.
    /// </summary>
    void Update() {
        if (spinning)
            transform.Rotate(transform.up * Time.deltaTime * speed, Space.Self); //spin the cube in a complex and somewhat unpredictable way

        float hue = Time.time * 0.1f;
        hue = hue % 1;
        cam.backgroundColor = HSBToRGBConversion(hue, 0.75f, 0.25f); //cycle the background color through a rainbow.
    }

    /// <summary>
    /// Converts HSB to RGB.
    /// </summary>
    public static Color HSBToRGBConversion(float hue, float saturation, float brightness) {
        float red, green, blue;

        if (saturation == 0) {
            red = green = blue = brightness; // achromatic
        } else {
            var q = brightness < 0.5 ? brightness * (1 + saturation) : brightness + saturation - brightness * saturation;
            var p = 2 * brightness - q;
            red = hue2rgb(p, q, hue + 1f / 3);
            green = hue2rgb(p, q, hue);
            blue = hue2rgb(p, q, hue - 1f / 3);
        }
        return new Color(red, green, blue);
    }

    /// <summary>
    /// Used by HSBToRGBConversion.
    /// </summary>
    static float hue2rgb(float p, float q, float t) {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1f / 6) return p + (q - p) * 6 * t;
        if (t < 1f / 2) return q;
        if (t < 2f / 3) return p + (q - p) * (2f / 3 - t) * 6;
        return p;
    }
}
