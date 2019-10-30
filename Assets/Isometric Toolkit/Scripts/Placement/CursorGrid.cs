using UnityEngine;
using System.Collections;
 
/// <summary>
/// Cursor grid.
/// </summary>
/// <see cref="http://answers.unity3d.com/questions/442581/how-to-draw-a-grid-over-parts-of-the-terrain.html"/>
public class CursorGrid : MonoBehaviour
{
    
	/// <summary>
	/// The size of the grid both columns and rows.
	/// </summary>
	public int GridSize = 9;
	
	/// <summary>
	/// The size of the indicator.
	/// </summary>
	/// <remarks>
	/// indicatorSize changes the size of the grid. 
	/// The grid is just a texture so it doesn't add more squares 
	/// (it's possible with some modification to the uv calculations).
	/// </remarks>
	public float indicatorSize = 1.0f;
	
	/// <summary>
	/// How high the grid floats over the terrain
	/// </summary>
	/// <remarks>
	/// indicatorOffsetY is to raise the mesh from the terrain, 
	/// for some reason converting the height from terrain size 
	/// to heightmap size and back again messes the actual height value. 
	/// And further away from the terrain, there is some z-axis fighting. 
	/// This could probably be fixed by adding more vertices to the mesh.
	/// </remarks>
	public float indicatorOffsetY = 1.0f;
	public Terrain terrain;
	
	private Vector3[,] mapGrid;
	private TerrainData terrainData;
	private Vector3 terrainSize;
	private int heightmapWidth;
	private int heightmapHeight;
	private float[,] heightmapData;
       
	void GetTerrainData ()
	{
		mapGrid = new Vector3[ GridSize, GridSize];
		
		if (!terrain) {
			terrain = Terrain.activeTerrain;
		}
         
		terrainData = terrain.terrainData;
         
		terrainSize = terrain.terrainData.size;
         
		heightmapWidth = terrain.terrainData.heightmapWidth;
		heightmapHeight = terrain.terrainData.heightmapHeight;
         
		heightmapData = terrainData.GetHeights (0, 0, heightmapWidth, heightmapHeight);
	}
       
	private Vector3 rayHitPoint;
	private Vector3 heightmapPos;
       
	// Use this for initialization
	void Start ()
	{
		GetTerrainData ();
		ConstructMesh ();
	}
       
	// Update is called once per frame
	void Update ()
	{
		// raycast to the terrain
		RaycastToTerrain ();
         
		// find the heightmap position of that hit
		GetHeightmapPosition ();
         
		// Calculate Grid
		CalculateGrid ();
         
		// Update Mesh
		UpdateMesh (); 
	}
       
	void RaycastToTerrain ()
	{
		if (Globals.CurrentMouseHitOnTerrain.HasValue) {
			rayHitPoint = Globals.CurrentMouseHitOnTerrain.Value.point;
		}
	}
         
	void GetHeightmapPosition ()
	{
		// find the heightmap position of that hit
		heightmapPos.x = (rayHitPoint.x / terrainSize.x) * ((float)heightmapWidth);
		heightmapPos.z = (rayHitPoint.z / terrainSize.z) * ((float)heightmapHeight);
         
		// convert to integer
		heightmapPos.x = Mathf.Round (heightmapPos.x);
		heightmapPos.z = Mathf.Round (heightmapPos.z);
         
		// clamp to heightmap dimensions to avoid errors
		heightmapPos.x = Mathf.Clamp (heightmapPos.x, 0, heightmapWidth - 1);
		heightmapPos.z = Mathf.Clamp (heightmapPos.z, 0, heightmapHeight - 1);
	}
         
         
	// --------------------------------------------------------------------------- Calculate Grid
         
	void CalculateGrid ()
	{
		for (int z = -4; z < 5; z ++) {
			for (int x = -4; x < 5; x ++) {
				Vector3 calcVector;
				float calcPosX, calcPosZ;
    
				calcVector.x = heightmapPos.x + (x * indicatorSize);
				calcVector.x /= ((float)heightmapWidth);
				calcVector.x *= terrainSize.x;
         
				calcPosX = heightmapPos.x + (x * indicatorSize);
				calcPosX = Mathf.Clamp (calcPosX, 0, heightmapWidth - 1);
         
				calcPosZ = heightmapPos.z + (z * indicatorSize);
				calcPosZ = Mathf.Clamp (calcPosZ, 0, heightmapHeight - 1);
         
				calcVector.y = heightmapData [(int)calcPosZ, (int)calcPosX] * terrainSize.y; // heightmapData is Y,X ; not X,Y (reversed)
				calcVector.y += indicatorOffsetY; // raise slightly above terrain
         
				calcVector.z = heightmapPos.z + (z * indicatorSize);
				calcVector.z /= ((float)heightmapHeight);
				calcVector.z *= terrainSize.z;
         
				mapGrid [x + 4, z + 4] = calcVector;
			}
		}
	}
         
         
	// --------------------------------------------------------------------------- INDICATOR MESH
         
	private Mesh mesh;
	private Vector3[] verts;
	private Vector2[] uvs;
	private int[] tris;
         
	void ConstructMesh ()
	{
		if (!mesh) {
			mesh = new Mesh ();
			MeshFilter f = GetComponent ("MeshFilter") as MeshFilter;
			f.mesh = mesh;
			mesh.name = gameObject.name + "Mesh";
		}
         
		mesh.Clear ();  
         
		verts = new Vector3[GridSize * GridSize];
		uvs = new Vector2[GridSize * GridSize];
		tris = new int[ 8 * 2 * 8 * 3];
         
		float uvStep = 1.0f / 8.0f;
         
		int index = 0;
		int triIndex = 0;
         
		for (int z = 0; z < GridSize; z ++) {
			for (int x = 0; x < GridSize; x ++) {
				verts [index] = new Vector3 (x, 0, z);
				uvs [index] = new Vector2 (((float)x) * uvStep, ((float)z) * uvStep);
         
				if (x < 8 && z < 8) {
					tris [triIndex + 0] = index + 0;
					tris [triIndex + 1] = index + 9;
					tris [triIndex + 2] = index + 1;
         
					tris [triIndex + 3] = index + 1;
					tris [triIndex + 4] = index + 9;
					tris [triIndex + 5] = index + 10;
         
					triIndex += 6;
				}
         
				index ++;
			}
		}
         
         
		// - Build Mesh -
		mesh.vertices = verts;
		mesh.uv = uvs;
		mesh.triangles = tris;
         
		mesh.RecalculateBounds ();  
		mesh.RecalculateNormals ();
	}
         
	void UpdateMesh ()
	{
		int index = 0;
         
		for (int z = 0; z < GridSize; z ++) {
			for (int x = 0; x < GridSize; x ++) {
				verts [index] = mapGrid [x, z];
         
				index ++;
			}
		}
         
		// assign to mesh
		mesh.vertices = verts;
        
		mesh.RecalculateBounds ();
		mesh.RecalculateNormals ();
	}
}