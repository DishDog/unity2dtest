using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class RubyController : MonoBehaviour
{
    // ========= MOVEMENT =================
    public float speed = 4;
    public InputAction moveAction;
     private Vector3 targetPosition;
    private bool isMoving = false;
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
        Vector2 move = moveAction.ReadValue<Vector2>();
        
        if(!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }

        currentInput = move;

 if (Input.GetMouseButtonDown(0))
        {
            // 将鼠标点击的屏幕坐标转换为世界坐标
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0; // 确保Z轴为0，因为我们是在2D平面上
            targetPosition = mouseWorldPosition;
            isMoving = true;

        }
 
        // 如果正在移动，则向目标位置移动
        if (isMoving)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            
            // 检查是否到达目标位置
            if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
            {
                isMoving = false;
            }
        }
        // ============== ANIMATION =======================

       animator.SetFloat("Look X", lookDirection.x);
       animator.SetFloat("Look Y", lookDirection.y);
       animator.SetFloat("Speed", move.magnitude);

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
}
