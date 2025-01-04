using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MessageText", menuName = "ScriptableObject/MessageText")]
public class MessageSO : ScriptableObject
{
    public string messageTitle;
    public bool hasViewed;
    [TextArea(5, 15)]
    public List<string> messages;
}
