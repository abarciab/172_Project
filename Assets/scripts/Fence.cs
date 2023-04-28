using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class Fence : MonoBehaviour
{
    public Vector3 point1, point2;
    [SerializeField] LineRenderer rope1, rope2;
    public bool makeNew;
    public Fence left, right;
    public int ID;

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.K) && Selection.activeGameObject == gameObject) makeNew = true;

        if (makeNew) {
            makeNew = false;
            if (left != null && right != null) return;
            MakeNew();
        }
        if (rope1 != null && rope2 != null) DrawRopes();
    }

    void MakeNew()
    {
        var newPost = Instantiate(gameObject, transform.position, transform.rotation, transform.parent);
        var fenceScript = newPost.GetComponent<Fence>();
        if (right == null) {
            fenceScript.ID = ID + 1;
            right = fenceScript;
            fenceScript.left = this;
        }
        else if (left == null) {
            fenceScript.ID = ID + 1;
            left = fenceScript;
            fenceScript.right = this;
        }
        newPost.gameObject.name = "fencePost " + fenceScript.ID;
        //Selection.activeGameObject = newPost;
    }

    void DrawRopes()
    {
        if (right != null) {
            rope1.enabled = rope2.enabled = true;

            rope1.SetPosition(0, point1 + transform.position);
            rope1.SetPosition(1, right.point1 + right.transform.position);
            rope2.SetPosition(0, point2 + transform.position);
            rope2.SetPosition(1, right.point2 + right.transform.position);
        } 
        else {
            rope1.enabled = rope2.enabled = false;
        }
    }


}
