using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class AvatarNameDisplay : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        var nameLabel = GetComponent<TextMeshPro>();
        // プレイヤー名とプレイヤーIDを表示する
        nameLabel.text = $"{photonView.Owner.NickName}({photonView.OwnerActorNr})";
    }
}
