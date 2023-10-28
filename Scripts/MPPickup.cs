
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Components;

namespace llaser.MultiPickup
{
    /// <summary>
    /// Object to be picked up by MPPicker
    /// </summary>
    public class MPPickup : UdonSharpBehaviour
    {
        // transform.positionなどの変更はTeleportToの使用を推奨
        [SerializeField]
        private Transform _originalParent;

        [SerializeField]
        private bool _dontControlKinematic;

        private bool _initialized = false;
        private VRCObjectSync _objSync;
        private VRCPickup _pickup;
        private Rigidbody _rigidbody;
        private bool _isKinematicAtInit;

        private void Start()
        {
            if (!_initialized) Initialize();
        }

        private void Initialize()
        {
            _objSync = GetComponent<VRCObjectSync>();
            _pickup = GetComponent<VRCPickup>();
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody)
            {
                _isKinematicAtInit = _rigidbody.isKinematic;
            }
            if (!_originalParent) { _originalParent = transform.parent; }
            _initialized = true;
        }

        public override void OnPickup() => RevertParent();

        public override bool OnOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner)
        {
            if (Utilities.IsValid(requestedOwner) && !requestedOwner.isLocal)
            {
                // when localplayer has lost ownership
                RevertParent();
            }
            return true;
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            // when localplayer gets ownership
            if (Utilities.IsValid(player) && player.isLocal)
            {
                RevertRigidbodyParam();
            }
        }

        public void TeleportTo(Transform target, bool keepParent = false)
        {
            if (target == null) { return; }
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            if (!keepParent) { RevertParent(); }
            RevertRigidbodyParam();
            if (_objSync)
            {
                _objSync.TeleportTo(target);
            }
            else
            {
                transform.SetPositionAndRotation(target.position, target.rotation);
            }
        }

        public void TeleportTo(Vector3 position, Quaternion rotation, bool keepParent = false)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            if (!keepParent) { RevertParent(); }
            RevertRigidbodyParam();
            if (_objSync)
            {
                transform.SetPositionAndRotation(position, rotation);
                _objSync.FlagDiscontinuity();
            }
            else
            {
                transform.SetPositionAndRotation(position, rotation);
            }
        }

        public void RevertParent()
        {
            if (!_initialized) { Initialize(); }
            transform.SetParent(_originalParent, true);
        }

        public void RevertRigidbodyParam()
        {
            if (!_rigidbody || _dontControlKinematic) return;
            if (_objSync)
            {
                _objSync.SetKinematic(_isKinematicAtInit);
            }
            else
            {
                _rigidbody.isKinematic = _isKinematicAtInit;
            }
        }

        public void AttachToMPPicker(MPPicker parent)
        {
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }
            if (_pickup && _pickup.IsHeld) { _pickup.Drop(); }
            if (!_dontControlKinematic)
            {
                if (_objSync)
                {
                    _objSync.SetKinematic(true);
                }
                else if (_rigidbody)
                {
                    _rigidbody.isKinematic = true;
                }
            }
            transform.SetParent(parent.transform, true);
        }
    }
}
