using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "New Conversation", menuName = "Conversations")]
public class Conversation : ScriptableObject
{
    [TextArea(3, 5)]
    public List<string> Lines;
}
