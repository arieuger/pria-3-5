using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;

public class GameManager : NetworkBehaviour {

    public static GameManager Instance;
    public List<Material> materials; // 0,1 Colores de inicio - 2,3 vermello e verde

    void Awake() {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    public override void OnNetworkSpawn() {
        if (IsServer) StartCoroutine(PerkCoroutine());
    }

    private IEnumerator PerkCoroutine() {
        while (true) {
            // Espera entre inicialización de perks
            // Recorremos todos os clientes e escollemos un ao azar
            ulong clientId = NetworkManager.Singleton.ConnectedClientsIds[Random.Range(0,NetworkManager.Singleton.ConnectedClientsList.Count)];
            Debug.Log("Client ID: " + clientId);
            yield return new WaitForSeconds(10f);
            
            // Espera duración de perk
            // Cambiamos a cor (e almacenamos a previa para recuperala despois)
            Player selectedPlayer = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId).GetComponent<Player>();
            selectedPlayer.SetPerkClientRpc(Random.Range(0,2) == 0);
            yield return new WaitForSeconds(5f);

            // Recuperamos o estado normal
            selectedPlayer.DisallowPerkClientRpc();
        }
        
    }
    
    private void OnGUI() {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) 
            StartButtons();
        else 
            StatusLabels();

        GUILayout.EndArea();
    }

    private void StartButtons() {
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
        if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
    }

    private void StatusLabels() {
        var mode = NetworkManager.Singleton.IsHost ? "Host" : "Client";
        GUILayout.Label("Transport: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }
}
