using System;
using UnityEngine;

namespace GMTK
{
    public class Collectable : Interactable
    {
        private MeshRenderer _meshRenderer;
        private bool _hasBeenCollected;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip collectSound;

        void Start()
        {
            SetCollected(false);
        }

        private void Update()
        {
            if (HasBeenInteractedWith != _hasBeenCollected)
            {
                SetCollected(HasBeenInteractedWith);

                if (_hasBeenCollected && audioSource != null && collectSound != null)
                {
                    audioSource.PlayOneShot(collectSound);
                }
            }
        }

        private void SetCollected(bool collected)
        {
            _hasBeenCollected = collected;
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(!collected);
            }
        }
    }
}
