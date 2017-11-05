using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basically the Star from Mario Kart
/// </summary>
public class Chilli : MonoBehaviour
{
    KartMovement parent;
    KartCollider parentCollider;

    const float chilliTime = 15f, pulseTime = 2f;

	// Use this for initialization
	void Start ()
    {
        parent = transform.parent.GetComponent<KartMovement>();
        parent.Boost(chilliTime, KartMovement.BoostMode.Chilli);

        parentCollider = parent.GetComponent<KartCollider>();
        parentCollider.godMode = true;

        StartCoroutine(DoChilli());
    }
	
	private IEnumerator DoChilli()
    {
        //Store Old Materials
        Dictionary<GameObject, Material> oldMaterials = new Dictionary<GameObject, Material>();

        foreach (MeshRenderer mr in parent.gameObject.GetComponentsInChildren<MeshRenderer>())
        {
            oldMaterials.Add(mr.gameObject, new Material(mr.material));
        }

        foreach (SkinnedMeshRenderer mr in parent.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            oldMaterials.Add(mr.gameObject, new Material(mr.material));
        }

        //Swap Chilli to Chilli Material
        foreach (MeshRenderer mr in parent.gameObject.GetComponentsInChildren<MeshRenderer>())
        {
            Texture baseTexture = mr.material.GetTexture("_MainTex");
            Texture normalTexture = mr.material.GetTexture("_BumpMap");

            mr.material = new Material(Resources.Load<Material>("Materials/Chilli"));
            mr.material.SetTexture("_MainTex", baseTexture);
            mr.material.SetTexture("_BumpMap", normalTexture);
        }

        foreach (SkinnedMeshRenderer mr in parent.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            Texture baseTexture = mr.material.GetTexture("_MainTex");
            Texture normalTexture = mr.material.GetTexture("_BumpMap");

            mr.material = new Material(Resources.Load<Material>("Materials/Chilli"));
            mr.material.SetTexture("_MainTex", baseTexture);
            mr.material.SetTexture("_BumpMap", normalTexture);
        }

        //Do Effect
        float startTime = Time.time;
        while(Time.time - startTime < chilliTime)
        {
            float amount = Mathf.Abs(Mathf.Sin((Time.time - startTime) * pulseTime)) * 2f;

            //Swap Chilli to Chilli Material
            foreach (MeshRenderer mr in parent.gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                mr.material.SetFloat("_FresnelBrightness", amount);
            }

            foreach (SkinnedMeshRenderer mr in parent.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                mr.material.SetFloat("_FresnelBrightness", amount);
            }

            yield return null;
        }

        //Swap back to previous Material
        foreach (MeshRenderer mr in parent.gameObject.GetComponentsInChildren<MeshRenderer>())
        {
            mr.material = oldMaterials[mr.gameObject];
            oldMaterials.Remove(mr.gameObject);
        }

        foreach (SkinnedMeshRenderer mr in parent.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            mr.material = oldMaterials[mr.gameObject];
            oldMaterials.Remove(mr.gameObject);
        }

        parentCollider.godMode = false;

        Destroy(gameObject);
    }
}
