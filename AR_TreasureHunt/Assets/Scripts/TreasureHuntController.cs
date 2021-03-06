﻿    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;
    using GoogleARCore;

    public class TreasureHuntController : MonoBehaviour
    {
		public Camera m_firstPersonCamera;

		public GameObject m_trackedPlanePrefab;

        public GameObject m_firstObject;

		public GameObject m_secondObject;

		public GameObject m_endObject;

		public GameObject m_applauseAudio;

		public GameObject m_confettiParticle;
		
		private Quaternion m_firstObjectRot;

		private Vector3 m_firstObjectPos;

		private bool m_firstCreated = false;
		
		private bool m_secondCreated = false;
		
		private bool m_thirdCreated = false;

		private bool m_keyIsCollected = false;

		public GameObject m_keyCollectedUI;

        public GameObject m_searchingForPlaneUI;

        private List<TrackedPlane> m_newPlanes = new List<TrackedPlane>();

        private List<TrackedPlane> m_allPlanes = new List<TrackedPlane>();

        private Color[] m_planeColors = new Color[] {
            new Color(1.0f, 1.0f, 1.0f),
            new Color(0.956f, 0.262f, 0.211f),
            new Color(0.913f, 0.117f, 0.388f),
            new Color(0.611f, 0.152f, 0.654f),
            new Color(0.403f, 0.227f, 0.717f),
            new Color(0.247f, 0.317f, 0.709f),
            new Color(0.129f, 0.588f, 0.952f),
            new Color(0.011f, 0.662f, 0.956f),
            new Color(0f, 0.737f, 0.831f),
            new Color(0f, 0.588f, 0.533f),
            new Color(0.298f, 0.686f, 0.313f),
            new Color(0.545f, 0.764f, 0.290f),
            new Color(0.803f, 0.862f, 0.223f),
            new Color(1.0f, 0.921f, 0.231f),
            new Color(1.0f, 0.756f, 0.027f)
        };
			
		public void Start ()
		{
			m_applauseAudio.SetActive (false);
			m_confettiParticle.SetActive (false);
			m_keyCollectedUI.SetActive (false);
		}
		

		public void Update ()
        {
            _QuitOnConnectionErrors();
            // The tracking state must be FrameTrackingState.Tracking in order to access the Frame.
            if (Frame.TrackingState != FrameTrackingState.Tracking)
            {
                const int LOST_TRACKING_SLEEP_TIMEOUT = 15;
                Screen.sleepTimeout = LOST_TRACKING_SLEEP_TIMEOUT;
                return;
            }

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Frame.GetNewPlanes(ref m_newPlanes);

            // Iterate over planes found in this frame and instantiate corresponding GameObjects to visualize them.
            for (int i = 0; i < m_newPlanes.Count; i++) {
                // Instantiate a plane visualization prefab and set it to track the new plane. The transform is set to
                // the origin with an identity rotation since the mesh for our prefab is updated in Unity World
                // coordinates.
              	GameObject planeObject = Instantiate(m_trackedPlanePrefab, Vector3.zero, Quaternion.identity,
                    transform);
				planeObject.GetComponent<GoogleARCore.HelloAR.TrackedPlaneVisualizer>().SetTrackedPlane(m_newPlanes[i]);
                // Apply a random color and grid rotation.
                planeObject.GetComponent<Renderer>().material.SetColor("_GridColor", m_planeColors[Random.Range(0,
                    m_planeColors.Length - 1)]);
                planeObject.GetComponent<Renderer>().material.SetFloat("_UvRotation", Random.Range(0.0f, 360.0f));
            }

            // Disable the snackbar UI when no planes are valid.
            bool showSearchingUI = true;
            Frame.GetAllPlanes(ref m_allPlanes);
			for (int i = 0; i < m_allPlanes.Count; i++) {
                if (m_allPlanes[i].IsValid){
                    showSearchingUI = false;
                    break;
                }
            }

            m_searchingForPlaneUI.SetActive(showSearchingUI);

     
			//BASIC GAME LOGIC CONDITIONS	
			if (m_firstCreated == false && m_secondCreated == false && m_thirdCreated == false) {
				MakeObjectNow (m_firstObject, "first" );
				}

			if (m_firstCreated == true && m_secondCreated == false  && m_thirdCreated == false) {
				MakeObjectNow (m_secondObject, "second" );
				}

			if (m_firstCreated == true && m_secondCreated == true  && m_thirdCreated == false) {
														
				if (m_keyIsCollected == true) {
					// swap the closed chest with the open chest
					TouchParty (m_endObject);
					}
				}
				
			if (m_firstCreated == true && m_secondCreated == true  && m_thirdCreated == true) {
				// do nothing
				}

			if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began)) {
				Ray raycast = m_firstPersonCamera.ScreenPointToRay(Input.GetTouch(0).position);
				RaycastHit raycastHit;
				if (Physics.Raycast(raycast, out raycastHit)) {
					// touch the key
					if (raycastHit.collider.CompareTag("key")){
						// destroy the key
						GameObject[] gameobjects = GameObject.FindGameObjectsWithTag("key");
						foreach (GameObject g in gameobjects) {
							Destroy(g);
						}
						// instantiate the key in 2D screenspace
						m_keyCollectedUI.SetActive(true);
						// set a flag that the chest is unlocked
						m_keyIsCollected = true;

					}
				}
			}

		}
			
		void MakeObjectNow (GameObject prefabObject, string gateCondition)
		{

			Touch touch;
			if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began) {
				return;
			}

			TrackableHit hit;
			TrackableHitFlag raycastFilter = TrackableHitFlag.PlaneWithinBounds | TrackableHitFlag.PlaneWithinPolygon;


		if (Session.Raycast (m_firstPersonCamera.ScreenPointToRay (touch.position), raycastFilter, out hit)) {

			// Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
			// world evolves.
			var anchor = Session.CreateAnchor (hit.Point, Quaternion.identity);

			// Intanstiate an Andy Android object as a child of the anchor; it's transform will now benefit
			// from the anchor's tracking.
			var andyObject = Instantiate (prefabObject, hit.Point, Quaternion.identity, anchor.transform);

				//***set this to true so we only do ^^^ 1 time.
				if (gateCondition == "first") {
					m_firstCreated = true;
				}

				if (gateCondition == "second") {
					m_secondCreated = true;
				}

			// Andy should look at the camera but still be flush with the plane.
			andyObject.transform.LookAt (m_firstPersonCamera.transform);
			andyObject.transform.rotation = Quaternion.Euler (0.0f,
			andyObject.transform.rotation.eulerAngles.y, andyObject.transform.rotation.z);

			//set first position as these things so it can be replaced later
			if (gateCondition == "first") {
				//firstObjectRot = andyObject.transform.rotation;
				m_firstObjectRot = Quaternion.identity;
				m_firstObjectPos = hit.Point;				
			}

			// Use a plane attachment component to maintain Andy's y-offset from the plane
			// (occurs after anchor updates).
			andyObject.GetComponent<GoogleARCore.HelloAR.PlaneAttachment> ().Attach (hit.Plane);
			}

		}

	
		void TouchParty (GameObject showObject)
		{

					Touch touch;
			if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began) {
				return;
			}

			TrackableHit hit;
			TrackableHitFlag raycastFilter = TrackableHitFlag.PlaneWithinBounds | TrackableHitFlag.PlaneWithinPolygon;


		if (Session.Raycast (m_firstPersonCamera.ScreenPointToRay (touch.position), raycastFilter, out hit)) {

				// destroy the 2D screenspace key
				m_keyCollectedUI.SetActive(false);


				// this probably doesn't need to be an array and loop but i kinda want to keep it for a minute...
				GameObject[] gameobjects = GameObject.FindGameObjectsWithTag("destroyable");
				foreach (GameObject g in gameobjects) {
					Destroy(g);
				}

				Object.Instantiate (showObject, m_firstObjectPos, m_firstObjectRot);

				m_applauseAudio.SetActive (true);
				m_confettiParticle.SetActive (true);
				m_thirdCreated = true;

			
			}

		}


        private void _QuitOnConnectionErrors()
        {
            // Do not update if ARCore is not tracking.
            if (Session.ConnectionState == SessionConnectionState.DeviceNotSupported)
            {
                _ShowAndroidToastMessage("This device does not support ARCore.");
                Application.Quit();
            }
            else if (Session.ConnectionState == SessionConnectionState.UserRejectedNeededPermission)
            {
                _ShowAndroidToastMessage("Camera permission is needed to run this application.");
                Application.Quit();
            }
            else if (Session.ConnectionState == SessionConnectionState.ConnectToServiceFailed)
            {
                _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
                Application.Quit();
            }
        }


        private static void _ShowAndroidToastMessage(string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                        message, 0);
                    toastObject.Call("show");
                }));
            }
        }
    }
