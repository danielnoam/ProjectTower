using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class WorldEventTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayerMask triggerLayer;
    [SerializeField] private bool triggerOnce;
    [SerializeField] private bool triggerOnEnter;
    [SerializeField] private bool triggerOnExit;
    [SerializeField, Min(0)] private float triggerDelay;
    
    [Header("Events")]
    [SerializeField] private bool spawnPointReset;
    [SerializeField] private bool reloadScene;
    [SerializeField] private UnityEvent events;
    
    
    private bool _wasTriggered;
    
    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnEnter)
        {
            TryTrigger(other);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (triggerOnExit)
        {
            TryTrigger(other);
        }
    }
    
    private void TryTrigger(Collider other)
    {
        if (triggerOnce && _wasTriggered)
        {
            return;
        }
        
        if (triggerLayer == (triggerLayer | (1 << other.gameObject.layer)))
        {
            _wasTriggered = true;
            
            if (triggerDelay > 0)
            {
                StartCoroutine(DelayedTrigger(other));
            }
            else
            {
                Trigger(other);
            }
        }
    }
    
    private IEnumerator DelayedTrigger(Collider other)
    {
        yield return new WaitForSeconds(triggerDelay);
        Trigger(other);

    }

    private void Trigger(Collider other)
    {
        if (reloadScene)
        {
            ReloadScene();
        }

        if (spawnPointReset)
        {
            ResetToSpawnPoint(other);
        }
        
        events?.Invoke();
    }
    
    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ResetToSpawnPoint(Collider other)
    {
        if (other.TryGetComponent(out SpawnPointReset spawnReset))
        {
            spawnReset.ResetToSpawn();
        }
    }
    
    public void ResetTrigger()
    {
        _wasTriggered = false;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
