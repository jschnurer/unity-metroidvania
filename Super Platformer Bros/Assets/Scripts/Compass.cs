using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Compass : MonoBehaviour {
    public float blinkDuration;
    Image imgComponent;

	public void Start () {
        imgComponent = GetComponent<Image>();
	}

    private void Update()
    {
        var c = imgComponent.color;
        float lerp = Mathf.PingPong(Time.unscaledTime, blinkDuration) / blinkDuration;
        var alpha = Mathf.Lerp(0.0f, 1f, Mathf.SmoothStep(0.0f, 1f, lerp));
        c.a = alpha;
        imgComponent.color = c;
    }
}
