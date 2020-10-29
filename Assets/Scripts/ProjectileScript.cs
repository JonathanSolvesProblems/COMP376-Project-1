﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    void Start()
    {
        Physics2D.IgnoreLayerCollision(14, 14); // projectiles ignore collision with each other
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject); // destroy the projectile if they hit something with a collider
    }
}
