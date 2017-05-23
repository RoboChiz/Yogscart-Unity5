using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Projectile : MonoBehaviour
{

    protected Vector3 direction;
    protected bool actingShield = false;

    public virtual void Setup(Vector3 _direction, bool _actingShield)
    {
        direction = _direction;
        actingShield = _actingShield;
    }
}
