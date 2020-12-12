﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    public enum Weapon
    {
        Dagger = 0,
        Sword = 1,
        Hammer = 2
    }

    public enum EAbility
    {
        Dash = 0,
        ProjectileExplosion = 1,
        Slowmo = 2
    }

    [Serializable]
    private class PlayerAbilityDefinition
    {
        public EAbility Ability = default;
        public float Cooldown = 1;
    }

    [Header("References")]
    [SerializeField] private Image m_healthBar = default;
    [SerializeField] private Image m_cooldownReady = default;
    [SerializeField] private GameObject m_menu = default;
    [SerializeField] private GameObject m_gameOverMenu = default;
    [SerializeField] private LayerMask m_enemyProjectiles = default;
    [SerializeField] private PlayerWeapon[] m_weapons = default;
    [SerializeField] private PlayerProjectile[] m_projectiles = default;
    [SerializeField] private PlayerAbilityDefinition[] m_playerAbilities = default;
    [SerializeField] private TimeSpeedChanger m_timeSpeedChanger = default;
    [SerializeField] private Image imageMeleeCooldown, imageRangedCooldown, imageAbilityCooldown;

    [Header("Player Attributes")]
    [SerializeField] private Weapon m_chosenWeapon = default;
    [SerializeField] private EAbility m_chosenAbility = default;
    [SerializeField] private PlayerProjectile m_chosenProjectile = default;
    [SerializeField] private float m_meleeDamageModifier = 0f;
    [SerializeField] private float m_rangedDamageModifier = 0f;
    [SerializeField] private float m_maxHealth = 5f;
    [SerializeField] private float m_currentHealth = 5f;
    [SerializeField] private float m_walkSpeed = 1f;
    [SerializeField] private float m_dashForce = 5f;
    [SerializeField] private float m_dashInvincibilityTime = 0.5f;
    [SerializeField] private float m_slowmoDuration = 2f;
    [SerializeField] private float m_slowmoTimeSpeed = 0.5f;
    [SerializeField] private float m_invincibilityCooldown = 0.5f;

    private Rigidbody2D m_rigidbody = default;
    private Animator m_animator = default;
    private PlayerWeapon m_weapon = default;
    private float invincibilityTime = 0;

    private float attackCooldownTime = 0;
    private float projectileCooldownTime = 0;

    //Abilities
    private float abilityCooldownTime = 0;

    //Dash
    private bool dashInvincible = false;
    private float dashInvincibleTime = 0.5f;
    private Vector2 dashDirection = default;

    private UnityEvent restartEvent = default;

    //Animation const strings
    private const string k_attackAnim = "Attack";
    private const string m_playerHitAnim = "PlayerHit";
    private const string k_playerAtkSpeed = "SwingSpeed";

    //Input const strings
    private const string k_fireButton = "Fire";
    private const string k_projectileButton = "FireRanged";
    private const string k_horizontalAxis = "Horizontal";
    private const string k_verticalAxis = "Vertical";

    private AudioManager audioManager = null;

    // idle checking
    private float lastAction = 0;
    private bool isIdle = false;
    private static int IDLE_TIMEOUT = 20;
    
    private void ReportAction()
    {
        lastAction = Time.time;
    }

    // updates and sets boolean for if player is considered idle
    private bool IsIdle()
    {
        if (!isIdle && Time.time - lastAction > IDLE_TIMEOUT)
        {
            // its been 20 seconds
            isIdle = true;
            lastAction = Time.time; // for simplicity, reset the idle timer here
        } else {
            isIdle = false;
        }
        return isIdle;
    }

    public PlayerWeapon GetChosenWeapon()
    {
        return this.m_weapons[(int)this.m_chosenWeapon];
    }

    public PlayerProjectile GetChosenProjectile()
    {
        return this.m_chosenProjectile;
    }

    public float GetCurrentHealth()
    {
        return this.m_currentHealth;
    }

    public float GetMaxHealth()
    {
        return this.m_maxHealth;
    }

    public float GetWalkSpeed() {
        return this.m_walkSpeed;
    }

    public float GetAttackCooldownTime()
    {
        return this.attackCooldownTime;
    }

    public float GetProjectileCooldownTime()
    {
        return this.projectileCooldownTime;
    }

    public float GetAbilityCooldownTime()
    {
        return this.abilityCooldownTime;
    }

    void Start ()
    {
        audioManager = FindObjectOfType<AudioManager>();
        ReportAction();
    }

    //UI
    private bool menuActive = false;

    public void Initialize(Weapon choice, int projectileChoice, UnityEvent restart = null)
    {
        m_chosenWeapon = choice;
        m_weapon = m_weapons[(int) choice];
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
        restartEvent = restart;

        m_chosenProjectile = m_projectiles[projectileChoice];
        m_weapon.gameObject.SetActive(true);

        m_animator.SetFloat(k_playerAtkSpeed, m_weapon.SwingSpeed);

        if (restart != null)
            restartEvent = restart;

        m_currentHealth = m_maxHealth;

        ChangePlayerAbility(m_chosenAbility);
    }

    #region Updates

    void Update()
    {
        if (IsIdle())
        {
            System.Random random = new System.Random();
            int select = random.Next(1, 6);
            string soundName = "Idle" + select;
            audioManager.Play(soundName);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReportAction();
            menuActive = !menuActive;
            Time.timeScale = menuActive ? 0 : 1;
            m_menu.SetActive(menuActive);
        }

        if (menuActive)
        {
            m_timeSpeedChanger.Stop();
            return;
        }

        UpdateCharacterStates();
        UpdatePlayerUI();
        UpdateAnimations();  
    }

    private void UpdatePlayerUI()
    {
        m_healthBar.fillAmount = m_currentHealth / m_maxHealth;
        imageMeleeCooldown.fillAmount = attackCooldownTime / m_weapon.Cooldown;
        imageRangedCooldown.fillAmount = projectileCooldownTime / m_chosenProjectile.Cooldown;
        imageAbilityCooldown.fillAmount = abilityCooldownTime / m_playerAbilities[(int)m_chosenAbility].Cooldown;
    }

    private void UpdateCharacterStates()
    {
        Walk();
        Attack();
        Projectile();
        PlayerAbility();

        if (invincibilityTime > 0)
            invincibilityTime -= Time.deltaTime;

        if (attackCooldownTime > 0)
            attackCooldownTime -= Time.deltaTime;

        if (projectileCooldownTime > 0)
            projectileCooldownTime -= Time.deltaTime;

        if (abilityCooldownTime > 0)
            abilityCooldownTime -= Time.deltaTime;

        if (dashInvincibleTime > 0)
            dashInvincibleTime -= Time.deltaTime;
    }

    private void Walk()
    {
        Vector2 walkVector = new Vector2(Input.GetAxisRaw(k_horizontalAxis), Input.GetAxisRaw(k_verticalAxis));

        if (Mathf.Abs(walkVector.magnitude) > 1)
        {
            ReportAction();
            walkVector = walkVector.normalized;
        }

        dashDirection = walkVector;

        transform.Translate(walkVector * m_walkSpeed * Time.unscaledDeltaTime);

        float horizontalSpeed = walkVector.x;
        float verticalSpeed = walkVector.y;

        m_animator.SetFloat("HorizontalSpeed", Mathf.Abs(horizontalSpeed));
        m_animator.SetFloat("VerticalSpeed", Mathf.Abs(verticalSpeed));

        Vector3 theScale = transform.localScale;
        if (horizontalSpeed < 0)
        {
            // Multiply the player's x local scale by -1.
            if (theScale.x > 0)
            {
                // only rotate if necessary
                theScale.x *= -1;
                transform.localScale = theScale;
            }
        }
        else if (horizontalSpeed > 0)
        {
            // Multiply the player's x local scale by -1.
            if (theScale.x < 0)
            {
                // only rotate if necessary
                theScale.x *= -1;
                transform.localScale = theScale;
            }
        }
    }

    private void Attack()
    {
        if (attackCooldownTime > 0)
        {
            return;
        }

        if (Input.GetButton(k_fireButton))
        {
            ReportAction();
            m_animator.SetTrigger(k_attackAnim);
            attackCooldownTime = m_weapon.Cooldown;
        }
    }

    private void Projectile()
    {
        if (projectileCooldownTime > 0)
        {
            return;
        }

        if (Input.GetButton(k_projectileButton))
        {
            ReportAction();
            projectileCooldownTime = m_chosenProjectile.Cooldown;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            GameObject projectile = Instantiate(m_chosenProjectile.gameObject, transform.position, Quaternion.identity);

            Vector2 direction = (mousePos - (Vector2) transform.position).normalized;
            projectile.transform.right = direction;

            PlayerProjectile playerProjectile = projectile.GetComponent<PlayerProjectile>();

            if (playerProjectile)
            {
                playerProjectile.IncreaseDamage(m_meleeDamageModifier);
            }
            else
            {
                Debug.LogError("No player projectile script on " + projectile.gameObject.name);
            }

            Rigidbody2D projRB = projectile.GetComponent<Rigidbody2D>();
            if (projRB)
            {
                projRB.AddForce(direction * 5f, ForceMode2D.Impulse);
            }
        }
    }

    private void UpdateAnimations()
    {
        m_animator.SetBool(m_playerHitAnim, invincibilityTime > 0);
    }

    private void PlayerAbility()
    {
        if (Input.GetButton("Ability") && abilityCooldownTime <= 0)
        {
            ReportAction();
            PlayerAbilityDefinition ability = m_playerAbilities[(int)m_chosenAbility];
            abilityCooldownTime = ability.Cooldown;
            if (ability.Ability == EAbility.Dash)
                DashAbility();
            else if (ability.Ability == EAbility.ProjectileExplosion)
                ProjectileExplosionAbility();
            else
                StartCoroutine(SlowMotionAbility());
        }

        if (dashInvincibleTime < 0)
        {
            dashInvincible = false;
        }

    }

    #endregion

    private void DashAbility()
    {
        dashInvincible = true;
        dashInvincibleTime = m_dashInvincibilityTime;
        m_rigidbody.AddForce(dashDirection * m_dashForce, ForceMode2D.Impulse);
    }

    private void ProjectileExplosionAbility()
    {
        for(int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 && j == 0)
                    continue;

                GameObject projectile = Instantiate(m_chosenProjectile.gameObject, transform.position, Quaternion.identity);
                Vector2 direction = ((new Vector2(i, j) + (Vector2)transform.position) - (Vector2)transform.position).normalized;
                projectile.transform.right = direction;
                Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
                if(rb)
                {
                    rb.AddForce(new Vector2(i, j).normalized * 5f, ForceMode2D.Impulse);
                }
            }
        }
    }

    private IEnumerator SlowMotionAbility()
    {
        m_timeSpeedChanger.SetTargetTimeSpeed(m_slowmoTimeSpeed);
        yield return new WaitForSecondsRealtime(m_slowmoDuration);
        m_timeSpeedChanger.SetTargetTimeSpeed(1);
    }

    public void DamagePlayer(float damage)
    {
        int select = 1;
        string soundName = "";
        System.Random random = new System.Random();

        if (invincibilityTime > 0 || dashInvincible)
            return;

        invincibilityTime = m_invincibilityCooldown;
        m_currentHealth -= damage;

        
        if (m_currentHealth > 2) {
            select = random.Next(1,7);
            soundName = "Injured";

            if (select == 6) {
                soundName += "NotReally";
            } else {
                soundName += select;
            }

            audioManager.Play(soundName);

        } else if (m_currentHealth <= 2) {
            select = random.Next(1,5);
            soundName = "Flock";

            switch(select) {
                case 1:
                    soundName += "Off";
                    break;
                case 2:
                    soundName += "Me1";
                    break;
                case 3:
                    soundName += "Me2";
                    break;
                case 4:
                    soundName += "It";
                    break;
            }

            audioManager.Play(soundName);
        }
        
        if (m_currentHealth <= 0)
        {
            Time.timeScale = 0;
            m_gameOverMenu.SetActive(true);
            menuActive = true;
        }
    }

    public void ChangePlayerStat(Powerup.PowerupType type, float amount)
    {
        System.Random random = new System.Random();
        int select = 1;
        string soundName = "";
        switch (type)
        {
            case Powerup.PowerupType.Attack:
                soundName = "AttackPowerup";
                select = random.Next(1, 3);
                soundName += select;
                audioManager.Play(soundName);

                m_meleeDamageModifier += amount;
                m_weapon.IncreaseByDamageMod(m_meleeDamageModifier);
                m_rangedDamageModifier += amount;
                break;
            case Powerup.PowerupType.Health:
                soundName = "HealthPowerup";
                select = random.Next(1, 3);
                soundName += select;
                audioManager.Play(soundName);

                m_maxHealth += amount;
                break;
            case Powerup.PowerupType.Speed:
                soundName = "SpeedBoost";
                select = random.Next(1, 3);
                soundName += select;
                audioManager.Play(soundName);
                
                m_walkSpeed += amount;
                break;
            case Powerup.PowerupType.Heal:
                soundName = "HealthPickup";
                audioManager.Play(soundName);

                m_currentHealth += amount;
                if (m_currentHealth > m_maxHealth)
                    m_currentHealth = m_maxHealth;
                break;
            default:
                break;
        }
    }

	public void ChangePlayerAbility(EAbility type)
	{
        string soundName = "ActivePickup";
        
        // necessary guard, as this method gets called before audioManager is intialized
        if (audioManager != null) {
            audioManager.Play(soundName);
        }

		m_chosenAbility = m_playerAbilities[(int)type].Ability;
        
        switch(m_chosenAbility)
		{
			case PlayerController.EAbility.Dash:
				m_cooldownReady.color = Color.blue;
				break;
			case PlayerController.EAbility.ProjectileExplosion:
				m_cooldownReady.color = Color.red;
				break;
			case PlayerController.EAbility.Slowmo:
				m_cooldownReady.color = Color.yellow;
				break;
		}
	}

	public EAbility GetPlayerAbility()
	{
		return m_chosenAbility;
	}

	void OnCollisionEnter2D(Collision2D collision)
    {
        if (m_enemyProjectiles == (m_enemyProjectiles | (1 << collision.gameObject.layer)))
        {
            ProjectileScript projectile = collision.gameObject.GetComponent<ProjectileScript>();
            if (projectile)
            {
                DamagePlayer(projectile.damage);
            }
            else
            {
                Debug.LogError("No projectile script on " + collision.gameObject.name);
            }
        }
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        menuActive = false;
        m_menu.SetActive(false);
        m_timeSpeedChanger.SetTargetTimeSpeed(1);
        m_timeSpeedChanger.Resume();
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public bool MenuActive
    {
        get => menuActive;
        set => menuActive = value;
    }
}
