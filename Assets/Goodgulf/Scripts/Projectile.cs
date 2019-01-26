using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;

// Code from: https://vilbeyli.github.io/Projectile-Motion-Tutorial-for-Arrows-and-Missiles-in-Unity3D/
// by Volkan Ilbeyli
// Make sure you check the comments on his blog post since there's an important bug fix in it.


namespace Goodgulf
{
    public class Projectile : ProjectileBehavior
    {
        /*
         * The projectile class uses motion physics to launch the projectile towards the position of the Aim.
         * If you aim correctly its will hit the wall of cubes sitting on the T bar:
         * 
         *             ***Aim**     C    
         *         ****         ****C
         * Player**                 T
         * 
         */

        // launch variables
        [SerializeField] private Transform TargetObjectTF;
        [Range(1.0f, 15.0f)] public float TargetRadius;
        [Range(20.0f, 75.0f)] public float LaunchAngle;
        [Range(0.0f, 10.0f)] public float TargetHeightOffsetFromGround;
        public bool RandomizeHeightOffset;

        // state of the projectile
        private bool bTargetReady;
        private bool bTouchingGround;
        private bool bLaunched;

        // Cache the rigidbody, collider and the initial rotation of the projectile (before it launches). The latter will be used to fix the projectile orientation during flight
        private Rigidbody rigid;
        private Quaternion initialRotation;
        private CapsuleCollider myCollider;

        // Properties used for networking
        //  Has the NetworkStart method run?
        public bool _networkReady;
        //  Is this network instance of the cube (host or client) the local owner?
        public bool _isLocalOwner;
        //  Has the Initialize method run?
        private bool _initialized;


        protected override void NetworkStart()
        {
            base.NetworkStart();

            if (NetworkManager.Instance.IsServer)
            {
                // If we're on the host we can enable the RigidBody since all Physics will be done on the host.
                rigid.isKinematic = false;
                myCollider.enabled = true;
            }
            else
            {
                // If we're not the host we can disable the RigidBody since all Physics will be done on the host.
                rigid.isKinematic = true;
                myCollider.enabled = false;
            }

            // Sometimes you'll see a strange effect on the client when the projectile is launched:
            // The missile first moves (when interpolation is on) throug a loop what appears to be through (0,0,0).
            // See #community-answers (May 28, Shadowworm) in the Forge Networking Discord channel
            networkObject.position = transform.position;
            networkObject.rotation = transform.rotation;
            networkObject.positionInterpolation.target = transform.position;
            networkObject.rotationInterpolation.target = transform.rotation;
            networkObject.SnapInterpolations();

            _networkReady = true;
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

            // We only Launch on the server because that's where the projectile has the rigidbody enabled
            // So the projectile on the client only follows the position and rotation from the one on the host
            if (NetworkManager.Instance.IsServer) {

                // Find the Aim object in the scene which corresponds to the playerID of the projectile
                // We'll need the AIm since this is what we're shooting for.
                Aim foundAim = null;

                // Find all Aim objects in the scene 
                Aim[] allAims = FindObjectsOfType<Aim>();
                foreach (Aim aim in allAims)
                {
                    if (aim._myPlayerID == networkObject.playerID)
                        foundAim = aim;
                }

                if (foundAim)
                {
                    // We have found the Aim, now use it as the target for this projectile then launch it
                    SetTarget(foundAim.transform.gameObject);
                    LaunchProjectile();
                }
                else Debug.LogError("Projectile.Launch(): couldn't find Aim for ownerid=" + networkObject.ownerNetId);
            }

            _initialized = true;
            return _initialized;
        }


        public void SendLaunch(uint ownerid)
        {
            // Obselete
        }

        public override void Launch(RpcArgs args)
        {
            // Obselete
        }

        // Use this for initialization
        void Awake()
        {
            // Store references to my Collider and RigidBody
            rigid = GetComponent<Rigidbody>();
            myCollider = GetComponent<CapsuleCollider>();

            // Until NetworkStart() has been processed we'll disable Physics for the projectile
            rigid.isKinematic = true;
            myCollider.enabled = false;

            // Reset the projectile state
            bTargetReady = true;
            bTouchingGround = true;
            bLaunched = false;
            initialRotation = transform.rotation;
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

            if (!bTouchingGround && !bTargetReady)
            {
                // update the rotation of the projectile during trajectory motion
                transform.rotation = Quaternion.LookRotation(rigid.velocity) * initialRotation;
            }

            // Since we are the owner, tell the network the updated position
            networkObject.position = transform.position;

            // Since we are the owner, tell the network the updated rotation
            networkObject.rotation = transform.rotation;
        }


        // The event when the projctile collides with another collider (floor, cubes, etc...)
        void OnCollisionEnter(Collision col)
        {
            bTouchingGround = true;

            if (col.gameObject.name == "Floor" && bLaunched)
            {
                // Debug.Log("Projectile.OnCollisionEnter():Projectile on floor");
                // We could do some code here to execute if the projectile hits the floor, for example an explosion
            }
        }

        void OnCollisionExit()
        {
            bTouchingGround = false;
        }

        // This function is used to set the target to which the projectile is launched towards to
        public void SetTarget(GameObject target)
        {
            //Debug.Log("Projectile.SetTarget() called with position "+target.transform.position);

            TargetObjectTF = target.transform;
            bTargetReady = true;
            rigid.velocity = Vector3.zero;
        }

        // launches the object towards the TargetObject with a given LaunchAngle
        public void LaunchProjectile()
        {
            //Debug.Log("Projectile.LaunchProjectile() called");

            // think of it as top-down view of vectors: 
            //   we don't care about the y-component(height) of the initial and target position.
            Vector3 projectileXZPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            Vector3 targetXZPos = new Vector3(TargetObjectTF.position.x, transform.position.y, TargetObjectTF.position.z);

            // rotate the object to face the target
            transform.LookAt(targetXZPos);

            // shorthands for the formula
            float R = Vector3.Distance(projectileXZPos, targetXZPos);
            float G = Physics.gravity.y;
            float tanAlpha = Mathf.Tan(LaunchAngle * Mathf.Deg2Rad);
            float H = TargetObjectTF.position.y - transform.position.y;

            // calculate the local space components of the velocity 
            // required to land the projectile on the target object 
            float Vz = Mathf.Sqrt(G * R * R / (2.0f * (H - R * tanAlpha)));
            float Vy = tanAlpha * Vz;

            // create the velocity vector in local space and get it in global space
            Vector3 localVelocity = new Vector3(0f, Vy, Vz);
            Vector3 globalVelocity = transform.TransformDirection(localVelocity);

            // launch the object by setting its initial velocity and flipping its state
            rigid.velocity = globalVelocity;
            bTargetReady = false;
            bLaunched = true;
        }

    }


}