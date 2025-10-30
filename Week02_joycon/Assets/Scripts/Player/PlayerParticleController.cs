using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class HatMovementParticle
{
    public HatType hatType;
    public ParticleSystem particleSystem;
}

public class PlayerParticleController : MonoBehaviour
{
    [SerializeField] private PlayerHatController playerHatController;

    [SerializeField] private List<HatMovementParticle> hatMovementParticles;

    private Vector2 lastPosition;

    private ParticleSystem _currentMovementParticles;
    private ParticleSystem.EmissionModule _particleEmissionModule;
    private HatType _lastKnownHatType = (HatType)(-1);

    private void Start()
    {
        lastPosition = transform.position;

        if (playerHatController == null)
        {
            playerHatController = GetComponentInParent<PlayerHatController>();
            if (playerHatController == null)
            {
                Debug.LogError("PlayerParticleController: PlayerHatController를 찾을 수 없습니다!");
                enabled = false;
                return;
            }
        }

        foreach (var entry in hatMovementParticles)
        {
            if (entry.particleSystem != null)
            {
                var emissionModule = entry.particleSystem.emission;

                emissionModule.enabled = false;
            }
        }

        _lastKnownHatType = (HatType)(-1);
    }

    private void LateUpdate()
    {
        HatType currentHat = playerHatController.CurrentHatType;
        if (currentHat != _lastKnownHatType)
        {
            UpdateHatParticle(currentHat);
            _lastKnownHatType = currentHat;
        }

        if (_currentMovementParticles == null) return;

        float distanceMoved = Vector2.Distance(lastPosition, transform.position);
        bool isMoving = distanceMoved > 0.01f;

        if (isMoving != _particleEmissionModule.enabled)
        {
            _particleEmissionModule.enabled = isMoving;
        }

        lastPosition = transform.position;
    }

    private void UpdateHatParticle(HatType newHatType)
    {
        if (_currentMovementParticles != null)
        {
            _particleEmissionModule.enabled = false;
        }

        _currentMovementParticles = null;
        foreach (var entry in hatMovementParticles)
        {
            if (entry.hatType == newHatType)
            {
                _currentMovementParticles = entry.particleSystem;
                break;
            }
        }

        if (_currentMovementParticles != null)
        {
            _particleEmissionModule = _currentMovementParticles.emission;
            _particleEmissionModule.enabled = false; 
        }
    }
}