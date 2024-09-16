using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Troop : TowerCore
{
    public float animatedMoveSpeed;

    public int movesPerTurn;
    [HideInInspector]
    public int movesLeft;


    private SpriteRenderer[] moveArrowRenderers;
    public Color[] moveArrowColors;




    protected override void OnSetupTower()
    {
        moveArrowRenderers = selectStateAnim.GetComponentsInChildren<SpriteRenderer>();

        selectStateAnim.transform.rotation = Quaternion.identity;

        if (GodCore.Instance.IsAthena)
        {
            GrantTurn();
        }
    }



    #region Tower Select/Deselect

    protected override void OnSelectTower()
    {
        foreach (var sprite in moveArrowRenderers)
        {
            sprite.color = moveArrowColors[(movesLeft == 0 || canTakeAction == false) ? 1 : 0];
        }
    }
    protected override void OnDeSelectTower()
    {
        
    }
    #endregion


    protected override void OnGrantTurn()
    {
        movesLeft = movesPerTurn;
    }


    protected override IEnumerator AttackTargetAnimation(Vector3 targetPos, float combinedSize, TowerCore target = null)
    {
        float maxDist = Vector3.Distance(transform.position, targetPos) - combinedSize;

        targetPos = VectorLogic.InstantMoveTowards(transform.position, targetPos, maxDist);

        Vector3 towerStartpos = transform.position;
        targetPos.y = towerStartpos.y;


        while (Vector3.Distance(transform.position, targetPos) > 0.0001f)
        {
            yield return null;

            transform.position = VectorLogic.InstantMoveTowards(transform.position, targetPos, animatedMoveSpeed * Time.deltaTime);
        }

        while (Vector3.Distance(transform.position, towerStartpos) > 0.0001f)
        {
            yield return null;

            transform.position = VectorLogic.InstantMoveTowards(transform.position, towerStartpos, animatedMoveSpeed * Time.deltaTime);
        }

        if (target != null)
        {
            target.GetAttacked(dmg, GodCore.Instance.RandomStunChance());
        }
    }


    #region Move Tower

    public void MoveTower(Vector2Int currentGridPos, Vector2Int newGridPos)
    {
        movesLeft -= 1;

        GridManager.Instance.UpdateTowerData(currentGridPos, null);
        GridManager.Instance.UpdateTowerData(newGridPos, this);


        StartCoroutine(MoveTowerAnimation(currentGridPos, newGridPos));

        MoveTower_ServerRPC(currentGridPos, newGridPos);
    }


    [ServerRpc(RequireOwnership = false)]
    private void MoveTower_ServerRPC(Vector2Int currentGridPos, Vector2Int newGridPos, ServerRpcParams rpcParams = default)
    {
        ulong fromClientId = rpcParams.Receive.SenderClientId;
        MoveTower_ClientRPC(fromClientId, currentGridPos, newGridPos);
    }

    [ClientRpc(RequireOwnership = false)]
    private void MoveTower_ClientRPC(ulong fromClientId, Vector2Int currentGridPos, Vector2Int newGridPos)
    {
        if (TurnManager.Instance.localClientId != fromClientId)
        {
            StartCoroutine(MoveTowerAnimation(currentGridPos, newGridPos));
        }
    }

    private IEnumerator MoveTowerAnimation(Vector2Int currentGridPos, Vector2Int newGridPos)
    {
        GridManager.Instance.UpdateTowerData(currentGridPos, null);
        GridManager.Instance.UpdateTowerData(newGridPos, this);


        Vector3 newPos = GridManager.Instance.GetGridData(newGridPos).worldPos;
        newPos = new Vector3(newPos.x, transform.position.y, newPos.z);

        while (Vector3.Distance(transform.position, newPos) > 0.001f)
        {
            yield return null;
            transform.position = VectorLogic.InstantMoveTowards(transform.position, newPos, animatedMoveSpeed * Time.deltaTime);
        }
    }
    #endregion
}