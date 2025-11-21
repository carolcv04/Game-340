using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public class ConnectUIScript : NetworkBehaviour
{
    [SerializeField] public Button hostButton;
    [SerializeField] public Button clientButton;
    [SerializeField] private TextMeshProUGUI playerCount;

    private NetworkVariable<int> playersNum = new NetworkVariable<int>();
    private void Start()
    {
        hostButton.onClick.AddListener(HostButtonOnClick);
        clientButton.onClick.AddListener(ClientButtonOnClick);
    }

    private void HostButtonOnClick()
    {
        NetworkManager.Singleton.StartHost();
    }
    private void ClientButtonOnClick()
    {
        NetworkManager.Singleton.StartClient();
    }

    private void Update()
    {
        if (!IsOwner) return;
        playersNum.Value = NetworkManager.Singleton.ConnectedClients.Count;
        playerCount.text = playersNum.Value.ToString();
    }
    
}
