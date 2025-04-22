using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class RubyController : MonoBehaviour
{
    // ========= MOVEMENT =================
    public float speed = 4;
    public InputAction moveAction;
     private Vector3 targetPosition;
   private bool isMovingToTarget = false;
	private Vector2 move;
    // ======== HEALTH ==========
    public int maxHealth = 5;
    public float timeInvincible = 2.0f;
    public Transform respawnPosition;
    public ParticleSystem hitParticle;
    
    // ======== PROJECTILE ==========
    public GameObject projectilePrefab;
    public InputAction launchAction;

    // ======== AUDIO ==========
    public AudioClip hitSound;
    public AudioClip shootingSound;
    
    // ======== HEALTH ==========
    public int health
    {
        get { return currentHealth; }
    }
    
    // ======== DIALOGUE ==========
    public InputAction dialogAction;
    
    // =========== MOVEMENT ==============
    Rigidbody2D rigidbody2d;
    Vector2 currentInput;
    
    // ======== HEALTH ==========
    int currentHealth;
    float invincibleTimer;
    bool isInvincible;
   
    // ==== ANIMATION =====
    Animator animator;
    Vector2 lookDirection = new Vector2(1, 0);
    
    // ================= SOUNDS =======================
    AudioSource audioSource;
    
    void Start()
    {targetPosition = transform.position;
        // =========== MOVEMENT ==============
        rigidbody2d = GetComponent<Rigidbody2D>();
        moveAction.Enable();        
        
        // ======== HEALTH ==========
        invincibleTimer = -1.0f;
        currentHealth = maxHealth;
        
        // ==== ANIMATION =====
        animator = GetComponent<Animator>();
        
        // ==== AUDIO =====
        audioSource = GetComponent<AudioSource>();
        
        // ==== ACTIONS ====
        launchAction.Enable();
        dialogAction.Enable();

        launchAction.performed += LaunchProjectile;
    }

    void Update()
    {
        // ================= HEALTH ====================
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0)
                isInvincible = false;
        }

        // ============== MOVEMENT ======================

        // ============== ANIMATION =======================

        HandleMovementInput();
        UpdateAnimatorParameters();

        // ======== DIALOGUE ==========
        if (dialogAction.WasPressedThisFrame())
        {
            RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, 1 << LayerMask.NameToLayer("NPC"));
            if (hit.collider != null)
            {
                NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                if (character != null)
                {
                    UIHandler.instance.DisplayDialog();
                }  
            }
        }
		
		
    }

    void FixedUpdate()
    {
        Vector2 position = rigidbody2d.position;
        
        position = position + currentInput * speed * Time.deltaTime;
        
        rigidbody2d.MovePosition(position);
    }

    // ===================== HEALTH ==================
    public void ChangeHealth(int amount)
    {
        if (amount < 0)
        { 
            if (isInvincible)
                return;
            
            isInvincible = true;
            invincibleTimer = timeInvincible;
            
            animator.SetTrigger("Hit");
            audioSource.PlayOneShot(hitSound);
Debug.Log(currentHealth + "/" + maxHealth);
            Instantiate(hitParticle, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }
        
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        Debug.Log(currentHealth + "/" + maxHealth);
        if(currentHealth == 0)
            Respawn();
        
        //UIHandler.instance.SetHealthValue(currentHealth / (float)maxHealth);
        //UIHealthBar.Instance.SetValue(currentHealth / (float)maxHealth);
    }
    
    void Respawn()
    {
        ChangeHealth(maxHealth);
        transform.position = respawnPosition.position;
    }
    
    // =============== PROJECTICLE ========================
    void LaunchProjectile(InputAction.CallbackContext context)
    {
        GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.Launch(lookDirection, 300);
        
        animator.SetTrigger("Launch");
        audioSource.PlayOneShot(shootingSound);
    }
    
    // =============== SOUND ==========================

    //Allow to play a sound on the player sound source. used by Collectible
    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }






  void HandleMovementInput()
    {
        // Handle mouse click for target position
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0; // Assuming 2D game with z = 0
            targetPosition = mouseWorldPosition;
            isMovingToTarget = true;
        }
 
        // If moving to a target position, move towards it
        if (isMovingToTarget)
        {
            move = (targetPosition - transform.position).normalized;
            transform.position = Vector2.MoveTowards((Vector2)transform.position, targetPosition, speed * Time.deltaTime);
 
            // Check if we have reached the target position
            if (Vector2.Distance((Vector2)transform.position, targetPosition) < 0.1f)
            {
                isMovingToTarget = false;
                move = Vector2.zero; // Reset move direction when not moving to a target
            }
        }
        else
        {
            // Handle keyboard input when not moving to a target
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
	   Vector2 position = transform.position;
      position.x = position.x + speed * horizontal * Time.deltaTime;
        position.y = position.y + speed * vertical * Time.deltaTime;
       transform.position = position;
            move = new Vector2(horizontal, vertical);
 
            if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
            {
                isMovingToTarget = false; // Ensure keyboard movement is not overridden by target movement
                lookDirection.Set(move.x, move.y);
                lookDirection.Normalize();
            }
        }
 
        // Ensure lookDirection is updated correctly when moving via keyboard
        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }
    }
 
    void UpdateAnimatorParameters()
    {
        // Set Look X and Look Y based on lookDirection
        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
 
        // Set Speed based on movement magnitude
        animator.SetFloat("Speed", move.magnitude);
    }
}
