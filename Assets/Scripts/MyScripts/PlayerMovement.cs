using System;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Settings")] public float moveSpeed = 6f;
    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    [Header("Ground Check")] public Transform groundCheck;
    // public float groundDistance = 0.2f;
    // public LayerMask groundMask;
    // private bool isGrounded;

    private Rigidbody rb;
    private Vector3 horizontalVelocity;

    GameObject objPlayerIsNear = null;
    public GameObject ObjPlayerIsNear => objPlayerIsNear;

    [HideInInspector] [SyncVar(hook = nameof(OnEquipItemChanged))]
    public string currentEquippedItem;

    ItemsManager itemsManager;

    private Animator _animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 锁定旋转 X、Z，避免物理碰撞时翻滚
        rb.freezeRotation = true;
        _animator = GetComponent<Animator>();

        itemsManager = FindObjectOfType<ItemsManager>();

        OnEquipItemChanged(null, currentEquippedItem);
    }

    void Update()
    {
        if (!isLocalPlayer) // 确保只在本地玩家上执行
            return;

        if (transform.position.y < -5f)
        {
            transform.position = Vector3.zero;
        }

        // 地面检测
        // isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(h, 0f, v).normalized;

        horizontalVelocity = Vector3.zero;
        if (inputDir.magnitude >= 0.1f)
        {
            // 计算并平滑朝向
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref turnSmoothVelocity,
                turnSmoothTime
            );
            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);

            // 计算水平速度向量
            horizontalVelocity = transform.forward * moveSpeed;
        }

        _animator.SetFloat("Speed", horizontalVelocity.magnitude);

        Collider[] objectsDetected;
        LayerMask interactableMask = LayerMask.GetMask("Interactable");

        if (isServer)
        {
            PhysicsScene physicsScene = gameObject.scene.GetPhysicsScene();

            // 准备一个足够大的缓存（根据你场景中最大的交互物体数调整大小）
            Collider[] buffer = new Collider[16];

            // 调用 OverlapSphere，将碰撞体写入 buffer，返回命中数量
            int hitCount = physicsScene.OverlapSphere(
                transform.position,
                2f,
                buffer,
                interactableMask,
                QueryTriggerInteraction.UseGlobal
            );

            // 将有效结果复制到 objectsDetected
            objectsDetected = new Collider[hitCount];
            Array.Copy(buffer, objectsDetected, hitCount);
        }
        else
        {
            objectsDetected = Physics.OverlapSphere(transform.position, 2f, interactableMask);
        }


        GameObject objShortestDistance = null;
        foreach (var obj in objectsDetected)
        {
            NetworkIdentity netId = obj.GetComponent<NetworkIdentity>();
            if (netId != null && netId.netId != 0)
            {
                if (objShortestDistance == null)
                {
                    objShortestDistance = obj.gameObject;
                }
                else
                {
                    Vector3 colliderOffset = this.GetComponent<BoxCollider>().center;
                    Vector3 objShortestColliderOffset = objShortestDistance.GetComponent<BoxCollider>().center;

                    float newDist = Vector3.Distance(this.transform.position + colliderOffset,
                        obj.transform.position + colliderOffset);
                    float oldDist = Vector3.Distance(this.transform.position + colliderOffset,
                        objShortestDistance.transform.position + objShortestColliderOffset);

                    if (newDist < oldDist)
                    {
                        objShortestDistance = obj.gameObject;
                    }
                }
            }
        }

        objPlayerIsNear = objShortestDistance;
    }

    void FixedUpdate()
    {
        // 保持原有的 y 向速度（重力、跳跃等由物理系统处理）
        Vector3 newVelocity = new Vector3(
            horizontalVelocity.x,
            rb.linearVelocity.y,
            horizontalVelocity.z
        );
        rb.linearVelocity = newVelocity;
    }

    void OnEquipItemChanged(string oldItem, string newItem)
    {
        if (itemsManager != null)
        {
            foreach (Transform item in transform.Find("EquippedItem").transform)
            {
                Destroy(item.gameObject);
            }

            if (newItem != "")
            {
                Transform newObj = Instantiate(itemsManager.items.transform.Find(newItem),
                    transform.Find("EquippedItem").transform);
                newObj.transform.name = newItem;
                newObj.gameObject.SetActive(true);
            }
        }
    }
}