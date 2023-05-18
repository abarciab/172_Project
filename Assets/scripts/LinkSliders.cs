using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class LinkSliders : MonoBehaviour
{
    [SerializeField] Slider controller, follower;

    private void Update()
    {
        if (controller && follower) follower.value = controller.value;
    }
}
