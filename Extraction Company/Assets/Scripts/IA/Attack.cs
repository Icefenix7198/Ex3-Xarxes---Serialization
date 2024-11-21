using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : State
{
    public override State RunCurrentState()
    {
        return this;
    }
}
