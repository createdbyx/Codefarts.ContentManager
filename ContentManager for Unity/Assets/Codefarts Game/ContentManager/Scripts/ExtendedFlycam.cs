// <copyright>
//   Copyright (c) 2012 Codefarts
//   All rights reserved.
//   contact@codefarts.com
//   http://www.codefarts.com
// </copyright>
/*
EXTENDED FLYCAM
   Desi Quintans (CowfaceGames.com), 17 August 2012.
   Based on FlyThrough.js by Slin (http://wiki.unity3d.com/index.php/FlyThrough), 17 May 2011.
 
LICENSE
   Free as in speech, and free as in beer.
 
FEATURES
   WASD/Arrows:    Movement
             Q:    Climb
             E:    Drop
                 Shift:    Move faster
               Control:    Move slower
                   End:    Toggle cursor locking to screen (you can also press Ctrl+P to toggle play mode on and off).
*/
namespace Codefarts.ContentManager.Scripts
{
    using UnityEngine;

    public class ExtendedFlycam : MonoBehaviour
    {
        public float cameraSensitivity = 90;
        public float climbSpeed = 4;
        public float normalMoveSpeed = 10;
        public float slowMoveFactor = 0.25f;
        public float fastMoveFactor = 3;

        private float rotationX = 0.0f;
        private float rotationY = 0.0f;

        void Start()
        {
            //  Screen.lockCursor = true;
            this.transform.localRotation = Quaternion.AngleAxis(this.rotationX, Vector3.up);
            this.transform.localRotation *= Quaternion.AngleAxis(this.rotationY, Vector3.left);
        }

        private void Update()
        {
            if (Input.GetMouseButton(1))
            {
                this.rotationX += Input.GetAxis("Mouse X") * this.cameraSensitivity * Time.deltaTime;
                this.rotationY += Input.GetAxis("Mouse Y") * this.cameraSensitivity * Time.deltaTime;
                this.rotationY = Mathf.Clamp(this.rotationY, -90, 90);

                this.transform.localRotation = Quaternion.AngleAxis(this.rotationX, Vector3.up);
                this.transform.localRotation *= Quaternion.AngleAxis(this.rotationY, Vector3.left);
            }

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                this.transform.position += this.transform.forward * (this.normalMoveSpeed * this.fastMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
                this.transform.position += this.transform.right * (this.normalMoveSpeed * this.fastMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
            }
            else
            {
                this.transform.position += this.transform.forward * this.normalMoveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
                this.transform.position += this.transform.right * this.normalMoveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.Space))
            {
                this.transform.position += this.transform.up * this.climbSpeed * Time.deltaTime;
            }
        }
    } 
}