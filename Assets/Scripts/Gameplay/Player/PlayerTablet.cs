using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTablet : MonoBehaviour
{
    public TextMeshProUGUI textUI;
    public TextMeshProUGUI pageUI;
    public TextMeshProUGUI titleUI;
    public GameObject tabletGO;
    public List<MessageSO> messagesSO;
    public int curPageIdx;
    public int curMessageIdx;
    public MessageSO curMessageSO;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnOpenTablet(InputAction.CallbackContext callbackContext){
        if (callbackContext.started) { 
            tabletGO.SetActive(!tabletGO.activeSelf);
            //curMessageSO = messagesSO[0];
            //titleUI.text = curMessageSO.messageTitle;
            //textUI.text = curMessageSO.messages[0];
            //pageUI.text = "1/" + curMessageSO.messages.Count;
            //curMessageIdx = 0;
            //curPageIdx = 0;
        }
    }

    public void OnNextPage(InputAction.CallbackContext callbackContext) {
        if (callbackContext.started) { 
            if(curPageIdx + 1 < curMessageSO.messages.Count){
                SetMessageText(++curPageIdx);
            }
        }
    }

    public void OnPrevPage(InputAction.CallbackContext callbackContext) {
        if (callbackContext.started) {
            if (curPageIdx - 1 >= 0) {
                SetMessageText(--curPageIdx);
            }
        }
    }

    public void SetMessageText(int idx){
        textUI.text = curMessageSO.messages[idx];
        pageUI.text = idx + 1 + "/" + curMessageSO.messages.Count;
    }

    public void OnExitTablet(InputAction.CallbackContext callbackContext) {
        if (callbackContext.started) {
           
        }
    }

    private void OnEnable() {
        curMessageSO = messagesSO[0];
        titleUI.text = curMessageSO.messageTitle;
        textUI.text = curMessageSO.messages[0];
        pageUI.text = "1/" + curMessageSO.messages.Count;
        curMessageIdx = 0;
        curPageIdx = 0;
    }
}
