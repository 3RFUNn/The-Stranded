using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup : MonoBehaviour
{
    public RectTransform popupUI;
    public float popupStayDuration, popupMoveDuration;
    public Vector2 popupDest;

    public IEnumerator DoPopup() {
        Vector2 origin = popupUI.anchoredPosition;
        float timer = 0;
        while (timer < popupMoveDuration) {
            timer += Time.deltaTime;
            float t = Mathf.Sin(timer * Mathf.PI / 2 / popupMoveDuration);
            popupUI.anchoredPosition = Vector2.Lerp(origin, popupDest, t);
            yield return null;
        }
        //when timer > popupMoveDuration, we directly move it to destination instead of calculating lerp
        popupUI.anchoredPosition = popupDest;
        yield return new WaitForSeconds(popupStayDuration);
        timer = 0;
        while (timer < popupMoveDuration) {
            timer += Time.deltaTime;
            float t = Mathf.Sin(timer * Mathf.PI / 2 / popupMoveDuration);
            popupUI.anchoredPosition = Vector2.Lerp(popupDest, origin, t);
            yield return null;
        }
        popupUI.anchoredPosition = origin;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartPopup(){
        StartCoroutine(DoPopup());
    }
}
