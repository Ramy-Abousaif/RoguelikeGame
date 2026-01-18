using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class Abilities : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private PhysicsBasedCharacterController controller;
    [Header("Strafing")]
    [SerializeField] private float strafingReleaseDelay = 0.25f;
    private Vector3 _attackInput;
    // ability system: abilities[0] == primary (hold-to-repeat)
    [System.Serializable]
    public class Ability
    {
        public string stateName = "Main Attack";
        public int layerIndex = 1;

        [Header("Attack")]
        public AbilityEmitter abilityEmitter;

        public GameObject effectPrefab;
        public Transform effectSpawnPoint;

        public float baseAbilityCooldown = 1f;
        
        public float currentAbilityCooldown = 1f;

        public float baseAbilityRange = 1f;

        public float currentAbilityRange = 1f;

        public float baseAbilitySpeed = 1f;

        public float currentAbilitySpeed = 1f;

        public float baseAbilityDamage = 10f;
        public float currentAbilityDamage = 1f;

        public bool affectedByAttackSpeed = false;

        [HideInInspector] public float cooldownTimer = 0f;
        [HideInInspector] public AnimationClip clip = null;
        [HideInInspector] public bool isPlaying = false;
        [HideInInspector] public float playTimer = 0f;
    }

    [SerializeField] private Ability[] _abilities = new Ability[4];
    // getter and setter for abilities
    public Ability[] abilities { get { return _abilities; } set { _abilities = value; } }

    private float _primaryAttackTimer = 0f;   // time elapsed in current primary attack
    private bool _attackHeld = false;

    private bool _isAttacking = false;

    [Header("Layer Weight Smoothing")]
    [SerializeField] private float layerWeightSmoothTime = 0.08f;
    private float _currentLayerWeight = 0f;
    private float _layerWeightVelocity = 0f;
    private float _targetLayerWeight = 0f;

    // cached primary clip accessor
    private AnimationClip PrimaryClip => (_abilities != null && _abilities.Length > 0) ? _abilities[0].clip : null;

    private void Start()
    {
        controller = GetComponent<PhysicsBasedCharacterController>();
        // ensure abilities array length is 4
        if (_abilities == null || _abilities.Length != 4)
        {
            _abilities = new Ability[4];
            for (int i = 0; i < 4; i++) _abilities[i] = new Ability();
        }

        // initialize clips for all abilities
        for (int i = 0; i < _abilities.Length; i++)
        {
            _abilities[i].currentAbilityCooldown = _abilities[i].baseAbilityCooldown;
            _abilities[i].currentAbilityRange = _abilities[i].baseAbilityRange;
            _abilities[i].currentAbilitySpeed = _abilities[i].baseAbilitySpeed;
            _abilities[i].currentAbilityDamage = _abilities[i].baseAbilityDamage;
            controller.Anim.SetFloat("AbilitySpeed" + (i + 1), _abilities[i].currentAbilitySpeed);

            if (!string.IsNullOrEmpty(_abilities[i].stateName) && anim != null)
                _abilities[i].clip = FindAnimation(_abilities[i].stateName);
        }
        if (controller == null)
            controller = GetComponentInParent<PhysicsBasedCharacterController>();
    }

    /// <summary>
    /// Reads the player attack input.
    /// </summary>
    /// <param name="context">The attack input's context.</param>
    public void Ability1InputAction(InputAction.CallbackContext context)
    {
        float attackContext = context.ReadValue<float>();
        _attackInput = new Vector3(0, attackContext, 0);

        if (context.started || context.performed)
        {
            TryUseAbility(0);
        }

        if (context.canceled)
        {
            _attackHeld = false;
            // do not stop immediately: let current attack finish (so attack lasts until delay is over)
        }
    }

    public void Ability2InputAction(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
            TryUseAbility(1);
    }

    public void Ability3InputAction(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
            TryUseAbility(2);
    }

    public void Ability4InputAction(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
            TryUseAbility(3);
    }

    private void StartAttack(int index)
    {
        _isAttacking = true;
        if (controller != null)
            controller.SetStrafing(true);
        if (anim != null)
        {
            // set target weight to 1; actual layer weight will interpolate in Update (primary)
            int attackLayerIndex = _abilities[index].layerIndex;
            if (anim.layerCount > attackLayerIndex)
                _targetLayerWeight = 1f;
            anim.SetBool("isAttacking", true);
            anim.SetInteger("AbilityIndex", index);
            // replay the attack state so the animation restarts even when the bool is already true
            anim.Play(_abilities[index].stateName, attackLayerIndex, 0f);
        }

        _primaryAttackTimer = 0f;
    }

    public void SpawnEffectAtAbility(int index)
    {
        if(_abilities[index].effectPrefab != null)
        {
            Instantiate(_abilities[index].effectPrefab, transform.position, Quaternion.identity);
        }
    }

    private void StopAttack()
    {
        _isAttacking = false;
        if (controller != null)
            controller.EndStrafingAfter(strafingReleaseDelay);
        if (anim != null)
        {
            // set target weight to 0; actual layer weight will interpolate in Update
            int attackLayerIndex = _abilities[0].layerIndex;
            if (anim.layerCount > attackLayerIndex)
                _targetLayerWeight = 0f;
            anim.SetBool("isAttacking", false);
        }
    }

    public void OnHit(Enemy enemy, int abilityIndex)
    {
        enemy.TakeDamage(_abilities[abilityIndex].currentAbilityDamage, true);
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;
        // Primary attack handling (hold-to-repeat)
        if (_isAttacking && anim.GetInteger("AbilityIndex") == 0)
        {
            _primaryAttackTimer += dt;
            AnimationClip clip = PrimaryClip;
            float clipLen = (clip != null && anim != null) ? clip.length / Mathf.Max(0.0001f, anim.GetFloat("AbilitySpeed1")) : 0f;
            if (_primaryAttackTimer >= clipLen)
            {
                if (_attackHeld)
                {
                    _primaryAttackTimer = 0f;
                    if (anim != null)
                    {
                        int attackLayerIndex = _abilities[0].layerIndex;
                        if (anim.layerCount > attackLayerIndex)
                        {
                            anim.Play(_abilities[0].stateName, attackLayerIndex, 0f);
                        }
                    }
                }
                else
                {
                    StopAttack();
                }
            }
        }

        // Other abilities: update cooldowns and playing timers
        if (_abilities != null)
        {
            for (int i = 1; i < _abilities.Length; i++)
            {
                var a = _abilities[i];
                if (a == null) continue;
                if (a.cooldownTimer > 0f)
                    a.cooldownTimer = Mathf.Max(0f, a.cooldownTimer - dt);

                if (a.isPlaying)
                {
                    a.playTimer += dt;
                    float clipLen = (a.clip != null && anim != null) ? a.clip.length / anim.GetFloat("AbilitySpeed" + (i + 1)) : 0f;
                    if (a.playTimer >= clipLen)
                    {
                        StopAttack();
                        a.isPlaying = false;
                        a.playTimer = 0f;
                        // start cooldown only after the animation has finished
                        a.cooldownTimer = a.currentAbilityCooldown;
                        if (anim != null && anim.layerCount > a.layerIndex)
                            anim.SetLayerWeight(a.layerIndex, 0f);
                    }
                }
            }
        }

        // interpolate layer weight towards target
        // interpolate primary layer weight towards target
        if (_abilities != null && _abilities.Length > 0)
        {
            int attackLayerIndex = _abilities[0].layerIndex;
            if (anim != null && anim.layerCount > attackLayerIndex)
            {
                _currentLayerWeight = Mathf.SmoothDamp(_currentLayerWeight, _targetLayerWeight, ref _layerWeightVelocity, layerWeightSmoothTime);
                anim.SetLayerWeight(attackLayerIndex, _currentLayerWeight);
                anim.SetBool("isAttacking", _isAttacking);
            }
        }
    }

    
    public AnimationClip FindAnimation (string name) 
    {
        foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips)
        {
            if (clip.name == name)
            {
                return clip;
            }
        }

        return null;
    }

    private void TryUseAbility(int index)
    {
        if (_abilities == null || index < 0 || index >= _abilities.Length) return;
        var a = _abilities[index];
        if (a == null) return;
        // Primary is hold-to-repeat and handled separately
        if (index == 0)
        {
            _attackHeld = true;
            if (!_isAttacking)
                StartAttack(0);
            return;
        }

        // For non-primary abilities: block if already playing, on cooldown, or currently attacking
        if (a.isPlaying || a.cooldownTimer > 0f || _isAttacking) return;

        // start ability playback (do NOT start cooldown yet; cooldown begins after animation finishes)
        a.isPlaying = true;
        a.playTimer = 0f;
        if (anim != null)
        {
            if (anim.layerCount > a.layerIndex)
                anim.SetLayerWeight(a.layerIndex, 1f);
            if (!string.IsNullOrEmpty(a.stateName))
                anim.Play(a.stateName, a.layerIndex, 0f);
        }
        // Start the shared attack state
        StartAttack(index);
    }
}
