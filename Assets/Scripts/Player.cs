using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour {

    private float baseSpeed = 5f;
    private float jumpForce = 2.5f;
    private bool isGrounded;
    private Rigidbody rb;
    private Vector3 jump;

    internal NetworkVariable<int> IsRed = new NetworkVariable<int>();

    
    public override void OnNetworkSpawn() {

        IsRed.OnValueChanged += OnColorChanged;

        rb = GetComponent<Rigidbody>();
        jump = new Vector3(0.0f, 2.0f, 0.0f);
        isGrounded = true;  

        if (IsOwner) InitializeServerRpc();  
        else gameObject.GetComponent<MeshRenderer>().material = GameManager.Instance.materials[IsRed.Value];
    }

    public override void OnNetworkDespawn() {
        IsRed.OnValueChanged -= OnColorChanged;
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
        IsRed.Value = -1;
        transform.position = new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        IsRed.Value = Random.Range(0,2);
    }

    [ServerRpc]
    private void MoveRequestServerRpc(Vector3 direction) {
        if (direction.Equals(Vector3.up)) {
            rb.AddForce(jump * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        } else {
            transform.position += direction * baseSpeed * Time.deltaTime;
        }
        
    }

    void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag("Ground")) isGrounded = true;

    }

    public void OnColorChanged(int previous, int current) {
        if (IsRed.Value > -1)
            gameObject.GetComponent<MeshRenderer>().material = GameManager.Instance.materials[current];
    }

}
