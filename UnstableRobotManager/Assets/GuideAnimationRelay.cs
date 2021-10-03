using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideAnimationRelay : MonoBehaviour
{

    public ArmController arm;


    public void ReleaseArmMoment()
    {
        arm.GuideAnimFinished();
    }
}
