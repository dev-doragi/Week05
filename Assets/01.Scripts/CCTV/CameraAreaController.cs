using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraAreaController : MonoBehaviour
{
    public Texture roomBackgroundSprite;
    public Image cameraUIBackground;
    public Color normalColor = Color.black;
    public Color blinkColor = Color.white;
    public float blinkInterval = 0.5f;
    private Coroutine blinkCoroutine;
    public void StartBlinking()
    {
        StopBlinking();
        blinkCoroutine = StartCoroutine(BlinkLoop());
    }

    public void StopBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        if (cameraUIBackground != null)
            cameraUIBackground.color = normalColor;
    }

    private IEnumerator BlinkLoop()
    {
        bool toggle = false;

        while (true)
        {
            cameraUIBackground.color = toggle ? blinkColor : normalColor;
            toggle = !toggle;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}