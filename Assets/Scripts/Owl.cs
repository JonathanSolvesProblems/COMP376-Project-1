﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Owl : MonoBehaviour
{
    [Header("Owl")]
    public float hitPoints;
    public float moveSpeed;
    public float touchDamage;
    public LayerMask playerLayer;

    protected GameObject player = default;

    private Animator animator = default;

    private const string k_owlHitAnim = "OwlHit";
    
    private RoomManager _mRoomManager;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player");
        animator = GetComponent<Animator>();
        _mRoomManager = gameObject.transform.parent.GetComponent<RoomManager>();
    }

    public void ApplyDamage(float damage)
    {
        hitPoints -= damage;

        if (hitPoints <= 0)
        {
            Destroy(gameObject);
        }

        animator.SetTrigger(k_owlHitAnim);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (playerLayer == (playerLayer | (1 << collision.gameObject.layer)))
        {
            PlayerController playerCtrl = collision.gameObject.GetComponent<PlayerController>();
            if (playerCtrl)
            {
                playerCtrl.DamagePlayer(touchDamage);
            }
            else
            {
                Debug.LogError("No playercontroller script on " + player.name);
            }
        }
    }

    public bool IsActive()
    {
        return _mRoomManager.currentRoom;
    }
}
