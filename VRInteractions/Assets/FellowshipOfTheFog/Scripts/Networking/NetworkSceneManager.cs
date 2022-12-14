using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkSceneManager : NetworkSceneManagerBase
{
    private struct PlayerData
    {
        public CharacterSheet sheet;
        public float scale;
    }

    protected override IEnumerator SwitchScene(SceneRef prevScene, SceneRef newScene, FinishedLoadingDelegate finished)
    {
        Scene loadedScene;
        Scene activeScene = SceneManager.GetActiveScene();

        loadedScene = default;

        IEnumerable<PlayerRef> players = Runner.ActivePlayers;
        Dictionary<PlayerRef, PlayerData> playerCharacters = new Dictionary<PlayerRef, PlayerData>();

        if (Runner.IsServer)
        {   
            foreach (PlayerRef playerRef in players)
            {
                NetworkObject obj = Runner.GetPlayerObject(playerRef);
                if (obj != null)
                {
                    NetworkPlayerRig rig = obj.GetComponentInChildren<NetworkPlayerRig>();

                    if (rig != null)
                    {
                        playerCharacters[playerRef] = new PlayerData()
                        {
                            sheet = rig.character,
                            scale = obj.transform.localScale.x
                        };
                    }
                }
            }
        }

        yield return SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Single);

        loadedScene = SceneManager.GetActiveScene();

        if (!loadedScene.IsValid())
        {
            throw new InvalidOperationException($"Failed to load scene {newScene}: async op failed");
        }

        //Delay frames for safety
        for (int i = 5; i > 0; --i)
        {
            yield return null;
        }

        if (Runner.IsServer)
        {
            NetworkManager manager = FindObjectOfType<NetworkManager>();
            foreach (var playerRef in players)
            {
                if (playerCharacters.ContainsKey(playerRef))
                {
                    NetworkPlayerRig rig = manager.SpawnCharacter(playerRef, playerCharacters[playerRef].sheet, playerCharacters[playerRef].scale);
                    rig.characterName = playerCharacters[playerRef].sheet.name;
                }
            }
        }

        var sceneObjects = FindNetworkObjects(loadedScene, disable: true);
        finished(sceneObjects);
    }
}
