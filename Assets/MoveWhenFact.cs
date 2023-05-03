using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWhenFact : MonoBehaviour
{
    [System.Serializable]
    public class Item
    {
        public Fact fact;
        public GameObject obj;
        public Quaternion rotation;
        public Vector3 Pos;
        public bool set;
    }

    void OnValidate() {
        foreach (var i in items) {
            if (!i.set || !i.obj) return;
            i.set = false;
            i.rotation = i.obj.transform.rotation;
            i.Pos = i.obj.transform.position;
        }
    }

    [SerializeField] List<Item> items = new List<Item>();

    private void Update() {

        for (int i = 0; i < items.Count; i++) {
            var item = items[i];
            if (!item.obj || !FactManager.i.IsPresent(item.fact)) continue;
            item.obj.transform.position = item.Pos;
            item.obj.transform.rotation = item.rotation;
            items.RemoveAt(i);
        }
    }
}
