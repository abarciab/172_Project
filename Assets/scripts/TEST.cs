using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST : MonoBehaviour 
{
    [ButtonMethod]
    private void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

}
