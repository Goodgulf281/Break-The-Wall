using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking;
using System;

namespace Goodgulf
{

    public class GameManager : GameManagerBehavior
    {
       /* The GameManager is the object which handles most of the game's logic:
        * - Setting up the wall of cubes
        * - Accepting new players and network instantiating them
        * - Player disconnects
        * - Keep track of player count
        * - Game status text
        * - Game score text
        * - Game duration, game over
        * - Player scores 
        */ 

        // Singleton instance used by the Cubes to call the GameManager's RPC_PLAYER_SCORES_POINT remote procedure call
        public static GameManager Instance;

        // Keep track of the number of players in the game
        private int playerCount = 0;
       
        // Status text at the top left of the UI
        public Text statusText;
        // Score text at the top left of the UI, player 1 score : player 2 score
        public Text scoreText;

        // Reference to the player 1 -green- cubes to be network instantiated
        public GameObject greenCubePreFab;
        // Reference to the player 2 -red- cubes to be network instantiated
        public GameObject redCubePreFab;

        // A shuffleBag is used to enasure we have a fixed number of green and red cubes which will be ordered in a random manner. See SuffleBag.cs
        private ShuffleBag allCubes;

        // Used to check if the network part of the GameManager is ready
        private bool _networkReady;

        // A Dictionairy containing all players. Players get added on playerAccepted event and are retrieved from the list in the playerDisconnected event
        private readonly Dictionary<uint, PlayerBehavior> _playerObjects = new Dictionary<uint, PlayerBehavior>();

        // Start coordinates for the Cubes in the scene
        const float startX = 0.0f;
        const float startY = 5.0f;
        const float startZ = -4.5f;
        
        // Duration of the game in seconds
        const float gameDuration = 60.0f;

        // Has the game started (when 2nd player joins)?
        private bool  gameStarted = false;

        // Internal timer in the main (Update) loop
        private float gameTimer = 0.0f;

        // Time in the game in (rounded down) seconds
        private int gameSeconds = 0;
        // Used to check if we progressed to the next second
        private int gameSecondsPrevious = 0;

        // Array of int used to track player scores
        private int[] playerScores = new[] { 0,0};

        // Array to store player start positions
        Vector3[] playerPositions = new [] {new Vector3(40,0,0), new Vector3(-40,0,0)};

        private void Awake()
        {
            // Code to instantiate the Singleton instance
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
            DontDestroyOnLoad(Instance);
        }

        protected override void NetworkStart()
        {
            base.NetworkStart();

            if (NetworkManager.Instance.IsServer)
            {
                //Debug.Log("GameManager.NetworkStart(): IsServer");

                // Setup code for the player on the host. Instantiate the player and user playerCount as an index for the prefab to be selected and take the position from the starting
                //  positions array
                PlayerBehavior h = NetworkManager.Instance.InstantiatePlayer(index:playerCount, position: playerPositions[playerCount]);
                // Assign the unique ownerNetId and playerID which we'll use to track local ownership and which object belongs to which player (1 or 2).
                h.networkObject.ownerNetId = networkObject.MyPlayerId;
                h.networkObject.playerID = playerCount + 1;
                // Add tghe player to the _playerObjects dictionary using the ownerNetId==MyPlayerId as a key
                _playerObjects.Add(networkObject.MyPlayerId, h);
                
                // We now created the player on the host so increase the playercount
                playerCount++;

                // Setup the wall of cubes
                SetupWall();

                // Setup code for new remote players:
                NetworkManager.Instance.Networker.playerAccepted += (player, sender) =>
                {
                    // Instantiate the player on the main Unity thread, get the Id of its owner and add it to a list of players
                    MainThreadManager.Run(() =>
                    {

                        //Debug.Log("GameManager.NetworkStart(): playerAccepted called with playerCount = "+playerCount);
                        //Debug.Log("Aim position should be "+ aimPositions[playerCount]);
                        //Debug.Log("Aim ownerNetId = " + player.NetworkId);
                        PlayerBehavior p = NetworkManager.Instance.InstantiatePlayer(index: playerCount, position:playerPositions[playerCount]);
                        // Use the NetworkId of the player passed through as the argument of the playerAccepted event
                        p.networkObject.ownerNetId = player.NetworkId; 
                        p.networkObject.playerID = playerCount + 1;
                        _playerObjects.Add(player.NetworkId, p);

                        playerCount++;
                        if (playerCount == 2)
                        {
                            // Not sure if this actally works. It should switch off the server accepting new players
                            ((IServer)NetworkManager.Instance.Networker).StopAcceptingConnections();

                            // Now start the game
                            GameStart();
                        }
                    });
                };

                // Setup code for disconnecting players:
                NetworkManager.Instance.Networker.playerDisconnected += (player, sender) =>
                {
                    // Remove the player from the list of players and destroy it
                    PlayerBehavior p = _playerObjects[player.NetworkId];
                    _playerObjects.Remove(player.NetworkId);
                    p.networkObject.Destroy();

                    // The aim associated with this player gets destroyed when Player is Destroyed, see end of Player Initialize method.
                };
            }
            else
            {
                // This is a non host client, no need to do anything here
            }

            // Network start is completed
            _networkReady = true;
        }

        // Set the status text at the top left of the GUI overlay
        public void SetStatusText (string txt)
        {
            if(statusText!=null)
            {
                statusText.text = txt;
            }
        }

        // Set the score text at the top left -just below the status text- of the GUI overlay
        public void SetScoreText(string txt)
        {
            if (scoreText != null)
            {
                scoreText.text = txt;
            }
        }

        // Setup the wall of cubes: 10x10 cubes on top of the T bar in the middle of the game area
        public void SetupWall()
        {
            // Create wall of cubes using the Shufflebag to get 50x green and 50x red cubes
            // Each integer in the Shufflebag represents a green (0) or red (1) cube
            // Usign the Shufflebag we know for sure that the number of green and red cubes is the same while the distribution is random.
            allCubes = new ShuffleBag(100);
            allCubes.Add(0, 50);    // Add 50x Green
            allCubes.Add(1, 50);    // Add 50x Red

            for (int y = 0; y < 10; y++)
            {
                for (int z = 0; z < 10; z++)
                {
                    // The position on top of the T bar where the cube will be instantiated
                    Vector3 pos = new Vector3(startX, startY + y, startZ + z);

                    CubeBehavior cb;

                    if (allCubes.Next() == 0)
                    {
                        cb = NetworkManager.Instance.InstantiateCube(0, pos, Quaternion.identity);
                        cb.networkObject.colorId = 1; // Green
                        
                    }
                    else
                    {
                        cb = NetworkManager.Instance.InstantiateCube(1, pos, Quaternion.identity);
                        cb.networkObject.colorId = 2; // Red
                    }

                    // Assign the host's Id to the ownerNetId
                    cb.networkObject.ownerNetId = networkObject.MyPlayerId;
                    
                }
            }
        }

        // Clean the wall of Cubes when the game is over
        public void ClearWall()
        {
            // Find all cubes in the scene based on the attached script.
            // This is an expensive method so alternatively we coudl have tracked all cubes in a list.
            Cube[] allCubes = FindObjectsOfType<Cube>();
            foreach (Cube cube in allCubes)
            {
                // Network destroy to ensure the cubes get destroyed on host and client
                cube.networkObject.Destroy();
            }
        }

        // Called from the playerAccepted event to start the game time and send a status text with instructions
        public void GameStart()
        {
            networkObject.SendRpc(RPC_GAME_STATUS, Receivers.All, "Game starts! Use W,A,S,D, keys to move your aim and Space to fire.");
            gameStarted = true;
        }

        // Called from the Update method to clear the wall of cubes, send a status text to display the score
        public void GameOver ()
        {
            //networkObject.SendRpc(RPC_GAME_STATUS, Receivers.All, "Game Over!");
            gameStarted = false;
            ClearWall();

            networkObject.SendRpc(RPC_GAME_STATUS, Receivers.All, "Player 1 scores "+playerScores[0]+" - Player 2 scores "+playerScores[1]);
        }

        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            // Don't do anything until the NetworStart has been executed
            if (!_networkReady) return;

            // We'll only keep track of the time on the host
            if (NetworkManager.Instance.IsServer)
            {
                if (gameStarted)
                {
                    gameTimer += Time.deltaTime;

                    // keep track of the number of seconds into the game
                    gameSeconds = Convert.ToInt32(gameTimer % 60);

                    if (gameSeconds > gameSecondsPrevious)
                    {
                        // just reached new second tick mark
                        if (gameSeconds > 5)
                        {
                            //Send this second counter only after 5 seconds in order for the instructions to be displayed
                            networkObject.SendRpc(RPC_GAME_TIME, Receivers.All, (Convert.ToInt32(gameDuration) - gameSeconds));
                        }
                    }
                    // Save the gameSeconds so we can check if a new second tick mark is reached
                    gameSecondsPrevious = gameSeconds;

                    // The game is done after the gameDuration has been reached
                    if (gameTimer > gameDuration)
                    {
                        GameOver();
                    }
                }

            }
        }

        // RPC to set the game status text
        public override void GameStatus(RpcArgs args)
        {
            MainThreadManager.Run(() =>
            {
                string txt = args.GetNext<string>();

                SetStatusText(txt);
            });

        }

        // RPC to sync the game time between host GameManager and client GameManager. Sync only displays the time in the status text
        public override void GameTime(RpcArgs args)
        {
            MainThreadManager.Run(() =>
            {
                int tick = args.GetNext<int>();

                SetStatusText(tick + " seconds left in the game.");
            });

        }

        // RPC called by the cubes to indicate a player has scored a point
        public override void PlayerScoresPoint(RpcArgs args)
        {
            MainThreadManager.Run(() =>
            {
                // the index contains the index of the player who scored the point
                int index = args.GetNext<int>();

                // increase the score for that player
                playerScores[index] = playerScores[index] + 1;

                // Display the score
                SetScoreText(playerScores[0]+" : "+playerScores[1]);
            });
        }

        private void FixedUpdate()
        {
            // Run the Networkstart if the neatwork isn't ready
            if (!_networkReady && networkObject != null)
            {
                NetworkStart();
            }
        }

    }
}
