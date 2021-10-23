using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SetMeshIDColor : MonoBehaviour
{
    SkinnedMeshRenderer[] renderers;
    MeshFilter[] mfs;
    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
        renderers = GameObject.FindObjectsOfType<SkinnedMeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Color[] cs = new Color[renderers[i].sharedMesh.vertexCount];
            Color _c = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f);
            for (int c = 0; c < cs.Length; c++)
            {
                cs[c] = _c;
            }
            renderers[i].sharedMesh.colors = cs;
        }

        mfs = FindObjectsOfType<MeshFilter>();
        for (int i = 0; i < mfs.Length; i++)
        {
            Color[] cs = new Color[mfs[i].sharedMesh.vertexCount];
            Color _c = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f);
            for (int c = 0; c < cs.Length; c++)
            {
                cs[c] = _c;
            }
            mfs[i].sharedMesh.colors = cs;
        }
    }

    
}