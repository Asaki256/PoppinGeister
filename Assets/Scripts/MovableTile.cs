using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableTile : MonoBehaviour
{
    MeshRenderer mesh;

    bool alphaFlag = true;
    float alphaNum = 0.4f;
    float r, g, b;
    [SerializeField, Range(0, 1)] float alphaParam = 0.01f;

    private void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        mesh.material.color = mesh.material.color - new Color32(0, 0, 0, 0);
        //StartCoroutine("Transparent");

        r = mesh.material.color.r;
        g = mesh.material.color.g;
        b = mesh.material.color.b;

        StartCoroutine("AlphaChange");
    }

    IEnumerator AlphaChange()
    {
        while (true)
        {
            if (alphaFlag)
            {
                alphaNum = alphaNum + alphaParam;
                if (alphaNum >= 0.8f)
                {
                    alphaFlag = false;
                }
            }
            else
            {
                alphaNum = alphaNum - alphaParam;
                if (alphaNum <= 0.4f)
                {
                    alphaFlag = true;
                }
            }

            mesh.material.color = new Color(r, g, b, alphaNum);

            yield return new WaitForSeconds(0.01f);
        }
    }

    private void Update()
    {

    }
}
