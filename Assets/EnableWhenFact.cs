using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnableWhenFact : MonoBehaviour
{
    [System.Serializable]
    public class Item
    {
        public Fact fact;
        public GameObject obj;
    }

    [SerializeField] List<Item> items = new List<Item>();

    private void Update()
    {
        foreach (var i in items) if (FactManager.i.IsPresent(i.fact)) i.obj.SetActive(true);

        for (int i = 0; i < items.Count; i++) {
            if (items[i].obj.activeInHierarchy) items.RemoveAt(i);
        }
    }
}
