using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour {

    private float baseSpeed = 5f;
    private float jumpForce = 2.5f;
    private bool isGrounded;
    private Rigidbody rb;
    private MeshRenderer mr;
    private Vector3 jump;
    private float perkSpeed = 1f;
    private NetworkVariable<int> ColorIndex = new NetworkVariable<int>();

    [SerializeField] Material disadvantageColor;
    [SerializeField] Material perkColor;
    
    public override void OnNetworkSpawn() {

        ColorIndex.OnValueChanged += OnColorChanged;

        rb = GetComponent<Rigidbody>();
        mr = GetComponent<MeshRenderer>();
        jump = new Vector3(0.0f, 2.0f, 0.0f);
        isGrounded = true;  

        if (IsOwner) InitializeServerRpc();  
        else mr.material = GameManager.Instance.materials[ColorIndex.Value];
    }

    public override void OnNetworkDespawn() {
        ColorIndex.OnValueChanged -= OnColorChanged;
    }

    void Update() {
        if (IsOwner) {
            if (Input.GetKey(KeyCode.W)) MoveRequestServerRpc(Vector3.forward);
            if (Input.GetKey(KeyCode.S)) MoveRequestServerRpc(Vector3.back);
            if (Input.GetKey(KeyCode.A)) MoveRequestServerRpc(Vector3.left);
            if (Input.GetKey(KeyCode.D)) MoveRequestServerRpc(Vector3.right);
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded) 
                MoveRequestServerRpc(Vector3.up);
        }            
    }

    [ServerRpc]
    void InitializeServerRpc() {
        ColorIndex.Value = -1;  // Así sempre se executará o método "OnColorChanged" de inicio
        transform.position = new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        ColorIndex.Value = Random.Range(0,2);
    }

    [ServerRpc]
    private void MoveRequestServerRpc(Vector3 direction) {
        if (direction.Equals(Vector3.up)) {
            rb.AddForce(jump * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        } else {
            transform.position += direction * baseSpeed * Time.deltaTime * perkSpeed;
        }
        
    }

    [ClientRpc]
    public void SetPerkClientRpc(bool isDisadvantage) {
        if (isDisadvantage) {
            mr.material = disadvantageColor;
            perkSpeed = 0.5f;
        } else {
            mr.material = perkColor;
            perkSpeed = 2f;
        }
    }

    [ClientRpc]
    public void DisallowPerkClientRpc() {
        mr.material = GameManager.Instance.materials[ColorIndex.Value];
        perkSpeed = 1f;
    }

    void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag("Ground")) isGrounded = true;
    }

    public void OnColorChanged(int previous, int current) {
        if (ColorIndex.Value > -1)
            mr.material = GameManager.Instance.materials[current];
    }

}
