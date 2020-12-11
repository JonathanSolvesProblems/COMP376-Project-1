﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{

    public enum PowerupType
    {
        Attack,
        Health,
        Speed,
        Heal
    }

    [SerializeField] private PowerupType m_powerupType = default;
    [SerializeField] private float m_powerupIncrease = default;
    [SerializeField] private LayerMask m_playerLayer = default;
    [SerializeField] private Canvas m_tooltip = default;

    private void ShowTooltip()
    {
        m_tooltip.gameObject.SetActive(true);
    }

    private void HideTooltip()
    {
        m_tooltip.gameObject.SetActive(false);
    }

	Ray ray;
	RaycastHit2D[] hits;

	private void Update()
	{
		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		hits = Physics2D.RaycastAll(ray.origin, ray.direction, Mathf.Infinity);
		if (hits.Length > 0)
		{
			foreach (RaycastHit2D hit in hits)
			{
				if (hit.collider.gameObject == gameObject)
				{
					ShowTooltip();
					return;
				}
			}
		}

		HideTooltip();
	}

	private void OnTriggerEnter2D(Collider2D collider)
    {
        if (m_playerLayer == (m_playerLayer | (1 << collider.gameObject.layer)))
        {
            PlayerController playerCtrl = collider.gameObject.GetComponent<PlayerController>();
            if (playerCtrl)
            {
                playerCtrl.ChangePlayerStat(m_powerupType, m_powerupIncrease);
            }
            else
            {
                Debug.LogError("No playercontroller script on " + collider.gameObject.name);
            }

            //spawn effect here

            Destroy(gameObject);
        }
    }
}
