using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using Unity.Mathematics;
using UnityEngine.UI;

public class AICompanion : MonoBehaviour
{
    public Transform playerTransform;
    NavMeshAgent agent;
    private Renderer _MeshRenderer;
    public float maxDistance = 2.0f;
    public float speedupDistance = 5.0f;
    public float offset = 1.0f;
    public float walkingSpeed = 2.0f;
    public float runningSpeed = 5.0f;
    float Timer = 0.0f;
    public float MaxTime = 1.0f;
    public bool velocity;
    public bool desiredvelocity;
    public bool path;
    
    private Animator _animator;
    private bool _hasAnimator;
    private CharacterController _controller;


    public GameObject AICompanionMesh;
    private Material[] SkinnedMaterials;
    private List<Material> AI_Companion_Old_MaterialList;
    public bool IsDisappering = false;
    public float dissolverate = 0.0125f;
    public float RefreshRate = 0.025f;
    private float DissolveTime;

    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;
    
    //add jump related events
    public delegate void LinkEvent();

    public LinkEvent OnLinkStart;
    public LinkEvent OnLinkEnd;

    
    //assign animation
    private int _animIDGrounded;
    private int _animIDFreeFall;



    
    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;


    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;
    
    [Tooltip("For Jump/Nav mesh link movements")]
    public AnimationCurve m_Curve;
    
    [Tooltip("AI State Machine implementing")]
    //https://www.toptal.com/unity-unity3d/unity-ai-development-finite-state-machine-tutorial


    private void AssignAnimationIDs()
    {
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
    }
    
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        agent.speed = walkingSpeed;
        _MeshRenderer = AICompanionMesh.transform.GetComponent<Renderer>();
        SkinnedMaterials = _MeshRenderer.materials; 
        if(agent == null)
        {
            Debug.LogError("agent has not been assigned.", this);
            // Notice, that we pass 'this' as a context object so that Unity will highlight this object when clicked.
        }

        DissolveTime = 1 / dissolverate;
        
        //add link handler
        OnLinkStart += HandleLinkEnd;
        OnLinkEnd += HandleLinkStart;

    }

    void OnDrawGizmos()
    {
        if (velocity)
        { 
           Vector3 AIposition = transform.position;
           Gizmos.color = Color.green;
           Gizmos.DrawLine(AIposition, AIposition + agent.velocity);
        }

        if (path)
        {
            Gizmos.color=Color.black;
            var agentPath = agent.path;
            float3 prevCorner = transform.position;
            foreach (var Corner in agentPath.corners)
            {
                Gizmos.DrawLine(prevCorner,Corner);
                Gizmos.DrawSphere(Corner,0.1f);
                prevCorner = Corner;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Jump();
        GroundedCheck();

    }

    public void Move()
    {
        Timer -= Time.deltaTime;
        if (Timer < 0.0f)
        {
            float sqDistance = (playerTransform.position - agent.destination).sqrMagnitude;
            if (sqDistance > maxDistance * maxDistance)
            {
                agent.destination = playerTransform.position - new Vector3(offset,offset,0);
                if (sqDistance > speedupDistance * speedupDistance)
                {
                    agent.speed = runningSpeed;
                }
                else
                {
                    agent.speed = walkingSpeed;
                }
            }

            Timer = MaxTime;
        }
        _animator.SetFloat("Speed",agent.velocity.magnitude);
       

    }
    
    private void GroundedCheck()
    {
        // set sphere position, with offset
        float3 spherePosition = new float3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, Grounded);
        }
    }

    public void Jump()
    {
        if (agent.isOnOffMeshLink)
        {
            OnLinkStart?.Invoke();
            StartCoroutine(Curve(agent, 0.5f));
            agent.CompleteOffMeshLink();
            OnLinkEnd?.Invoke();
        }

        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDFreeFall, false);
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDFreeFall, true);
                }
            }
        }
    }
    
    IEnumerator Curve(NavMeshAgent agent, float duration)
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        float3 startPos = agent.transform.position;
        float3 endPos = data.endPos + Vector3.up * agent.baseOffset;
        float normalizedTime = 0.0f;
        while (normalizedTime < 1.0f)
        {
            float yOffset = m_Curve.Evaluate(normalizedTime);
            agent.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime) + yOffset * Vector3.up;
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }
    }

    private void HandleLinkStart()
    {
        _animator.SetTrigger("Jump");
    }

    private void HandleLinkEnd()
    {
        _animator.SetTrigger("Landed");
    }
    
    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = UnityEngine.Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(transform.position), FootstepAudioVolume);
            }
        }
    }
    
    public void TeammateOff()
    {
        StopCoroutine(UnDissolveCo());
        StartCoroutine(DissolveCo());
        _MeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
    }
    
    public void TeammateOn()
    {
        StopCoroutine(DissolveCo());
        StartCoroutine(UnDissolveCo());
        _MeshRenderer.shadowCastingMode = ShadowCastingMode.On;
    }

    IEnumerator DissolveCo()
    {
        if (SkinnedMaterials.Length > 0)
        {
            float Counter = 0;
            while (SkinnedMaterials[0].GetFloat("_DissolveThreshold") < 1)
            {
                Counter += dissolverate;
                for (int i = 0; i < SkinnedMaterials.Length; i++)
                {
                    SkinnedMaterials[i].SetFloat("_DissolveThreshold", Counter);

                }
                yield return new WaitForSeconds(RefreshRate);
            }
        }
    }
    
    IEnumerator UnDissolveCo()
    {
        if (SkinnedMaterials.Length > 0)
        {
            float Counter = 1;
            Counter -= dissolverate;
            for (int i = 0; i < SkinnedMaterials.Length; i++)
            {
                SkinnedMaterials[i].SetFloat("_DissolveThreshold", Counter);
            }
            yield return new WaitForSeconds(RefreshRate);
        }
    }

    
    
    void OnDestroy()
    {
        for (int i = 0; i < SkinnedMaterials.Length; i++)
        {
            SkinnedMaterials[i].SetFloat("_DissolveThreshold", 0);

        }
    }
}
