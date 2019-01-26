using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;

namespace Goodgulf
{
    public class Cube : CubeBehavior
    {

        /* The cube script is attached to each of the cubes in the wall of cubes. It contains the code to:
         * - Disable physics on the client/enable it on the host
         * - Attach itself as a child to the Wall of Cubes in the scene (to prevent clutter in the scene hierarchy)
         * - Synchronize its position and rotation over the network
         * - Check collision with the floor for the player to score a point: green cube = point for player 1, red for player 2
         *  
         */

        // Obselete
        public int playerScores;

        // Reference to this cube's collider and rigidbody
        private BoxCollider myCollider;
        private Rigidbody myRigidbody;

        // Properties used for networking
        //  Has the NetworkStart method run?
        public bool _networkReady;
        //  Is this network instance of the cube (host or client) the local owner?
        public bool _isLocalOwner;
        //  Has the Initialize method run?
        public bool _initialized;

        // Reference to the scene object "Wall of Cubes"
        private GameObject wall;

        // Reference to the GameManager (Singleton)
        private GameManager gm;


        protected override void NetworkStart()
        {
            base.NetworkStart();
            _networkReady = true;
        }


        // This is the first method to run for the cube when instantiated
        void Awake()
        {
            // Store references to my Collider and RigidBody
            myCollider = GetComponent<BoxCollider>();
            myRigidbody = GetComponent<Rigidbody>();

            // Before initialization is done we disable Physics for the cubes
            myRigidbody.isKinematic = true;
            myCollider.enabled = false;

            // Find the scene object wall of cubes so we can parent the cube to it later
            wall = GameObject.Find("Wall of Cubes");
            if (wall == null)
            {
                Debug.LogError("Cube.Awake(): cannot find Scene object <Wall of Cubes>");
                return;
            }

            // Store reference to the GameManager
            gm = GameManager.Instance;
        }

        private bool Initialize()
        {
            
            if (!networkObject.IsServer)
            {
                if (networkObject.colorId == 0) // colorID = 1 (green) or 2 (red) so while it's 0 the networkObject properties have not yet synchronized
                {
                    _initialized = false;
                    return _initialized;
                }
            }

            // We're the local owner if the MyPlayerId and ownerNetId match
            _isLocalOwner = networkObject.MyPlayerId == networkObject.ownerNetId;

            // Link the Cube to the wall of cubes in the scene
            if (wall != null)
            {
                transform.SetParent(wall.transform);
            }

            if (!networkObject.IsServer)
            {
                // If we're not the host we can disable the RigidBody since all Physics will be done on the host.
                myRigidbody.isKinematic = true;
                myCollider.enabled = false;
            }
            else
            {
                // If we're the host we enable Physics.
                myRigidbody.isKinematic = false;
                myCollider.enabled = true;
            }

            _initialized = true;
            return _initialized;
        }



        // Update is called once per frame
        void Update()
        {
            // Do nothing until NetworkStart() has executed
            if (!_networkReady) return;

            // Do nothing more until Initialize has been been run and returned true. It will return false if the networkObject has not yet been synchronized
            if (!_initialized && !Initialize()) return;

            // If this is not owned by the current network client then it needs to
            // assign it to the position and rotation specified
            if (!_isLocalOwner)
            {
                // Assign the position of this cube to the position sent on the network
                transform.position = networkObject.position;

                // Assign the rotation of this cube to the rotation sent on the network
                transform.rotation = networkObject.rotation;

                // Stop the function here and don't run any more code in this function
                return;
            }

            // Since we are the owner, tell the network the updated position
            networkObject.position = transform.position;

            // Since we are the owner, tell the network the updated rotation
            networkObject.rotation = transform.rotation;
        }

        void OnCollisionEnter(Collision col)
        {
            // Check if we collide with the Floor and if this is the first collision with the floor (stored in the tag).
            if (col.gameObject.name == "Floor" && this.gameObject.tag != "Point")
            {
                //Debug.Log("Point");

                // We change the tag to indicate this is the first time hitting the floor.
                this.gameObject.tag = "Point";

                // Send the RPC through the GameManager indication that the player associated with the colorId of the Cube (green = 1 = player 1, red = 2 = player 2) has scored a point
                gm.networkObject.SendRpc(GameManager.RPC_PLAYER_SCORES_POINT, Receivers.All, networkObject.colorId-1);
            }
        }
    }
}