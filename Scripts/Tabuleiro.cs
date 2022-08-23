using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tabuleiro : MonoBehaviour
{
    [Header("art")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.004f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deadSize = 0.3f, deadSpace;
    [SerializeField] private float dragOffset = 1.5f;
    [SerializeField] private GameObject victoryScreen ; 

    [Header("Prefabs e materiais ")]
    [SerializeField]
    private GameObject[] prefabs;
    [SerializeField]
    private Material[] teamMaterial;


    // variaveis 
    private XadrezPeca[,] chessPiece;
    private XadrezPeca currentlyDragging;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private List<XadrezPeca>  deadWhites = new List<XadrezPeca>();
    private List<XadrezPeca>  deadWBlacks = new List<XadrezPeca>();
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamara;
    private Vector2Int currentHover;
    private Vector3 bounds;
    private bool isWhiteTurn; 

    // variaveis 

    private void Awake()
    {
        isWhiteTurn = true;
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        spwanAllPieces();
        positionAllPieces();
    }

    private void Update()
    {
        if (!currentCamara)
        {
            currentCamara = Camera.main;
            return;
        }

        RaycastHit info;
        Ray ray = currentCamara.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Highlight", "Hover")))
        {
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            if(Input.GetMouseButtonDown(0)){
                if(chessPiece[hitPosition.x, hitPosition.y] != null){
                    // turno
                    if( chessPiece[hitPosition.x, hitPosition.y].team == 0 && isWhiteTurn || chessPiece[hitPosition.x, hitPosition.y].team == 1 && !isWhiteTurn){
                        currentlyDragging = chessPiece[hitPosition.x, hitPosition.y];
                        availableMoves = currentlyDragging.GetAvaliabeMoves(ref chessPiece , TILE_COUNT_X ,TILE_COUNT_Y);
                        HighlightTiles(); 

                    }
                }
            }

            if(currentlyDragging !=null && Input.GetMouseButtonUp(0)){
                Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);

                bool validMove = moveTo(currentlyDragging, hitPosition.x, hitPosition.y);
                if(!validMove)
                    currentlyDragging.setPosition(getTileCenter(previousPosition.x, previousPosition.y));
                    
                currentlyDragging = null;
                RemoveHighlightTiles();
            }
            else
            {
                if (currentHover != -Vector2Int.one)
                {
                    tiles[currentHover.x, currentHover.y].layer =  (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                    currentHover = -Vector2Int.one;
                }

                if(currentlyDragging && Input.GetMouseButtonUp(0)){
                    currentlyDragging.setPosition(getTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));
                    currentlyDragging = null;
                    RemoveHighlightTiles();

                }   
            }
        }

        // if(currentlyDragging){
        //     Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
        //     float distannce = 0.0f;
        //     if(horizontalPlane.Raycast(ray, out distannce))
        //         currentlyDragging.setPosition(ray.GetPoint(distannce) + Vector3.up * dragOffset);
        // }
    }
    // gerador de tiles
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];

        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
            }
        }
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };


        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    // gera peças 
    private void spwanAllPieces()
    {
        chessPiece = new XadrezPeca[TILE_COUNT_X, TILE_COUNT_Y];

        int whiteTeam = 0, blackTeam = 1;

        // brancas 
        chessPiece[0, 0] = spawSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPiece[1, 0] = spawSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPiece[2, 0] = spawSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPiece[3, 0] = spawSinglePiece(ChessPieceType.Queen, whiteTeam);
        chessPiece[4, 0] = spawSinglePiece(ChessPieceType.King, whiteTeam);
        chessPiece[5, 0] = spawSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPiece[6, 0] = spawSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPiece[7, 0] = spawSinglePiece(ChessPieceType.Rook, whiteTeam);
        // for (int i = 0; i < TILE_COUNT_X; i++)
        // {
        //     chessPiece[i, 1] = spawSinglePiece(ChessPieceType.Pawn, whiteTeam);
        // }

        // pretas 
        chessPiece[0, 7] = spawSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPiece[1, 7] = spawSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPiece[2, 7] = spawSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPiece[3, 7] = spawSinglePiece(ChessPieceType.Queen, blackTeam);
        chessPiece[4, 7] = spawSinglePiece(ChessPieceType.King, blackTeam);
        chessPiece[5, 7] = spawSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPiece[6, 7] = spawSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPiece[7, 7] = spawSinglePiece(ChessPieceType.Rook, blackTeam);
        // for (int i = 0; i < TILE_COUNT_X; i++)
        // {
        //     chessPiece[i, 6] = spawSinglePiece(ChessPieceType.Pawn, blackTeam);
        // }
    }

    private XadrezPeca spawSinglePiece(ChessPieceType type, int team)
    
    {
        XadrezPeca cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<XadrezPeca>();

        cp.type = type;
        cp.team = team;
        cp.GetComponent<MeshRenderer>().material = teamMaterial[team];

        return cp;
    }

    // posicionando as peças 
    private void positionAllPieces(){
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if(chessPiece[x,y] != null){
                    positionSinglePieces(x,y,true);
                }
            }   
        }
    }

     private void positionSinglePieces(int x, int y ,bool force = false){
        chessPiece[x,y].currentX = x;
        chessPiece[x,y].currentY = y;
        chessPiece[x,y].setPosition(getTileCenter(x,y), force);
    
    }

    private Vector3 getTileCenter(int x, int y){
        return new Vector3(x * tileSize , yOffset, y * tileSize) - bounds + new Vector3 (tileSize/2, 0, tileSize /2 ); 
    }

// opreaçaoes
    private bool ContainsValidMove(ref List<Vector2Int> moves , Vector2 pos){
        for (int i = 0; i < moves.Count; i++)
            if(moves[i].x == pos.x && moves[i].y == pos.y)
                return true;

        return false;   
        
    }

    private bool moveTo(XadrezPeca cp, int x, int y){
        if(!ContainsValidMove(ref availableMoves, new Vector2Int(x, y)))
            return false;

        Vector2Int previousPosition = new Vector2Int(cp.currentX, cp.currentY);

        if(chessPiece[x,y] != null){
            XadrezPeca ocp = chessPiece[x,y];

            if(cp.team == ocp.team){
                return false;
            }

            if(ocp.team == 0){

                if(ocp.type == ChessPieceType.King){
                    CheckMate(1);
                }

                deadWhites.Add(ocp);
                ocp.setScale(Vector3.one * deadSize);
                ocp.setPosition( new Vector3(8 * tileSize, yOffset, -1* tileSize)- bounds + new Vector3(tileSize/2 , 0, tileSize/2) + (Vector3.forward * deadSpace)* deadWhites.Count);
            }else{
                if(ocp.type == ChessPieceType.King){
                    CheckMate(0);
                }
                deadWBlacks.Add(ocp);
                ocp.setScale(Vector3.one * deadSize);
                ocp.setPosition( new Vector3(-1 * tileSize, yOffset, 8* tileSize)- bounds + new Vector3(tileSize/2 , 0, tileSize/2) + (Vector3.back * deadSpace)* deadWBlacks.Count);
            }
        }

        chessPiece[x,y] = cp;
        chessPiece[previousPosition.x, previousPosition.y] = null;

        positionSinglePieces(x,y);

        isWhiteTurn = ! isWhiteTurn;

        return true ;
    }

    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (tiles[x, y] == hitInfo)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return -Vector2Int.one;
    }

    // HighlightTiles

    private void HighlightTiles(){
        for (int i = 0; i < availableMoves.Count; i++)
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
        
    }

    private void RemoveHighlightTiles(){
        for (int i = 0; i < availableMoves.Count; i++)
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");

        availableMoves.Clear();
    }
    

    //CheckMte

    private void CheckMate(int team){
        DisplayVictory(team);
    }

    private void DisplayVictory(int winningTeam){
        victoryScreen.SetActive(true);
        victoryScreen.transform.GetChild(winningTeam).gameObject.SetActive(true);
    }

    public void OnResetButton(){
        victoryScreen.transform.GetChild(0).gameObject.SetActive(false);
        victoryScreen.transform.GetChild(1).gameObject.SetActive(false);
        victoryScreen.SetActive(false);
    }

    public void OnExitButton(){
        Application.Quit();
    }

}
