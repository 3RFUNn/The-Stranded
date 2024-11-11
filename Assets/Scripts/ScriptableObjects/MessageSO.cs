using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MessageText", menuName = "ScriptableObject/MessageText")]
public class MessageSO : ScriptableObject
{
    public string messageTitle;
    [TextArea(5, 17)]
    public List<string> messages;
}
