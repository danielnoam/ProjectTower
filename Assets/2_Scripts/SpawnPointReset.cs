using DNExtensions.Button;
using UnityEngine;

public class SpawnPointReset : MonoBehaviour
{
    private Vector3 _spawnPosition;
    private Quaternion _spawnRotation;
    private Rigidbody _rb;
    private CharacterController _cc;
    
    private void Awake()
    {
        _spawnPosition = transform.position;
        _spawnRotation = transform.rotation;
        
        _rb = GetComponent<Rigidbody>();
        _cc = GetComponent<CharacterController>();
    }
    
    [Button]
    public void ResetToSpawn()
    {
        if (_rb)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.position = _spawnPosition;
            _rb.rotation = _spawnRotation;
        }
        else if (_cc)
        {
            _cc.enabled = false;
            transform.position = _spawnPosition;
            transform.rotation = _spawnRotation;
            _cc.enabled = true;
        }
        else
        {
            transform.position = _spawnPosition;
            transform.rotation = _spawnRotation;
        }
    }
}
