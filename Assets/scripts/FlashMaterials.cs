using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashMaterials : MonoBehaviour
{
    [SerializeField] Renderer mesh;
    [SerializeField] List<Material> mats;
    [SerializeField] float flashTime;
    float currentCountdown;

    private void Update()
    {
        currentCountdown -= Time.deltaTime;
        if (currentCountdown <= 0) {
            mats.Add(mats[0]);
            mats.RemoveAt(0);
            mesh.material = mats[0];
            currentCountdown = flashTime;
        }
    }
}
