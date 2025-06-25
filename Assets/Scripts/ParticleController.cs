using UnityEngine;

/// <summary>
/// Controller to dynamically adjust particle system settings
/// </summary>
public class ParticleController : MonoBehaviour
{
    [Header("Particle System Settings")]
    [SerializeField] private ParticleSystem targetParticleSystem;
    
    [Header("Particle Count Settings")]
    [SerializeField] private int maxParticles = 1000;
    [SerializeField] private float emissionRate = 10f;
    [SerializeField] private float particleLifetime = 5f;
    
    [Header("Control Options")]
    [SerializeField] private bool updateInRealTime = false;
    
    private void Start()
    {
        // Get particle system if not assigned
        if (targetParticleSystem == null)
        {
            targetParticleSystem = GetComponent<ParticleSystem>();
        }
        
        if (targetParticleSystem == null)
        {
            Debug.LogError("ParticleController: No particle system found!");
            return;
        }
        
        UpdateParticleSettings();
    }
    
    private void Update()
    {
        if (updateInRealTime)
        {
            UpdateParticleSettings();
        }
    }
    
    /// <summary>
    /// Update particle system settings with current values
    /// </summary>
    public void UpdateParticleSettings()
    {
        if (targetParticleSystem == null) return;
        
        // Update max particles
        var main = targetParticleSystem.main;
        main.maxParticles = maxParticles;
        main.startLifetime = particleLifetime;
        
        // Update emission rate
        var emission = targetParticleSystem.emission;
        emission.rateOverTime = emissionRate;
        
        Debug.Log($"Particle settings updated: Max={maxParticles}, Rate={emissionRate}, Lifetime={particleLifetime}");
    }
    
    /// <summary>
    /// Increase particle count by a specific amount
    /// </summary>
    /// <param name="amount">Amount to increase by</param>
    public void IncreaseParticleCount(int amount)
    {
        maxParticles += amount;
        emissionRate += amount * 0.1f; // Increase emission rate proportionally
        UpdateParticleSettings();
    }
    
    /// <summary>
    /// Set particle count to a specific value
    /// </summary>
    /// <param name="count">New particle count</param>
    public void SetParticleCount(int count)
    {
        maxParticles = count;
        UpdateParticleSettings();
    }
    
    /// <summary>
    /// Double the current particle count
    /// </summary>
    public void DoubleParticleCount()
    {
        maxParticles *= 2;
        emissionRate *= 1.5f;
        UpdateParticleSettings();
    }
    
    /// <summary>
    /// Reset to default values
    /// </summary>
    public void ResetToDefault()
    {
        maxParticles = 1000;
        emissionRate = 10f;
        particleLifetime = 5f;
        UpdateParticleSettings();
    }
    
    // Context menu methods for testing in editor
    [ContextMenu("Increase Particles (+500)")]
    private void TestIncrease()
    {
        IncreaseParticleCount(500);
    }
    
    [ContextMenu("Double Particles")]
    private void TestDouble()
    {
        DoubleParticleCount();
    }
    
    [ContextMenu("Reset Particles")]
    private void TestReset()
    {
        ResetToDefault();
    }
}
