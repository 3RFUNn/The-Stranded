using NUnit.Framework.Internal;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerTablet : MonoBehaviour
{
    public GameObject tabletGO;
    public List<MessageSO> messagesSO;
    public Popup popup;

    //main page
    [Header("---------main page---------")]
    public GameObject mainPageGO;
    public GameObject titleUIPrefab;
    public RectTransform listParent, viewport;
    public VerticalLayoutGroup vLayout;
    //public List<GameObject> titlesGO;
    public Color colorSelected, colorNormal;
    public float scrollTime;

    [SerializeField]
    private int curSelectionIdx;
    [SerializeField]
    private int _upperBound, _lowerBound;
    private float _childHeight;
    private Coroutine _scrollCoroutine;

    //message page
    [Header("-------message page-------")]
    public GameObject messagePageGO;
    public float displayInterval;
    public TextMeshProUGUI textUI;
    public TextMeshProUGUI pageUI;
    public TextMeshProUGUI titleUI;
    public MessageSO curMessageSO;

    [SerializeField]
    private int _curMessageIdx, _curPageIdx;
    private Coroutine _displayCoroutine;

    private IEnumerator DisplayMessage(){
        if (textUI.text != null) {
            textUI.text = null;
        }
        if(curMessageSO.hasViewed){
            textUI.text = curMessageSO.messages[_curPageIdx];
        }else{
            //only display charater by character when player read the message first time
            for (int i = 0; i < curMessageSO.messages[_curPageIdx].Length; i++) {
                textUI.text += curMessageSO.messages[_curPageIdx][i];
                yield return new WaitForSeconds(displayInterval);
            }
        }

    }

    private IEnumerator Scroll(float dest){
        float y = listParent.anchoredPosition.y;
        float timer = 0;
        while (timer < scrollTime) { 
            timer += Time.deltaTime;
            float t = Mathf.Sin(timer * Mathf.PI / 2 / scrollTime);
            listParent.anchoredPosition = new Vector2(0, Mathf.Lerp(y, dest, t));
            yield return null;
        }
        listParent.anchoredPosition = new Vector2(0, dest);
        _scrollCoroutine = null;
    }



    
    void Start()
    {
        _childHeight = titleUIPrefab.GetComponent<RectTransform>().rect.height;
    }

    void Update()
    {
        
    }

    public void OnOpenTablet(InputAction.CallbackContext callbackContext){
        if (callbackContext.started) {
            if (tabletGO.activeSelf) {
                tabletGO.SetActive(false);
            }else{
                tabletGO.SetActive(true);
            }
            
        }
    }

    public void OnNext(InputAction.CallbackContext callbackContext) {
        if (callbackContext.performed) {
            if (mainPageGO.activeSelf) {
                //next selection in main screen
                SetSelection(curSelectionIdx + 1);
            }
            else if(messagePageGO.activeSelf){
                //next page of the message
                SetMessagePage(_curPageIdx + 1);
            }
            
        }
    }

    public void OnPrev(InputAction.CallbackContext callbackContext) {
        if (callbackContext.performed) {
            if (mainPageGO.activeSelf) {
                SetSelection(curSelectionIdx - 1);
            }
            else if (messagePageGO.activeSelf) {
                SetMessagePage(_curPageIdx - 1);
            }
        }
    }

    public void SetMessagePage(int idx) {
        //check if the idx number is legal
        if (idx >= 0 && idx < curMessageSO.messages.Count) {
            _curPageIdx = idx;
            if (_displayCoroutine != null) { 
                StopCoroutine(_displayCoroutine);
            }
            _displayCoroutine = StartCoroutine(DisplayMessage());
            //the page number is 1-based, while the index is 0-based, so we need to plus 1
            pageUI.text = idx + 1 + "/" + curMessageSO.messages.Count;
        }
    }

    public void SetSelection(int idx){
        //check if the idx number is legal
        if (idx >= 0 && idx < listParent.childCount && _scrollCoroutine == null) {
            listParent.GetChild(curSelectionIdx).GetComponent<Image>().color = colorNormal;
            listParent.GetChild(idx).GetComponent<Image>().color = colorSelected;
            curSelectionIdx = idx;
            //only when the total children height exceeds the viewport it should scroll
            if(listParent.childCount * _childHeight > viewport.rect.height){
                _upperBound = (int)(listParent.anchoredPosition.y / _childHeight);
                _lowerBound = (int)((listParent.anchoredPosition.y + viewport.rect.height) / _childHeight);
                //sometimes the position could become a number very close to but no equal to 0,
                //so we use 0.1f to avoid scrolling up when it has reached the top
                if (idx <= _upperBound && listParent.anchoredPosition.y > 0.1f) {
                    Debug.Log("up");
                    _scrollCoroutine = StartCoroutine(Scroll(listParent.anchoredPosition.y - 100));
                }
                else if (idx >= _lowerBound) {
                    Debug.Log("down");
                    _scrollCoroutine = StartCoroutine(Scroll(listParent.anchoredPosition.y + 100));
                }
            }
            
        }
    }

    //when it's in main screen, jump to message screen and display selected message;
    //when it's in message screen, go back to main screen
    public void OnAction(InputAction.CallbackContext callbackContext) {
        if (callbackContext.started) {
           if (messagePageGO.activeSelf){
                messagePageGO.SetActive(false);
                mainPageGO.SetActive(true);
                //update viewing state both in SO and UI
                curMessageSO.hasViewed = true;
                listParent.GetChild(curSelectionIdx).Find("UnreadMark").gameObject.SetActive(false);
            }
            else if(mainPageGO.activeSelf){
                mainPageGO.SetActive(false);
                messagePageGO.SetActive(true);
                SetMessage(curSelectionIdx);
           }
        }
    }

    public void SetMessage(int idx){
        _curMessageIdx = idx;
        _curPageIdx = 0;
        curMessageSO = messagesSO[idx];
        titleUI.text = curMessageSO.messageTitle;
        pageUI.text = "1/" + curMessageSO.messages.Count;
        if (_displayCoroutine != null){
            StopCoroutine(_displayCoroutine);
        }
        _displayCoroutine = StartCoroutine(DisplayMessage());

    }

    public void AddMessage(MessageSO message){
        GameObject go = Instantiate(titleUIPrefab, listParent);
        LayoutRebuilder.ForceRebuildLayoutImmediate(listParent);
        go.GetComponent<Image>().color = colorNormal;
        var tm = go.GetComponentInChildren<TextMeshProUGUI>();
        tm.text = message.messageTitle;
        message.hasViewed = false;
        //move to first both in GO and List
        go.transform.SetSiblingIndex(0);
        messagesSO.Insert(0, message);

        if (tabletGO.activeSelf) {
            popup.StartCoroutine(popup.DoPopup());
            //increment index by 1 because we added a new message at the head of the list
            curSelectionIdx++;
        }
        else if (!tabletGO.activeSelf) {
            //popup prompt if player aren't holding tablet
            popup.StartCoroutine(popup.DoPopup());
        }
    }

    private void OnEnable() {
        curSelectionIdx = 0;
        listParent.GetChild(0).GetComponent<Image>().color = colorSelected;
    }

    //clear selection color and reset page
    private void OnDisable() {
        listParent.GetChild(curSelectionIdx).GetComponent<Image>().color = colorNormal;
        listParent.anchoredPosition = Vector2.zero;
        mainPageGO.SetActive(true);
        messagePageGO.SetActive(false);
    }

    private void Awake() {
        mainPageGO.SetActive(false);
        messagePageGO.SetActive(true);
        //instaniate message titles in main screen
        foreach (MessageSO message in messagesSO) {
            GameObject go = Instantiate(titleUIPrefab, listParent);
            go.GetComponent<Image>().color = colorNormal;
            var tm = go.GetComponentInChildren<TextMeshProUGUI>();
            tm.text = message.messageTitle;
            //if (message.hasViewed) {
            //    go.transform.Find("UnreadMark").gameObject.SetActive(false);
            //}
            //ScriptableObject will permanently update viewed flag even in play mode.
            //we clear all the viewed flag in SO only for the convenience of testing,
            //in the release we should use above codes.
            message.hasViewed = false;
        }
        curSelectionIdx = 0;
        listParent.GetChild(curSelectionIdx).GetComponent<Image>().color = colorSelected;
    }
}
