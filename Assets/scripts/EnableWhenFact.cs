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
        public bool state = true;
        public bool invert = false;
    }

    [SerializeField] List<Item> items = new List<Item>();

    private void Update()
    {

        foreach (var i in items) if (FactManager.i.IsPresent(i.fact) == i.state) i.obj.SetActive(!i.invert);

        for (int i = 0; i < items.Count; i++) {
            if ( items[i].obj.activeInHierarchy != items[i].invert) items.RemoveAt(i);
        }
    }
}
