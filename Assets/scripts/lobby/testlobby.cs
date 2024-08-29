using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;

public class testlobby : MonoBehaviour
{
    private Lobby hostlobby;
    private Lobby joinedlobby;
    private float heartBeat;
    private float lobbyUpdater;
    [SerializeField] private PlayerJoiner pj;
    [SerializeField] private playersBoard pb;

    private string playerId;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            playerId = AuthenticationService.Instance.PlayerId;
            //Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

    }

    private void Update()
    {
        HandleLobbyHearBeat();
        HandleLobbyUpdate();
    }

    private async void HandleLobbyHearBeat()
    {
        if(hostlobby != null)
        {
            heartBeat -= Time.deltaTime;

            // checking if timer is less than 0 and then resetting it for the heartbeat to keep lobby alive
            if(heartBeat < 0f)
            {
                // needs to be lower than 30 seconds because if no data recieved after 30 secs shuts down lobby visibility
                float heartbeatTimerMax = 20;
                heartBeat = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostlobby.Id);
            }
        }
    }

    private async void HandleLobbyUpdate()
    {
        if (joinedlobby != null)
        {
            lobbyUpdater -= Time.deltaTime;

            // checking if timer is less than 0 and then resetting it for the heartbeat to keep lobby alive
            if (lobbyUpdater < 0f)
            {
                // needs to be lower than 30 seconds because if no data recieved after 30 secs shuts down lobby visibility
                float updaterTimerMax = 20;
                lobbyUpdater = updaterTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedlobby.Id);
                joinedlobby = lobby;

                //updating player boards
                pb.DisplayPlayers(joinedlobby);
            }
        }
    }

    public async void CreateLobby()
    {
        try
        {
            string lobName = "Bean";
            int maxPlayers = 2;

            // create lobby options
            CreateLobbyOptions clo = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "CODE", new DataObject(DataObject.VisibilityOptions.Member, "0")}
                }
            };

            // creating the lobby
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobName, maxPlayers, clo);

            hostlobby = lobby;
            joinedlobby = lobby;
            //Debug.Log("Created Lobby! " + lobby.Name + " " + lobby.MaxPlayers + " Code:"+lobby.LobbyCode);

            //starting game
            CreateRelay();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void ListLobbies()
    {
        try
        {
            // lobby filter thing
            QueryLobbiesOptions qlo = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false,QueryOrder.FieldOptions.Created)
                }
            };
            // getting lobby list
            QueryResponse qr = await Lobbies.Instance.QueryLobbiesAsync(qlo);

            //Debug.Log("Lobbies Found: " + qr.Results.Count);
            foreach (Lobby lobby in qr.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
            }
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobby(string code)
    {
        try
        {
            JoinLobbyByCodeOptions jlbco = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(code, jlbco);
            joinedlobby = lobby;
            //Debug.Log("Joined Lobby: " + code);
            //PrintPlayers();
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions jlbco = new QuickJoinLobbyOptions
            {
                Player = GetPlayer()
            };
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(jlbco);
            joinedlobby = lobby;

            //joing game
            JoinRelay();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerId)},
                        {"Name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerData.username)}
                    }
        };
    }

    public void PrintPlayers()
    {
        //Debug.Log("Player count: " + joinedlobby.Players.Count);
        foreach(Player player in joinedlobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }

    private async void UpdateLobbyRelayCode(string code)
    {
        try
        {
            hostlobby = await Lobbies.Instance.UpdateLobbyAsync(hostlobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "CODE", new DataObject(DataObject.VisibilityOptions.Member, code) }
                }
            });

            joinedlobby = hostlobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }


    // relay related stuff
    private async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            RelayServerData rsd = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(rsd);
            Debug.Log(joinCode);

            UpdateLobbyRelayCode(joinCode);

            pj.HostLobby();
        }
        catch(RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void JoinRelay()
    {
        try
        {
            string code = joinedlobby.Data["CODE"].Value;
            //Debug.Log("Joining relay with " + code);
            JoinAllocation ja = await RelayService.Instance.JoinAllocationAsync(code);

            RelayServerData rsd = new RelayServerData(ja, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(rsd);

            pj.JoinLobby();
        }
        catch(RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}
