//MJ

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTileScript : MonoBehaviour
{
    public int index;

    //private SpriteRenderer spriteRenderer;
    public bool onTileFace = false;
    GameManagerScript gm;

    // Start is called before the first frame update
    void Start()
    {
        this.toggleTileFace();
        gm = FindObjectOfType<GameManagerScript>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void toggleTileFace()
    {
        this.transform.Rotate(180,0,0);
        //create indicator to show if face is shown or not
    }

    //HANDLE INTERACTIONS
    void OnMouseEnter()
    {
        //If your mouse hovers over the GameObject with the script attached, output this message
        //Debug.Log("Mouse is over GameObject." + this.name);

    }

    void OnMouseDown()
    {
        //Debug.Log("Mouse click on " + this.name);

        string currentPlayerTiles = "P" + gm.CurrentPlayer + "Tiles";

        if (gm.GetParent(this) == currentPlayerTiles)
        {
            //Debug.Log("Yes Current, " + currentPlayerTiles);
            //gm.DiscardTile(this);
            gm.tiletodiscard = this.name;
            gm.responded = true;
        }
        else {
            //Debug.Log("Not current player's tiles, Current player: P" + gm.CurrentPlayer);
        }

    }


    void OnMouseExit()
    {
        //The mouse is no longer hovering over the GameObject so output this message each frame
        //Debug.Log("Mouse is no longer on GameObject." + this.name);
    }

}
