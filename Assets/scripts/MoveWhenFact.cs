using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
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
        for (int i = 0; i < items.Count; i++) {
            var item = items[i];
            if (!item.set || !item.obj) continue;
            item.set = false;
            item.rotation = item.obj.transform.rotation;
            item.Pos = item.obj.transform.position;
        }
    }

    [SerializeField] List<Item> items = new List<Item>();
    [SerializeField] Sound moveSound;

    private void Start()
    {
        if (moveSound) moveSound = Instantiate(moveSound);
    }

    private void Update() {

        if (!Application.isPlaying) return;

        for (int i = 0; i < items.Count; i++) {
            var item = items[i];
            if (!item.obj || !FactManager.i.IsPresent(item.fact)) continue;
            item.obj.transform.position = item.Pos;
            item.obj.transform.rotation = item.rotation;
            items.RemoveAt(i);
            if (moveSound) moveSound.Play(item.obj.transform);
        }
    }
}
