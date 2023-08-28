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
        public Sound playWhenTrigger;
    }

    [SerializeField] List<Item> items = new List<Item>();

    private void Start()
    {
        foreach (var i in items) if (i.playWhenTrigger) i.playWhenTrigger = Instantiate(i.playWhenTrigger);
    }

    private void Update()
    {
        foreach (var i in items) {
            if (FactManager.i.IsPresent(i.fact) == i.state) {
                i.obj.SetActive(!i.invert);
                if (i.playWhenTrigger) StartCoroutine(PlayIfActive(i.obj, i.playWhenTrigger));
            }
        }
        for (int i = 0; i < items.Count; i++) {
            if ( items[i].obj.activeInHierarchy != items[i].invert) items.RemoveAt(i);
        }
    }

    IEnumerator PlayIfActive(GameObject obj, Sound sound)
    {
        yield return new WaitForSeconds(0.5f);

        if (obj.activeInHierarchy && obj.transform.childCount > 0) {
            sound.Play();
            //print(obj.name + ", playing sound");
        }
    }
}
