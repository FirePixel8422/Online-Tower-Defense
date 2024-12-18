using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    public bool isMyTurn;
    public ulong localClientId;
    public ulong clientOnTurnId;

    public UnityEvent OnTurnChangedEvent;
    public UnityEvent OnMyTurnEndedEvent;
    public UnityEvent OnMyTurnStartedEvent;

    public GameObject endTurnButton;
    public Animator yourTurnText;



    #region Setup And Start Game

    public override void OnNetworkSpawn()
    {
        localClientId = NetworkManager.LocalClientId;

        endTurnButton.GetComponent<Button>().onClick.AddListener(() => NextTurn_Button());
        endTurnButton.GetComponent<Button>().onClick.AddListener(() => endTurnButton.SetActive(false));


        if (IsServer)
        {
            clientOnTurnId = (ulong)Random.Range(0, 2);

            if (clientOnTurnId == localClientId)
            {
                isMyTurn = true;
                endTurnButton.SetActive(true);
            }
        }
        else
        {
            StartGame_ServerRPC();
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void StartGame_ServerRPC()
    {
        StartGame_ClientRPC(clientOnTurnId);
    }
    [ClientRpc(RequireOwnership = false)]
    private void StartGame_ClientRPC(ulong _clientOnTurnId)
    {
        clientOnTurnId = _clientOnTurnId;

        if (clientOnTurnId == localClientId)
        {
            isMyTurn = true;
            OnMyTurnStartedEvent.Invoke();

            yourTurnText.SetTrigger("YourTurn");
            endTurnButton.SetActive(true);
        }
    }
    #endregion


    #region Grant Next Turn

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            NextTurn_ServerRPC();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            clientOnTurnId = 0;
            isMyTurn = true;
            endTurnButton.SetActive(true);
        }
    }
#endif

    public void NextTurn_Button()
    {
        if (isMyTurn)
        {
            isMyTurn = false;

            NextTurn_ServerRPC();

            OnMyTurnEndedEvent.Invoke();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void NextTurn_ServerRPC()
    {
        ulong nextClientOnTurnId = clientOnTurnId + 1;

        if((int)nextClientOnTurnId == NetworkManager.ConnectedClientsIds.Count)
        {
            nextClientOnTurnId = 0;
        }

        NextTurn_ClientRPC(nextClientOnTurnId);
    }

    [ClientRpc(RequireOwnership = false)]
    private void NextTurn_ClientRPC(ulong nextClientOnTurnId)
    {
        OnTurnChangedEvent.Invoke();

        clientOnTurnId = nextClientOnTurnId;

        if (nextClientOnTurnId == localClientId)
        {
            isMyTurn = true;

            yourTurnText.SetTrigger("YourTurn");
            endTurnButton.SetActive(true);

            OnMyTurnStartedEvent.Invoke();
        }
    }
    #endregion
}
