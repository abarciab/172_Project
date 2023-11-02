using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpandThenDestroy : MonoBehaviour
{
    public float speed, lifeTime;
    float maxLife;
    [SerializeField] AnimationCurve speedProfile;
    [SerializeField] float maxScale;
    private bool getBigger = true;
    //[SerializeField] Material goopExploMat;
    //[SerializeField] Material goopBlackMat;

    private void Start()
    {
        maxLife = lifeTime;
    }
    private void Update()
    {
        lifeTime -= Time.deltaTime;
        transform.Rotate(0,2,0, Space.Self);

        if (lifeTime <= 0)
        {
            //goopExploMat.color = new Color(goopExploMat.color.r, goopExploMat.color.g, goopExploMat.color.b, 255f);
            Destroy(gameObject);
        }

        float progress = 1 - (lifeTime / maxLife);
        if (transform.localScale.x <= maxScale && lifeTime >= maxLife/2)
        {
            transform.localScale += transform.localScale * speedProfile.Evaluate(progress) * speed * Time.deltaTime;
        }
        else if(transform.localScale.x > 0 && lifeTime <= maxLife/2)
        {
            transform.localScale -= transform.localScale * speed * Time.deltaTime;
        }
        {//Destroy(gameObject);
        }
        //if (goopExploMat.color.a > 0) 
        //goopExploMat.color = new Color(goopExploMat.color.r, goopExploMat.color.g, goopExploMat.color.b, goopExploMat.color.a - .01f);
    }
}
