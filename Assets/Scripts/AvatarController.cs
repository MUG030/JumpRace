using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class AvatarController : MonoBehaviourPunCallbacks, IPunObservable
{
    public float moveSpeed = 5f; // プレイヤーの移動速度
    public float jumpForce = 1f; // ジャンプ力
    public float maxJumpForce = 5f;
    public float groundCheckDistance = 0.1f; // 地面判定の距離
    public LayerMask groundLayer; 

    private const float MaxStamina = 10f;
    private float jumpForceMultiplier = 1f;

    [SerializeField] private Image staminaBar = default;
    [SerializeField] private float junpStamina = 2.0f;

    public static bool isGrounded;
    private int jumpCount = 0;

    private Rigidbody2D rb; // プレイヤーのRigidbody2Dコンポーネント

    private float currentStamina = MaxStamina;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            Vector3 input = new(Input.GetAxis("Horizontal"), 0, 0);
            /*if (input.sqrMagnitude > 0f)
            {
                // transform.Translate(6f * Time.deltaTime * input.normalized);
            }*/
            if (input.sqrMagnitude == 0f && isGrounded)
            {
                // 入力がなかったら、スタミナを回復させる
                currentStamina = Mathf.Min(currentStamina + Time.deltaTime * 2, MaxStamina);
            }

            if (Input.GetKey(KeyCode.Space))
            {
                if (currentStamina <= junpStamina) return;
                // スペースキーが押されている時間に応じてジャンプ力を上げる
                jumpForceMultiplier += Time.deltaTime;
                // ジャンプ力が上限を超えないように制限
                jumpForceMultiplier = Mathf.Clamp(jumpForceMultiplier, 1f, maxJumpForce);
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                Jump();
            }
        }

        // スタミナをゲージに反映する
        staminaBar.fillAmount = currentStamina / MaxStamina;
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        isGrounded = CheckGround();

        if (isGrounded && currentStamina > 0f)
        {
            rb.velocity = new Vector2(Input.GetAxis("Horizontal") * moveSpeed, rb.velocity.y);
            currentStamina = Mathf.Max(0f, currentStamina - (Time.deltaTime / 2));
        }
    }

    private void Jump()
    {
        isGrounded = CheckGround();
        if (jumpCount == 2)
        {
            jumpCount = 0;
            jumpForceMultiplier = 1f;
        } else if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce * jumpForceMultiplier, ForceMode2D.Impulse);
            currentStamina = Mathf.Max(0f, currentStamina - (junpStamina + jumpForceMultiplier));
            jumpForceMultiplier = 1f;
            jumpCount++;
        } else if (jumpCount == 1)
        {
            rb.velocity = new Vector2(Input.GetAxis("Horizontal") * moveSpeed, 0f);
            rb.AddForce(Vector2.up * jumpForce * jumpForceMultiplier, ForceMode2D.Impulse);
            currentStamina = Mathf.Max(0f, currentStamina - (junpStamina + jumpForceMultiplier));
            jumpForceMultiplier = 1f;
            jumpCount++;
        }
    }

    private bool CheckGround()
    {
        Vector2 position = transform.position;
        Vector2 direction = Vector2.down;
        float distance = groundCheckDistance;

        RaycastHit2D leftHit = Physics2D.Raycast(position - new Vector2(0.45f, 0.5f), direction, distance, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(position - new Vector2(-0.45f, 0.5f), direction, distance, groundLayer);

        // レイを可視化（デバッグ目的）
        Debug.DrawRay(position - new Vector2(0.45f, 0.5f), direction * distance, Color.red);

        return leftHit.collider != null || rightHit.collider != null;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        int layerMask = 1 << col.gameObject.layer;
        if ((groundLayer.value & layerMask) != 0)
        {
            Debug.Log("着地");
            jumpCount = 0;
        }
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 自身のアバターのスタミナを送信する
            stream.SendNext(currentStamina);
        }
        else
        {
            // 他プレイヤーのアバターのスタミナを受信する
            currentStamina = (float)stream.ReceiveNext();
        }
    }
}
