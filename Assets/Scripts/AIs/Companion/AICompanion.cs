using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class AICompanion : MonoBehaviour
{
    public Transform playerTransform;
    NavMeshAgent agent;
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

    public GameObject AICompanionMesh;
    public Material VanishingMaterial;
    private List<Material> AI_Companion_Old_MaterialList;
    private bool IsDisappering;


    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;
    
    [Tooltip("Variables for vanishing effects")]
    [SerializeField] private float noiseStrength = 0.25f; 
    public float objectHeight = 3.0f;

    public float VanishingThrehold = 6.0f;

    private float Dissolve_time = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        agent.speed = walkingSpeed;
        IsDisappering = false;
        VanishingMaterial.SetFloat("_CutoffHeight", objectHeight);
    }

    void OnDrawGizmos()
    {
        if (velocity)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position,transform.position + agent.velocity);
        }

        if (path)
        {
            Gizmos.color=Color.black;
            var agentPath = agent.path;
            Vector3 prevCorner = transform.position;
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

        if (IsDisappering == true)
        {
            VanishingMaterial.SetFloat("_CutoffHeight", objectHeight);
            Dissolve_time += Time.deltaTime * Mathf.PI * 0.15f;
            print(Dissolve_time);
            float height = objectHeight;
            if (height <= VanishingThrehold)
            {
                height += Mathf.Sin(Dissolve_time) * (objectHeight * 10.0f / 2.0f);
            }
            else
            {
                height -= Mathf.Sin(Dissolve_time) * (objectHeight * 10.0f / 2.0f);
            }
            SetHeight(height);
        }
    }

    public void ChangeMaterial()
    {
        Material[] Mat = new Material[3] { VanishingMaterial, VanishingMaterial,VanishingMaterial };
        AICompanionMesh.transform.GetComponent<Renderer>().materials = Mat;
        IsDisappering = true;
    }
    
    private void SetHeight(float height)
    {
        VanishingMaterial.SetFloat("_CutoffHeight", height);
        VanishingMaterial.SetFloat("_NoiseStrength", noiseStrength);
    }
}
