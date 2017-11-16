using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpinCube : MonoBehaviour {

    public bool spinning;
    public float speed = 90; //degrees per second

    public Camera cam;

    public void SetSpinning(Toggle button) {
        spinning = button.isOn;
    }

    void Update() {
        if (spinning)
            transform.Rotate(transform.up * Time.deltaTime * speed, Space.Self);

        float hue = Time.time * 0.1f;
        hue = hue % 1;
        cam.backgroundColor = HSBToRGBConversion(hue, 0.75f, 0.25f);
    }

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

    public static float hue2rgb(float p, float q, float t) {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1f / 6) return p + (q - p) * 6 * t;
        if (t < 1f / 2) return q;
        if (t < 2f / 3) return p + (q - p) * (2f / 3 - t) * 6;
        return p;
    }
}
