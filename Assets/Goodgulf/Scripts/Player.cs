using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking;

namespace Goodgulf
{
    public class Player : PlayerBehavior
    {
        /* This is the player which sets up the Aim and the Camera in the scene. It also launches the projectile.
         * 
         * We'll have two player objects in the scene, Player 1 shown as a sphere and Player 2 shown as a cube.
         * 
         */ 

        // Cache the main camera (note we have a second camera with a render texture in the scene but this is the camera used to display the player's view
        private Camera mainCamera;

        // Properties used for networking
        //  Has the NetworkStart method run?
        public bool _networkReady;
        //  Is this network instance of the cube (host or client) the local owner?
        public bool _isLocalOwner;
        //  Has the Initialize method run?
        private bool _initialized;

        // Store a reference to the Aim associated with this player, used when we Destroy the player.
        private AimBehavior _myAim;

        // Delay in seconds after which the launched projectile gets destroyed.
        const int destroyDelay = 7;

        // The start positions for the Aim in the scene for player 1 and 2
        Vector3[] aimPositions = new[] { new Vector3(20, 8, 0), new Vector3(-20, 8, 0) };

        protected override void NetworkStart()
        {
            base.NetworkStart();
            _networkReady = true;
        }

        // Use this for initialization
        void Start()
        {
        }

        private bool Initialize()
        {
            if (!networkObject.IsServer)
            {
                if (networkObject.ownerNetId == 0)
                {
                    _initialized = false;
                    return _initialized;
                }
            }

            // We're the local owner if the MyPlayerId and ownerNetId match
            _isLocalOwner = networkObject.MyPlayerId == networkObject.ownerNetId;

            //Debug.Log("Initialize.Initialize(): playerID = " + networkObject.playerID + "isLocalOwner = " + _isLocalOwner);
            //Utilities.WriteDebugString(networkObject.playerID, "Initialize.NetworkStart(): playerID = " + networkObject.playerID + "isLocalOwner = " + _isLocalOwner);

            if (_isLocalOwner)
            {
                // Debug.Log("Initialize.NetworkStart(): isLocalOwner");
                //Utilities.WriteDebugString(networkObject.playerID, "Player.NetworkStart(): isLocalOwner");

                // Move the camera to match with the player position in the scene
                mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    //Debug.Log("Initialize.NetworkStart(): moving camera");
                    //Utilities.WriteDebugString(networkObject.playerID, "Player.NetworkStart(): moving camera");

                    // Take the player's position and move it behind and up from the player's position
                    Vector3 newCameraPosition = new Vector3(transform.position.x * 1.1f, 3.5f, 0);
                    mainCamera.transform.position = newCameraPosition;

                    // Have the camera look at the T bar in the scene
                    Vector3 lookAtPosition = new Vector3(0, 3.5f, 0);
                    mainCamera.transform.LookAt(lookAtPosition);
                }
                else Debug.LogError("Player.Initialize(): cannot find main camera");

                // Instantiate the Aim for this player using the aimPositions array
                _myAim = NetworkManager.Instance.InstantiateAim(0, position: aimPositions[networkObject.playerID - 1]);
                _myAim.networkObject.ownerNetId = networkObject.MyPlayerId;
                _myAim.networkObject.playerID = networkObject.playerID;

            }
            else {
                // Debug.Log("Initialize.NetworkStart(): not isLocalOwner");
            }

            // Add an event to trigger when this Player object receives the network Destroy
            networkObject.onDestroy += OnPlayerDestroy;

            _initialized = true;
            return _initialized;
        }

        // If the Player object received the network Destroy then also Destroy its associated Aim
        void OnPlayerDestroy(NetWorker sender)
        {
            if (_myAim != null)
                _myAim.networkObject.Destroy();
        }


        // Update is called once per frame
        void Update()
        {
            // Do nothing until NetworkStart() has executed
            if (!_networkReady) return;

            // Do nothing more until Initialize has been been run and returned true. It will return false if the networkObject has not yet been synchronized
            if (!_initialized && !Initialize()) return;

            // If we are not the local owner do nothing
            if (!_isLocalOwner) return;
 
            // If we are the local owner look for the space key command to launch the projectile
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //Debug.Log("Player.Update(): Space pressed by player <"+networkObject.MyPlayerId+">");
                Vector3 pos = new Vector3(transform.position.x*0.9f, transform.position.y+1.1f, transform.position.z);

                // Run a CreateProjectile(pos, playerID) RPC here
                // in the RPC create the projectile server side
                // use PlayerID to find the Aim on the server associated with the player who pressed space
                networkObject.SendRpc(RPC_CREATE_PROJECTILE, Receivers.Server, pos, networkObject.playerID);
            }
        }

        // The RPC to launch the projectile.
        public override void CreateProjectile(RpcArgs args)
        {

            MainThreadManager.Run(() =>
            {
                // Retrieve the arguments from the RPC call: start position of the projectile and the player ID who shot it
                Vector3 pos = args.GetNext<Vector3>();
                int pid = args.GetNext<int>();

                //Debug.Log("Player.CreateProjectile(): called with position=" + pos + " for playerID=" + pid);

                // Network instantiate the projectile on the server
                if (NetworkManager.Instance.IsServer)
                {
                    //Debug.Log("Player.CreateProjectile(): we are on the host");
                    ProjectileBehavior pb = NetworkManager.Instance.InstantiateProjectile(0, pos, Quaternion.identity);
                    pb.networkObject.ownerNetId = networkObject.MyPlayerId;
                    pb.networkObject.playerID = pid;

                    // We'll automatically destroy the projectile after the destroyDelay in seconds
                    pb.networkObject.Destroy(destroyDelay * 1000);

                    // The actual launch of the projectile takes place in its Projectile.Inialize() function
                }
            });
        }
     }
}
