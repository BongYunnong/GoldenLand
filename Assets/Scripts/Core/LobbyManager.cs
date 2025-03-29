using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : GameManager
{
    [SerializeField] Character lobbyCharacterPrefab;
    public List<Character> lobbyCharacters = new List<Character>();

    private void Start()
    {
        InitializeGame();
    }

    public void AddLobbyCharacter(string characterId)
    {
        Character currBot = Instantiate(lobbyCharacterPrefab, transform.position, Quaternion.identity);
        currBot.InitializeCharacter();
        lobbyCharacters.Add(currBot);
    }
}
