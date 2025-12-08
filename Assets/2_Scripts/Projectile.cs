using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    private bool _isInitialized;
    private Vector3 _moveDirection;
    private float _moveSpeed;


    private void OnCollisionEnter(Collision other)
    {  
        DestroyProjectile();
    }

    private void Update()
    {
        if (!_isInitialized) return;

        transform.position += _moveDirection * (_moveSpeed * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(_moveDirection);
    }


    public void Initialize(Vector3 moveDirection, float moveSpeed)
    {
        _moveDirection = moveDirection;
        _moveSpeed = moveSpeed;
        _isInitialized = true;
    }


    private void DestroyProjectile()
    {
        _isInitialized = false;
        Destroy(gameObject);
    }
}
