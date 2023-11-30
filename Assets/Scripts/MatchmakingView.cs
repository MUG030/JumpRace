using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchmakingView : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private TMP_InputField passwordInputField = default;
    [SerializeField]
    private Button joinRoomButton = default;
    [SerializeField]
    private TextMeshProUGUI statusText;

    private const int MaxPlayerPerRoom = 2;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        // マスターサーバーに接続するまでは、入力できないようにする
        canvasGroup.interactable = false;

        // パスワードを入力する前は、ルーム参加ボタンを押せないようにする
        joinRoomButton.interactable = false;

        passwordInputField.onValueChanged.AddListener(OnPasswordInputFieldValueChanged);
        joinRoomButton.onClick.AddListener(OnJoinRoomButtonClick);
    }

    public override void OnConnectedToMaster()
    {
        // マスターサーバーに接続したら、入力できるようにする
        canvasGroup.interactable = true;
    }

    private void OnPasswordInputFieldValueChanged(string value)
    {
        // パスワードを6桁入力した時のみ、ルーム参加ボタンを押せるようにする
        joinRoomButton.interactable = (value.Length == 1 || value.Length == 6);
    }

    private void OnJoinRoomButtonClick()
    {
        // ルーム参加処理中は、入力できないようにする
        canvasGroup.interactable = false;

        var playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        if (playerCount != MaxPlayerPerRoom)
        {
            statusText.text = "対戦相手を待っています。";
        }
        else
        {
            statusText.text = "対戦相手が揃いました。バトルシーンに移動します。";
        }


        // ルームを非公開に設定する（新規でルームを作成する場合）
        var roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        roomOptions.IsVisible = false;
        if (passwordInputField.text.Length == 6)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (PhotonNetwork.CurrentRoom.PlayerCount == MaxPlayerPerRoom)
                {
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    statusText.text = "対戦相手が揃いました。バトルシーンに移動します。";
                    // パスワードと同じ名前のルームに参加する（ルームが存在しなければ作成してから参加する）
                    PhotonNetwork.JoinOrCreateRoom(passwordInputField.text, roomOptions, TypedLobby.Default);
                }
            }
            
        } else if (passwordInputField.text.Length == 1)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (PhotonNetwork.CurrentRoom.PlayerCount == MaxPlayerPerRoom)
                {
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    statusText.text = "対戦相手が揃いました。バトルシーンに移動します。";
                    PhotonNetwork.JoinRandomRoom();
                }
            }
        }
    }

    // ランダムで参加できるルームが存在しないなら、新規でルームを作成する
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // ルームの参加人数を2人に設定する
        var roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;

        PhotonNetwork.CreateRoom(null, roomOptions);
    }

    public override void OnJoinedRoom()
    {
        // ルームへの参加が成功したら、UIを非表示にする
        gameObject.SetActive(false);

        // ルームが満員になったら、以降そのルームへの参加を不許可にする
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        // ルームへの参加が失敗したら、パスワードを再び入力できるようにする
        passwordInputField.text = string.Empty;
        canvasGroup.interactable = true;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == MaxPlayerPerRoom)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                statusText.text = "対戦相手が揃いました。バトルシーンに移動します。";
                PhotonNetwork.LoadLevel("BattleScene");
            }
        }
    }
}
