// <author>Michal Kolasinski</author>
// <email>michal.kolasinski@kolmich.at</email>
// <date>2010-09-01</date>

// PLEASE uncomment these lines if you do own the corresponding modules
// #define KGFDebug
// #define KGFConsole

// comment this, if you do not want to change values while in play mode
#define OnlineChangeMode

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Cache for assetbundles to instantiate them at a later time
/// </summary>
public class KGFMapSystem : KGFModule, KGFICustomGUI, KGFIValidator
{
	public static void KGFSetChildrenActiveRecursively(GameObject theGameObject, bool theActive)
	{
		if (theGameObject == null)
			return;
		
		#if UNITY_4_0
		theGameObject.SetActive(theActive);
		#else
		theGameObject.SetActiveRecursively(theActive);
		#endif
	}
	
	public static bool KGFGetActive(GameObject theGameObject)
	{
		#if UNITY_4_0
		return theGameObject.activeSelf;
		#else
		return theGameObject.active;
		#endif
	}
	
	public class KGFPhotoData
	{
		public Vector3 itsPosition = Vector3.zero;
		public Texture2D itsTexture = null;
		public float itsTextureSize = 0;
		public float itsMeters = 0;
		public GameObject itsPhotoPlane = null;
		public Material itsPhotoPlaneMaterial = null;
	}
	
	public KGFMapSystem() : base(new Version(1,10,0,0), new Version(1,2,0,0))
	{}
	
	#region event args
	public class KGFClickEventArgs : EventArgs
	{
		public Vector3 itsPosition = Vector3.zero;
		
		public KGFClickEventArgs(Vector3 thePosition)
		{
			itsPosition = thePosition;
		}
	}
	
	public class KGFMarkerEventArgs : EventArgs
	{
		public KGFIMapIcon itsMarker = null;
		
		public KGFMarkerEventArgs(KGFIMapIcon theMarker)
		{
			itsMarker = theMarker;
		}
	}
	#endregion
	
	#region Internal classes
	public enum KGFMapSystemOrientation
	{
		XYSideScroller, // sidescroller
		XZDefault // default
//		YZ // do not use
	};
	
	public class KGFFlagEventArgs : EventArgs
	{
		public KGFFlagEventArgs(Vector3 thePosition)
		{
			itsPosition = thePosition;
		}
		
		public Vector3 itsPosition = Vector3.zero;
	}
	
	/// <summary>
	/// Base class for all items
	/// </summary>
	[System.Serializable]
	public class KGFDataMinimap
	{
		public minimap_global_settings itsGlobalSettings = new minimap_global_settings();
		
		/// <summary>
		/// Gui settings
		/// </summary>
		public minimap_gui_settings itsAppearanceMiniMap = new KGFMapSystem.minimap_gui_settings();
		
		public minimap_gui_fullscreen_settings itsAppearanceMap = new KGFMapSystem.minimap_gui_fullscreen_settings();
		
		public minimap_panning_settings itsPanning = new minimap_panning_settings();
		
		public minimap_fogofwar_settings itsFogOfWar = new KGFMapSystem.minimap_fogofwar_settings();
		
		public minimap_zoom_settings itsZoom = new KGFMapSystem.minimap_zoom_settings();
		
		public minimap_viewport_settings itsViewport = new KGFMapSystem.minimap_viewport_settings();
		
		public minimap_photo_settings itsPhoto = new KGFMapSystem.minimap_photo_settings();
		
		public minimap_userflags_settings itsUserFlags = new KGFMapSystem.minimap_userflags_settings();
		
		public minimap_shader_settings itsShaders = new KGFMapSystem.minimap_shader_settings();
		
		public minimap_tooltip_settings itsToolTip = new KGFMapSystem.minimap_tooltip_settings();
	}
	
	[System.Serializable]
	public class minimap_panning_settings
	{
		public bool itsActive = false;
		public bool itsUseBounds = false;
		public LayerMask itsBoundsLayers = -1;
	}
	
	[System.Serializable]
	public class minimap_tooltip_settings
	{
		public bool itsActive = false;
		public Texture2D itsTextureBackground;
		public RectOffset itsBackgroundBorder;
		public RectOffset itsBackgroundPadding;
		public Color itsColorText = Color.white;
		public Font itsFontText;
	}
	
	[System.Serializable]
	public class minimap_global_settings
	{
		public bool itsHideGUI = false;
		public LayerMask itsRenderLayers = 0;
		
		/// <summary>
		/// the minimap camera will be attached to this object.
		/// </summary>
		public GameObject itsTarget = null;
		public bool itsIsStatic = true;
		public float itsStaticNorth = 0;
		public bool itsIsActive = true;
		public Color itsColorMap = Color.white;
		public Color itsColorBackground = Color.black;
		public Color itsColorAll = Color.white;
		public bool itsEnableLogMessages = true;
		
		public KGFMapSystemOrientation itsOrientation = KGFMapSystemOrientation.XZDefault;
	}
	
	[System.Serializable]
	public class minimap_photo_settings
	{
		public bool itsTakePhoto = true;
		public LayerMask itsPhotoLayers = -1;
		public float itsPixelPerMeter = 5;
	}
	
	[System.Serializable]
	public class minimap_shader_settings
	{
		public Shader itsShaderMapIcon = null;
		public Shader itsShaderPhotoPlane = null;
		public Shader itsShaderFogOfWar = null;
		public Shader itsShaderMapMask = null;
	}
	
	[System.Serializable]
	public class minimap_userflags_settings
	{
		public bool itsActive = true;
		public KGFMapIcon itsMapIcon = null;
	}
	
	[System.Serializable]
	public class minimap_viewport_settings
	{
		public bool itsActive = false;
		public Color itsColor = Color.grey;
		public Camera itsCamera;
	}
	
	[System.Serializable]
	public class minimap_gui_settings
	{
		public float itsSize = 0.2f;
		public float itsButtonSize = 0.1f;
		public float itsButtonPadding = 0;
		
		public Texture2D itsButton;
		public Texture2D itsButtonHover;
		public Texture2D itsButtonDown;
		
		public Texture2D itsIconZoomIn;
		public Texture2D itsIconZoomOut;
		public Texture2D itsIconZoomLock;
		public Texture2D itsIconFullscreen;
		public Texture2D itsBackground;
		public int itsBackgroundBorder = 0;
		
		public Texture2D itsMask;
		
		public float itsScaleIcons=1;
		public float itsScaleArrows=0.2f;
		public float itsRadiusArrows = 1;
		
		public KGFAlignmentVertical itsAlignmentVertical = KGFAlignmentVertical.Top;
		public KGFAlignmentHorizontal itsAlignmentHorizontal = KGFAlignmentHorizontal.Right;
		
		public float itsMarginHorizontal = 0;
		public float itsMarginVertical = 0;
	}
	
	[System.Serializable]
	public class minimap_gui_fullscreen_settings
	{
		public float itsButtonSize = 0.1f;
		public float itsButtonPadding = 0;
		public float itsButtonSpace = 0.01f;
		
		public Texture2D itsButton;
		public Texture2D itsButtonHover;
		public Texture2D itsButtonDown;
		
		public Texture2D itsIconZoomIn;
		public Texture2D itsIconZoomOut;
		public Texture2D itsIconZoomLock;
		public Texture2D itsIconFullscreen;
		public Texture2D itsBackground;
		public int itsBackgroundBorder = 0;
		
		public Texture2D itsMask;
		
		public float itsScaleIcons=1;
		
		public KGFAlignmentVertical itsAlignmentVertical = KGFAlignmentVertical.Top;
		public KGFAlignmentHorizontal itsAlignmentHorizontal = KGFAlignmentHorizontal.Right;
		public KGFOrientation itsOrientation;
	}
	
	[System.Serializable]
	public class minimap_fogofwar_settings
	{
		public bool itsActive = true;
		public int itsResolutionX = 10;
		public int itsResolutionY = 10;
		public float itsRevealDistance = 10;
		public float itsRevealedFullDistance = 5;
		public bool itsHideMapIcons = false;
	}
	
	[System.Serializable]
	public class minimap_zoom_settings
	{
		/// <summary>
		/// The start zoom of the minimap in meters (unity3d units). Range 15 means that objects are vivible on the minmap
		/// in a distance of 15 meters in each direction from the target
		/// </summary>
		public float itsZoomStartValue = 20.0f;
		/// <summary>
		/// When zooming this is the minimal range that can be reached.
		/// </summary>
		public float itsZoomMin = 10;
		
		/// <summary>
		/// When zoomout this is the maximal range that can be reached.
		/// </summary>
		public float itsZoomMax = 30;
		
		/// <summary>
		/// When zooming in or out this is the value that will be added or substracted to the current zoom value
		/// </summary>
		public float itsZoomChangeValue = 10.0f;
	}
	
	/// <summary>
	/// Internal class for listing map icon
	/// </summary>
	public class mapicon_listitem_script
	{
		public KGFMapSystem itsModule = null;
		
		public KGFIMapIcon itsMapIcon = null;
		public GameObject itsRepresentationInstance;
		public Transform itsRepresentationInstanceTransform;
		public bool itsRotate;
		public GameObject itsRepresentationArrowInstance;
		public Transform itsRepresentationArrowInstanceTransform;
		
		public Transform itsMapIconTransform;
		private bool itsVisibility = false;
		private bool itsVisibilityArrow = false;
		
		public Vector3 itsCachedRepresentationSize = Vector3.zero;
		
		#region public methods
		
		/// <summary>
		/// Get representation size
		/// </summary>
		/// <returns></returns>
		public Vector3 GetRepresentationSize()
		{
			if (itsRepresentationInstanceTransform != null)
				return itsCachedRepresentationSize * itsRepresentationInstanceTransform.localScale.x;
			else
				return itsCachedRepresentationSize;
		}
		
		/// <summary>
		/// Update visibility of map icon and arrows
		/// </summary>
		public void UpdateVisibility()
		{
			if (itsMapIcon != null)
			{
				if (itsRepresentationInstance != null)
				{
					bool aNewVisibility = itsMapIcon.GetIsVisible() && itsVisibility;
					if (aNewVisibility != KGFGetActive(itsRepresentationInstance))
					{
						foreach(Transform aTransform in itsRepresentationInstance.transform)
						{
							GameObject aGameObject = aTransform.gameObject;
							KGFSetChildrenActiveRecursively(aGameObject,aNewVisibility);
						}
						KGFSetChildrenActiveRecursively(itsRepresentationInstance,aNewVisibility);
						
						//itsRepresentationInstance.SetActive(aNewVisibility);
						if(itsModule != null)
							itsModule.LogInfo(string.Format("Icon of '{0}' (category='{1}') changed visibility to: {2}",
							                                itsMapIcon.GetTransform().name,
							                                itsMapIcon.GetCategory(),
							                                aNewVisibility),itsModule.name,itsModule);
					}
				}
				if (itsRepresentationArrowInstance != null)
				{
					KGFSetChildrenActiveRecursively(itsRepresentationArrowInstance,itsMapIcon.GetIsVisible() && itsVisibility && itsVisibilityArrow && itsMapIcon.GetIsArrowVisible());
				}
			}
		}
		
		/// <summary>
		/// Update colors and textures of this icon
		/// </summary>
		public void UpdateIcon()
		{
			if (itsMapIcon != null)
			{
				// colors
				SetColorsInChildren(itsRepresentationArrowInstance,itsMapIcon.GetColor());
				SetColorsInChildren(itsRepresentationInstance,itsMapIcon.GetColor());
				
				// textures
				if (itsRepresentationArrowInstance != null)
				{
					MeshRenderer aRenderer = itsRepresentationArrowInstance.GetComponent<MeshRenderer>();
					if (aRenderer != null)
					{
						aRenderer.material.mainTexture = itsMapIcon.GetTextureArrow();
					}
				}
			}
		}
		
		/// <summary>
		/// Change the colors of all meshrenderers that are found in all child objects
		/// </summary>
		/// <param name="theGameObject"></param>
		/// <param name="theColor"></param>
		void SetColorsInChildren(GameObject theGameObject,Color theColor)
		{
			if (theGameObject != null)
			{
				MeshRenderer []aMeshrendererList = theGameObject.GetComponentsInChildren<MeshRenderer>(true);
				if (aMeshrendererList != null)
				{
					foreach (MeshRenderer aMeshRenderer in aMeshrendererList)
					{
						Material aMaterial = aMeshRenderer.sharedMaterial;
						if (aMaterial != null)
						{
							aMaterial.color = theColor;
						}
					}
				}
			}
		}
		
		/// <summary>
		/// Change visibility of map icon
		/// </summary>
		/// <param name="theVisible"></param>
		public void SetVisibility(bool theVisible)
		{
			if (theVisible != itsVisibility)
			{
				itsVisibility = theVisible;
				UpdateVisibility();
			}
		}
		
		/// <summary>
		/// Returns TRUE, if the map icon is visible effectively (all conditions need to be true)
		/// </summary>
		/// <returns></returns>
		public bool GetMapIconVisibilityEffective()
		{
			if (itsMapIcon != null)
			{
				return itsVisibility && itsMapIcon.GetIsVisible();
			}
			return false;
		}
		
		/// <summary>
		/// Change visibility of arrow, only displayed if map icon itsself is also visible
		/// </summary>
		/// <param name="theShow"></param>
		public void ShowArrow(bool theShow)
		{
			if (theShow != itsVisibilityArrow && itsRepresentationArrowInstance != null)
			{
				itsVisibilityArrow = theShow;
				UpdateVisibility();
			}
		}
		
		/// <summary>
		/// Get set visibility of arrow
		/// </summary>
		/// <returns></returns>
		public bool GetIsArrowVisible()
		{
			return itsVisibilityArrow;
		}
		
		/// <summary>
		/// Destroy this item
		/// </summary>
		public void Destroy()
		{
			if (itsRepresentationArrowInstance != null)
			{
				GameObject.Destroy(itsRepresentationArrowInstance);
			}
			if (itsRepresentationInstance != null)
			{
				GameObject.Destroy(itsRepresentationInstance);
			}
		}
		#endregion
	}
	#endregion
	
	/// <summary>
	/// Data component
	/// </summary>
	public KGFDataMinimap itsDataModuleMinimap = new KGFDataMinimap();
	
	#region private variables
	const string itsLayerName = "mapsystem";
	
	/// <summary>
	/// State of minimap
	/// </summary>
	private bool itsMinimapActive = true;
	
	/// <summary>
	/// List of all map icons
	/// </summary>
	private List<mapicon_listitem_script> itsListMapIcons = new List<mapicon_listitem_script>();
	
	/// <summary>
	/// Layer for items visible on the minimap
	/// </summary>
	private int itsLayerMinimap = -1;
	/// <summary>
	/// Caching camera target transform for performance reasons. Unity3D .transform access is imperformant
	/// </summary>
	private Transform itsTargetTransform;
	
	/// <summary>
	/// Container transform for all user created flags
	/// </summary>
	private Transform itsContainerFlags;
	/// <summary>
	/// Container transform for all user created icons
	/// </summary>
	private Transform itsContainerUser;
	/// <summary>
	/// Container transform for all map icons
	/// </summary>
	private Transform itsContainerIcons;
	/// <summary>
	/// Container transform for all arrows
	/// </summary>
	private Transform itsContainerIconArrows;
	
	/// <summary>
	/// Material for autocreated photo
	/// </summary>
	private Material itsMaterialMaskedMinimap = null;
	
	/// <summary>
	/// Material for the viewport
	/// </summary>
	private Material itsMaterialViewport = null;
	
	/// <summary>
	/// this camera will render all objects in the minimap layer
	/// </summary>
	private Camera itsCamera = null;
	/// <summary>
	/// Caching camera transform for performance reasons. Unity3D .transform access is imperformant
	/// </summary>
	private Transform itsCameraTransform;
	/// <summary>
	/// Output camera for minimap
	/// </summary>
	private Camera itsCameraOutput;
	/// <summary>
	/// itsRendertexture will be applied to the material of this plane
	/// </summary>
	private GameObject itsMinimapPlane = null;
	/// <summary>
	/// cachevariable will save performance by preventing using getcomponent in lateupdate
	/// </summary>
	private MeshFilter itsMinimapMeshFilter = null;
	/// <summary>
	/// itsCamera will render into this texture
	/// </summary>
	private RenderTexture itsRendertexture = null;
	
	/// <summary>
	/// Target draw rect of the map
	/// </summary>
	private Rect itsTargetRect;
	
	private Rect itsRectZoomIn;
	private Rect itsRectZoomOut;
	private Rect itsRectStatic;
	private Rect itsRectFullscreen;
	
	/// <summary>
	/// Gui style for buttons
	/// </summary>
	GUIStyle itsGuiStyleButton;
	
	/// <summary>
	/// Gui style for buttons in fullscreen mode
	/// </summary>
	GUIStyle itsGuiStyleButtonFullscreen;
	
	/// <summary>
	/// Empty gui style for background
	/// </summary>
	GUIStyle itsGuiStyleBack;
	
	MeshFilter itsMeshFilterFogOfWarPlane;
//	Color itsColorFogOfWarBlack = new Color(0,0,0,1);
	Color itsColorFogOfWarRevealed = new Color(0,0,0,0);
	Vector2 itsSizeTerrain = Vector2.zero;
	Vector2 itsScalingFogOfWar = Vector2.zero;
	MeshRenderer itsMeshRendererMinimapPlane;
	
	Bounds itsTerrainBoundsPanning;
	Bounds itsTerrainBoundsPhoto;
	
	Vector2 ?itsSavedResolution = null;
	/// <summary>
	/// Fullscreen mode active flag
	/// </summary>
	bool itsModeFullscreen = false;
	/// <summary>
	/// The current zoom level in pixels per meter
	/// </summary>
	float itsCurrentZoom;
	float itsCurrentZoomDest;
	/// <summary>
	/// List of all user created icons
	/// </summary>
	List<KGFMapIcon> itsListUserIcons = new List<KGFMapIcon>();
	/// <summary>
	/// List of all icons created by click
	/// </summary>
	List<KGFIMapIcon> itsListClickIcons = new List<KGFIMapIcon>();
	
	private List<KGFPhotoData> itsListOfPhotoData = new List<KGFPhotoData>();
	private KGFPhotoData[] itsArrayOfPhotoData = null;
	private int itsArrayOfPhotoDataIndex = 0;
	
	private GameObject itsTempCameraGameObject = null;
	
	#endregion
	
	#region public events
	/// <summary>
	/// Triggered if:
	///  * map icon got too far away from character to be displayed on minimap
	///  * map icon got closer so it gots displayed on minimap
	/// </summary>
	public KGFDelegate EventVisibilityOnMinimapChanged = new KGFDelegate();
	
	/// <summary>
	/// Triggered if:
	///  * a new map icon was created by click
	/// </summary>
	public KGFDelegate EventUserFlagCreated = new KGFDelegate();
	
	/// <summary>
	/// Triggered if:
	///  * a click on the minimap occured
	/// </summary>
	public KGFDelegate EventClickedOnMinimap = new KGFDelegate();
	
	/// <summary>
	/// Triggered if:
	///  * mouse enters the map area
	/// </summary>
	public KGFDelegate EventMouseMapEntered = new KGFDelegate();
	/// <summary>
	/// Triggered if:
	///  * mouse leaves the map area
	/// </summary>
	public KGFDelegate EventMouseMapLeft = new KGFDelegate();
	
	/// <summary>
	/// Triggered if:
	///  * mouse enteres the area of a map marker
	/// </summary>
	public KGFDelegate EventMouseMapIconEntered = new KGFDelegate();
	/// <summary>
	/// Triggered if:
	///  * mouse leaves the area of a map marker
	/// </summary>
	public KGFDelegate EventMouseMapIconLeft = new KGFDelegate();
	/// <summary>
	/// Triggered if:
	///  * mouse click on the area of a map marker
	/// </summary>
	/// <example>Enable/Disable blinking of map icons on click:
	/// <code>
	/// using System;
	/// using System.Collections;
	/// using UnityEngine;
	/// 
	/// public class KGFMapSystemDemoScene : MonoBehaviour
	/// {
	/// 	KGFMapSystem itsMapSystem;
	///
	/// 	void Start ()
	/// 	{
	/// 		KGFAccessor.GetExternal<KGFMapSystem>(OnMapSystemRegistered);
	/// 	}
	///
	/// 	void OnMapSystemRegistered(object theSender, EventArgs theArgs)
	/// 	{
	/// 		KGFAccessor.KGFAccessorEventargs anArgs = (KGFAccessor.KGFAccessorEventargs)theArgs;
	/// 		itsMapSystem = (KGFMapSystem) anArgs.GetObject();
	/// 		itsMapSystem.EventMouseMapIconClicked += OnMapMarkerClicked;
	/// 	}
	///
	/// 	void OnMapMarkerClicked(object theSender, EventArgs theArgs)
	/// 	{
	/// 		// toggle blinking on clicked marker
	/// 		KGFMapSystem.KGFMarkerEventArgs aMarkerArgs = (KGFMapSystem.KGFMarkerEventArgs)theArgs;
	/// 		aMarkerArgs.itsMarker.SetIsBlinking(!aMarkerArgs.itsMarker.GetIsBlinking());
	/// 	}
	/// }
	/// </code>
	/// </example>
	public KGFDelegate EventMouseMapIconClicked = new KGFDelegate();
	
	/// <summary>
	/// Triggered if:
	/// * Fullscreen mode is changed with SetFullscreen()
	/// </summary>
	public KGFDelegate EventFullscreenModeChanged = new KGFDelegate();
	#endregion
	
	#region plane helper methods
	Vector3 ChangeVectorHeight(Vector3 theVector, float theHeight)
	{
		switch (itsDataModuleMinimap.itsGlobalSettings.itsOrientation)
		{
			case KGFMapSystemOrientation.XYSideScroller:
				theVector.z = theHeight;
				break;
			case KGFMapSystemOrientation.XZDefault:
				theVector.y = theHeight;
				break;
//			case KGFMapSystemOrientation.YZ:
//				theVector.x = theHeight;
//				break;
		}
		return theVector;
	}
	
	Vector3 ChangeVectorPlane(Vector3 theVector, float theI,float theJ)
	{
		switch (itsDataModuleMinimap.itsGlobalSettings.itsOrientation)
		{
			case KGFMapSystemOrientation.XYSideScroller:
				theVector.x = theI;
				theVector.y = theJ;
				break;
			case KGFMapSystemOrientation.XZDefault:
				theVector.x = theI;
				theVector.z = theJ;
				break;
//			case KGFMapSystemOrientation.YZ:
//				theVector.y = theI;
//				theVector.z = theJ;
//				break;
		}
		return theVector;
	}
	
	Vector3 CreateVector(float theI,float theJ,float theHeight)
	{
		return ChangeVectorPlane(ChangeVectorHeight(Vector3.zero,theHeight),theI,theJ);
	}
	
	float GetVector3Height(Vector3 theVector)
	{
		switch (itsDataModuleMinimap.itsGlobalSettings.itsOrientation)
		{
			case KGFMapSystemOrientation.XYSideScroller:
				return theVector.z;
			case KGFMapSystemOrientation.XZDefault:
				return theVector.y;
//			case KGFMapSystemOrientation.YZ:
//				return theVector.x;
		}
		return 0f;
	}
	
	Vector2 GetVector3Plane(Vector3 theVector)
	{
		switch (itsDataModuleMinimap.itsGlobalSettings.itsOrientation)
		{
			case KGFMapSystemOrientation.XYSideScroller:
				return new Vector2(theVector.x,theVector.y);
			case KGFMapSystemOrientation.XZDefault:
				return new Vector2(theVector.x,theVector.z);
//			case KGFMapSystemOrientation.YZ:
//				return new Vector2(theVector.y,theVector.z);
		}
		return Vector2.zero;
	}
	#endregion
	
	#region Internal methods
	float GetTerrainHeight(float theAddHeight)
	{
		if (itsDataModuleMinimap.itsGlobalSettings.itsOrientation == KGFMapSystemOrientation.XZDefault)
		{
			if (itsDataModuleMinimap.itsPhoto.itsTakePhoto)
				return itsTerrainBoundsPhoto.min.y - 1.0f + theAddHeight;
			else
				return itsTerrainBoundsPhoto.max.y + 1.0f + theAddHeight;
		}
		else
		{
			return itsTerrainBoundsPhoto.max.z + 1.0f - theAddHeight;
		}
	}
	
	float GetHeightFog()
	{
		if (itsDataModuleMinimap.itsFogOfWar.itsHideMapIcons)
		{
			return GetTerrainHeight(0.5f);
		}
		else
		{
			return GetTerrainHeight(0.2f);
		}
	}
	
	float GetHeightIcons()
	{
		return GetTerrainHeight(0.3f);
	}
	
	float GetHeightArrows()
	{
		return GetTerrainHeight(0.9f);
	}
	
	float GetHeightViewPort()
	{
		return GetTerrainHeight(0.1f);
	}
	
	float GetHeightFlags()
	{
		return GetTerrainHeight(0.35f);
	}
	
	float GetHeightPhoto()
	{
		return GetTerrainHeight(0.0f);
	}
	
	bool itsErrorMode = false;
	protected override void KGFAwake()
	{
		UpdateStyles();
		
		itsLayerMinimap = LayerMask.NameToLayer(itsLayerName);
		if (Validate().itsHasErrors)
		{
//			LogError(string.Format("Missing layer '{0}'.",itsLayerName),name,this);
//			enabled = false;
			itsErrorMode = true;
			return;
		}
		
		CreateCameras();
		CreateRenderTexture();
		
		itsContainerFlags = (new GameObject("flags")).transform;
		itsContainerFlags.parent = transform;
		
		itsContainerUser = (new GameObject("user")).transform;
		itsContainerUser.parent = transform;
		
		itsContainerIcons = (new GameObject("icons")).transform;
		itsContainerIcons.parent = transform;
		
		itsContainerIconArrows = (new GameObject("arrows")).transform;
		itsContainerIconArrows.parent = transform;
		
		// register existing map icons
		foreach (KGFIMapIcon anIcon in KGFAccessor.GetObjects<KGFIMapIcon>())
			RegisterIcon(anIcon);
		
		SetTarget(itsDataModuleMinimap.itsGlobalSettings.itsTarget);
		// wait for new map icons
		KGFAccessor.RegisterAddEvent<KGFIMapIcon>(OnMapIconAdd);
		KGFAccessor.RegisterRemoveEvent<KGFIMapIcon>(OnMapIconRemove);
		
		#if KGFConsole
		KGFConsole.AddCommand("k.ms.ac","Enable/disable minimap",name,this,"SetMinimapEnabled");
		KGFConsole.AddCommand("k.ms.st","Set static mode",name,this,"SetModeStatic");
		KGFConsole.AddCommand("k.ms.v","Change visibility of icons by category",name,this,"SetIconsVisibleByCategory");
		KGFConsole.AddCommand("k.ms.r","Refresh icon visibility",name,this,"RefreshIconsVisibility");
		
		KGFConsole.AddCommand("k.ms.zi","Zoom in",name,this,"ZoomIn");
		KGFConsole.AddCommand("k.ms.zo","Zoom out",name,this,"ZoomOut");
		KGFConsole.AddCommand("k.ms.zmin","Set zoom to minimum",name,this,"ZoomMin");
		KGFConsole.AddCommand("k.ms.zmax","Set zoom to maximum",name,this,"ZoomMax");
		
		KGFConsole.AddCommand("k.ms.fs","Set fullscreen mode of minimap",name,this,"SetFullscreen");
		KGFConsole.AddCommand("k.ms.vp","Set visibility of camera viewport",name,this,"SetViewportEnabled");
		
		KGFConsole.AddCommand("k.ms.fws","Save current fog of war",name,this,"Save");
		KGFConsole.AddCommand("k.ms.fwl","Load fog of war",name,this,"Load");
		KGFConsole.AddCommand("k.ms.fwr","Reveal fog of war",name,this,"RevealFogOfWar");

		KGFConsole.AddCommand("k.ms.ph","Take photo of scene",name,this,"TakePhotoOfScene");
		
		#endif
		
//		EventMouseMapIconClicked += OnIconClicked;
	}
	
	/// <summary>
	/// Checks if the user has pro version installed
	/// </summary>
	/// <returns></returns>
	bool GetHasProVersion()
	{
		return SystemInfo.supportsRenderTextures;
	}
	
	/// <summary>
	/// Returns true if the mouse is hovering over the map or one of the map buttons.
	/// Can be used to disable e.g.: Character movement while interacting with the map
	/// </summary>
	/// <returns></returns>
	public bool GetHover()
	{
		Vector2 aMousePosition = Input.mousePosition;
		aMousePosition.y = Screen.height - aMousePosition.y;
		
		if(itsTargetRect.Contains(aMousePosition))
			return true;
		if(itsRectZoomIn.Contains(aMousePosition))
			return true;
		if(itsRectZoomOut.Contains(aMousePosition))
			return true;
		if(itsRectStatic.Contains(aMousePosition))
			return true;
		if(itsRectFullscreen.Contains(aMousePosition))
			return true;
		return false;
	}
	
	/// <summary>
	/// Returns true if the mouse is hovering over the map, but not the buttons.
	/// </summary>
	/// <returns></returns>
	public bool GetHoverWithoutButtons()
	{
		Vector2 aMousePosition = Input.mousePosition;
		aMousePosition.y = Screen.height - aMousePosition.y;
		
		if(itsRectZoomIn.Contains(aMousePosition))
			return false;
		if(itsRectZoomOut.Contains(aMousePosition))
			return false;
		if(itsRectStatic.Contains(aMousePosition))
			return false;
		if(itsRectFullscreen.Contains(aMousePosition))
			return false;
		if(itsTargetRect.Contains(aMousePosition))
			return true;
		return false;
	}

	IEnumerator DeferedPhoto ()
	{
		yield return new WaitForSeconds(0.1f);
		AutoCreatePhoto();
		yield break;
	}
	
	void Start()
	{
		if (itsErrorMode)
		{
			return;
		}
		
		MeasureScene();

		// check if autocreate mode is active.
		if (itsDataModuleMinimap.itsPhoto.itsTakePhoto)
			StartCoroutine(DeferedPhoto());
		
		if(itsDataModuleMinimap.itsShaders.itsShaderFogOfWar == null)
		{
			LogWarning("itsDataModuleMinimap.itsShaders.itsShaderFogOfWar is not assigned. Please install the standard unity particle package. Assign the Particle Alpha Blend Shader to itsDataModuleMinimap.itsShaders.itsShaderFogOfWar.",name,this);
		}
		else
		{
			if (itsDataModuleMinimap.itsFogOfWar.itsActive)
			{
				InitFogOfWar();
			}
		}
		
		if (GetHasProVersion())
		{
			itsMinimapPlane = GenerateMinimapPlane();
			MeshRenderer aMeshRenderer = itsMinimapPlane.GetComponent<MeshRenderer>();
			if(aMeshRenderer == null)
			{
				LogError("Cannot find meshrenderer",name,this);
			}
			else
			{
				aMeshRenderer.material.SetTexture("_MainTex",itsRendertexture);
			}
		}
		
		SetViewportEnabled(itsDataModuleMinimap.itsViewport.itsActive);
		SetMinimapEnabled(itsDataModuleMinimap.itsGlobalSettings.itsIsActive);
	}
	
	/// <summary>
	/// Get real map width in pixels
	/// </summary>
	/// <returns></returns>
	float GetWidth()
	{
		if (GetFullscreen())
		{
			return Screen.width;
		}
		else
		{
			return GetHeight();//itsDataModuleMinimap.itsAppearanceMiniMap.itsSize * (float)Screen.width;
		}
	}
	
	/// <summary>
	/// Get real map height in pixels
	/// </summary>
	/// <returns></returns>
	float GetHeight()
	{
		if (GetFullscreen())
		{
			return Screen.height;
		}
		else
		{
			return itsDataModuleMinimap.itsAppearanceMiniMap.itsSize * (float)Screen.height;//itsDataModuleMinimap.itsAppearanceMiniMap.itsSize * (float)Screen.height;
		}
	}
	
	/// <summary>
	/// Get real button size in pixels
	/// </summary>
	/// <returns></returns>
	float GetButtonSize()
	{
		if (GetFullscreen())
		{
			return itsDataModuleMinimap.itsAppearanceMap.itsButtonSize * GetWidth();
		}
		else
		{
			return itsDataModuleMinimap.itsAppearanceMiniMap.itsButtonSize * GetWidth();
		}
	}
	
	/// <summary>
	/// Get real button padding in pixels
	/// </summary>
	/// <returns></returns>
	float GetButtonPadding()
	{
		if (GetFullscreen())
		{
			return itsDataModuleMinimap.itsAppearanceMap.itsButtonPadding * GetWidth();
		}
		else
		{
			return itsDataModuleMinimap.itsAppearanceMiniMap.itsButtonPadding * GetWidth();
		}
	}
	
	#region log abstraction
	private void LogError(string theError,string theCategory,MonoBehaviour theObject)
	{
		if(!itsDataModuleMinimap.itsGlobalSettings.itsEnableLogMessages)
			return;
		#if KGFDebug
		KGFDebug.LogError(theError,theCategory,theObject);
		#else
		Debug.LogError(string.Format("{0} - {1}",theCategory,theError));
		#endif
	}
	
	private void LogWarning(string theWarning,string theCategory,MonoBehaviour theObject)
	{
		if(!itsDataModuleMinimap.itsGlobalSettings.itsEnableLogMessages)
			return;
		#if KGFDebug
		KGFDebug.LogWarning(theWarning,theCategory,theObject);
		#else
		Debug.LogWarning(string.Format("{0} - {1}",theCategory,theWarning));
		#endif
	}
	
	private void LogInfo(string theError,string theCategory,MonoBehaviour theObject)
	{
		if(!itsDataModuleMinimap.itsGlobalSettings.itsEnableLogMessages)
			return;
		#if KGFDebug
		KGFDebug.LogInfo(theError,theCategory,theObject);
		#else
		Debug.Log(string.Format("{0} - {1}",theCategory,theError));
		#endif
	}
	#endregion
	
	#region mesh creation
	/// <summary>
	/// Generate a new plane mesh. it is 1x1 in size
	/// </summary>
	/// <returns></returns>
	private Mesh GeneratePlaneMeshXZ()
	{
		Mesh aMesh = new Mesh();
		
		Vector3[] aVertices = new Vector3[4];
		aVertices[0] = new Vector3(0.0f,0.0f,0.0f);
		aVertices[1] = new Vector3(1.0f,0.0f,0.0f);
		aVertices[2] = new Vector3(1.0f,0.0f,1.0f);
		aVertices[3] = new Vector3(0.0f,0.0f,1.0f);
		
		Vector3[] aNormals = new Vector3[4];
		aNormals[0] = new Vector3(0.0f,1.0f,0.0f);
		aNormals[1] = new Vector3(0.0f,1.0f,0.0f);
		aNormals[2] = new Vector3(0.0f,1.0f,0.0f);
		aNormals[3] = new Vector3(0.0f,1.0f,0.0f);
		
		Vector2[] anUVs = new Vector2[4];
		anUVs[0] = new Vector2(0.0f,0.0f);
		anUVs[1] = new Vector2(1.0f,0.0f);
		anUVs[2] = new Vector2(1.0f,1.0f);
		anUVs[3] = new Vector2(0.0f,1.0f);
		
		
		int[] aTriangles = new int[6];
		aTriangles[0] = 0;
		aTriangles[1] = 3;
		aTriangles[2] = 2;
		aTriangles[3] = 0;
		aTriangles[4] = 2;
		aTriangles[5] = 1;
		
		aMesh.vertices = aVertices;
		aMesh.normals = aNormals;
		aMesh.uv = anUVs;
		aMesh.triangles = aTriangles;
		
		return aMesh;
	}
	
	/// <summary>
	/// Generate a new plane mesh. it is 1x1 in size
	/// </summary>
	/// <returns></returns>
	private static Mesh GeneratePlaneMeshXZCentered()
	{
		Mesh aMesh = new Mesh();
		
		Vector3[] aVertices = new Vector3[4];
		aVertices[0] = new Vector3(-0.5f,0.0f,-0.5f);
		aVertices[1] = new Vector3(0.5f,0.0f,-0.5f);
		aVertices[2] = new Vector3(0.5f,0.0f,0.5f);
		aVertices[3] = new Vector3(-0.5f,0.0f,0.5f);
		
		Vector3[] aNormals = new Vector3[4];
		aNormals[0] = new Vector3(0.0f,1.0f,0.0f);
		aNormals[1] = new Vector3(0.0f,1.0f,0.0f);
		aNormals[2] = new Vector3(0.0f,1.0f,0.0f);
		aNormals[3] = new Vector3(0.0f,1.0f,0.0f);
		
		Vector2[] anUVs = new Vector2[4];
		anUVs[0] = new Vector2(0.0f,0.0f);
		anUVs[1] = new Vector2(1.0f,0.0f);
		anUVs[2] = new Vector2(1.0f,1.0f);
		anUVs[3] = new Vector2(0.0f,1.0f);
		
		
		int[] aTriangles = new int[6];
		aTriangles[0] = 0;
		aTriangles[1] = 3;
		aTriangles[2] = 2;
		aTriangles[3] = 0;
		aTriangles[4] = 2;
		aTriangles[5] = 1;
		
		aMesh.vertices = aVertices;
		aMesh.normals = aNormals;
		aMesh.uv = anUVs;
		aMesh.triangles = aTriangles;
		
		return aMesh;
	}
	
//	Mesh CreateHexagonMesh()
//	{
//		Mesh aMesh = new Mesh();
//
//		Vector3[] aVertices = new Vector3[4];
//		aVertices[0] = new Vector3(0.0f,0.0f,0.0f);
//		aVertices[1] = new Vector3(1.0f,0.0f,0.0f);
//		aVertices[2] = new Vector3(1.0f,1.0f,0.0f);
//		aVertices[3] = new Vector3(0.0f,1.0f,0.0f);
//
//		Vector3[] aNormals = new Vector3[4];
//		aNormals[0] = new Vector3(0.0f,0.0f,1.0f);
//		aNormals[1] = new Vector3(0.0f,0.0f,1.0f);
//		aNormals[2] = new Vector3(0.0f,0.0f,1.0f);
//		aNormals[3] = new Vector3(0.0f,0.0f,1.0f);
//
	////		Vector2[] anUVs = new Vector2[4];
	////		anUVs[0] = new Vector2(0.0f,0.0f);
	////		anUVs[1] = new Vector2(1.0f,0.0f);
	////		anUVs[2] = new Vector2(1.0f,1.0f);
	////		anUVs[3] = new Vector2(0.0f,1.0f);
//
//
//		int[] aTriangles = new int[6];
//		aTriangles[0] = 0;
//		aTriangles[1] = 3;
//		aTriangles[2] = 2;
//		aTriangles[3] = 0;
//		aTriangles[4] = 2;
//		aTriangles[5] = 1;
//
//		aMesh.vertices = aVertices;
//		aMesh.normals = aNormals;
	////		aMesh.uv = anUVs;
//		aMesh.triangles = aTriangles;
//
//		return aMesh;
//	}
	
	Mesh CreatePlaneMesh(int theWidth,int theHeight)
	{
		Mesh aMesh = new Mesh();
		
		Vector3[] aVertices = new Vector3[(theWidth+1)*(theHeight+1)];
		
		for (int y=0;y<=theHeight;y++)
		{
			for (int x=0;x<=theWidth;x++)
			{
				aVertices[y*(theWidth+1)+x] = new Vector3(x,0.0f,y);
			}
		}
		
		Vector3[] aNormals = new Vector3[aVertices.Length];
		for (int i=0;i<aNormals.Length;i++)
		{
			aNormals[i] = new Vector3(0.0f,1.0f,0.0f);
		}
		
		int[] aTriangles = new int[aVertices.Length * 2 * 3];
		int aTriangleCurrent = 0;
		
		for (int y=0;y<theHeight;y++)
		{
			for (int x=0;x<theWidth;x++)
			{
				int index = y*(theWidth+1) + x;
				
				if (index%2 == 0)
				{
					aTriangles[aTriangleCurrent++] = index;
					aTriangles[aTriangleCurrent++] = index+(theWidth+2);
					aTriangles[aTriangleCurrent++] = index+(theWidth+1);
					aTriangles[aTriangleCurrent++] = index;
					aTriangles[aTriangleCurrent++] = index+1;
					aTriangles[aTriangleCurrent++] = index+theWidth+2;
				}else
				{
					aTriangles[aTriangleCurrent++] = index;
					aTriangles[aTriangleCurrent++] = index+1;
					aTriangles[aTriangleCurrent++] = index+(theWidth+1);
					
					aTriangles[aTriangleCurrent++] = index+1;
					aTriangles[aTriangleCurrent++] = index+theWidth+2;
					aTriangles[aTriangleCurrent++] = index+(theWidth+1);
				}
			}
		}
		
		Vector2[] anUVs = new Vector2[aVertices.Length];
		for (int i=0;i<anUVs.Length;i++)
		{
			anUVs[i] = Vector2.zero;
		}
		
		aMesh.vertices = aVertices;
		aMesh.normals = aNormals;
		aMesh.uv = anUVs;
		aMesh.triangles = aTriangles;
		
		return aMesh;
	}
	#endregion
	
	#region fog of war
	const string itsSaveIDFogOfWarValues = "minimap_FogOfWar_values";
	
	/// <summary>
	/// Serialize fog of war to string
	/// </summary>
	/// <returns></returns>
	string SerializeFogOfWar()
	{
		if (itsMeshFilterFogOfWarPlane != null)
		{
			if (itsMeshFilterFogOfWarPlane.mesh.colors != null)
			{
				string[] anArrayAlphas = new string[itsMeshFilterFogOfWarPlane.mesh.vertices.Length];
				for (int i=0;i<itsMeshFilterFogOfWarPlane.mesh.vertices.Length;i++)
				{
					anArrayAlphas[i] = ""+itsMeshFilterFogOfWarPlane.mesh.colors[i].a;
				}
				string aSaveString = string.Join(";",anArrayAlphas);
				return aSaveString;
			}
		}
		return null;
	}
	
	/// <summary>
	/// Save current fog of war state
	/// </summary>
	void Save(string theSaveGameName)
	{
		string aSaveString = SerializeFogOfWar();
		if (aSaveString != null)
		{
			PlayerPrefs.SetString(theSaveGameName+itsSaveIDFogOfWarValues,aSaveString);
		}
	}
	
	/// <summary>
	/// Load fog of war state from previously serialized string
	/// </summary>
	/// <param name="theSaveString"></param>
	void DeserializeFogOfWar(string theSavedString)
	{
		if (theSavedString != null)
		{
			if (itsMeshFilterFogOfWarPlane != null)
			{
				if (itsMeshFilterFogOfWarPlane.mesh.colors != null)
				{
					Color[] aColorArray = itsMeshFilterFogOfWarPlane.mesh.colors;
					string[] anArrayAlphas = theSavedString.Split(';');
					if (anArrayAlphas.Length == aColorArray.Length)
					{
						for (int i=0;i<anArrayAlphas.Length;i++)
						{
							try{
								aColorArray[i].a = float.Parse(anArrayAlphas[i]);
							}
							catch
							{
								LogError("Could not parse saved fog of war",name,this);
								return;
							}
						}
						itsMeshFilterFogOfWarPlane.mesh.colors = aColorArray;
					}else
					{
						LogError("Saved fog of war size different from current.",name,this);
					}
				}
			}
		}
		else
		{
			LogError("No saved fog of war to load.",name,this);
		}
	}
	
	/// <summary>
	/// Load fog of war state
	/// </summary>
	void Load(string theSaveGameName)
	{
		string aSavedString = PlayerPrefs.GetString(theSaveGameName+itsSaveIDFogOfWarValues,null);
		DeserializeFogOfWar(aSavedString);
	}
	
	/// <summary>
	/// Reveals the whole fog of war
	/// </summary>
	void RevealFogOfWar()
	{
		if(itsMeshFilterFogOfWarPlane == null)
			return;
		if(itsMeshFilterFogOfWarPlane.mesh == null)
			return;
		
		Color[] aColorArray = itsMeshFilterFogOfWarPlane.mesh.colors;
		for(int i = 0; i< aColorArray.Length; i++)
		{
			aColorArray[i].a = 0.0f;
		}
		itsMeshFilterFogOfWarPlane.mesh.colors = aColorArray;
	}
	
	GameObject CreateFogOfWarPlane()
	{
		GameObject aGameobject = new GameObject("fog_of_war");
		aGameobject.transform.parent = transform;
		aGameobject.layer = itsLayerMinimap;
		
		MeshFilter aFilter = aGameobject.AddComponent<MeshFilter>();
		aFilter.mesh = CreatePlaneMesh((int)itsDataModuleMinimap.itsFogOfWar.itsResolutionX,(int)itsDataModuleMinimap.itsFogOfWar.itsResolutionY);
		MeshRenderer aRenderer = aGameobject.AddComponent<MeshRenderer>();
		aRenderer.material = new Material(itsDataModuleMinimap.itsShaders.itsShaderFogOfWar);
		return aGameobject;
	}
	
	public void InitFogOfWar()
	{
		itsScalingFogOfWar = new Vector2(itsSizeTerrain.x / itsDataModuleMinimap.itsFogOfWar.itsResolutionX,itsSizeTerrain.y / itsDataModuleMinimap.itsFogOfWar.itsResolutionY);
		
		if (itsMeshFilterFogOfWarPlane != null)
			Destroy(itsMeshFilterFogOfWarPlane.gameObject);
		itsMeshFilterFogOfWarPlane = CreateFogOfWarPlane().GetComponent<MeshFilter>();
		Vector3 aPlanePosition = itsTerrainBoundsPhoto.center - itsTerrainBoundsPhoto.extents;
		aPlanePosition = ChangeVectorHeight(aPlanePosition,GetHeightFog());
		
		if (itsDataModuleMinimap.itsGlobalSettings.itsOrientation == KGFMapSystemOrientation.XYSideScroller)
		{
			itsMeshFilterFogOfWarPlane.transform.eulerAngles = new Vector3(270,0,0);
//			aPlanePosition = ChangeVectorHeight(aPlanePosition,-GetHeightFog());
		}
		
		itsMeshFilterFogOfWarPlane.transform.position = aPlanePosition;
		itsMeshFilterFogOfWarPlane.transform.localScale = new Vector3(itsScalingFogOfWar.x,1,
		                                                              itsScalingFogOfWar.y);
		
		Color [] aColorArray = itsMeshFilterFogOfWarPlane.mesh.colors;
		aColorArray = new Color[itsMeshFilterFogOfWarPlane.mesh.vertexCount];
		for (int i=0;i<aColorArray.Length;i++)
		{
			aColorArray[i] = itsDataModuleMinimap.itsGlobalSettings.itsColorBackground;//itsColorFogOfWarBlack;
		}
		itsMeshFilterFogOfWarPlane.mesh.colors = aColorArray;
		
		itsFOWColors = aColorArray;
		itsFOWVertices = itsMeshFilterFogOfWarPlane.mesh.vertices;
	}
	
	Color[] itsFOWColors = null;
	Vector3 [] itsFOWVertices = null;
	
//	void PaintFogOfWarPixel(Vector2 thePoint)
//	{
//		if (itsTextureFogWar != null)
//		{
//			if (thePoint.x >= 0 && thePoint.x < itsTextureFogWar.width &&
//			    thePoint.y >= 0 && thePoint.y < itsTextureFogWar.height)
//			{
//				itsTextureFogWar.SetPixel((int)thePoint.x,(int)thePoint.y,new Color(1,1,1));
//			}
//		}
//	}
//
//	void PaintFogOfWarCircle(Vector2 thePoint, int theRadius)
//	{
//		if (itsTextureFogWar != null)
//		{
//			Vector2 aCurrentPoint = new Vector2();
//			for (int x=(int)thePoint.x-theRadius;x<=thePoint.x+theRadius;x++)
//			{
//				for (int y=(int)thePoint.y-theRadius;y<=thePoint.y+theRadius;y++)
//				{
//					aCurrentPoint.x = x;
//					aCurrentPoint.y = y;
//					if (Vector2.Distance(thePoint,aCurrentPoint) <= theRadius)
//					{
//						PaintFogOfWarPixel(aCurrentPoint);
//					}
//				}
//			}
//			itsTextureFogWar.Apply();
//		}
//	}
	
	//	float itsTimeLastFogOfWarUpdate = 0;
	
	public void RevealFogOfWarAtPoint(Vector3 thePosition)
	{
		//TODO: this code for much better performance depends on doing animation differently
//		if (Time.time - itsTimeLastFogOfWarUpdate < 0.1f)
//		{
//			return;
//		}
//		itsTimeLastFogOfWarUpdate = Time.time;
		
		if (itsMeshFilterFogOfWarPlane == null)
		{
			return;
		}
		
		// calculate nearest vertext
		Vector2 aMyLocalPosition = GetVector3Plane(thePosition - itsMeshFilterFogOfWarPlane.transform.position);
		Vector2 aNearestVertex = new Vector2(Mathf.RoundToInt((aMyLocalPosition.x/itsSizeTerrain.x)*itsDataModuleMinimap.itsFogOfWar.itsResolutionX),
		                                     Mathf.RoundToInt((aMyLocalPosition.y/itsSizeTerrain.y)*itsDataModuleMinimap.itsFogOfWar.itsResolutionY));
//		print("nearest:"+aNearestVertex);
		// calculate how many vertexes we have to look at (width and height)
		Vector2 aNeededVertexSize = new Vector2(Mathf.RoundToInt((itsDataModuleMinimap.itsFogOfWar.itsRevealDistance/itsSizeTerrain.x)*itsDataModuleMinimap.itsFogOfWar.itsResolutionX),
		                                        Mathf.RoundToInt((itsDataModuleMinimap.itsFogOfWar.itsRevealDistance/itsSizeTerrain.y)*itsDataModuleMinimap.itsFogOfWar.itsResolutionY))*2;
//			new Vector2(5,5);//Vector2.zero;
		// itsMeshFilterFogOfWarPlane.transform.position;
		// itsSizeTerrain
		// -> itsTargetTransform.position
		Vector3 aVertex = Vector3.zero;
		int yMax = Math.Min(itsDataModuleMinimap.itsFogOfWar.itsResolutionY+1,(int)(aNearestVertex.y+aNeededVertexSize.y));
		for (int y=Math.Max(0,(int)(aNearestVertex.y-aNeededVertexSize.y));y<yMax;y++)
		{
			int xMax = Math.Min(itsDataModuleMinimap.itsFogOfWar.itsResolutionX+1,(int)(aNearestVertex.x+aNeededVertexSize.x));
			for (int x=Math.Max(0,(int)(aNearestVertex.x-aNeededVertexSize.x));x<xMax;x++)
			{
				int i = (int)(y*(itsDataModuleMinimap.itsFogOfWar.itsResolutionX+1) + x);
				aVertex = itsMeshFilterFogOfWarPlane.transform.position + new Vector3(itsFOWVertices[i].x * itsScalingFogOfWar.x,
				                                                                      itsFOWVertices[i].z * itsScalingFogOfWar.y,
				                                                                      itsFOWVertices[i].z * itsScalingFogOfWar.y);
				aVertex = ChangeVectorHeight(aVertex,GetVector3Height(thePosition));
				float aCurrentDistance = Vector3.Distance(aVertex,thePosition);
				if (aCurrentDistance < itsDataModuleMinimap.itsFogOfWar.itsRevealedFullDistance)
				{
					itsFOWColors[i] = itsColorFogOfWarRevealed;
				}
				else if (aCurrentDistance < itsDataModuleMinimap.itsFogOfWar.itsRevealDistance)
				{
					float aMin = Mathf.Min(itsFOWColors[i].a,
					                       Mathf.Clamp((aCurrentDistance-itsDataModuleMinimap.itsFogOfWar.itsRevealedFullDistance)/
					                                   (itsDataModuleMinimap.itsFogOfWar.itsRevealDistance-itsDataModuleMinimap.itsFogOfWar.itsRevealedFullDistance),0,1));
					itsFOWColors[i].a = aMin;
				}
			}
		}
		
		// always fully reveal one nearest vertex
		int j = (int)(aNearestVertex.y*(itsDataModuleMinimap.itsFogOfWar.itsResolutionX+1) + aNearestVertex.x);
		if (j >= 0 && j < itsFOWColors.Length)
		{
			itsFOWColors[j] = itsColorFogOfWarRevealed;
		}
//		for (int i=0;i<itsFOWVertices.Length;i++)
//		{
//			aVertex = itsMeshFilterFogOfWarPlane.transform.position + new Vector3(itsFOWVertices[i].x * itsScalingFogOfWar.x,
//			                                                                      0,
//			                                                                      itsFOWVertices[i].z * itsScalingFogOfWar.y);
//			aVertex.y = itsTargetTransform.position.y;
//			float aCurrentDistance = Vector3.Distance(aVertex,itsTargetTransform.position);
//			if (aCurrentDistance < itsDataModuleMinimap.itsFogOfWarRevealedFullDistance)
//			{
//				aColorArray[i] = itsColorFogOfWarRevealed;
//			}
//			else if (aCurrentDistance < itsDataModuleMinimap.itsFogOfWarRevealDistance)
//			{
//				float aMin = Mathf.Min(aColorArray[i].a,
//				                       Mathf.Clamp((aCurrentDistance-itsDataModuleMinimap.itsFogOfWarRevealedFullDistance)/
//				                                   (itsDataModuleMinimap.itsFogOfWarRevealDistance-itsDataModuleMinimap.itsFogOfWarRevealedFullDistance),0,1));
//				aColorArray[i].a = aMin;
//			}
//		}
		
		itsMeshFilterFogOfWarPlane.mesh.colors = itsFOWColors;
	}
	#endregion
	
	#region viewport
	GameObject itsGameObjectViewPort = null;
	Mesh itsViewPortCubeMesh = null;
	/// <summary>
	/// Update the viewport represenation in the minimap
	/// </summary>
	void UpdateViewPortCube()
	{
		if (itsDataModuleMinimap.itsGlobalSettings.itsOrientation == KGFMapSystemOrientation.XYSideScroller)
		{
			return;
		}
		if (itsDataModuleMinimap.itsGlobalSettings.itsTarget == null)
		{
			return;
		}
		if (itsDataModuleMinimap.itsViewport.itsCamera == null)
		{
			return;
		}
		
		// create viewport
		if (itsViewPortCubeMesh == null)
		{
//			GameObject aGameObjectCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			itsGameObjectViewPort = new GameObject();
			itsGameObjectViewPort.AddComponent<MeshFilter>().mesh = GeneratePlaneMeshXZ();
			
			
			itsMaterialViewport = new Material(itsDataModuleMinimap.itsShaders.itsShaderMapIcon);
			itsGameObjectViewPort.AddComponent<MeshRenderer>().material = itsMaterialViewport;
			itsGameObjectViewPort.name = "minimap viewport";
			itsGameObjectViewPort.transform.parent = transform;
			SetLayerRecursively(itsGameObjectViewPort,itsLayerMinimap);
			itsViewPortCubeMesh = itsGameObjectViewPort.GetComponent<MeshFilter>().mesh;
		}
		
		if (KGFGetActive(itsGameObjectViewPort) != itsDataModuleMinimap.itsViewport.itsActive)
		{
			KGFSetChildrenActiveRecursively(itsGameObjectViewPort,itsDataModuleMinimap.itsViewport.itsActive);
		}
		
		if (!itsDataModuleMinimap.itsViewport.itsActive)
		{
			return;
		}
		
		// change vertices of the viewport
		Vector3 []aVertexList = itsViewPortCubeMesh.vertices;
		
		aVertexList[1] = itsDataModuleMinimap.itsViewport.itsCamera.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height/2,itsDataModuleMinimap.itsViewport.itsCamera.farClipPlane));
		aVertexList[2] = itsDataModuleMinimap.itsViewport.itsCamera.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height/2,itsDataModuleMinimap.itsViewport.itsCamera.nearClipPlane));
		aVertexList[3] = itsDataModuleMinimap.itsViewport.itsCamera.ScreenToWorldPoint(new Vector3(0,Screen.height/2,itsDataModuleMinimap.itsViewport.itsCamera.nearClipPlane));
		aVertexList[0] = itsDataModuleMinimap.itsViewport.itsCamera.ScreenToWorldPoint(new Vector3(0,Screen.height/2,itsDataModuleMinimap.itsViewport.itsCamera.farClipPlane));
		
		for (int i=0;i<4;i++)
		{
			aVertexList[i].y = GetHeightViewPort();
		}
		
//		aVertexList[4] = itsViewPortCamera.ScreenToWorldPoint(new Vector3(0,0,itsViewPortCamera.farClipPlane));
//		aVertexList[5] = itsViewPortCamera.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height,itsViewPortCamera.farClipPlane));
//		aVertexList[6] = itsViewPortCamera.ScreenToWorldPoint(new Vector3(0,Screen.height,itsViewPortCamera.farClipPlane));
//		aVertexList[7] = itsViewPortCamera.ScreenToWorldPoint(new Vector3(Screen.width,0,itsViewPortCamera.farClipPlane));
		
		itsViewPortCubeMesh.vertices = aVertexList;
		itsViewPortCubeMesh.RecalculateBounds();
		
		itsMaterialViewport.SetColor("_Color",itsDataModuleMinimap.itsViewport.itsColor);
	}
	#endregion
	
	#region bounds
	Bounds? GetBoundsOfTerrain(GameObject theTerrain)
	{
		// measure terrain
		MeshRenderer aMeshrenderer = theTerrain.GetComponent<MeshRenderer>();
		if (aMeshrenderer != null)
		{
			return aMeshrenderer.bounds;
		}
		else
		{
			TerrainCollider aTerrain = theTerrain.GetComponent<TerrainCollider>();
			if (aTerrain != null)
			{
				return aTerrain.bounds;
			}
			else
			{
				// could not measure terrain
				LogError("Could not get measure bounds of terrain.",name,this);
				return null;
			}
		}
	}
	
	void InitLayer()
	{
		if (itsLayerMinimap < 0)
		{
			itsLayerMinimap = LayerMask.NameToLayer(itsLayerName);
		}
	}
	
	public static bool IsInLayerMask(GameObject obj, LayerMask mask)
	{
		return ((mask.value & (1 << obj.layer)) > 0);
	}
	
	/// <summary>
	/// Create bounding box and scale it to contain all scene game objects, if terrain is found it is used
	/// </summary>
	bool GetMeasuredBounds(LayerMask theLayers, out Bounds theBounds)
	{
		Bounds ?aBounds = null;
		
		InitLayer();
		
		// try to find terrain
		if (Terrain.activeTerrain != null)
		{
			aBounds = GetBoundsOfTerrain(Terrain.activeTerrain.gameObject);
		}
		
		// measure all game objects
		Renderer []aListRenderers = UnityEngine.Object.FindObjectsOfType(typeof(Renderer)) as Renderer[];
		if (aListRenderers != null)
		{
			foreach (Renderer aRenderer in aListRenderers)
			{
				// do not add self
				if (aRenderer.gameObject.layer == itsLayerMinimap)
					continue;
				// only use stuff on the layers itsLayersBoundaries
				if (!IsInLayerMask(aRenderer.gameObject,theLayers))
					continue;
				
				{
					if (aBounds == null)
					{
						aBounds = aRenderer.bounds;
					}
					else
					{
						Bounds aLocalBounds = aBounds.Value;
						aLocalBounds.Encapsulate(aRenderer.bounds);
						aBounds = aLocalBounds;
					}
				}
			}
		}
		
		if (!aBounds.HasValue)
		{
			// could not measure bounds
			LogError("Could not find terrain nor any other bounds in scene",name,this);
			theBounds = new Bounds();
			return false;
		}
		
		theBounds = aBounds.Value;
//		print("Bounds:"+itsSizeTerrain+"//"+itsTerrainBounds);
		return true;
	}
	#endregion
	
	#region auto photo
	float GetHighestNPOTSizeSmallerThanScreen()
	{
		float aSizeMax = Screen.width;
		if (Screen.height < aSizeMax)
			aSizeMax = Screen.height;
		float aSize = 1;
		while (aSize <= aSizeMax)
		{
			aSize *= 2;
		}
		return aSize/2;
	}

	public void TakePhotoOfScene (bool theStartedFromEditor)
	{
		MeasureScene();
		AutoCreatePhoto();
	}

	private void ClearPhotoData()
	{
		if (Application.isPlaying)
		{
			foreach(KGFPhotoData aPhotoData in itsListOfPhotoData)
			{
				Destroy(aPhotoData.itsPhotoPlane);
				Destroy(aPhotoData.itsTexture);
			}
		}
		itsListOfPhotoData.Clear();
	}
	
	private KGFPhotoCapture CreatePhotoCamera(float anOrtographicSize, float aTextureSize)
	{
		// create temp camera
		itsTempCameraGameObject = new GameObject("TempCamera");
		switch (itsDataModuleMinimap.itsGlobalSettings.itsOrientation)
		{
			case KGFMapSystemOrientation.XYSideScroller:
				itsTempCameraGameObject.transform.eulerAngles = new Vector3(0,0,0);
				break;
			case KGFMapSystemOrientation.XZDefault:
				itsTempCameraGameObject.transform.eulerAngles = new Vector3(90,0,0);
				break;
//			case KGFMapSystemOrientation.YZ:
//				itsTempCameraGameObject.transform.eulerAngles = new Vector3(0,270,270);
//				break;
		}
		
		Camera aTempCamera = itsTempCameraGameObject.AddComponent<Camera>();
		aTempCamera.depth = -100;
		
		aTempCamera.clearFlags = CameraClearFlags.SolidColor;
		aTempCamera.backgroundColor = Color.red;
		aTempCamera.isOrthoGraphic = true;
		
		// position camera over terrain
		if(GetOrientation() == KGFMapSystemOrientation.XZDefault)
			aTempCamera.farClipPlane = 2.0f + itsTerrainBoundsPhoto.size.y*2.0f;
		else
			aTempCamera.farClipPlane = 2.0f + itsTerrainBoundsPhoto.size.z*2.0f;
		aTempCamera.cullingMask = itsDataModuleMinimap.itsPhoto.itsPhotoLayers;
		aTempCamera.backgroundColor = itsDataModuleMinimap.itsGlobalSettings.itsColorBackground;
		aTempCamera.clearFlags = CameraClearFlags.SolidColor;
		aTempCamera.aspect = 1;
		aTempCamera.orthographicSize = anOrtographicSize/2.0f;
		aTempCamera.pixelRect = new Rect(0,0,aTextureSize,aTextureSize);
		
		KGFSetChildrenActiveRecursively(aTempCamera.gameObject,true);
		aTempCamera.enabled = true;
		
		KGFPhotoCapture aPhotoCapture = itsTempCameraGameObject.AddComponent<KGFPhotoCapture>();
		aPhotoCapture.itsMapSystem = this;
		
		return aPhotoCapture;
	}
	
	private GameObject itsGameObjectPhotoParent = null;
	void AutoCreatePhoto()
	{
		// try to find photo parent
		if (itsGameObjectPhotoParent == null)
		{
			Transform aTransform = transform.Find("photo");
			if (aTransform != null)
			{
				itsGameObjectPhotoParent = aTransform.gameObject;
			}
		}
		
		if(itsTempCameraGameObject != null)
		{
			if(Application.isPlaying)
				Destroy(itsTempCameraGameObject);
			else
				DestroyImmediate(itsTempCameraGameObject);
		}
		
		if (itsGameObjectPhotoParent != null)
		{
			if (Application.isPlaying)
				GameObject.Destroy(itsGameObjectPhotoParent);
			else
				GameObject.DestroyImmediate(itsGameObjectPhotoParent);
		}
		itsGameObjectPhotoParent = new GameObject("photo");
		itsGameObjectPhotoParent.transform.parent = transform;
		
		Vector2 aTerrainBoundsMin = GetVector3Plane(itsTerrainBoundsPhoto.min);
		Vector2 aTerrainBoundsMax = GetVector3Plane(itsTerrainBoundsPhoto.max);
		
		{
			float aTextureSize = GetHighestNPOTSizeSmallerThanScreen();
			float aMeters = aTextureSize / itsDataModuleMinimap.itsPhoto.itsPixelPerMeter;
			float aMetersBorder = aTextureSize / itsDataModuleMinimap.itsPhoto.itsPixelPerMeter;
			
			int i = 0;
			int j = 0;
			
			ClearPhotoData();
			while (true)
			{
				KGFPhotoData aPhotoData = new KGFPhotoData();
				aPhotoData.itsMeters = aMeters;
				Vector3 aDirection = itsTerrainBoundsPhoto.max - itsTerrainBoundsPhoto.min;
				aDirection = ChangeVectorHeight(aDirection,0); // aDirection.y = 0;
				
				
				switch (itsDataModuleMinimap.itsGlobalSettings.itsOrientation)
				{
					case KGFMapSystemOrientation.XYSideScroller:
						aPhotoData.itsPosition = CreateVector(aTerrainBoundsMin.x + aMeters * i,
						                                      aTerrainBoundsMin.y + aMeters * j,
						                                      itsTerrainBoundsPhoto.max.z + 1);
						break;
					case KGFMapSystemOrientation.XZDefault:
						aPhotoData.itsPosition = CreateVector(aTerrainBoundsMin.x + aMeters * i,
						                                      aTerrainBoundsMin.y + aMeters * j,
						                                      itsTerrainBoundsPhoto.min.y - 1);
						break;
				}
				
				// render to texture
				aPhotoData.itsTexture = new Texture2D((int)aTextureSize,(int)aTextureSize,TextureFormat.ARGB32,false);
				{
					aPhotoData.itsTextureSize = aTextureSize;
//					aPhotoData.itsTexture.wrapMode = TextureWrapMode.Clamp;
				}
				
				// show photo in plane under terrain
				{
					// create plane
					// create material with photo
					// assign material to plane
					GameObject aPhotoPlane = GeneratePhotoPlane(aPhotoData);
					KGFSetChildrenActiveRecursively(aPhotoPlane,false);
					aPhotoPlane.transform.parent = itsGameObjectPhotoParent.transform;
					aPhotoPlane.name = aPhotoPlane.name+"_"+i+"_"+j;
					
					// use minimap layer
					SetLayerRecursively(aPhotoPlane.gameObject,itsLayerMinimap);
					
					// rescale plane
					Vector3 aPhotoPlanePosition = aPhotoData.itsPosition;// - Vector3.one * aMeters;
//					aPhotoPlanePosition = ChangeVectorHeight(aPhotoPlanePosition,GetHeightPhoto());//aPhotoPlanePosition.y = itsFixedHeightPhoto;
					aPhotoPlane.transform.position = aPhotoPlanePosition;
					aPhotoPlane.transform.localScale = new Vector3(aMeters,1,aMeters); // CreateVector(aMeters,aMeters,1);
					switch (itsDataModuleMinimap.itsGlobalSettings.itsOrientation)
					{
						case KGFMapSystemOrientation.XYSideScroller:
							aPhotoPlane.transform.localEulerAngles = new Vector3(270,0,0);
							break;
						case KGFMapSystemOrientation.XZDefault:
							aPhotoPlane.transform.localEulerAngles = Vector3.zero;
							break;
//						case KGFMapSystemOrientation.YZ:
//							aPhotoPlane.transform.localEulerAngles = new Vector3(0,0,270);
//							break;
					}
//					KGFSetChildrenActiveRecursively(aPhotoPlane,true);
					
					aPhotoData.itsPhotoPlane = aPhotoPlane;
				}
				itsListOfPhotoData.Add(aPhotoData);
				
				i++;
				if (aTerrainBoundsMin.x + (i)*aMeters > aTerrainBoundsMax.x)
				{
					i = 0;
					j++;
				}
				if (aTerrainBoundsMin.y + (j)*aMeters > aTerrainBoundsMax.y)
				{
					break;
				}
			}
			itsArrayOfPhotoData = itsListOfPhotoData.ToArray();
			SetColors();
			//everything prepared for photo session. Create the camera. It will destroy itself after the photosession.
			CreatePhotoCamera(aMetersBorder,aTextureSize);
		}
	}
	
	public KGFPhotoData[] GetPhotoData()
	{
		if (itsListOfPhotoData != null)
		{
			return itsListOfPhotoData.ToArray();
		}
		return null;
	}
	
	public GameObject GetPhotoParent()
	{
		return itsGameObjectPhotoParent;
	}
	
	/// <summary>
	/// returns the photodata at the current index
	/// returns null if no photodata available or index > size
	/// </summary>
	/// <returns></returns>
	public KGFPhotoData GetNextPhotoData()
	{
		if(itsArrayOfPhotoDataIndex < itsArrayOfPhotoData.Length)
		{
			itsArrayOfPhotoDataIndex++;
			return itsArrayOfPhotoData[itsArrayOfPhotoDataIndex-1];
		}
		else
		{
			return null;
		}
	}
	
	#endregion
	
	/// <summary>
	/// Set layer recursively
	/// </summary>
	/// <param name="theGameObject"></param>
	/// <param name="theLayer"></param>
	void SetLayerRecursively(GameObject theGameObject, int theLayer)
	{
		theGameObject.layer = theLayer;
		foreach(Transform aTransform in theGameObject.transform)
		{
			GameObject aGameObject = aTransform.gameObject;
			SetLayerRecursively(aGameObject,theLayer);
		}
	}
	
	/// <summary>
	/// creates the ortographic minimap camera and assigns all parameters
	/// </summary>
	void CreateCameras()
	{
		GameObject aCamera = new GameObject("minimapcamera");
		aCamera.transform.parent = transform;
		itsCamera = aCamera.AddComponent<Camera>();
		itsCamera.aspect = 1.0f;
		itsCamera.orthographic = true;
		itsCamera.clearFlags = CameraClearFlags.SolidColor;
//		itsCamera.cullingMask = 1 << itsLayerMinimap;
		itsCamera.backgroundColor = itsDataModuleMinimap.itsGlobalSettings.itsColorBackground;
		
		itsCameraTransform = itsCamera.transform;
//		itsCameraTransform.position = new Vector3(0.0f,itsTerrainBounds.max.y + 100,0.0f);
//		itsCameraTransform.rotation = Quaternion.Euler(90.0f,0.0f,0.0f);
		
//		switch (itsDataModuleMinimap.itsGlobalSettings.itsOrientation)
//		{
//			case KGFMapSystemOrientation.XY:
//				itsCameraTransform.eulerAngles = new Vector3(180,0,0);
//				break;
//			case KGFMapSystemOrientation.XZ:
//				itsCameraTransform.eulerAngles = new Vector3(90,0,0);
//				break;
//			case KGFMapSystemOrientation.YZ:
//				itsCameraTransform.eulerAngles = new Vector3(0,270,270);
//				break;
//		}
		
		aCamera = new GameObject("outputcamera");
		itsCameraOutput = aCamera.AddComponent<Camera>();
		itsCameraOutput.transform.parent = itsCamera.transform;
		itsCameraOutput.transform.localPosition = Vector3.zero;
		itsCameraOutput.transform.localEulerAngles = new Vector3(0,180,0);
		itsCameraOutput.transform.localScale = Vector3.one;
		itsCameraOutput.orthographic = true;
		itsCameraOutput.clearFlags = CameraClearFlags.Depth;
		itsCameraOutput.depth = 50;
		itsCameraOutput.cullingMask = 1 << itsLayerMinimap;
		
		itsCurrentZoom = itsDataModuleMinimap.itsZoom.itsZoomStartValue;
		itsCurrentZoomDest = itsCurrentZoom;
		UpdateOrthographicSize();
	}

	/// <summary>
	/// creates the rendertexture the minmap camera will render to
	/// </summary>
	void CreateRenderTexture()
	{
		itsRendertexture = new RenderTexture(2048,2048,16,RenderTextureFormat.ARGB32);
		if(itsRendertexture != null)
		{
			itsRendertexture.isPowerOfTwo = true;
			itsRendertexture.name = "minimap_rendertexture";
			itsRendertexture.Create();
			itsCamera.targetTexture = itsRendertexture;
		}
		else
		{
			LogError("cannot create rendertexture for minimap",name,this);
		}
	}
	
//	public UISprite itsSprite;
	
	/// <summary>
	/// For performance reasons this method should not be used.
	/// Include the RenderGUI method into your one and only OnGUI methos of
	/// your project and delete this OnGUI method.
	/// </summary>
	void OnGUI()	//only for debug purpose
	{
		RenderGUI();
	}
	
	void OnMapIconAdd(object theSender, EventArgs theArgs)
	{
		KGFAccessor.KGFAccessorEventargs anEventArgs = theArgs as KGFAccessor.KGFAccessorEventargs;
		if (anEventArgs != null)
		{
			KGFIMapIcon aMapIcon = anEventArgs.GetObject() as KGFIMapIcon;
			if (aMapIcon != null)
			{
				RegisterIcon(aMapIcon);
			}
		}
	}
	
	void OnMapIconRemove(object theSender, EventArgs theArgs)
	{
		KGFAccessor.KGFAccessorEventargs anEventArgs = theArgs as KGFAccessor.KGFAccessorEventargs;
		if (anEventArgs != null)
		{
			KGFIMapIcon aMapIcon = anEventArgs.GetObject() as KGFIMapIcon;
			if (aMapIcon != null)
			{
				UnregisterMapIcon(aMapIcon);
			}
		}
	}
	
	KGFMapIcon CreateIconInternal(Vector3 theWorldPoint,KGFMapIcon theIcon,Transform theParent)
	{
		GameObject aGameObject = (GameObject)GameObject.Instantiate(theIcon.gameObject);
		aGameObject.name = "Flag";
		aGameObject.transform.parent = itsContainerFlags;
		aGameObject.transform.position = theWorldPoint;
		return aGameObject.GetComponent<KGFMapIcon>();
	}
	
	/// <summary>
	/// generates the 2D Plane that will show the rendertexture of the minimap camera.
	/// This plane will be alighted in the correspronding corner in the viewport of the Camera.main
	/// </summary>
	/// <returns></returns>
	private GameObject GenerateMinimapPlane()
	{
		GameObject aMiniMapPlane = new GameObject("output_plane");
		aMiniMapPlane.layer = itsLayerMinimap;
		aMiniMapPlane.transform.parent = transform;
		itsMinimapMeshFilter = aMiniMapPlane.AddComponent<MeshFilter>();
		itsMinimapMeshFilter.mesh = GeneratePlaneMeshXZ();
		
		itsMeshRendererMinimapPlane = aMiniMapPlane.gameObject.AddComponent<MeshRenderer>();
		itsMeshRendererMinimapPlane.material = new Material(itsDataModuleMinimap.itsShaders.itsShaderMapMask);
		itsMeshRendererMinimapPlane.material.SetTexture("_Mask",itsDataModuleMinimap.itsAppearanceMiniMap.itsMask);
		
		itsMaterialMaskedMinimap = itsMeshRendererMinimapPlane.material;
		return aMiniMapPlane;
	}
	
	public void SetMask(Texture2D theMinimapMask, Texture2D theMapMask)
	{
		if(itsMeshRendererMinimapPlane.material == null)
			return;
		
		itsDataModuleMinimap.itsAppearanceMiniMap.itsMask = theMinimapMask;
		itsDataModuleMinimap.itsAppearanceMap.itsMask = theMapMask;
		
		UpdateMaskTexture();
	}

	/// <summary>
	/// Generate a new plane mesh on a new gameobject for a map icon
	/// </summary>
	/// <param name="theTexture"></param>
	/// <returns></returns>
	public static GameObject GenerateTexturePlane(Texture2D theTexture, Shader theShader)
	{
		GameObject aGameObject = new GameObject("MapIconPlane");
		MeshFilter aMiniMapPlane = aGameObject.AddComponent<MeshFilter>();
		aMiniMapPlane.transform.eulerAngles = new Vector3(0,0,0);
		aMiniMapPlane.mesh = GeneratePlaneMeshXZCentered();
		
		MeshRenderer aMeshRenderer = aMiniMapPlane.gameObject.AddComponent<MeshRenderer>();
		aMeshRenderer.material = new Material(theShader);
		aMeshRenderer.material.mainTexture = theTexture;
		aMeshRenderer.castShadows = false;
		aMeshRenderer.receiveShadows = false;
		
		return aGameObject;
	}

	/// <summary>
	/// Generate a new photo plane
	/// </summary>
	/// <returns></returns>
	private GameObject GeneratePhotoPlane(KGFPhotoData thePhotoData)
	{
//		GameObject aMiniMapPlaneRoot = new GameObject("minimap_photo_plane");
//		aMiniMapPlaneRoot.transform.parent = transform;
//		aMiniMapPlaneRoot.transform.localPosition = Vector3.zero;
//		aMiniMapPlaneRoot.transform.localRotation = Quaternion.identity;
//		aMiniMapPlaneRoot.transform.localScale = Vector3.one;
		
		GameObject aGameObject = new GameObject("photo_plane");
		aGameObject.layer = itsLayerMinimap;
		aGameObject.transform.parent = transform;
		MeshFilter aMiniMapPlane = aGameObject.AddComponent<MeshFilter>();
		aMiniMapPlane.transform.eulerAngles = Vector3.zero;
		aMiniMapPlane.transform.position = Vector3.zero;
		aMiniMapPlane.mesh = GeneratePlaneMeshXZ();
		
		MeshRenderer aMeshRenderer = aMiniMapPlane.gameObject.AddComponent<MeshRenderer>();
		aMeshRenderer.castShadows = false;
		aMeshRenderer.receiveShadows = false;
		
		Material aMaterial = new Material(itsDataModuleMinimap.itsShaders.itsShaderPhotoPlane);
		aMaterial.mainTexture = thePhotoData.itsTexture;
//		aMaterial.SetColor("_Color",Color.black);
		aMeshRenderer.material = aMaterial;
//		aMeshRenderer.material.mainTexture = thePhotoData.itsTexture;
		thePhotoData.itsPhotoPlaneMaterial = aMaterial;
		return aGameObject;
	}
	
	void UpdateMinimapOutputPlane()
	{
		Camera aCameraThatFilmsPlane = itsCameraOutput;//Camera.main;
		
		if(aCameraThatFilmsPlane != null && itsMinimapPlane != null)
		{
			if(itsMinimapMeshFilter == null) return;
			
			Mesh aMiniMapMesh = itsMinimapMeshFilter.mesh;
			if(aMiniMapMesh == null) return;
			
			Rect aRect = itsTargetRect;
			aRect.y = Screen.height - aRect.y;
			
			Vector3[] aVertices = aMiniMapMesh.vertices;
			aVertices[0] = aCameraThatFilmsPlane.ScreenToWorldPoint(new Vector3(aRect.x,aRect.y - aRect.height,aCameraThatFilmsPlane.nearClipPlane+0.01f));
			aVertices[1] = aCameraThatFilmsPlane.ScreenToWorldPoint(new Vector3(aRect.x + aRect.width,aRect.y - aRect.height,aCameraThatFilmsPlane.nearClipPlane+0.01f));
			aVertices[2] = aCameraThatFilmsPlane.ScreenToWorldPoint(new Vector3(aRect.x + aRect.width,aRect.y,aCameraThatFilmsPlane.nearClipPlane+0.01f));
			aVertices[3] = aCameraThatFilmsPlane.ScreenToWorldPoint(new Vector3(aRect.x,aRect.y,aCameraThatFilmsPlane.nearClipPlane+0.01f));
			
			aMiniMapMesh.vertices = aVertices;
			aMiniMapMesh.RecalculateBounds();
			
			Vector3 aNormal = aCameraThatFilmsPlane.transform.forward;
			
			Vector3[] aNormals = aMiniMapMesh.normals;
			aNormals[0] = aNormal;
			aNormals[1] = aNormal;
			aNormals[2] = aNormal;
			aNormals[3] = aNormal;
			aMiniMapMesh.normals = aNormals;
		}
	}
	
	/// <summary>
	/// Clean click icons list from destroyed game objects
	/// </summary>
	void CleanClickIconsList()
	{
		for (int i=itsListClickIcons.Count-1;i>=0;i--)
		{
			if (itsListClickIcons[i] == null)
			{
				itsListClickIcons.RemoveAt(i);
				continue;
			}
			if (itsListClickIcons[i] is MonoBehaviour)
			{
				if (((MonoBehaviour)itsListClickIcons[i]) == null)
				{
					itsListClickIcons.RemoveAt(i);
					continue;
				}
			}
		}
	}
	
	/// <summary>
	/// set foreground or background rendering of icons (if they should disappear behind fog of war or not)
	/// </summary>
	/// <param name="anIcon"></param>
	void UpdateIconLayer(KGFIMapIcon theMapIcon)
	{
		GameObject aSpatialNewMapIcon = theMapIcon.GetRepresentation();
		MeshRenderer []aListRepRenderer = aSpatialNewMapIcon.GetComponentsInChildren<MeshRenderer>();
		CleanClickIconsList();
		foreach (MeshRenderer aRepRenderer in aListRepRenderer)
		{
			if (itsDataModuleMinimap.itsFogOfWar.itsHideMapIcons && !itsListClickIcons.Contains(theMapIcon))
			{
				aRepRenderer.sharedMaterial.renderQueue = 3000;
			}else
			{
				aRepRenderer.sharedMaterial.renderQueue = 3200;
			}
		}
	}
	
	/// <summary>
	/// Returns bounds of a gameobject with all children
	/// </summary>
	Vector3 GetGameObjectSize(GameObject theGO)
	{
		MeshRenderer []aMRList = theGO.GetComponentsInChildren<MeshRenderer>(true);
		if (aMRList.Length == 0)
		{
			Debug.LogError("found not meshrenderers on mapicon:"+theGO.name);
			return Vector3.zero;
		}
		Bounds aBounds = aMRList[0].bounds;
		
		for (int i=1;i<aMRList.Length;i++)
		{
			aBounds.Encapsulate(aMRList[i].bounds);
		}
		return aBounds.size;
	}
	
	/// <summary>
	/// Register map icon
	/// </summary>
	/// <param name="theMapIcon"></param>
	void RegisterIcon(KGFIMapIcon theMapIcon)
	{
		// create copy of static representation
		GameObject aSpatialArrow = null;
		// create copy of representation
		GameObject aSpatialNewMapIcon = null;
		
		aSpatialNewMapIcon = theMapIcon.GetRepresentation();
		if(aSpatialNewMapIcon == null)
		{
			LogError("missing icon representation for: "+theMapIcon.GetGameObjectName(),name,this);
			return;
		}
		
		UpdateIconLayer(theMapIcon);
		
		if (theMapIcon.GetTextureArrow() != null)
		{
			aSpatialArrow = GenerateTexturePlane(theMapIcon.GetTextureArrow(),itsDataModuleMinimap.itsShaders.itsShaderMapIcon);
			aSpatialArrow.transform.parent = itsContainerIconArrows;
			aSpatialArrow.transform.localPosition = Vector3.zero;
			aSpatialArrow.transform.localScale = Vector3.one;
			aSpatialArrow.GetComponent<MeshRenderer>().material.renderQueue = 3200;
			SetLayerRecursively(aSpatialArrow.gameObject,itsLayerMinimap);
		}
		
		// reparent it
		aSpatialNewMapIcon.transform.parent = itsContainerIcons;
		aSpatialNewMapIcon.transform.position = Vector3.zero;
		SetLayerRecursively(aSpatialNewMapIcon.gameObject,itsLayerMinimap);

		// remember it
		mapicon_listitem_script aNewItem = new mapicon_listitem_script();
		aNewItem.itsModule = this;
		aNewItem.itsMapIcon = theMapIcon;
		aNewItem.itsRepresentationInstance = aSpatialNewMapIcon;
		aNewItem.itsRepresentationInstanceTransform = aSpatialNewMapIcon.transform;
		aNewItem.itsRotate = theMapIcon.GetRotate();
		
		aNewItem.itsRepresentationArrowInstance = aSpatialArrow;
		aNewItem.itsMapIconTransform = theMapIcon.GetTransform();
		aNewItem.SetVisibility(true);
		if (aSpatialArrow != null)
			aNewItem.itsRepresentationArrowInstanceTransform = aSpatialArrow.transform;
		aNewItem.itsCachedRepresentationSize = GetGameObjectSize(aSpatialNewMapIcon);
		itsListMapIcons.Add(aNewItem);
		
		aNewItem.UpdateIcon();
		UpdateIconScale();
		
		LogInfo(string.Format("Added icon of category '{0}' for '{1}'",theMapIcon.GetCategory(),theMapIcon.GetTransform().name),name,this);
	}
	
	/// <summary>
	/// Unregister a map icon
	/// </summary>
	/// <param name="theMapIcon"></param>
	void UnregisterMapIcon(KGFIMapIcon theMapIcon)
	{
		for (int i=0;i<itsListMapIcons.Count;i++)
		{
			mapicon_listitem_script anItem = itsListMapIcons[i];
			if (anItem.itsMapIcon == theMapIcon)
			{
				LogInfo("Removed map icon of "+anItem.itsMapIconTransform.gameObject.GetObjectPath(),name,this);
				
				anItem.Destroy();
				itsListMapIcons.RemoveAt(i);
				break;
			}
		}
	}
	
	/// <summary>
	/// Update scaling of each map icon that is equally sized on every zoom level
	/// </summary>
	void UpdateIconScale()
	{
		float aScaleIcons = GetScaleIcons();
		float aScaleArrows = GetScaleArrows();
		
		foreach (mapicon_listitem_script aListItem in itsListMapIcons)
		{
			if (aListItem.itsRepresentationInstanceTransform != null)
			{
				if (aListItem.itsMapIcon != null)
					aListItem.itsRepresentationInstanceTransform.localScale = Vector3.one*aScaleIcons*aListItem.itsMapIcon.GetIconScale();
				else
					aListItem.itsRepresentationInstanceTransform.localScale = Vector3.one*aScaleIcons;
			}
			if (aListItem.itsRepresentationArrowInstanceTransform != null)
			{
				aListItem.itsRepresentationArrowInstanceTransform.localScale = Vector3.one*aScaleArrows;
			}
		}
	}
	
	/// <summary>
	/// Returns the scale factor for the arrows
	/// </summary>
	/// <returns></returns>
	float GetScaleArrows()
	{
		if (GetFullscreen() || GetPanningActive())
		{
			return 0;
		}
		
		return GetCurrentRange() * itsDataModuleMinimap.itsAppearanceMiniMap.itsScaleArrows * 2;
	}
	
	/// <summary>
	/// Returns the scale factor for the icons
	/// </summary>
	/// <returns></returns>
	float GetScaleIcons()
	{
		if (GetFullscreen())
		{
			return GetCurrentRange() * itsDataModuleMinimap.itsAppearanceMap.itsScaleIcons * 2 * (itsSavedResolution.Value.y / (float)GetHeight());
		}
		else
		{
			return GetCurrentRange() * itsDataModuleMinimap.itsAppearanceMiniMap.itsScaleIcons * 2;
		}
	}
	
	Texture2D itsTextureRenderMaskCurrent = null;
	void UpdateMaskTexture()
	{
		if (GetFullscreen())
		{
			itsTextureRenderMaskCurrent = itsDataModuleMinimap.itsAppearanceMap.itsMask;
		}else
		{
			itsTextureRenderMaskCurrent = itsDataModuleMinimap.itsAppearanceMiniMap.itsMask;
		}
		if (itsTextureRenderMaskCurrent != null && itsMeshRendererMinimapPlane != null)
		{
			itsMeshRendererMinimapPlane.material.SetTexture("_Mask",itsTextureRenderMaskCurrent);
		}
		
		if (GetOutputPlaneActive())
		{
			if (itsMeshRendererMinimapPlane != null)
				itsMeshRendererMinimapPlane.enabled = true;
			itsCameraOutput.enabled = true;
			itsCamera.targetTexture = itsRendertexture;
			itsCamera.rect = new Rect(0,0,1,1);
		}else
		{
			if (itsMeshRendererMinimapPlane != null)
				itsMeshRendererMinimapPlane.enabled = false;
			itsCameraOutput.enabled = false;
			itsCamera.targetTexture = null;
			itsCamera.pixelRect = new Rect(itsTargetRect.x,Screen.height - itsTargetRect.y - itsTargetRect.height,itsTargetRect.width,itsTargetRect.height);
		}
	}
	
	void UpdateCameraLayer()
	{
		itsCamera.cullingMask = itsDataModuleMinimap.itsGlobalSettings.itsRenderLayers | 1 << itsLayerMinimap;
	}
	
	private void SetColors()
	{
		if (itsMaterialMaskedMinimap != null)
			itsMaterialMaskedMinimap.SetColor("_Color",itsDataModuleMinimap.itsGlobalSettings.itsColorAll);
		if(itsArrayOfPhotoData !=  null)
		{
			foreach(KGFPhotoData aPhotoData in itsArrayOfPhotoData)
			{
				if(aPhotoData != null)
				{
					aPhotoData.itsPhotoPlaneMaterial.SetColor("_Color",itsDataModuleMinimap.itsGlobalSettings.itsColorMap);
				}
			}
		}
	}
	
	void Update()
	{
		if (itsErrorMode)
		{
			return;
		}
		
		Profiler.BeginSample("UpdateZoomCorrectionScale()");
		UpdateCameraLayer();
		UpdateZoom();
		
		#if OnlineChangeMode
		UpdateIconScale();
		UpdateOrthographicSize();
		SetColors();
		#endif
		Profiler.EndSample();
		
		if (itsTargetTransform == null)
		{
			enabled = false;
			return;
		}
		
		Profiler.BeginSample("ScrollWheelZooming");
		ScrollWheelZooming();
		Profiler.EndSample();
		
		Profiler.BeginSample("Panning");
		UpdatePanning();
		Profiler.EndSample();
		
		Profiler.BeginSample("UpdateMaskTexture");
		UpdateMaskTexture();
		Profiler.EndSample();
		
		Profiler.BeginSample("UpdateMapIconHover()");
		bool aClickOnIcon;
		UpdateMapIconHover(out aClickOnIcon);
		Profiler.EndSample();
		
		// do not allow creating new markers if the click was on already existing default marker (this does not include user markers)
		if (!aClickOnIcon)// && !GetPanningActive())
		{
			Profiler.BeginSample("CheckForClicksOnMinimap()");
			CheckForClicksOnMinimap();
			Profiler.EndSample();
		}
		
		Profiler.BeginSample("UpdateMapIconRotation()");
		UpdateMapIconRotation();
		Profiler.EndSample();
		
		Profiler.BeginSample("UpdateViewPortCube()");
		UpdateViewPortCube();
		Profiler.EndSample();
	}
	
	void ScrollWheelZooming()
	{
		if (GetHoverWithoutButtons())
		{
			SetZoom(GetZoom() - Input.GetAxis("Mouse ScrollWheel")*50);
		}
	}
	
	#region map mapping
	Vector2 itsMapPanning = Vector2.zero;
	Vector2 itsMapPanningDest = Vector2.zero;
	Vector2 itsMapPanningMousePosLast = Vector2.zero;
	float itsPanningMinMouseDistanceStart = 2;
	int itsPanningButton = 0;
	float itsVelX = 0, itsVelY = 0;
	bool GetPanningActive()
	{
		if (!itsDataModuleMinimap.itsPanning.itsActive)
			return false;
		return itsMapPanning != Vector2.zero;
	}
	
	bool GetPanningMoveActive()
	{
		if (!itsDataModuleMinimap.itsPanning.itsActive)
			return false;
		return Input.GetMouseButton(itsPanningButton);
	}
	
	void ForcePanningStart()
	{
		itsMapPanning = new Vector2(0.01f,0.01f);
		UpdateIconScale();
	}
	
	void StopMapPanning()
	{
		itsMapPanningDest = Vector2.zero;
	}
	
	void UpdatePanning()
	{
		if (!itsDataModuleMinimap.itsPanning.itsActive)
		{
			itsMapPanning = Vector2.zero;
			itsMapPanningDest = Vector2.zero;
			return;
		}
		
		if (GetHover())
		{
			if (Input.GetMouseButtonDown(itsPanningButton))
			{
				itsMapPanningMousePosLast = Input.mousePosition;
			}
			if (!GetPanningActive())
			{
				if (Input.GetMouseButton(itsPanningButton))
				{
					if (Vector2.Distance(Input.mousePosition,itsMapPanningMousePosLast) > itsPanningMinMouseDistanceStart)
					{
						ForcePanningStart();
					}
				}
			}
		}
		if (GetPanningActive())
		{
			if (Input.GetMouseButton(itsPanningButton))
			{
				float aMultiHorizontal = itsCamera.orthographicSize * 1f * itsCamera.aspect / itsTargetRect.width;
				float aMultiVertical = itsCamera.orthographicSize * 1f / itsTargetRect.height;
				
				Vector2 aDiff = (Vector2)Input.mousePosition - itsMapPanningMousePosLast;
				itsMapPanning -= new Vector2(aDiff.x * aMultiHorizontal,aDiff.y * aMultiVertical) * 2;
				itsMapPanningDest = itsMapPanning;
				
				itsMapPanningMousePosLast = Input.mousePosition;
			}
			if (Input.GetMouseButtonUp(itsPanningButton))
			{
				if (!GetFullscreen())
				{
					// return instantly in minimap
					StopMapPanning();
				}
			}
			
			if (itsMapPanning != itsMapPanningDest)
			{
				const float aTime = 0.1f;
				float aNewPositionX = Mathf.SmoothDamp(itsMapPanning.x,itsMapPanningDest.x,ref itsVelX,aTime);
				float aNewPositionY = Mathf.SmoothDamp(itsMapPanning.y,itsMapPanningDest.y,ref itsVelY,aTime);
				itsMapPanning = new Vector2(aNewPositionX,aNewPositionY);
				
				if (itsMapPanning.magnitude < 0.1f)
				{
					itsMapPanning = Vector2.zero;
					itsMapPanningDest = itsMapPanning;
					
					UpdateIconScale();
				}
			}
		}
	}
	#endregion
	
	bool GetOutputPlaneActive()
	{
		return GetHasProVersion() && itsTextureRenderMaskCurrent != null;
	}
	
	void LateUpdate()
	{
		if (GetOutputPlaneActive())
		{
			UpdateMinimapOutputPlane();
		}
	}
	
//	Mesh CreateMeshCube()
//	{
//		Mesh aMesh = new Mesh();
//
//		// vertices
//		aMesh.vertices = new Vector3[8];
//		aMesh.vertices[0] = new Vector3(0,1,0);
//		aMesh.vertices[1] = new Vector3(0,1,1);
//		aMesh.vertices[2] = new Vector3(1,1,1);
//		aMesh.vertices[3] = new Vector3(1,1,0);
//
//		aMesh.vertices[4] = new Vector3(0,0,0);
//		aMesh.vertices[5] = new Vector3(0,0,1);
//		aMesh.vertices[6] = new Vector3(1,0,1);
//		aMesh.vertices[7] = new Vector3(1,0,0);
//
//		// tris
//		aMesh.triangles = new int[12 * 3];
//		// tris: side 1
//		aMesh.triangles[0] = 0;
//		aMesh.triangles[1] = 1;
//		aMesh.triangles[2] = 3;
//
//		aMesh.triangles[3] = 1;
//		aMesh.triangles[4] = 2;
//		aMesh.triangles[5] = 3;
//
//		// tris: side 2
//		aMesh.triangles[6] = 0;
//		aMesh.triangles[7] = 3;
//		aMesh.triangles[8] = 7;
//
//		aMesh.triangles[9] = 0;
//		aMesh.triangles[10] = 7;
//		aMesh.triangles[11] = 4;
//
//		// tris: side 3
//		aMesh.triangles[12] = 3;
//		aMesh.triangles[13] = 6;
//		aMesh.triangles[14] = 7;
//
//		aMesh.triangles[15] = 3;
//		aMesh.triangles[16] = 2;
//		aMesh.triangles[17] = 6;
//
//		// tris: side 4
//		aMesh.triangles[18] = 5;
//		aMesh.triangles[19] = 4;
//		aMesh.triangles[20] = 7;
//
//		aMesh.triangles[21] = 6;
//		aMesh.triangles[22] = 5;
//		aMesh.triangles[23] = 7;
//
//		// tris: side 5
//		aMesh.triangles[24] = 2;
//		aMesh.triangles[25] = 5;
//		aMesh.triangles[26] = 6;
//
//		aMesh.triangles[27] = 2;
//		aMesh.triangles[28] = 1;
//		aMesh.triangles[29] = 5;
//
//		// tris: side 6
//		aMesh.triangles[30] = 1;
//		aMesh.triangles[31] = 0;
//		aMesh.triangles[32] = 4;
//
//		aMesh.triangles[33] = 1;
//		aMesh.triangles[34] = 4;
//		aMesh.triangles[35] = 5;
//
//		return aMesh;
//	}
	
	Vector3? GetMouseToWorldPointOnMap()
	{
		Vector2 aMousePosition = Input.mousePosition;
		aMousePosition.y = Screen.height - aMousePosition.y;
		
		// check if mouse click was on minimap
		if (itsTargetRect.Contains(aMousePosition))
		{
			if(itsRectFullscreen.Contains(aMousePosition) || itsRectStatic.Contains(aMousePosition) || itsRectZoomIn.Contains(aMousePosition) || itsRectZoomOut.Contains(aMousePosition))
			{
				return null;
			}
			
			// calculate point of click on minimap
			Vector2 aPercentofImageClick = new Vector2((aMousePosition.x - itsTargetRect.x) / (itsTargetRect.width),
			                                           (aMousePosition.y - itsTargetRect.y) / (itsTargetRect.height));
			Vector2 aVirtual2DCoordinateOfClick = aPercentofImageClick - new Vector2(0.5f,0.5f);
			
			Vector3 aClickWorldPoint;
			if (itsDataModuleMinimap.itsGlobalSettings.itsOrientation == KGFMapSystemOrientation.XZDefault)
			{
				aClickWorldPoint =
					itsCameraTransform.position +
					itsCameraTransform.up * aVirtual2DCoordinateOfClick.y * itsCamera.orthographicSize * (-2) +
					itsCameraTransform.right * aVirtual2DCoordinateOfClick.x * itsCamera.orthographicSize * 2 * itsCamera.aspect;
			}
			else
			{
				aClickWorldPoint =
					itsCameraTransform.position +
					itsCameraTransform.up * aVirtual2DCoordinateOfClick.y * itsCamera.orthographicSize * (-2) +
					itsCameraTransform.right * aVirtual2DCoordinateOfClick.x * itsCamera.orthographicSize * 2 * itsCamera.aspect;
			}
			
			return aClickWorldPoint;
		}
		return null;
	}
	
	List<Vector3> itsDeferedClickList = new List<Vector3>();
	Vector3 itsSavedMouseDownPoint = Vector3.zero;
	void CheckForClicksOnMinimap()
	{
		CheckDeferedClickList();
		if (Input.GetMouseButtonDown(0))
		{
			itsSavedMouseDownPoint = Input.mousePosition;
		}
		
		// check for click
		if (Input.GetMouseButtonUp(0))
		{
			if (Vector3.Distance(Input.mousePosition,itsSavedMouseDownPoint) < 2)
			{
				Vector3? aClickWorldPointNull = GetMouseToWorldPointOnMap();
				if (aClickWorldPointNull != null)
				{
					Vector3 aClickWorldPoint = aClickWorldPointNull.Value;
					aClickWorldPoint = ChangeVectorHeight(aClickWorldPoint,GetHeightFlags());
					
					if (itsDataModuleMinimap.itsGlobalSettings.itsOrientation == KGFMapSystemOrientation.XZDefault)
					{
						EventClickedOnMinimap.Trigger(this,new KGFClickEventArgs(new Vector3(aClickWorldPoint.x,0.0f,aClickWorldPoint.z)));
					}
					else
					{
						EventClickedOnMinimap.Trigger(this,new KGFClickEventArgs(new Vector3(aClickWorldPoint.x,aClickWorldPoint.y,0.0f)));
					}
					
					// handle creation/removal of user flags
					if (itsDataModuleMinimap.itsUserFlags.itsActive)
					{
						itsDeferedClickList.Add(aClickWorldPoint);
					}
				}
			}
		}
	}
	
	int itsClickUsedInFrame = -1;
	
	public void SetClickUsed()
	{
		itsClickUsedInFrame = Time.frameCount;
	}
	
	bool GetClickUsed()
	{
		return Math.Abs(itsClickUsedInFrame-Time.frameCount) <= 1;
	}
	
	void CheckDeferedClickList()
	{
		if (!GetClickUsed())
		{
			for (int i=0;i<itsDeferedClickList.Count;i++)
			{
				Vector3 aClickWorldPoint = itsDeferedClickList[i];
				
				if (itsDataModuleMinimap.itsUserFlags.itsMapIcon != null)
				{
					// create a flag object at this point
					EventUserFlagCreated.Trigger(this,new KGFFlagEventArgs(aClickWorldPoint));
					LogInfo(string.Format("Added user flag at {0}",aClickWorldPoint),name,this);
					KGFMapIcon anIcon = CreateIconInternal(aClickWorldPoint,itsDataModuleMinimap.itsUserFlags.itsMapIcon,itsContainerFlags);
					itsListClickIcons.Add(anIcon);
					UpdateIconLayer(anIcon);
				}
			}
		}
		itsDeferedClickList.Clear();
	}
	
	mapicon_listitem_script itsMapIconHoveredCurrent = null;
	bool itsMouseEnteredMap = false;
	/// <summary>
	/// Check if the mouse is over the are of a map icon and send the events
	/// </summary>
	void UpdateMapIconHover(out bool theClickOnIcon)
	{
		theClickOnIcon = false;
//		if (GetPanningActive())
//		{
//			if (itsMapIconHoveredCurrent != null)
//			{
//				MapIconLeave(itsMapIconHoveredCurrent);
//				itsMapIconHoveredCurrent = null;
//			}
//			return;
//		}
		
		Vector2 aMousePosition = Input.mousePosition;
		aMousePosition.y = Screen.height - aMousePosition.y;
		
		// check if mouse is in area of the map
		if (itsTargetRect.Contains(aMousePosition) && !itsMouseEnteredMap)
		{
			itsMouseEnteredMap = true;
			EventMouseMapEntered.Trigger(this);
			LogInfo("Mouse entered map",name,this);
		}
		else if (!itsTargetRect.Contains(aMousePosition) && itsMouseEnteredMap)
		{
			itsMouseEnteredMap = false;
			EventMouseMapLeft.Trigger(this);
			LogInfo("Mouse left map",name,this);
		}
		
		// cleanup old hovered item
		if (itsMapIconHoveredCurrent != null)
		{
			if (((MonoBehaviour)itsMapIconHoveredCurrent.itsMapIcon) == null)
				itsMapIconHoveredCurrent = null;
		}
		
		// check if mouse is near any map icon
		Vector3? aClickWorldPointNull = GetMouseToWorldPointOnMap();
		if (aClickWorldPointNull != null)
		{
			Vector3 aClickWorldPoint = aClickWorldPointNull.Value;
			aClickWorldPoint = ChangeVectorHeight(aClickWorldPoint,GetHeightIcons());
			bool aFoundItem = false;
			
			for (int i=0;i<itsListMapIcons.Count;i++)
			{
				mapicon_listitem_script aListItem = itsListMapIcons[i];
				float aRepSize = aListItem.GetRepresentationSize().magnitude / 2;
				if (Vector3.Distance(aListItem.itsRepresentationInstanceTransform.position,aClickWorldPoint) < aRepSize)
				{
					MapIconEnter(aListItem);
					aFoundItem = true;
					break;
				}
			}
			
			if (itsMapIconHoveredCurrent != null && Input.GetMouseButtonUp(0))
			{
				{
					theClickOnIcon = true;
					MapIconClick(itsMapIconHoveredCurrent);
				}
			}
			
			if (!aFoundItem && itsMapIconHoveredCurrent != null)
			{
				MapIconLeave(itsMapIconHoveredCurrent);
			}
		}
	}
	
	void MapIconEnter(mapicon_listitem_script theMapIcon)
	{
		if (itsMapIconHoveredCurrent != theMapIcon)
		{
			LogInfo("Mouse entered map icon:"+theMapIcon.itsMapIcon.GetGameObjectName(),name,this);
			
			itsMapIconHoveredCurrent = theMapIcon;
			EventMouseMapIconEntered.Trigger(this,new KGFMarkerEventArgs(theMapIcon.itsMapIcon));
		}
	}
	
	void MapIconLeave(mapicon_listitem_script theMapIcon)
	{
		if (theMapIcon != null)
		{
			LogInfo("Mouse left map icon:"+theMapIcon.itsMapIcon.GetGameObjectName(),name,this);
			
			itsMapIconHoveredCurrent = null;
			EventMouseMapIconLeft.Trigger(this,new KGFMarkerEventArgs(theMapIcon.itsMapIcon));
		}
	}
	
	void MapIconClick(mapicon_listitem_script theMapIcon)
	{
		if (itsListClickIcons.Contains(theMapIcon.itsMapIcon))
		{
			RemoveClickMarker(theMapIcon);
		}
		else
		{
			LogInfo("Click on map icon:"+theMapIcon.itsMapIcon.GetGameObjectName(),name,this);
			EventMouseMapIconClicked.Trigger(this,new KGFMarkerEventArgs(theMapIcon.itsMapIcon));
		}
	}
	
	void RemoveClickMarker(KGFMapSystem.mapicon_listitem_script theMapIcon)
	{
		GameObject.Destroy(((MonoBehaviour)theMapIcon.itsMapIcon).gameObject);
	}
	
	Rect GetBounds2DPanning()
	{
		if (itsDataModuleMinimap.itsGlobalSettings.itsOrientation == KGFMapSystemOrientation.XZDefault)
			return new Rect(itsTerrainBoundsPanning.min.x,itsTerrainBoundsPanning.min.z,itsTerrainBoundsPanning.size.x,itsTerrainBoundsPanning.size.z);
		else
			return new Rect(itsTerrainBoundsPanning.min.x,itsTerrainBoundsPanning.min.y,itsTerrainBoundsPanning.size.x,itsTerrainBoundsPanning.size.y);
	}
	
	Vector2 ClampPoint(Vector2 thePoint,Rect theArea,float theBorderX, float theBorderY)
	{
		if (thePoint.x < theArea.x + theBorderX)
			thePoint.x = theArea.x + theBorderX;
		if (thePoint.y < theArea.y + theBorderY)
			thePoint.y = theArea.y + theBorderY;
		if (thePoint.x > theArea.xMax - theBorderX)
			thePoint.x = theArea.xMax - theBorderX;
		if (thePoint.y > theArea.yMax - theBorderY)
			thePoint.y = theArea.yMax - theBorderY;
		return thePoint;
	}
	
	/// <summary>
	/// Get current camera 2D point based on panning and target position
	/// </summary>
	Vector2 GetCurrentCameraPoint2D()
	{
		// get target point
		Vector2 aTargetPoint;
		
		if (itsDataModuleMinimap.itsGlobalSettings.itsOrientation == KGFMapSystemOrientation.XZDefault)
			aTargetPoint = new Vector2(itsTargetTransform.position.x,itsTargetTransform.position.z);
		else
			aTargetPoint = new Vector2(itsTargetTransform.position.x,itsTargetTransform.position.y);
		
		// no more change is needed if panning is not active
		if (!itsDataModuleMinimap.itsPanning.itsActive)
			return aTargetPoint;
		
		float aBorder = 0;
		if (itsDataModuleMinimap.itsGlobalSettings.itsIsStatic)
		{
			aBorder = itsCurrentZoom;
		}
		
		if (itsDataModuleMinimap.itsPanning.itsUseBounds)
		{
			// get bounds of area in 2D
			Rect aBounds = GetBounds2DPanning();
			
			// clamp target point to area
			aTargetPoint = ClampPoint(aTargetPoint,aBounds,aBorder*itsCamera.aspect,aBorder);
			
			// rotate panning vector
			Vector2 aMapPanningRotated = RotateVector2(itsMapPanning,itsRotation);
			
			// move target point with rotated panning
			Vector2 aTargetPointPlusPanning = aTargetPoint + aMapPanningRotated;
			Vector2 aNewTargetPoint = ClampPoint(aTargetPointPlusPanning,aBounds,aBorder*itsCamera.aspect,aBorder);
			
			// correct the panning
			Vector2 aRealPanningRotated = aNewTargetPoint - aTargetPoint;
			itsMapPanning = RotateVector2(aRealPanningRotated,-itsRotation);
			
			return aNewTargetPoint;
		}
		else
		{
			// rotate panning vector
			Vector2 aMapPanningRotated = RotateVector2(itsMapPanning,itsRotation);
			
			// move target point with rotated panning
			Vector2 aTargetPointPlusPanning = aTargetPoint + aMapPanningRotated;
			
			return aTargetPointPlusPanning;
		}
	}
	
	Vector2 RotateVector2(Vector2 theVector, float theRotation)
	{
		Vector2 aVectorRotated = Vector2.zero;
		
		aVectorRotated.x = theVector.x * Mathf.Cos(theRotation) - theVector.y * Mathf.Sin(theRotation);
		aVectorRotated.y = theVector.x * Mathf.Sin(theRotation) + theVector.y * Mathf.Cos(theRotation);
		
		return aVectorRotated;
	}
	
	Vector3 GetCurrentCameraPoint3D()
	{
		Vector2 aCameraPoint2D = GetCurrentCameraPoint2D();
		return CreateVector(aCameraPoint2D.x,aCameraPoint2D.y,GetTerrainHeight(5));
	}
	
	float itsRotation = 0;
	
	void UpdateMapIconRotation()
	{
		if (itsDataModuleMinimap.itsGlobalSettings.itsOrientation == KGFMapSystemOrientation.XZDefault)
		{
//			itsCameraTransform.position = CreateVector(itsTargetTransform.position.x,itsTargetTransform.position.z,GetTerrainHeight(50));
			if (itsDataModuleMinimap.itsGlobalSettings.itsIsStatic)
			{
				itsCameraTransform.eulerAngles = CreateVector(0,0,itsDataModuleMinimap.itsGlobalSettings.itsStaticNorth);
				itsCameraTransform.Rotate(90.0f,0.0f,0.0f);
			}
			else
			{
				Vector3 aForwardVector = itsTargetTransform.forward;
				aForwardVector.y = 0.0f;
				aForwardVector.Normalize();
				itsCameraTransform.rotation = Quaternion.LookRotation(aForwardVector,Vector3.up);
				itsCameraTransform.Rotate(90.0f,0.0f,0.0f);
			}
		}
		else
		{
//			itsCameraTransform.position = CreateVector(itsTargetTransform.position.x,itsTargetTransform.position.y,GetTerrainHeight(50));
			itsCameraTransform.eulerAngles = new Vector3(0,0,0);
		}
		itsRotation = (-1)*itsCameraTransform.eulerAngles.y * Mathf.Deg2Rad;
		itsCameraTransform.position = GetCurrentCameraPoint3D();
//		itsCameraTransform.localPosition += itsCameraTransform.right * itsMapPanning.x + itsCameraTransform.up * itsMapPanning.y;
//		Debug.Log("Rot:"+itsRotation);
		
		// for every map icon
		for (int i=itsListMapIcons.Count-1;i>=0;i--)
		{
			mapicon_listitem_script aListItem = itsListMapIcons[i];
			
			// remove map icons of destroyed gameobjects
			if (aListItem.itsMapIconTransform == null)
			{
				itsListMapIcons.RemoveAt(i);
				continue;
			}
			
			// MAP ICON
			if (aListItem.GetMapIconVisibilityEffective())
			{
				// rotation
				if (itsDataModuleMinimap.itsGlobalSettings.itsOrientation == KGFMapSystemOrientation.XZDefault)
				{
					if (aListItem.itsRotate)
					{
						aListItem.itsRepresentationInstanceTransform.eulerAngles = new Vector3(0,aListItem.itsMapIconTransform.eulerAngles.y,0);
					}
					else
					{
						aListItem.itsRepresentationInstanceTransform.eulerAngles = new Vector3(0,itsCameraTransform.eulerAngles.y,0);
					}
				}else
				{
					if (aListItem.itsRotate)
					{
						aListItem.itsRepresentationInstanceTransform.eulerAngles = new Vector3(aListItem.itsMapIconTransform.eulerAngles.z-90.0f,270,270);
					}
					else
					{
						aListItem.itsRepresentationInstanceTransform.eulerAngles = new Vector3(270,0,0);
					}
				}
				
				// position
				aListItem.itsRepresentationInstanceTransform.position = ChangeVectorHeight(aListItem.itsMapIconTransform.position,GetHeightIcons());
				
				// ARROW
				if (aListItem.itsRepresentationArrowInstance != null)
				{
					// calc new visibility: visible if map icon is in visible state and outside radius
					Vector3 aDistanceLine = itsTargetTransform.position - aListItem.itsRepresentationInstanceTransform.position;
					aDistanceLine = ChangeVectorHeight(aDistanceLine,0);
					
					bool aNewVisibilityArrow = aDistanceLine.magnitude > GetCurrentRange();
					if (aNewVisibilityArrow != aListItem.GetIsArrowVisible())
					{
						aListItem.ShowArrow(aNewVisibilityArrow);
						if (aNewVisibilityArrow)
						{
							LogInfo(string.Format("Icon '{0}' got invisible",aListItem.itsMapIconTransform.name),name,this);
						}else
						{
							LogInfo(string.Format("Icon '{0}' got visible",aListItem.itsMapIconTransform.name),name,this);
						}
						EventVisibilityOnMinimapChanged.Trigger(this,new KGFMarkerEventArgs(aListItem.itsMapIcon));
					}
					
					if (aListItem.GetIsArrowVisible())
					{
						float anAngle = 0;
						
						if (itsDataModuleMinimap.itsGlobalSettings.itsOrientation == KGFMapSystemOrientation.XZDefault)
						{
							anAngle = Vector3.Angle(Vector3.forward,aDistanceLine);
						}else
						{
							anAngle = Vector3.Angle(Vector3.up,aDistanceLine);
						}
						if (Vector3.Dot(Vector3.right,aDistanceLine) < 0)
							anAngle = 360 - anAngle;
						anAngle += 180;
						
						// position
						Vector3 aVector;
						
						float aRadius = GetCurrentRange() * itsDataModuleMinimap.itsAppearanceMiniMap.itsRadiusArrows;
						float aCorrectedAngle = anAngle - itsCameraTransform.localEulerAngles.y;
						
						aVector = itsCameraTransform.position +
							itsCameraTransform.right * aRadius * itsCamera.aspect * Mathf.Sin(aCorrectedAngle * Mathf.Deg2Rad) +
							itsCameraTransform.up * aRadius * Mathf.Cos(aCorrectedAngle * Mathf.Deg2Rad);
						
						aVector = ChangeVectorHeight(aVector,GetHeightArrows());
						aListItem.itsRepresentationArrowInstanceTransform.position = aVector;
						
						// rotation
						if (itsDataModuleMinimap.itsGlobalSettings.itsOrientation == KGFMapSystemOrientation.XZDefault)
						{
							aListItem.itsRepresentationArrowInstanceTransform.eulerAngles = new Vector3(0,anAngle,0);
						}else
						{
							aListItem.itsRepresentationArrowInstanceTransform.eulerAngles = new Vector3(anAngle-90,90,90);
						}
					}
				}
			}
		}
	}
	#endregion

	#region Public methods
	/// <summary>
	/// Measures the scene. This is used if you change the scene and want the panning to use the new scene.
	/// </summary>
	public void MeasureScene()
	{
		GetMeasuredBounds(itsDataModuleMinimap.itsPanning.itsBoundsLayers,out itsTerrainBoundsPanning);
		GetMeasuredBounds(itsDataModuleMinimap.itsPhoto.itsPhotoLayers,out itsTerrainBoundsPhoto);
		
		itsSizeTerrain = GetVector3Plane(itsTerrainBoundsPhoto.size);
	}
	
	/// <summary>
	/// Returns TRUE if the map icon is currently visible on the map.
	/// FALSE if it is to far away to be visible or if deactivated.
	/// </summary>
	public bool GetIsVisibleOnMap(KGFIMapIcon theMapIcon)
	{
		for (int i=itsListMapIcons.Count-1;i>=0;i--)
		{
			mapicon_listitem_script aListItem = itsListMapIcons[i];
			if (aListItem.itsMapIcon == theMapIcon)
			{
				return (!aListItem.GetIsArrowVisible() && aListItem.GetMapIconVisibilityEffective());
			}
		}
		
		return false;
	}
	
	/// <summary>
	/// returns the orientation of the mapsystem.
	/// The mapsystem can be used in the xy (sidescroller) and in the xz (top-down) plane.
	/// </summary>
	/// <returns></returns>
	public KGFMapSystemOrientation GetOrientation()
	{
		return itsDataModuleMinimap.itsGlobalSettings.itsOrientation;
	}
	
	/// <summary>
	/// Update internally created styles with icon textures
	/// </summary>
	public void UpdateStyles()
	{
		itsGuiStyleBack = new GUIStyle();
		itsGuiStyleBack.normal.background = itsDataModuleMinimap.itsAppearanceMiniMap.itsBackground;
		
		itsGuiStyleButton = new GUIStyle();
		itsGuiStyleButton.normal.background = itsDataModuleMinimap.itsAppearanceMiniMap.itsButton;
		itsGuiStyleButton.hover.background = itsDataModuleMinimap.itsAppearanceMiniMap.itsButtonHover;
		itsGuiStyleButton.active.background = itsDataModuleMinimap.itsAppearanceMiniMap.itsButtonDown;
		
		itsGuiStyleButtonFullscreen = new GUIStyle();
		itsGuiStyleButtonFullscreen.normal.background = itsDataModuleMinimap.itsAppearanceMap.itsButton;
		itsGuiStyleButtonFullscreen.hover.background = itsDataModuleMinimap.itsAppearanceMap.itsButtonHover;
		itsGuiStyleButtonFullscreen.active.background = itsDataModuleMinimap.itsAppearanceMap.itsButtonDown;
	}

	/// <summary>
	/// Update the map icon (color, texture)
	/// </summary>
	/// <param name="theIcon"></param>
	public void UpdateIcon(KGFIMapIcon theIcon)
	{
		foreach (mapicon_listitem_script anItem in itsListMapIcons)
		{
			if (anItem.itsMapIcon == theIcon)
			{
				anItem.UpdateIcon();
			}
		}
	}

	/// <summary>
	/// Serialize current minimap state to string
	/// Currently only fog of war is supported
	/// </summary>
	/// <returns></returns>
	public string GetSaveString()
	{
		return SerializeFogOfWar();
	}

	/// <summary>
	/// Load minimap state from string.
	/// Currently only fog of war is supported
	/// </summary>
	/// <param name="theSavedString">A string you can get from GetSaveString()</param>
	public void LoadFromString(string theSavedString)
	{
		DeserializeFogOfWar(theSavedString);
	}

	/// <summary>
	/// Create a flag map icon at a given world point
	/// </summary>
	/// <param name="theWorldPoint">The point in world coordinates where the new icon should be placed</param>
	/// <param name="theCategory">The category of the icon</param>
	/// <returns>The newly created icon</returns>
	public KGFMapIcon CreateIcon(Vector3 theWorldPoint,KGFMapIcon theMapIcon)
	{
		KGFMapIcon aNewIcon = CreateIconInternal(theWorldPoint,theMapIcon,itsContainerUser);
		itsListUserIcons.Add(aNewIcon);
		return aNewIcon;
	}

	/// <summary>
	/// Remove a user created map icon
	/// </summary>
	/// <param name="theIcon">The user created map icon to remove</param>
	public void RemoveIcon(KGFMapIcon theIcon)
	{
		if (itsListUserIcons.Contains(theIcon))
		{
			UnregisterMapIcon(theIcon);
			itsListUserIcons.Remove(theIcon);
		}
		else
		{
			LogError("Not a user created icon",name,this);
		}
	}

	/// <summary>
	/// Get all user icons created with CreateIcon
	/// </summary>
	/// <returns>A list of all user created icons</returns>
	public KGFMapIcon[] GetUserIcons()
	{
		return itsListUserIcons.ToArray();
	}

	/// <summary>
	/// Get all user icons created by click
	/// </summary>
	/// <returns>A list of all user created icons</returns>
	public KGFMapIcon[] GetUserFlags()
	{
		List<KGFMapIcon> aList = new List<KGFMapIcon>();
		
		foreach (Transform aChild in itsContainerFlags.transform)
		{
			aList.Add(aChild.GetComponent<KGFMapIcon>());
		}
		
		return aList.ToArray();
	}

	/// <summary>
	/// Enable/disable minimap
	/// </summary>
	/// <param name="theEnable"></param>
	public void SetMinimapEnabled(bool theEnable)
	{
		if (itsMinimapActive != theEnable)
		{
			itsMinimapActive = theEnable;
			KGFSetChildrenActiveRecursively(gameObject,theEnable);
			KGFSetChildrenActiveRecursively(itsMinimapPlane,theEnable);
			
			LogInfo("New map system state:"+theEnable,name,this);
		}
	}

	/// <summary>
	/// Refresh map icon visibility of all map icons, call this if your IMapIcon derived class instances changed visibility
	/// </summary>
	public void RefreshIconsVisibility()
	{
		foreach (mapicon_listitem_script aListItem in itsListMapIcons)
		{
			aListItem.UpdateVisibility();
		}
	}

	/// <summary>
	/// Set visibility of map icons by category
	/// </summary>
	/// <param name="theCategory"></param>
	public void SetIconsVisibleByCategory(string theCategory, bool theVisible)
	{
		LogInfo(string.Format("Icon category '{0}' changed visibility to: {1}",theCategory,theVisible),name,this);
		foreach (mapicon_listitem_script anitem in itsListMapIcons)
		{
			if (anitem.itsMapIcon.GetCategory() == theCategory)
			{
				anitem.SetVisibility(theVisible);
			}
		}
	}

	/// <summary>
	/// Set target that is centered on minimap
	/// </summary>
	/// <param name="theTarget"></param>
	public void SetTarget(GameObject theTarget)
	{
		if(theTarget == null)
		{
			LogError("Assign your character to KGFMapsystem.itsTarget. KGFMapSystem will not work without a target.",name,this);
			return;
		}
		itsDataModuleMinimap.itsGlobalSettings.itsTarget = theTarget;
		itsTargetTransform = theTarget.transform;
	}

	/// <summary>
	/// Returns current absolute zoom level
	/// </summary>
	/// <returns></returns>
	public float GetCurrentRange()
	{
		return itsCamera.orthographicSize;
	}

	/// <summary>
	/// Enable or disable static mode
	/// </summary>
	/// <param name="theModeStatic"></param>
	public void SetModeStatic(bool theModeStatic)
	{
		itsDataModuleMinimap.itsGlobalSettings.itsIsStatic = theModeStatic;
	}

	/// <summary>
	/// TRUE, if static mode is active
	/// </summary>
	/// <returns></returns>
	public bool GetModeStatic()
	{
		return itsDataModuleMinimap.itsGlobalSettings.itsIsStatic;
	}

	/// <summary>
	/// Set the size of the minimap on the screen
	/// </summary>
	/// <param name="theSize"></param>
	public void SetMinimapSize(float theSize)
	{
		itsDataModuleMinimap.itsAppearanceMiniMap.itsSize = theSize;
		UpdateOrthographicSize();
	}

	/// <summary>
	/// Get active status of fullscreen mode
	/// </summary>
	/// <returns></returns>
	public bool GetFullscreen()
	{
		return itsModeFullscreen;
	}

	/// <summary>
	/// Set the fullscreen mode
	/// </summary>
	/// <param name="theFullscreenMode"></param>
	public void SetFullscreen(bool theFullscreenMode)
	{
		if (theFullscreenMode && itsSavedResolution == null)
		{
			itsSavedResolution = new Vector2(GetWidth(),GetHeight());
		}
		else if (!theFullscreenMode && itsSavedResolution != null)
		{
			itsSavedResolution = null;
		}
		else
		{
			return;
		}
		
		itsModeFullscreen = theFullscreenMode;
		UpdateTargetRect();
		UpdateMaskTexture();
		UpdateOrthographicSize();
		UpdateIconScale();
		
		EventFullscreenModeChanged.Trigger(this,EventArgs.Empty);
	}

	/// <summary>
	/// Get gui style for buttons
	/// </summary>
	/// <returns></returns>
	GUIStyle GetButtonStyle()
	{
		if (GetFullscreen())
		{
			return itsGuiStyleButtonFullscreen;
		}
		else
		{
			return itsGuiStyleButton;
		}
	}

	/// <summary>
	/// Simple draw button method
	/// </summary>
	/// <param name="theTexture"></param>
	/// <returns></returns>
	bool DrawButton(Rect theRect, Texture2D theTexture)
	{
		if (theTexture == null)
			return false;
		
		return GUI.Button(theRect,theTexture,GetButtonStyle());
	}

	/// <summary>
	/// Update itsTargetRect with new values
	/// </summary>
	void UpdateTargetRect()
	{
		switch (itsDataModuleMinimap.itsAppearanceMiniMap.itsAlignmentHorizontal)
		{
			case KGFAlignmentHorizontal.Left:
				itsTargetRect.x = 0;
				itsTargetRect.x += (Screen.width-GetWidth())*itsDataModuleMinimap.itsAppearanceMiniMap.itsMarginHorizontal;
				break;
			case KGFAlignmentHorizontal.Middle:
				itsTargetRect.x = (Screen.width-GetWidth())/2;
				break;
			case KGFAlignmentHorizontal.Right:
				itsTargetRect.x = Screen.width-GetWidth();
				itsTargetRect.x -= (Screen.width-GetWidth())*itsDataModuleMinimap.itsAppearanceMiniMap.itsMarginHorizontal;
				break;
		}
		switch (itsDataModuleMinimap.itsAppearanceMiniMap.itsAlignmentVertical)
		{
			case KGFAlignmentVertical.Top:
				itsTargetRect.y = 0;
				itsTargetRect.y += (Screen.height-GetHeight())*itsDataModuleMinimap.itsAppearanceMiniMap.itsMarginVertical;
				break;
			case KGFAlignmentVertical.Middle:
				itsTargetRect.y = (Screen.height-GetHeight())/2;
				break;
			case KGFAlignmentVertical.Bottom:
				itsTargetRect.y = Screen.height-GetHeight();
				itsTargetRect.y -= (Screen.height-GetHeight())*itsDataModuleMinimap.itsAppearanceMiniMap.itsMarginVertical;
				break;
		}
		
		itsTargetRect.width = GetWidth();
		itsTargetRect.height = GetHeight();
	}

	/// <summary>
	/// renders the gui of the minimap
	/// </summary>
	public void RenderGUI()
	{
		if (itsErrorMode)
		{
			//TODO: paint some error message
			GUIStyle aStyle = new GUIStyle();
			aStyle.alignment = TextAnchor.MiddleCenter;
			aStyle.wordWrap = true;
			aStyle.normal.textColor = Color.red;
			
			GUI.Label(new Rect(0,0,Screen.width,Screen.height),"Please click on the KGFMapSystem gameobject and fix all the errors displayed in the inspector.",aStyle);
			return;
		}
		
		if (!itsMinimapActive)
			return;
		
		UpdateTargetRect();
		
		if (!itsDataModuleMinimap.itsGlobalSettings.itsHideGUI)
		{
			RenderMainGUI();
		}
		
		RenderToolTip();
	}
	
	void RenderMainGUI()
	{
		float aButtonSize = GetButtonSize();
		float aButtonPadding = GetButtonPadding();
		
		// background
		if (GetFullscreen())
		{
			itsGuiStyleBack.normal.background = itsDataModuleMinimap.itsAppearanceMap.itsBackground;
			itsGuiStyleBack.border = new RectOffset(itsDataModuleMinimap.itsAppearanceMap.itsBackgroundBorder,
			                                        itsDataModuleMinimap.itsAppearanceMap.itsBackgroundBorder,
			                                        itsDataModuleMinimap.itsAppearanceMap.itsBackgroundBorder,
			                                        itsDataModuleMinimap.itsAppearanceMap.itsBackgroundBorder);
		}
		else
		{
			itsGuiStyleBack.normal.background = itsDataModuleMinimap.itsAppearanceMiniMap.itsBackground;
			itsGuiStyleBack.border = new RectOffset(itsDataModuleMinimap.itsAppearanceMiniMap.itsBackgroundBorder,
			                                        itsDataModuleMinimap.itsAppearanceMiniMap.itsBackgroundBorder,
			                                        itsDataModuleMinimap.itsAppearanceMiniMap.itsBackgroundBorder,
			                                        itsDataModuleMinimap.itsAppearanceMiniMap.itsBackgroundBorder);
		}
		GUI.Box(itsTargetRect,"",itsGuiStyleBack);
		
		// calc button rects
		if (GetFullscreen())
		{
			int aSpace = (int)(itsDataModuleMinimap.itsAppearanceMap.itsButtonSpace * GetWidth());
			int aButtonCount = 4;
			int aButtonsLongSide = (int)(((aButtonCount-1)*aSpace)+GetButtonSize()*aButtonCount);
			int aButtonsShortSide = (int)(GetButtonSize());
			
			Rect aRect = new Rect();
			
			switch(itsDataModuleMinimap.itsAppearanceMap.itsAlignmentHorizontal)
			{
				case KGFAlignmentHorizontal.Left:
					aRect.x = itsTargetRect.x + aButtonPadding;
					break;
				case KGFAlignmentHorizontal.Middle:
					if (itsDataModuleMinimap.itsAppearanceMap.itsOrientation == KGFOrientation.Horizontal)
						aRect.x = (itsTargetRect.xMax-itsTargetRect.xMin)/2-aButtonsLongSide/2;
					else
						aRect.x = (itsTargetRect.xMax-itsTargetRect.xMin)/2-aButtonsShortSide/2;
					break;
				case KGFAlignmentHorizontal.Right:
					if (itsDataModuleMinimap.itsAppearanceMap.itsOrientation == KGFOrientation.Horizontal)
						aRect.x = itsTargetRect.xMax-aButtonsLongSide-aButtonPadding;
					else
						aRect.x = itsTargetRect.xMax-aButtonsShortSide-aButtonPadding;
					break;
			}
			
			switch(itsDataModuleMinimap.itsAppearanceMap.itsAlignmentVertical)
			{
				case KGFAlignmentVertical.Top:
					aRect.y = itsTargetRect.y + aButtonPadding;
					break;
				case KGFAlignmentVertical.Middle:
					if (itsDataModuleMinimap.itsAppearanceMap.itsOrientation == KGFOrientation.Horizontal)
						aRect.y = (itsTargetRect.yMax-itsTargetRect.yMin)/2-aButtonsShortSide/2;
					else
						aRect.y = (itsTargetRect.yMax-itsTargetRect.yMin)/2-aButtonsLongSide/2;
					break;
				case KGFAlignmentVertical.Bottom:
					if (itsDataModuleMinimap.itsAppearanceMap.itsOrientation == KGFOrientation.Horizontal)
						aRect.y = itsTargetRect.yMax-aButtonsShortSide-aButtonPadding;
					else
						aRect.y = itsTargetRect.yMax-aButtonsLongSide-aButtonPadding;
					break;
			}
			aRect.width = GetButtonSize();
			aRect.height = GetButtonSize();
			
			itsRectZoomIn = itsRectZoomOut = itsRectStatic = itsRectFullscreen = aRect;
			if (itsDataModuleMinimap.itsAppearanceMap.itsOrientation == KGFOrientation.Horizontal)
			{
				itsRectZoomOut.x = itsRectZoomIn.x+aSpace+GetButtonSize();
				itsRectStatic.x = itsRectZoomOut.x+aSpace+GetButtonSize();
				itsRectFullscreen.x = itsRectStatic.x+aSpace+GetButtonSize();
			}
			else
			{
				itsRectZoomOut.y = itsRectZoomIn.y+aSpace+GetButtonSize();
				itsRectStatic.y = itsRectZoomOut.y+aSpace+GetButtonSize();
				itsRectFullscreen.y = itsRectStatic.y+aSpace+GetButtonSize();
			}
		}
		else
		{
			// left top button
			itsRectZoomIn = new Rect(itsTargetRect.x+aButtonPadding,
			                         itsTargetRect.y+aButtonPadding,
			                         aButtonSize,aButtonSize);
			
			// left bottom button
			itsRectZoomOut = new Rect(itsTargetRect.x+aButtonPadding,
			                          itsTargetRect.y+itsTargetRect.height-aButtonSize-aButtonPadding,
			                          aButtonSize,aButtonSize);
			
			// right top button
			itsRectStatic = new Rect(itsTargetRect.x+itsTargetRect.width-aButtonSize-aButtonPadding,
			                         itsTargetRect.y+aButtonPadding,
			                         aButtonSize,aButtonSize);
			
			// right bottom button
			itsRectFullscreen = new Rect(itsTargetRect.x+itsTargetRect.width-aButtonSize-aButtonPadding,
			                             itsTargetRect.y+itsTargetRect.height-aButtonSize-aButtonPadding,
			                             aButtonSize,aButtonSize);
		}
		// draw buttons
		if (DrawButton(itsRectZoomIn,GetFullscreen()?itsDataModuleMinimap.itsAppearanceMap.itsIconZoomIn:itsDataModuleMinimap.itsAppearanceMiniMap.itsIconZoomIn))
		{
			ZoomIn();
		}
		if (DrawButton(itsRectZoomOut,GetFullscreen()?itsDataModuleMinimap.itsAppearanceMap.itsIconZoomOut:itsDataModuleMinimap.itsAppearanceMiniMap.itsIconZoomOut))
		{
			ZoomOut();
		}
		if (DrawButton(itsRectStatic,GetFullscreen()?itsDataModuleMinimap.itsAppearanceMap.itsIconZoomLock:itsDataModuleMinimap.itsAppearanceMiniMap.itsIconZoomLock))
		{
			SetModeStatic(!GetModeStatic());
		}
		if (DrawButton(itsRectFullscreen,GetFullscreen()?itsDataModuleMinimap.itsAppearanceMap.itsIconFullscreen:itsDataModuleMinimap.itsAppearanceMiniMap.itsIconFullscreen))
		{
			SetFullscreen(!GetFullscreen());
		}
	}
	
	/// <summary>
	/// Type for tooltip rendering methods
	/// </summary>
	public delegate void RenderToolTipMethodType(string theToolTipText);
	RenderToolTipMethodType itsRenderToolTipMethod;
	
	/// <summary>
	/// Change the current tooltip rendering method. It should render the text in the parameter to the current mouse position in most cases.
	/// If you the the method to null, the default method will be used.
	/// </summary>
	public void SetRenderToolTipMethod(RenderToolTipMethodType theMethod)
	{
		itsRenderToolTipMethod = theMethod;
	}
	
	/// <summary>
	/// Use default method for rendering tooltips again.
	/// </summary>
	public void ResetRenderToolTipMethod()
	{
		itsRenderToolTipMethod = null;
	}
	
	/// <summary>
	/// Tooltip rendering logic
	/// </summary>
	void RenderToolTip()
	{
		if (!GetHoverWithoutButtons())
			return;
		if (GetPanningMoveActive())
			return;
		
		if (itsMapIconHoveredCurrent != null && itsDataModuleMinimap.itsToolTip.itsActive)
		{
			if (itsMapIconHoveredCurrent.itsMapIcon != null)
			{
				if (itsRenderToolTipMethod != null)
					itsRenderToolTipMethod(itsMapIconHoveredCurrent.itsMapIcon.GetToolTipText());
				else
					RenderToolTipMethodDefault(itsMapIconHoveredCurrent.itsMapIcon.GetToolTipText());
			}
		}
	}
	
	/// <summary>
	/// Default render method for tooltips
	/// </summary>
	/// <param name="theText">the text to be rendered as tooltip</param>
	void RenderToolTipMethodDefault(string theText)
	{
		if (string.IsNullOrEmpty(theText))
			return;
		
		GUIStyle aStyle = new GUIStyle();
		aStyle.normal.background = itsDataModuleMinimap.itsToolTip.itsTextureBackground;
		aStyle.normal.textColor = itsDataModuleMinimap.itsToolTip.itsColorText;
		aStyle.font = itsDataModuleMinimap.itsToolTip.itsFontText;
		aStyle.border = itsDataModuleMinimap.itsToolTip.itsBackgroundBorder;
		aStyle.padding = itsDataModuleMinimap.itsToolTip.itsBackgroundPadding;
		
		Vector2 aMousePosition = Input.mousePosition;
		Vector2 theSize = aStyle.CalcSize(new GUIContent(theText)) +
			new Vector2(itsDataModuleMinimap.itsToolTip.itsBackgroundPadding.left+itsDataModuleMinimap.itsToolTip.itsBackgroundPadding.right,
			            itsDataModuleMinimap.itsToolTip.itsBackgroundPadding.top+itsDataModuleMinimap.itsToolTip.itsBackgroundPadding.bottom);
		
		GUI.Label(new Rect(aMousePosition.x - theSize.x / 2,Screen.height - aMousePosition.y + 15,theSize.x,theSize.y),theText,aStyle);
	}

	void UpdateOrthographicSize()
	{
		itsCamera.orthographicSize = itsCurrentZoom;
		itsCamera.aspect = ((float)GetWidth())/((float)GetHeight());
		
//		itsCamera.orthographicSize = ((float)GetHeight() / itsPixelPerMeter)/2;
//		itsCamera.aspect = ((float)GetWidth())/((float)GetHeight());
//		itsCamera.ResetAspect();
	}

	void CorrectCurrentZoom()
	{
		itsCurrentZoom = Mathf.Min(itsCurrentZoom,itsDataModuleMinimap.itsZoom.itsZoomMax);
		itsCurrentZoom = Mathf.Max(itsCurrentZoom,itsDataModuleMinimap.itsZoom.itsZoomMin);
		itsCurrentZoomDest = Mathf.Min(itsCurrentZoomDest,itsDataModuleMinimap.itsZoom.itsZoomMax);
		itsCurrentZoomDest = Mathf.Max(itsCurrentZoomDest,itsDataModuleMinimap.itsZoom.itsZoomMin);
	}

	/// <summary>
	/// Get current zoom factor
	/// </summary>
	/// <returns>The current zoom factor in pixels per meter</returns>
	public float GetZoom()
	{
		return itsCurrentZoomDest;
	}
	
	/// <summary>
	/// Set current zoom factor
	/// </summary>
	/// <param name="theZoom">The zoom factor in pixels per meter</param>
	public void SetZoom(float theZoom)
	{
		SetZoom(theZoom,true);
	}
	
	/// <summary>
	/// Set current zoom factor
	/// </summary>
	/// <param name="theZoom">The zoom factor in pixels per meter</param>
	/// <param name="theAnimate">FALSE = set instantly, TRUE = animate</param>
	public void SetZoom(float theZoom,bool theAnimate)
	{
		if (!theAnimate)
			itsCurrentZoom = theZoom;
		itsCurrentZoomDest = theZoom;
		
		CorrectCurrentZoom();
		UpdateOrthographicSize();
		UpdateIconScale();
	}
	
	float itsZoomChangeVelocity = 0;
	void UpdateZoom()
	{
		if (itsCurrentZoomDest != itsCurrentZoom)
		{
			itsCurrentZoom = Mathf.SmoothDamp(itsCurrentZoom,itsCurrentZoomDest,ref itsZoomChangeVelocity,0.3f);
			if (Mathf.Abs(itsCurrentZoomDest - itsCurrentZoom) < 0.05f)
				itsCurrentZoom = itsCurrentZoomDest;
			
			CorrectCurrentZoom();
			UpdateOrthographicSize();
			UpdateIconScale();
		}
	}
	
	/// <summary>
	/// Extends the minimap range by itsZoomRange
	/// The range will be clamped to itsMaxRange
	/// </summary>
	public void ZoomOut()
	{
		SetZoom(GetZoom() + itsDataModuleMinimap.itsZoom.itsZoomChangeValue);
	}

	/// <summary>
	/// Reduces the minimap range by itsZoomRange
	/// The range will be clamped to itsMinrange
	/// </summary>
	public void ZoomIn()
	{
		SetZoom(GetZoom() - itsDataModuleMinimap.itsZoom.itsZoomChangeValue);
	}

	/// <summary>
	/// Set zoom to minimum
	/// </summary>
	public void ZoomMin()
	{
		SetZoom(itsDataModuleMinimap.itsZoom.itsZoomMin);
	}

	/// <summary>
	/// Set zoom to maximum
	/// </summary>
	public void ZoomMax()
	{
		SetZoom(itsDataModuleMinimap.itsZoom.itsZoomMax);
	}

	/// <summary>
	/// Enable/disable display of viewport
	/// </summary>
	/// <param name="theEnable"></param>
	public void SetViewportEnabled(bool theEnable)
	{
		itsDataModuleMinimap.itsViewport.itsActive = theEnable;
	}
	
	/// <summary>
	/// Enable/disable display of viewport
	/// </summary>
	/// <param name="theEnable"></param>
	public bool GetViewportEnabled()
	{
		return itsDataModuleMinimap.itsViewport.itsActive;
	}

	/// <summary>
	/// Returns the percentage of revealed fog of war
	/// Value is between 0 and 1
	/// </summary>
	/// <returns></returns>
	public float GetRevealedPercent()
	{
		float aSum = 0;
		if (itsMeshFilterFogOfWarPlane != null)
		{
			foreach (Color aColor in itsMeshFilterFogOfWarPlane.mesh.colors)
			{
				aSum += aColor.a;
			}
			return 1 - aSum/itsMeshFilterFogOfWarPlane.mesh.colors.Length;
		}
		return 0;
	}
	#endregion

	#region Methods for KGFICustomGUI
	public override string GetName()
	{
		return name;
	}

	public string GetHeaderName()
	{
		return name;
	}

	public override Texture2D GetIcon()
	{
		return (Texture2D)Resources.Load("KGFMapSystem/textures/mapsystem_small", typeof(Texture2D));
	}

	string[] GetNamesFromLayerMask(LayerMask theLayers)
	{
		List<string> aNameList = new List<string>();
		for (int i=0;i<32;i++)
		{
			if ((theLayers & (1 << i)) != 0)
			{
				string aName = LayerMask.LayerToName(i);
				if (aName.Trim() != string.Empty)
				{
					aNameList.Add(aName);
				}
			}
		}
		return aNameList.ToArray();
	}

	void DrawCustomGuiMain()
	{
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		{
			KGFGUIUtility.Label("itsTarget");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsGlobalSettings.itsTarget.gameObject.GetObjectPath());
		}
		KGFGUIUtility.EndHorizontalBox();
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		{
			KGFGUIUtility.Label("itsStaticNorth");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsGlobalSettings.itsStaticNorth);
		}
		KGFGUIUtility.EndHorizontalBox();
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxBottom);
		{
			KGFGUIUtility.Label("Enabled");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsGlobalSettings.itsIsActive);
		}
		KGFGUIUtility.EndHorizontalBox();
	}

	void DrawCustomGuiAppearanceMinimap()
	{
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		{
			KGFGUIUtility.Label("itsSize");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsAppearanceMiniMap.itsSize);
		}
		KGFGUIUtility.EndHorizontalBox();
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		{
			KGFGUIUtility.Label("itsButtonSize");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsAppearanceMiniMap.itsButtonSize);
		}
		KGFGUIUtility.EndHorizontalBox();
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		{
			KGFGUIUtility.Label("itsButtonPadding");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsAppearanceMiniMap.itsButtonPadding);
		}
		KGFGUIUtility.EndHorizontalBox();
		
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		{
			KGFGUIUtility.Label("itsScaleIcons");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsAppearanceMiniMap.itsScaleIcons);
		}
		KGFGUIUtility.EndHorizontalBox();
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		{
			KGFGUIUtility.Label("itsScaleArrows");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsAppearanceMiniMap.itsScaleArrows);
		}
		KGFGUIUtility.EndHorizontalBox();
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxBottom);
		{
			KGFGUIUtility.Label("itsRadiusArrows");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsAppearanceMiniMap.itsRadiusArrows);
		}
		KGFGUIUtility.EndHorizontalBox();
	}

	void DrawCustomGuiAppearanceMap()
	{
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		{
			KGFGUIUtility.Label("itsButtonSize");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsAppearanceMap.itsButtonSize);
		}
		KGFGUIUtility.EndHorizontalBox();
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		{
			KGFGUIUtility.Label("itsButtonPadding");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsAppearanceMap.itsButtonPadding);
		}
		KGFGUIUtility.EndHorizontalBox();
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		{
			KGFGUIUtility.Label("itsButtonSpace");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsAppearanceMap.itsButtonSpace);
		}
		KGFGUIUtility.EndHorizontalBox();
		
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxBottom);
		{
			KGFGUIUtility.Label("itsScaleIcons");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsAppearanceMap.itsScaleIcons);
		}
		KGFGUIUtility.EndHorizontalBox();
	}

	void DrawCustomGuiFogOfWar()
	{
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		{
			KGFGUIUtility.Label("Enabled");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsFogOfWar.itsActive);
		}
		KGFGUIUtility.EndHorizontalBox();
		
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		{
			KGFGUIUtility.Label("itsResolutionX");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsFogOfWar.itsResolutionX);
		}
		KGFGUIUtility.EndHorizontalBox();
		
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		{
			KGFGUIUtility.Label("itsResolutionY");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsFogOfWar.itsResolutionY);
		}
		KGFGUIUtility.EndHorizontalBox();
		
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		{
			KGFGUIUtility.Label("itsRevealDistance");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsFogOfWar.itsRevealDistance);
		}
		KGFGUIUtility.EndHorizontalBox();
		
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		{
			KGFGUIUtility.Label("itsRevealedFullDistance");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsFogOfWar.itsRevealedFullDistance);
		}
		KGFGUIUtility.EndHorizontalBox();
		
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxBottom);
		{
			KGFGUIUtility.Label("Revealed");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(string.Format("{0:0.00}%",GetRevealedPercent()*100));
		}
		KGFGUIUtility.EndHorizontalBox();
	}

	void DrawCustomGuiZoom()
	{
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		{
			KGFGUIUtility.Label("Current zoom");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsCurrentZoom);
		}
		KGFGUIUtility.EndHorizontalBox();
		
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		{
			KGFGUIUtility.Label("itsZoomStartValue");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsZoom.itsZoomStartValue);
		}
		KGFGUIUtility.EndHorizontalBox();
		
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		{
			KGFGUIUtility.Label("itsZoomMin");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsZoom.itsZoomMin);
		}
		KGFGUIUtility.EndHorizontalBox();
		
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		{
			KGFGUIUtility.Label("itsZoomMax");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsZoom.itsZoomMax);
		}
		KGFGUIUtility.EndHorizontalBox();
		
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxBottom);
		{
			KGFGUIUtility.Label("itsZoomChangeValue");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsZoom.itsZoomChangeValue);
		}
		KGFGUIUtility.EndHorizontalBox();
	}

	void DrawCustomGuiViewport()
	{
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		{
			KGFGUIUtility.Label("Enabled");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsViewport.itsActive);
		}
		KGFGUIUtility.EndHorizontalBox();
		
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxBottom);
		{
			KGFGUIUtility.Label("itsCamera");
			GUILayout.FlexibleSpace();
			if (itsDataModuleMinimap.itsViewport.itsCamera != null)
				KGFGUIUtility.Label(""+itsDataModuleMinimap.itsViewport.itsCamera.gameObject.GetObjectPath());
			else
				KGFGUIUtility.Label("NONE");
		}
		KGFGUIUtility.EndHorizontalBox();
	}

	void DrawCustomGuiPhoto()
	{
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		{
			KGFGUIUtility.Label("Enabled");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsPhoto.itsTakePhoto);
		}
		KGFGUIUtility.EndHorizontalBox();
		
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxBottom);
		{
			KGFGUIUtility.Label("itsPhotoLayers");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxInvisible);
			{
				foreach (string aName in GetNamesFromLayerMask(itsDataModuleMinimap.itsPhoto.itsPhotoLayers))
				{
					GUILayout.Label(aName);
				}
			}
			KGFGUIUtility.EndVerticalBox();
		}
		KGFGUIUtility.EndHorizontalBox();
	}

	void DrawCustomGuiMapIcons()
	{
		// create statistics
		int aCountVisible = 0;
		Dictionary<string,int> aListCategories = new Dictionary<string, int>();
		foreach (mapicon_listitem_script anItem in itsListMapIcons)
		{
			if (anItem.GetMapIconVisibilityEffective())
			{
				aCountVisible++;
			}
			
			if (!aListCategories.ContainsKey(anItem.itsMapIcon.GetCategory()))
			{
				aListCategories[anItem.itsMapIcon.GetCategory()] = 1;
			}else{
				aListCategories[anItem.itsMapIcon.GetCategory()]++;
			}
		}
		
		// draw statistics
		KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBox);
		{
			KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
			{
				KGFGUIUtility.Label("Icons");
				GUILayout.FlexibleSpace();
				KGFGUIUtility.Label(""+itsListMapIcons.Count);
			}
			KGFGUIUtility.EndHorizontalBox();
			KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
			{
				KGFGUIUtility.Label("Icons visible");
				GUILayout.FlexibleSpace();
				KGFGUIUtility.Label(""+aCountVisible);
			}
			KGFGUIUtility.EndHorizontalBox();
			
			KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
			{
				KGFGUIUtility.Label("Icons by category");
				GUILayout.FlexibleSpace();
//				KGFGUIUtility.Label(""+aListCategories.Count);
			}
			KGFGUIUtility.EndHorizontalBox();
			
			KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxBottom);
			{
				KGFGUIUtility.Space();
				KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBox);
				{
					int i = 0;
					foreach (KeyValuePair<string,int>aCategory in aListCategories)
					{
						if (i == 0)
						{
							KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxTop);
						}
						else if (i == aListCategories.Count - 1)
						{
							KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxBottom);
						}
						else
						{
							KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
						}
						{
							KGFGUIUtility.Label(string.Format("\'{0}\'",aCategory.Key));
							GUILayout.FlexibleSpace();
							KGFGUIUtility.Label(""+aCategory.Value);
						}
						KGFGUIUtility.EndHorizontalBox();
						i++;
					}
				}
				KGFGUIUtility.EndVerticalBox();
			}
			KGFGUIUtility.EndHorizontalBox();
		}
		KGFGUIUtility.EndVerticalBox();
	}

	void DrawCustomGuiUserFlags()
	{
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		{
			KGFGUIUtility.Label("Enabled");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsUserFlags.itsActive);
		}
		KGFGUIUtility.EndHorizontalBox();
		
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		{
			KGFGUIUtility.Label("itsMapIcon");
			GUILayout.FlexibleSpace();
			if (itsDataModuleMinimap.itsUserFlags.itsMapIcon != null)
				KGFGUIUtility.Label(""+itsDataModuleMinimap.itsUserFlags.itsMapIcon.gameObject.GetObjectPath());
			else
				KGFGUIUtility.Label("NONE");
		}
		KGFGUIUtility.EndHorizontalBox();
		
//		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
//		{
//			KGFGUIUtility.Label("itsRemoveClickDistance");
//			GUILayout.FlexibleSpace();
//			KGFGUIUtility.Label(""+itsDataModuleMinimap.itsUserFlags.itsRemoveClickDistance);
//		}
//		KGFGUIUtility.EndHorizontalBox();
		
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxBottom);
		{
			KGFGUIUtility.Label("Flag count");
			GUILayout.FlexibleSpace();
			KGFGUIUtility.Label(""+GetUserFlags().Length);
		}
		KGFGUIUtility.EndHorizontalBox();
	}

	Vector2 itsCustomGuiPosition = Vector2.zero;
	public void Render()
	{
		itsCustomGuiPosition = KGFGUIUtility.BeginScrollView(itsCustomGuiPosition,false,false);
		{
			KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxInvisible);
			{
				KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxInvisible);
				{
					KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxDecorated);
					{
						KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkTop);
						KGFGUIUtility.Label("Main",GetIcon(),KGFGUIUtility.eStyleLabel.eLabelFitIntoBox);
						GUILayout.FlexibleSpace();
						KGFGUIUtility.EndHorizontalBox();
						DrawCustomGuiMain();
					}
					KGFGUIUtility.EndVerticalBox();
					
					KGFGUIUtility.Space();
					
					KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxDecorated);
					{
						KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkTop);
						KGFGUIUtility.Label("Appearance Minimap",GetIcon(),KGFGUIUtility.eStyleLabel.eLabelFitIntoBox);
						GUILayout.FlexibleSpace();
						KGFGUIUtility.EndHorizontalBox();
						DrawCustomGuiAppearanceMinimap();
					}
					KGFGUIUtility.EndVerticalBox();
					
					KGFGUIUtility.Space();
					
					KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxDecorated);
					{
						KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkTop);
						KGFGUIUtility.Label("Appearance Map",GetIcon(),KGFGUIUtility.eStyleLabel.eLabelFitIntoBox);
						GUILayout.FlexibleSpace();
						KGFGUIUtility.EndHorizontalBox();
						DrawCustomGuiAppearanceMap();
					}
					KGFGUIUtility.EndVerticalBox();
					
					KGFGUIUtility.Space();
					
					KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxDecorated);
					{
						KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkTop);
						KGFGUIUtility.Label("Fog of war",GetIcon(),KGFGUIUtility.eStyleLabel.eLabelFitIntoBox);
						GUILayout.FlexibleSpace();
						KGFGUIUtility.EndHorizontalBox();
						DrawCustomGuiFogOfWar();
					}
					KGFGUIUtility.EndVerticalBox();
					
					KGFGUIUtility.Space();
					
					KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxDecorated);
					{
						KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkTop);
						KGFGUIUtility.Label("Zoom",GetIcon(),KGFGUIUtility.eStyleLabel.eLabelFitIntoBox);
						GUILayout.FlexibleSpace();
						KGFGUIUtility.EndHorizontalBox();
						DrawCustomGuiZoom();
					}
					KGFGUIUtility.EndVerticalBox();
				}
				KGFGUIUtility.EndVerticalBox();
				
				KGFGUIUtility.Space();
				
				KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxInvisible);
				{
					KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxDecorated);
					{
						KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkTop);
						KGFGUIUtility.Label("Viewport",GetIcon(),KGFGUIUtility.eStyleLabel.eLabelFitIntoBox);
						GUILayout.FlexibleSpace();
						KGFGUIUtility.EndHorizontalBox();
						DrawCustomGuiViewport();
					}
					KGFGUIUtility.EndVerticalBox();
					
					KGFGUIUtility.Space();
					
					KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxDecorated);
					{
						KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkTop);
						KGFGUIUtility.Label("Photo",GetIcon(),KGFGUIUtility.eStyleLabel.eLabelFitIntoBox);
						GUILayout.FlexibleSpace();
						KGFGUIUtility.EndHorizontalBox();
						DrawCustomGuiPhoto();
					}
					KGFGUIUtility.EndVerticalBox();
					
					KGFGUIUtility.Space();
					
					KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxDecorated);
					{
						KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkTop);
						KGFGUIUtility.Label("Map Icons",GetIcon(),KGFGUIUtility.eStyleLabel.eLabelFitIntoBox);
						GUILayout.FlexibleSpace();
						KGFGUIUtility.EndHorizontalBox();
						DrawCustomGuiMapIcons();
					}
					KGFGUIUtility.EndVerticalBox();
					
					KGFGUIUtility.Space();
					
					KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxDecorated);
					{
						KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkTop);
						KGFGUIUtility.Label("User flags",GetIcon(),KGFGUIUtility.eStyleLabel.eLabelFitIntoBox);
						GUILayout.FlexibleSpace();
						KGFGUIUtility.EndHorizontalBox();
						DrawCustomGuiUserFlags();
					}
					KGFGUIUtility.EndVerticalBox();
				}
				KGFGUIUtility.EndVerticalBox();
			}
			KGFGUIUtility.EndHorizontalBox();
		}
		KGFGUIUtility.EndScrollView();
	}
	#endregion

	#region Methods for KGFIValidator
	void NullError(ref KGFMessageList theMessageList,string theName, object theValue)
	{
		if (theValue == null)
		{
			theMessageList.AddError(string.Format("value of '{0}' must not be null",theName));
		}
	}

	void RegionError(ref KGFMessageList theMessageList,string theName, float theValue,float theMin,float theMax)
	{
		if (theValue < theMin ||theValue > theMax)
		{
			theMessageList.AddError(string.Format("Value has to be between {0} and {1} ({2})",theMin,theMax,theName));
		}
	}

	void PositiveError(ref KGFMessageList theMessageList,string theName, float theValue)
	{
		if (theValue < 0)
		{
			theMessageList.AddError(string.Format("{0} must be positive",theName));
		}
	}

	public override KGFMessageList Validate()
	{
		KGFMessageList aMessageList = new KGFMessageList();
		
		// main
		RegionError(ref aMessageList,"itsDataModuleMinimap.itsStaticNorth",itsDataModuleMinimap.itsGlobalSettings.itsStaticNorth,0,360);
		if (itsDataModuleMinimap.itsGlobalSettings.itsTarget == null)
		{
			aMessageList.AddError("itsTarget must not be null. Please add a target that is always centered on the minimap (e.g.: the character).");
		}
		
		// appearance minimap
		if (itsDataModuleMinimap.itsAppearanceMiniMap.itsMask != null && !GetHasProVersion())
		{
			aMessageList.AddWarning("Masking texture does only work in Unity Pro version. (itsAppearanceMiniMap.itsMask)");
		}
		RegionError(ref aMessageList,"itsAppearanceMiniMap.itsSize",itsDataModuleMinimap.itsAppearanceMiniMap.itsSize,0,1);
		RegionError(ref aMessageList,"itsAppearanceMiniMap.itsButtonSize",itsDataModuleMinimap.itsAppearanceMiniMap.itsButtonSize,0,1);
		RegionError(ref aMessageList,"itsAppearanceMiniMap.itsButtonPadding",itsDataModuleMinimap.itsAppearanceMiniMap.itsButtonPadding,0,1);
		RegionError(ref aMessageList,"itsAppearanceMiniMap.itsScaleArrows",itsDataModuleMinimap.itsAppearanceMiniMap.itsScaleArrows,0,1);
		RegionError(ref aMessageList,"itsAppearanceMiniMap.itsScaleIcons",itsDataModuleMinimap.itsAppearanceMiniMap.itsScaleIcons,0,1);
		RegionError(ref aMessageList,"itsAppearanceMiniMap.itsRadiusArrows",itsDataModuleMinimap.itsAppearanceMiniMap.itsRadiusArrows,0,1);
		PositiveError(ref aMessageList,"itsAppearanceMiniMap.itsBackgroundBorder",itsDataModuleMinimap.itsAppearanceMiniMap.itsBackgroundBorder);
		if (itsDataModuleMinimap.itsAppearanceMiniMap.itsBackground == null)
		{
			aMessageList.AddWarning("Appearance texture should be set (itsAppearance.itsBackground)");
		}
		if (itsDataModuleMinimap.itsAppearanceMiniMap.itsButton == null)
		{
			aMessageList.AddWarning("Appearance texture should be set (itsAppearance.itsButton)");
		}
		if (itsDataModuleMinimap.itsAppearanceMiniMap.itsButtonDown == null)
		{
			aMessageList.AddWarning("Appearance texture should be set (itsAppearance.itsButtonDown)");
		}
		if (itsDataModuleMinimap.itsAppearanceMiniMap.itsButtonHover == null)
		{
			aMessageList.AddWarning("Appearance texture should be set (itsAppearance.itsButtonHover)");
		}
		if (itsDataModuleMinimap.itsAppearanceMiniMap.itsIconZoomIn == null)
		{
			aMessageList.AddWarning("Appearance texture should be set (itsAppearance.itsIconZoomIn)");
		}
		if (itsDataModuleMinimap.itsAppearanceMiniMap.itsIconZoomOut == null)
		{
			aMessageList.AddWarning("Appearance texture should be set (itsAppearance.itsIconZoomOut)");
		}
		if (itsDataModuleMinimap.itsAppearanceMiniMap.itsIconZoomLock == null)
		{
			aMessageList.AddWarning("Appearance texture should be set (itsAppearance.itsIconZoomLock)");
		}
		if (itsDataModuleMinimap.itsAppearanceMiniMap.itsIconFullscreen == null)
		{
			aMessageList.AddWarning("Appearance texture should be set (itsAppearance.itsIconFullscreen)");
		}
		
		// appearance map
		RegionError(ref aMessageList,"itsAppearanceMap.itsButtonSize",itsDataModuleMinimap.itsAppearanceMap.itsButtonSize,0,1);
		RegionError(ref aMessageList,"itsAppearanceMap.itsButtonPadding",itsDataModuleMinimap.itsAppearanceMap.itsButtonPadding,0,1);
		RegionError(ref aMessageList,"itsAppearanceMap.itsButtonSpace",itsDataModuleMinimap.itsAppearanceMap.itsButtonSpace,0,1);
		RegionError(ref aMessageList,"itsAppearanceMap.itsScaleIcons",itsDataModuleMinimap.itsAppearanceMap.itsScaleIcons,0,1);
		PositiveError(ref aMessageList,"itsAppearanceMap.itsBackgroundBorder",itsDataModuleMinimap.itsAppearanceMap.itsBackgroundBorder);
		if (itsDataModuleMinimap.itsGlobalSettings.itsColorAll != Color.white && !GetHasProVersion())
		{
			aMessageList.AddError("itsColorAll does only work in Unity Pro version. (itsColorAll)");
		}
		if (itsDataModuleMinimap.itsAppearanceMap.itsMask != null && !GetHasProVersion())
		{
			aMessageList.AddWarning("Masking texture does only work in Unity Pro version. (itsAppearanceMap.itsMask)");
		}
		if (itsDataModuleMinimap.itsAppearanceMap.itsButton == null)
		{
			aMessageList.AddWarning("Appearance texture should be set (itsAppearanceMap.itsButton)");
		}
		if (itsDataModuleMinimap.itsAppearanceMap.itsButtonDown == null)
		{
			aMessageList.AddWarning("Appearance texture should be set (itsAppearanceMap.itsButtonDown)");
		}
		if (itsDataModuleMinimap.itsAppearanceMap.itsButtonHover == null)
		{
			aMessageList.AddWarning("Appearance texture should be set (itsAppearanceMap.itsButtonHover)");
		}
		if (itsDataModuleMinimap.itsAppearanceMap.itsIconZoomIn == null)
		{
			aMessageList.AddWarning("Appearance texture should be set (itsAppearanceMap.itsIconZoomIn)");
		}
		if (itsDataModuleMinimap.itsAppearanceMap.itsIconZoomOut == null)
		{
			aMessageList.AddWarning("Appearance texture should be set (itsAppearanceMap.itsIconZoomOut)");
		}
		if (itsDataModuleMinimap.itsAppearanceMap.itsIconZoomLock == null)
		{
			aMessageList.AddWarning("Appearance texture should be set (itsAppearanceMap.itsIconZoomLock)");
		}
		if (itsDataModuleMinimap.itsAppearanceMap.itsIconFullscreen == null)
		{
			aMessageList.AddWarning("Appearance texture should be set (itsAppearanceMap.itsIconFullscreen)");
		}
		
		// fog-of-war
		PositiveError(ref aMessageList,"itsFogOfWar.itsResolutionX",itsDataModuleMinimap.itsFogOfWar.itsResolutionX);
		PositiveError(ref aMessageList,"itsFogOfWar.itsResolutionY",itsDataModuleMinimap.itsFogOfWar.itsResolutionY);
		PositiveError(ref aMessageList,"itsFogOfWar.itsRevealDistance",itsDataModuleMinimap.itsFogOfWar.itsRevealDistance);
		PositiveError(ref aMessageList,"itsFogOfWar.itsRevealedFullDistance",itsDataModuleMinimap.itsFogOfWar.itsRevealedFullDistance);
		if (itsDataModuleMinimap.itsFogOfWar.itsRevealedFullDistance > itsDataModuleMinimap.itsFogOfWar.itsRevealDistance)
		{
			aMessageList.AddError("itsFogOfWar.itsRevealDistance must be bigger than itsFogOfWar.itsRevealedFullDistance");
		}
		
		// zoom
		PositiveError(ref aMessageList,"itsZoom.itsZoomChangeValue",itsDataModuleMinimap.itsZoom.itsZoomChangeValue);
		PositiveError(ref aMessageList,"itsZoom.itsZoomMax",itsDataModuleMinimap.itsZoom.itsZoomMax);
		PositiveError(ref aMessageList,"itsZoom.itsZoomMin",itsDataModuleMinimap.itsZoom.itsZoomMin);
		PositiveError(ref aMessageList,"itsZoom.itsZoomStartValue",itsDataModuleMinimap.itsZoom.itsZoomStartValue);
		if (itsDataModuleMinimap.itsZoom.itsZoomMin > itsDataModuleMinimap.itsZoom.itsZoomMax)
		{
			aMessageList.AddError("itsZoom.itsZoomMax must be bigger than itsZoom.itsZoomMin");
		}
		if (itsDataModuleMinimap.itsZoom.itsZoomStartValue < itsDataModuleMinimap.itsZoom.itsZoomMin ||
		    itsDataModuleMinimap.itsZoom.itsZoomStartValue > itsDataModuleMinimap.itsZoom.itsZoomMax)
		{
			aMessageList.AddError("itsZoom.itsZoomStartValue has to be between itsZoom.itsZoomMin and itsZoom.itsZoomMin");
		}
		
		// viewport
		if (itsDataModuleMinimap.itsViewport.itsActive && itsDataModuleMinimap.itsViewport.itsCamera == null)
		{
			aMessageList.AddError("Active viewport needs a camera (itsViewport.itsCamera)");
		}
		if (itsDataModuleMinimap.itsViewport.itsActive && itsDataModuleMinimap.itsGlobalSettings.itsOrientation == KGFMapSystemOrientation.XYSideScroller)
		{
			aMessageList.AddError("Viewport display is not supported in SideScroller mode (itsViewport.itsCamera)");
		}
		if (itsDataModuleMinimap.itsViewport.itsColor.a == 0)
		{
			aMessageList.AddError("Viewport will be invisible if itsColor.a == 0");
		}
		
		// photo
		if (itsDataModuleMinimap.itsPhoto.itsTakePhoto && itsDataModuleMinimap.itsPhoto.itsPhotoLayers == 0)
		{
			aMessageList.AddError("itsPhoto.itsPhotoLayers has to contain some layers for the photo not to be empty");
		}
		
		// user flags
		if (itsDataModuleMinimap.itsUserFlags.itsActive)
		{
			
		}
		
		// layer check
		if (LayerMask.NameToLayer(itsLayerName) < 0)
		{
			aMessageList.AddError(string.Format("The map system needs a layer with the name '{0}'",itsLayerName));
		}
//		if (itsDataModuleMinimap.itsGlobalSettings.itsRenderLayers == 0)
//		{
//			aMessageList.AddError(string.Format("itsRenderLayer is not allowed to be Nothing"));
//		}
		
		// panning
		if (itsDataModuleMinimap.itsPanning.itsActive && itsDataModuleMinimap.itsPanning.itsUseBounds && itsDataModuleMinimap.itsPanning.itsBoundsLayers == 0)
		{
			aMessageList.AddError("itsPanning.itsBoundsLayers has to contain some layers for the panning bounds to work");
		}
		
		if(itsDataModuleMinimap.itsShaders.itsShaderMapIcon == null)
		{
			aMessageList.AddError(string.Format("itsDataModuleMinimap.itsShaders.itsShaderMapIcon is null"));
		}
		
		if(itsDataModuleMinimap.itsShaders.itsShaderPhotoPlane == null)
		{
			aMessageList.AddError(string.Format("itsDataModuleMinimap.itsShaders.itsShaderPhotoPlane is null"));
		}
		
		if(itsDataModuleMinimap.itsShaders.itsShaderMapMask == null)
		{
			aMessageList.AddError(string.Format("itsDataModuleMinimap.itsShaders.itsShaderMapMask is null"));
		}
		
		if(itsDataModuleMinimap.itsFogOfWar.itsActive == true && itsDataModuleMinimap.itsShaders.itsShaderFogOfWar == null)
		{
			aMessageList.AddWarning(string.Format("itsDataModuleMinimap.itsShaders.itsShaderFogOfWar is null, fog of war will not work"));
		}
		
//		Transform aTransform = transform.Find("measure_cube");
//		if(itsDataModuleMinimap.itsPhoto.itsTakePhoto == true && aTransform == null)
//		{
//			aMessageList.AddError(string.Format("please press the RecalcPhotoArea button"));
//		}
		
		return aMessageList;
	}
	#endregion

	#region KGFModule
	public override string GetForumPath()
	{
		return "";
	}

	public override string GetDocumentationPath()
	{
		return "";
	}
	#endregion
}
