using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

/// <summary>
/// サーバー関連の成功したか失敗したかを判断するためのコールバック処理
/// </summary>

public class ConnectionCallbacks : MonoBehaviourPunCallbacks
{
    // マスターサーバーへの接続が成功した時に呼ばれるコールバック
    public override void OnConnectedToMaster()
    {
        Debug.Log("マスターサーバーに接続しました");
    }

    // Photonのサーバーから切断された時に呼ばれるコールバック
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"サーバーとの接続が切断されました: {cause.ToString()}");
    }

    // ルームの作成が成功した時に呼ばれるコールバック
    public override void OnCreatedRoom()
    {
        Debug.Log("ルームの作成に成功しました");
    }

    // ルームの作成が失敗した時に呼ばれるコールバック
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log($"ルームの作成に失敗しました: {message}");
    }

    // ルームへの参加が成功した時に呼ばれるコールバック
    public override void OnJoinedRoom()
    {
        Debug.Log("ルームへ参加しました");
    }

    // ルーム名を指定したルームへの参加が失敗した時に呼ばれるコールバック
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log($"ルームへの参加に失敗しました: {message}");
    }

    // ランダムなルームへの参加が失敗した時に呼ばれるコールバック
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // ランダムで参加できるルームが存在しないなら、新規でルームを作成する
        PhotonNetwork.CreateRoom(null);
    }
}
