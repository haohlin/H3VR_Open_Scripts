using UnityEngine;
using FistVR;

namespace Cityrobo
{
    public class ForceMagazineMountingToMagMountPos : MonoBehaviour
    {
        public FVRFireArm fireArm;

        private bool posChanged;
#if!DEBUG
        public void Update()
        {
            if (posChanged && fireArm.Magazine == null)
            {
                if (posChanged) posChanged = false;
            }
            else if (!posChanged && fireArm.Magazine != null)
            {
                fireArm.Magazine.SetParentage(fireArm.MagazineMountPos.transform);
                posChanged = true;
            }
        }
#endif
    }
}
