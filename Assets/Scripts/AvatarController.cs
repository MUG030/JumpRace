using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEditorInternal.VersionControl;
using UnityEngine;
using UnityEngine.UI;

public class AvatarController : MonoBehaviourPunCallbacks, IPunObservable
{
    public float moveSpeed = 5f; // プレイヤーの移動速度
    public float jumpForce = 10f; // ジャンプ力
    public float groundCheckDistance = 0.1f; // 地面判定の距離
    public LayerMask groundLayer; 

    private const float MaxStamina = 6f;

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
            var input = new Vector3(Input.GetAxis("Horizontal"), 0, 0);
            isGrounded = CheckGround();
            /*if (input.sqrMagnitude > 0f)
            {
                // transform.Translate(6f * Time.deltaTime * input.normalized);
            }*/
            if (input.sqrMagnitude == 0f && isGrounded)
            {
                // 入力がなかったら、スタミナを回復させる
                currentStamina = Mathf.Min(currentStamina + Time.deltaTime * 2, MaxStamina);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (currentStamina <= junpStamina) return;
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
            currentStamina = Mathf.Max(0f, currentStamina - Time.deltaTime);
        }
    }

    private void Jump()
    {
        isGrounded = CheckGround();
        if (jumpCount == 2)
        {
            jumpCount = 0;
        } else if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            currentStamina = Mathf.Max(0f, currentStamina - junpStamina);
            jumpCount++;
        } else if (jumpCount == 1)
        {
            rb.velocity = new Vector2(Input.GetAxis("Horizontal") * moveSpeed, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            currentStamina = Mathf.Max(0f, currentStamina - junpStamina);
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
        if (col.gameObject.layer == LayerMask.NameToLayer("Floor"))
        {
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
