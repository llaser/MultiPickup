
namespace llaser.MultiPickup
{
    public class MPPickerOnUse : MPPicker 
    {
        public override void OnPickupUseDown()
        {
            base.OnPickupUseDown();
            AttachOverlappingMPPickups();
        }
    }
}
