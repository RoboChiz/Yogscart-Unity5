using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Projectile : MonoBehaviour
{

    protected Vector3 direction;
    protected bool actingShield = false;

    public virtual void Setup(float _direction, bool _actingShield)
    {
        direction = MathHelper.ZeroYPos(transform.forward * _direction);
        actingShield = _actingShield;
    }
}
