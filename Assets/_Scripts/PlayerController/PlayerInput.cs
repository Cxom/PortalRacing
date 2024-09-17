using FishNet.Object;
using Units;
using UnityEngine;

[RequireComponent(typeof(PlayerBasicRigidbodyMotor))]
public class PlayerInput : NetworkBehaviour
{
    PlayerBasicRigidbodyMotor _motor;

    // TODO experiment with input. try
    //  - disabling physics autosimulation, doubling the fixedupdate loop, polling every fixed update and Physics.Simulate-ing every other fixed update (new input system better supports polling on fixed update)
    //  - using rewired and seeing if it supports framerate independent input/realtime input
    
    //  - in theory, we only need inputs as frequently as we have physics for determinism.
    //  - but for ideal competitiveness, mouse angle would also be processed on fixed update for shooting
    //  - update, I almost said fuck it use fixed update, but fixed update is just guaranteed to be fixed within the
    //     temporal context of the physics simulation. We have no guarantee at all of its actual running frequency!
    //     This fact totally nullifies the first proposed option and roughly does in this one as well.
    //     The only real way around it is to run input polling on a separate thread!
    //     For simplicity, we might as well just run in update, process in fixed update, and if we're concerned about
    //     1-frame input delay caused by full decoupling from separate components, than just run in the same loop and
    //     call the corresponding action as needed. Code indirection but singular update loops.
    //     Oh? What about running in LateUpdate? Closer to top of next frame?
    //     Eh, you know what? easiest solution:
    //      Just don't decouple input.
    //     Update 2 - looks like the new input system actually supports async events https://discussions.unity.com/t/input-system-update/685526/4
    
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        // _motor = GetComponent<PlayerBasicRigidbodyMotor>();
    }

    void Update()
    {
        
        
    }

}
