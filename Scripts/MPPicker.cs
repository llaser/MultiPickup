
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace llaser.MultiPickup
{
    public class MPPicker : UdonSharpBehaviour
    {
        // 制限: parentを弄る都合上、これをアタッチするgameobjectのscaleの各要素は同じ値でないと、childを回転したときに歪む
        // 最初にMPPickerのchildであった場合、多分変な挙動になる
        // このgameObjectのcolliderはisTrigger=trueにするか、MPPickupとの衝突をしないように設定することを推奨
        // (保持している非kinematicなオブジェクトを取られた際にVRCObjectSyncのallow collision ownership transferが有効だとこのオブジェクトのownerも取られてしまうため)
        [SerializeField]
        private bool _releaseMPPickupOnDrop;

        [SerializeField]
        private BoxCollider _MPPickerBoxCollider;

        [SerializeField]
        private SphereCollider _MPPickerSphereCollider;

        [SerializeField]
        private LayerMask _MPPickupLayer = -1; // everything

        private MPPicker _MPPicker;

        private void Start()
        {
            _MPPicker = GetComponent<MPPicker>();
            if (!_MPPickerBoxCollider && !_MPPickerSphereCollider)
            {
                Debug.LogWarning($"[MultiPickup] _MPPickerCollider is invalid");
            }
        }

        public override void OnDrop()
        {
            if (_releaseMPPickupOnDrop)
            {
                ReleaseAllMPPickups();
            }
        }

        public override bool OnOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner)
        {
            if (Utilities.IsValid(requestedOwner) && !requestedOwner.isLocal)
            {
                // when localplayer loses ownership
                ReleaseAllMPPickups();
            }
            return true;
        }

        public void ReleaseAllMPPickups()
        {
            MPPickup[] pickups = GetComponentsInChildren<MPPickup>(true);
            if (pickups != null)
            {
                foreach (MPPickup p in pickups)
                {
                    if (p && p != _MPPicker)
                    {
                        p.RevertParent();
                        p.RevertRigidbodyParam();
                    }
                }
            }
        }

        public int AttachOverlappingMPPickups() => AttachOverlappingMPPickups<MPPickup>(this);

        public static int AttachOverlappingMPPickups<T>(MPPicker target) where T : MPPickup
        {
            if (!target) { return 0; }
            Collider[] cols = target.GetOverlappingColliders();
            if (cols == null || cols.Length == 0) { return 0; }
            T[] mPPickups = GetComponentsFromColliders<T>(cols);
            if (mPPickups == null) { return 0; }

            int ret = 0;
            Transform thisTransform = target.transform;
            foreach (T p in mPPickups)
            {
                if (!p) { continue; }
                Transform puTransform = p.transform;
                if (puTransform.parent != thisTransform && puTransform != thisTransform)
                {
                    p.AttachToMPPicker(target);
                    ret++;
                }
            }
            return ret;
        }

        public Collider[] GetOverlappingColliders()
        {
            Collider[] cols = null;
            if (_MPPickerBoxCollider)
            {
                Transform colliderTransform = _MPPickerBoxCollider.transform;
                Vector3 colliderCenter = colliderTransform.TransformPoint(_MPPickerBoxCollider.center);
                Vector3 halfExtents = Vector3.Scale(_MPPickerBoxCollider.size, colliderTransform.lossyScale) / 2;
                cols = Physics.OverlapBox(colliderCenter, halfExtents, colliderTransform.rotation, _MPPickupLayer.value);
            }
            else if (_MPPickerSphereCollider)
            {
                Transform colliderTransform = _MPPickerSphereCollider.transform;
                Vector3 colliderCenter = colliderTransform.TransformPoint(_MPPickerSphereCollider.center);
                // spherecolliderのboundsの各要素は2*radius*transform.lossyscaleの要素の最大値
                cols = Physics.OverlapSphere(colliderCenter, _MPPickerSphereCollider.bounds.size.x / 2, _MPPickupLayer.value);
            }
            return cols;
        }

        public static T[] GetComponentsFromColliders<T>(Collider[] colliders) where T : Component
        {
            if (colliders == null) { return new T[0]; }
            T[] tempRet = new T[colliders.Length];
            int i = 0;
            foreach (Collider c in colliders)
            {
                if (c)
                {
                    T comp = c.GetComponent<T>();
                    if (comp)
                    {
                        tempRet[i] = comp;
                        i++;
                    }
                }
            }
            if (i != tempRet.Length)
            {
                T[] ret = new T[i];
                System.Array.Copy(tempRet, ret, i);
                return ret;
            }
            else
            {
                return tempRet;
            }
        }
    }
}
