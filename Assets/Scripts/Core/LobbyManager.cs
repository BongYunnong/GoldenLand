using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : GameManager
{
    [SerializeField] Character lobbyCharacterPrefab;

    public List<Character> lobbyCharacters = new List<Character>();

    Coroutine dataSaveCoroutine;
    private int donationAcc = 0;

    public override void InitializeGame()
    {
        base.InitializeGame();
        dataSaveCoroutine = StartCoroutine("DataSaveCoroutine");
    }
    
    public void AddLobbyCharacter(string characterId)
    {
        Character currBot = Instantiate(lobbyCharacterPrefab, transform.position, Quaternion.identity);
        currBot.InitializeCharacter();
        lobbyCharacters.Add(currBot);
    }
    
    
    public override void HandleMouseClickInput(Vector2 mousePos)
    {
        RaycastHit hit = playerController.GetMouseRayHit();
        if (hit.collider != null)
        {
            if(hit.collider.TryGetComponent(out Character character))
            {
                // TODO
            }
        }

        if(donationAcc > 100)
        {
            if (dataSaveCoroutine != null)
            {
                StopCoroutine(dataSaveCoroutine);
            }
            dataSaveCoroutine = StartCoroutine("DataSaveCoroutine");
        }
    }


    IEnumerator DataSaveCoroutine()
    {
        donationAcc = 0;
        yield return new WaitForSeconds(300.0f);
        dataSaveCoroutine = StartCoroutine("DataSaveCoroutine");
    }
}
