using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking;

namespace Goodgulf
{
    public class Aim : AimBehavior
    {
        /* The Aim object in the scene (1 for each player) is used as the target for the projectile.
         * It sits in between the player and the wall of cubes and can be moved using the W,A,S,D keys.
         * 
         * It is instantiated from the player (see Player.Initialize()).
         * 
         */

        // Properties used for networking
        //  Has the NetworkStart method run?
        public bool _networkReady;
        //  Is this network instance of the cube (host or client) the local owner?
        public bool _isLocalOwner;
        //  Has the Initialize method run?
        public bool _initialized;

        // Store the associated ID of the player (1 or 2). This is used in the Projectile.Initialize() method to find the Aim in the scene associated with the player.
        public int _myPlayerID;

        // Default value for player 1. This value is reversed to -1 for player 2 since the A and D in the WASD commands need to move in "mirror" (since we're looking from the other side of the T bar)
        private int MoveZAxis = 1;

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
                if (networkObject.playerID == 0)  // while playerID == 0 (should be 1 or 2) we know the networkObject properties have not yet synchronized                 
                {
                    _initialized = false;
                    return _initialized;
                }
            }

            // We're the local owner if the MyPlayerId and ownerNetId match
            _isLocalOwner = networkObject.MyPlayerId == networkObject.ownerNetId;

            // Store the PlayerID for later use in the Projectile.Initialize() method
            _myPlayerID = networkObject.playerID;

            // If we're the client for Player 2 then reverse the movement along the horizontal axis
            if (!networkObject.IsServer)
                MoveZAxis = -1;

            _initialized = true;
            return _initialized;
        }


        // Update is called once per frame
        void Update()
        {
            // Only start processing when the network initialization has been completed.
            if (!_networkReady) return;

            // Do nothing more until Initialize has been been run and returned true. It will return false if the networkObject has not yet been synchronized
            if (!_initialized && !Initialize()) return;

            // If we are not the owner of this network object then we should
            // move this object to the position dictated by the owner
            if (!_isLocalOwner)                                                         
            {
                transform.position = networkObject.position; 
                return;
            }

            // Use key commands to move the Aim in the scene
            if (Input.GetKey(KeyCode.W))
            {
                MoveAim(0.1f, 0f);
            }
            if (Input.GetKey(KeyCode.S))
            {
                MoveAim(-0.1f, 0f);
            }
            if (Input.GetKey(KeyCode.A))
            {
                MoveAim(0f, -0.1f*MoveZAxis);
            }
            if (Input.GetKey(KeyCode.D))
            {
                MoveAim(0f, 0.1f*MoveZAxis);
            }

            // If we are the owner of the object we should send the new position
            // across the network for receivers to move to in the above code
            networkObject.position = transform.position;
        }

        // Update the position of the Aim
        void MoveAim(float y, float z)
        {
            transform.position += new Vector3(0,y,z);
        }

        // Obselete
        public override void MoveAimTo(RpcArgs args)
        {
            // RPC calls are not made from the main thread for performance, since we
            // are interacting with Unity engine objects, we will need to make sure
            // to run the logic on the main thread
            MainThreadManager.Run(() =>
            {
                transform.position = args.GetNext<Vector3>();
                Debug.Log("MoveAimTo called with "+transform.position);
            });
        }
     
    }
}