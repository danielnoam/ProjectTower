using System.Collections;
using DNExtensions.Button;
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
                StartCoroutine(DelayedTrigger());
            }
            else
            {
                events?.Invoke();
            }
        }
    }
    
    private IEnumerator DelayedTrigger()
    {
        yield return new WaitForSeconds(triggerDelay);
        events?.Invoke();
    }
    
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
