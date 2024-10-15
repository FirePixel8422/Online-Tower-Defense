using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class Meteor : NetworkBehaviour
{
    public float moveSpeed;
    public float rotateSpeed;

    public GameObject impactPrefab;

    public ParticleSystem trail;

    public float shrinkDelay;
    public float schrinkSpeed;

    public Transform endPoint;

    private Rigidbody rb;
    private MeshRenderer meshRenderer;



    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();

        endPoint.parent = null;

        Vector3 direction = (endPoint.position - transform.position).normalized;
        var rotation = Quaternion.LookRotation(direction);

        transform.rotation = rotation;

        rb.velocity = transform.forward * moveSpeed;
        rb.AddTorque(new Vector3(Random.Range(-1, 1f), Random.Range(-1, 1f), Random.Range(-1, 1f)) * rotateSpeed);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (IsServer)
        {
            GameObject impactObj = Instantiate(impactPrefab, endPoint.position, Quaternion.identity);

            NetworkObject impactNetwork = impactObj.GetComponent<NetworkObject>();
            impactNetwork.Spawn();

            SyncImpactEffect_ClientRPC(impactNetwork.NetworkObjectId);
        }
        


        meshRenderer.enabled = false;

        trail.Stop();
        Destroy(trail.gameObject, trail.main.duration + trail.main.startLifetime.constantMax);
    }


    [ClientRpc(RequireOwnership = false)]
    private void SyncImpactEffect_ClientRPC(ulong networkObjectId)
    {
        NetworkObject impactNetwork = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];

        StartCoroutine(ShrinkImpactEffectDelay(impactNetwork));
    }


    private IEnumerator ShrinkImpactEffectDelay(NetworkObject impactNetwork)
    {
        yield return new WaitForSeconds(shrinkDelay);

        List<Transform> impactTransforms = new List<Transform>();

        foreach (Transform t in impactNetwork.GetComponentsInChildren<Transform>())
        {
            impactTransforms.Add(t);
        }

        while (impactNetwork.transform.localScale.x > 0)
        {
            yield return null;

            for (int i = 0; i < impactTransforms.Count; i++)
            {
                impactTransforms[i].localScale = VectorLogic.InstantMoveTowards(impactTransforms[i].localScale, Vector3.zero, schrinkSpeed * Time.deltaTime);
            }
        }

        if (IsServer)
        {
            impactNetwork.Despawn();
            //NetworkObject.Despawn();
        }
    }
}