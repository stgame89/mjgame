using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameManagerScript : MonoBehaviour
{
    public GameObject[] TileFaces;
    public int CurrentPlayer;
    int readyGame = 0;
    int turncount = 0;
    int counttime;
    public bool responded = false;
    int winnerfound = -99;
    bool endofturn = true;
    public int btnSelected = -99;
    public string tiletodiscard = "";
    public bool startGame = false;
    bool[] waitinghand = new bool[] { false,false,false,false };
    bool newGame = true;
    List<GameObject> tileSetGameObject = new List<GameObject>();
    string freshdrawtile;
    bool isInExposed = false;
    int currentWind = 1;
    int currentDealer = 1;

    //TILE INDEXES
    //Bamboo - 0 to 8
    //Character - 9 to 17
    //Dots - 18 to 26
    //Dragon - 27 to 29
    //Wind - 30 to 33

    //LISTS VARIABLES
    public List<string> tileset;

    //GAME OBJECTS VARIABLES
    public GameObject GameCanvas;
    public GameObject TilePrefab;
    public GameObject TileContainer;
    public GameObject Player1Container;
    public GameObject Player2Container;
    public GameObject Player3Container;
    public GameObject Player4Container;
    public GameObject Player1ExposedContainer;
    public GameObject Player2ExposedContainer;
    public GameObject Player3ExposedContainer;
    public GameObject Player4ExposedContainer;
    public GameObject DiscardContainer;
    public GameObject OverlayPanelPrefab;
    public GameObject OptionsButtonPrefab;
    public GameObject MeldSetContainerPrefab;
    public GameObject GameOverPanelPrefab;
    public GameObject CurrentWindText;
    public GameObject DealerText;

    //SCRIPTS VARIABLES
    InputManagerScript inputmanager;

    bool playerchow = false;
    bool playerponggang = false;
    int pongplayer = -99;
    int gangplayer = -99;
    bool playergang = false;

    // Start is called before the first frame update
    void Start()
    {
        //INITIALISATION
        inputmanager = FindObjectOfType<InputManagerScript>();
        startGame = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (turncount == 84)
        {
            readyGame = 0;
            //end the game
        }
        if (startGame)
        {
            startGame = false;
            StartCoroutine(GameLoop());
        }

    }

    //Check Hu must implement AFTER:
    //DrawCardFromBackWall, DrawCardFromWall
    //Check Hu must implement RIGHT AFTER **ALL** tile discarded

    IEnumerator GameLoop() {
        SetupTiles();
        startGame = false;
        while (readyGame == 1)
        {
            if (endofturn == true && winnerfound == -99)
            {
                endofturn = false;
                Debug.Log("TURNCOUNT: " + turncount);

                //================DRAW FROM WALL================
                //check !first round and !just chow/pong
                if (turncount != 1 && playergang == true)
                {
                    playergang = false;
                    playerponggang = false;
                    DrawCardFromBackWall();
                    if (readyGame == 0) {
                        break;
                    }
                    if (CheckForHu(ComputeHand(CurrentPlayer)))
                    {
                        winnerfound = CurrentPlayer;
                        break;
                    }
                    //check for gang and if want to
                    if (CheckForPlayerGang())
                    {
                        DisplayPlayerGangOptions();
                        responded = false;
                        Debug.Log("Enter WaitForResponse-PLAYERGANG");
                        for (int i = 0; i < 5; i++)
                        {
                            yield return new WaitForSeconds(1);
                            if (responded == true)
                            {
                                break;
                            }
                        }
                        if (responded == true)
                        {
                            Debug.Log("WaitForResponse OPTION BUTTON - RESPONDED " + btnSelected);
                            responded = false;

                            if (btnSelected != 0)
                            {
                                ExecutePlayerGang();
                            }
                        }
                        else
                        {
                            Debug.Log("WaitForResponse No response but 5 seconds up");
                        }
                        //END OF WAIT FOR PONG GANG RESPONSE
                        DestroyOptionButtons();
                        Debug.Log("WaitForResponse Completed");
                    }
                }
                else if (turncount != 1 && playerchow == false && playerponggang == false)
                {
                    DrawCardFromWall();
                    if (readyGame == 0)
                    {
                        break;
                    }
                    if (CheckForHu(ComputeHand(CurrentPlayer)))
                    {
                        winnerfound = CurrentPlayer;
                        break;
                    }
                    //check for gang and if want to
                    if (CheckForPlayerGang())
                    {
                        DisplayPlayerGangOptions();
                        responded = false;
                        Debug.Log("Enter WaitForResponse-PLAYERGANG");
                        for (int i = 0; i < 5; i++)
                        {
                            yield return new WaitForSeconds(1);
                            if (responded == true)
                            {
                                break;
                            }
                        }
                        if (responded == true)
                        {
                            Debug.Log("WaitForResponse OPTION BUTTON - RESPONDED " + btnSelected);
                            responded = false;

                            if (btnSelected != 0)
                            {
                                ExecutePlayerGang();
                            }
                        }
                        else
                        {
                            Debug.Log("WaitForResponse No response but 5 seconds up");
                        }
                        //END OF WAIT FOR PONG GANG RESPONSE
                        DestroyOptionButtons();
                        Debug.Log("WaitForResponse Completed");
                    }
                }
                else
                {
                    playerchow = false;
                    playerponggang = false;
                }
                //================END OF DRAW FROM WALL================

                //================WAIT FOR DISCARD================
                Debug.Log("ENTER WaitForDiscard");
                for (int i = 0; i < 5; i++)
                {
                    yield return new WaitForSeconds(1);
                    if (responded == true)
                    {
                        break;
                    }
                }
                if (responded == true)
                {
                    Debug.Log("RESPONDED");
                    responded = false;
                    DiscardTile(tiletodiscard);
                }
                else
                {
                    Debug.Log("No response but 5 seconds up");
                    DiscardMostRightTile();
                }
                //================END OF WAIT FOR DISCARD================
                //CHECK FOR HU for players that are waitinghand = true;
                for (int i = 1; i < 5; i++)
                {
                    if (i != CurrentPlayer)
                    {
                        if (CheckForHu(InsertDiscardTemp(i)))
                        {
                            winnerfound = i;
                            break;
                        }
                    }

                }
                if (winnerfound != -99)
                {
                    break;
                }

                //================CHECK FOR PONG================
                bool[] ponggangset = new bool[2];
                ponggangset = CheckForPongGang();
                if (ponggangset.Contains(true))
                {
                    DisplayPongGangOptions(ponggangset);

                    //WAIT FOR PONG GANG RESPONSE
                    responded = false;
                    Debug.Log("Enter WaitForResponse-PONGGANG");
                    for (int i = 0; i < 5; i++)
                    {
                        yield return new WaitForSeconds(1);
                        if (responded == true)
                        {
                            break;
                        }
                    }
                    if (responded == true)
                    {
                        Debug.Log("WaitForResponse OPTION BUTTON - RESPONDED");
                        responded = false;
                        if (btnSelected != 0)
                        {
                            ExecutePongGang();
                            DestroyOptionButtons();
                        }
                        else
                        {
                            DestroyOptionButtons();
                            ////================CHECK FOR CHOW================
                            int[][] meldsets = new int[][] {
                                new int[3],
                                new int[3],
                                new int[3]
                            };

                            meldsets = CheckForChow();
                            if (meldsets != null)
                            {
                                DisplayChowOptions(meldsets);

                                //WAIT FOR CHOW RESPONSE
                                responded = false;
                                Debug.Log("Enter WaitForResponse-CHOW");
                                for (int i = 0; i < 5; i++)
                                {
                                    yield return new WaitForSeconds(1);
                                    if (responded == true)
                                    {
                                        break;
                                    }
                                }
                                Debug.Log("EXITED WAIT FOR RESPONSE WAIT 5 SECS LOOP");
                                if (responded == true)
                                {
                                    Debug.Log("WaitForResponse OPTION BUTTON - RESPONDED");
                                    responded = false;
                                    if (btnSelected != 0)
                                    {
                                        ExecuteChow(meldsets);
                                    }
                                    DestroyOptionButtons();
                                }
                                else
                                {
                                    Debug.Log("WaitForResponse No response but 5 seconds up");
                                    DestroyOptionButtons();
                                    NextPlayer();
                                }
                                //END OF WAIT FOR CHOW RESPONSE
                                Debug.Log("WaitForResponse Completed");
                            }
                            else {
                                NextPlayer();
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("WaitForResponse No response but 5 seconds up");
                        DestroyOptionButtons();
                        ////================CHECK FOR CHOW================
                        int[][] meldsets = new int[][] {
                            new int[3],
                            new int[3],
                            new int[3]
                        };

                        meldsets = CheckForChow();
                        if (meldsets != null)
                        {
                            DisplayChowOptions(meldsets);

                            //WAIT FOR CHOW RESPONSE
                            responded = false;
                            Debug.Log("Enter WaitForResponse-CHOW");
                            for (int i = 0; i < 5; i++)
                            {
                                yield return new WaitForSeconds(1);
                                if (responded == true)
                                {
                                    break;
                                }
                            }
                            Debug.Log("EXITED WAIT FOR RESPONSE WAIT 5 SECS LOOP");
                            if (responded == true)
                            {
                                Debug.Log("WaitForResponse OPTION BUTTON - RESPONDED");
                                responded = false;
                                if (btnSelected != 0)
                                {
                                    ExecuteChow(meldsets);
                                }
                                DestroyOptionButtons();
                            }
                            else
                            {
                                Debug.Log("WaitForResponse No response but 5 seconds up");
                                DestroyOptionButtons();
                                NextPlayer();
                            }
                            //END OF WAIT FOR CHOW RESPONSE
                            Debug.Log("WaitForResponse Completed");
                        }
                        else
                        {
                            NextPlayer();
                        }
                        ////================END OF CHECK FOR CHOW================
                    }
                    //END OF WAIT FOR PONG GANG RESPONSE

                }
                //================END OF CHECK FOR PONG================
                else
                {
                    //suppose to insert check for hu after discard (hu player can be anyone with waitinghand = true, not just right side)
                    ////================CHECK FOR CHOW================
                    int[][] meldsets = new int[][] {
                            new int[3],
                            new int[3],
                            new int[3]
                    };

                    meldsets = CheckForChow();
                    if (meldsets != null)
                    {
                        DisplayChowOptions(meldsets);

                        //WAIT FOR CHOW RESPONSE
                        responded = false;
                        Debug.Log("Enter WaitForResponse-CHOW");
                        for (int i = 0; i < 5; i++)
                        {
                            yield return new WaitForSeconds(1);
                            if (responded == true)
                            {
                                break;
                            }
                        }
                        Debug.Log("EXITED WAIT FOR RESPONSE WAIT 5 SECS LOOP");
                        if (responded == true)
                        {
                            Debug.Log("WaitForResponse OPTION BUTTON - RESPONDED");
                            responded = false;
                            if (btnSelected != 0)
                            {
                                ExecuteChow(meldsets);
                            }
                            else { 
                                NextPlayer();
                            }
                            DestroyOptionButtons();
                        }
                        else
                        {
                            Debug.Log("WaitForResponse No response but 5 seconds up");
                            DestroyOptionButtons();
                            NextPlayer();
                        }
                        //END OF WAIT FOR CHOW RESPONSE
                        Debug.Log("WaitForResponse Completed");
                    }
                    else {
                        NextPlayer();
                    }
                }
                endofturn = true;
            }
        }
        GameOverMenu();

    }

    void GameOverMenu()
    {
        GameObject gameOverPanel = Instantiate(GameOverPanelPrefab, GameCanvas.transform);
        if (winnerfound == -99)
        {
            gameOverPanel.gameObject.transform.Find("WinnerText").GetComponent<Text>().text = "NO WINNER";
        }
        else {
            gameOverPanel.gameObject.transform.Find("WinnerText").GetComponent<Text>().text = "WINNER: " + winnerfound;
        }

    }

    void ChangeDealer() {
        DealerText.transform.Find("DEALER" + currentDealer).gameObject.SetActive(false);
        if (currentDealer == 4)
        {
            currentDealer = 1;
            UpdateWind();
        }
        else {
            currentDealer++;
        }
        DealerText.transform.Find("DEALER" + currentDealer).gameObject.SetActive(true);
    }

    void UpdateWind() {
        string windText;
        if (currentWind == 4)
        {
            //end of 1 pot
            currentWind = 1;
        }
        else
        {
            currentWind++;
        }
        switch (currentWind)
        {
            case 1:
                windText = "DONG";
                break;
            case 2:
                windText = "NAN";
                break;
            case 3:
                windText = "XI";
                break;
            case 4:
                windText = "BEI";
                break;
            default:
                windText = "";
                break;

        }
        CurrentWindText.GetComponent<Text>().text = "CURRENT WIND: " + windText;
    }

    public void ClearCanvas() {

        if ((winnerfound != -99) && (winnerfound != currentDealer))
        {
            ChangeDealer();
        }
        playerchow = false;
        playerponggang = false;
        pongplayer = -99;
        gangplayer = -99;
        playergang = false;
        CurrentPlayer = -1;
        readyGame = 0;
        turncount = 0;
        responded = false;
        winnerfound = -99;
        endofturn = true;
        btnSelected = -99;
        tiletodiscard = "";
        startGame = false;
        for (int i = 0; i < 4; i++)
        {
            waitinghand[i] = false;
        }

        foreach (GameObject tile in tileSetGameObject) {
            if (tile.transform.parent != TileContainer.transform) { 
                tile.transform.SetParent(TileContainer.transform, false);
                tile.GetComponent<GameTileScript>().toggleTileFace();
                tile.transform.localPosition = new Vector3(0, 0, 0);
            }
        }

        GameObject gameOverPanel = GameObject.Find("GameOverPanel(Clone)");
        if (gameOverPanel)
        {
            Destroy(gameOverPanel);
        }

        foreach (GameObject meldcontainer in GameObject.FindGameObjectsWithTag("MeldContainer")) {
            Destroy(meldcontainer);
        }
    }

    void SetupTiles()
    {
        if (newGame) {         
            tileset = GenerateTileSet();
            Shuffle(tileset);
            CreateTileSet();
            newGame = false;
            DealerText.transform.Find("DEALER2").gameObject.SetActive(false);
            DealerText.transform.Find("DEALER3").gameObject.SetActive(false);
            DealerText.transform.Find("DEALER4").gameObject.SetActive(false);
            DealerText.transform.Find("DEALER1").gameObject.SetActive(true);
            CurrentWindText.GetComponent<Text>().text = "CURRENT WIND: DONG";   
        }
        else
        {
            Shuffle(tileset);
            RematchTiles();
        }
        Debug.Log("TILESET: " + string.Join(", ", tileset));
        DealTiles();
        Debug.Log("TILESET COUNT: " + tileset.Count);
    }

    static List<string> GenerateTileSet()
    {

        List<string> newTileSet = new List<string>();

        for (int i = 0; i < 34; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (i < 10)
                {
                    newTileSet.Add("0" + i.ToString());
                }
                else
                {
                    newTileSet.Add(i.ToString());
                }
            }
        }

        return newTileSet;
    }

    //shuffle the deck created to form a deck of card w random sequeunce. 
    void Shuffle<T>(List<T> list)
    {

        System.Random random = new System.Random();

        int n = list.Count;

        while (n > 1)
        {

            int k = random.Next(n);
            n--;

            //swap positions of index k and n, k at random
            T temp = list[k];
            list[k] = list[n];
            list[n] = temp;

        }
    }

    void RematchTiles() {
        int tileindex = 0;
        foreach (string tile in tileset)
        {
            for (int i = 0; i > -1; i++)
            {
                if (tile == TileContainer.transform.GetChild(i).gameObject.name) {
                    TileContainer.transform.GetChild(i).gameObject.transform.SetSiblingIndex(tileindex);
                    tileindex++;
                    break;
                }
            }
        }
    }

    //spawn deck cards
    void CreateTileSet()
    {
        foreach (string tile in tileset)
        {
            for (int i = 0; i > -1; i++)
            {
                if (tile == TileFaces[i].name)
                {
                    GameObject newTile = Instantiate(TileFaces[i], TileContainer.transform);
                    tileSetGameObject.Add(newTile);
                    newTile.name = tile;
                    break;
                }
            }
        }
    }

    void DealTiles()
    {
        int playernow = currentDealer;

        for (int i = 0; i < 53; i++)
        {
            Debug.Log("DEAL" + i);
            switch (playernow)
            {
                case 1:
                    TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player1Container.transform, false);
                    Debug.Log(Player1Container.transform.GetChild(Player1Container.transform.childCount - 1).gameObject.name + " to player 1 container");
                    Player1Container.transform.GetChild(Player1Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    Debug.Log(Player1Container.transform.GetChild(Player1Container.transform.childCount - 1).parent.gameObject.name);
                    break;
                case 2:
                    TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player2Container.transform, false);
                    Debug.Log(Player2Container.transform.GetChild(Player2Container.transform.childCount - 1).gameObject.name + " to player 2 container");
                    Player2Container.transform.GetChild(Player2Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    break;
                case 3:
                    TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player3Container.transform, false);
                    Debug.Log(Player3Container.transform.GetChild(Player3Container.transform.childCount - 1).gameObject.name + " to player 3 container");
                    Player3Container.transform.GetChild(Player3Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    break;
                case 4:
                    TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player4Container.transform, false);
                    Debug.Log(Player4Container.transform.GetChild(Player4Container.transform.childCount - 1).gameObject.name + " to player 4 container");
                    Player4Container.transform.GetChild(Player4Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    break;
                default:
                    Debug.Log("Invalid player number");
                    break;
            }
            switch (playernow)
            {
                case 1:
                    playernow = 2;
                    break;
                case 2:
                    playernow = 3;
                    break;
                case 3:
                    playernow = 4;
                    break;
                case 4:
                    playernow = 1;
                    break;
                default:
                    Debug.Log("Invalid player number");
                    break;
            }
        }

        CurrentPlayer = currentDealer;
        Debug.Log("REARRANGE FOR P1");
        RearrangeCards(Player1Container);
        
        Debug.Log("REARRANGE FOR P2");
        RearrangeCards(Player2Container);

        Debug.Log("REARRANGE FOR P3");
        RearrangeCards(Player3Container);

        Debug.Log("REARRANGE FOR P4");
        RearrangeCards(Player4Container);
        readyGame = 1;
        turncount = 1;
    }

    public string GetParent(Component card)
    {
        return card.gameObject.transform.parent.name;
    }

    public void DiscardTile(string tiletodiscard)
    {
        GameObject playerContainer;
        switch (CurrentPlayer)
        {
            case 1:
                playerContainer = Player1Container;
                break;
            case 2:
                playerContainer = Player2Container;
                break;
            case 3:
                playerContainer = Player3Container;
                break;
            case 4:
                playerContainer = Player4Container;
                break;
            default:
                Debug.Log("Invalid player number");
                playerContainer = null;
                break;
        }
        playerContainer.gameObject.transform.Find(tiletodiscard).gameObject.transform.SetParent(DiscardContainer.transform);
    }

    int GetDiscardTile()
    {
        string discardTileName = DiscardContainer.transform.GetChild(DiscardContainer.transform.childCount - 1).gameObject.name;
        int discardTile;
        Debug.Log("DISCARD TILE NAME FROM GETDISCARDTILE:  " + discardTileName);
        int.TryParse(discardTileName, out discardTile);
        Debug.Log("DISCARD TILE INT FROM GETDISCARDTILE:  " + discardTile);
        return discardTile;
    }

    int[] InsertDiscardTemp(int player) {
        int[] tilesetcount = new int[34];
        string discardTileName = DiscardContainer.transform.GetChild(DiscardContainer.transform.childCount - 1).gameObject.name;
        int discardTile;
        int.TryParse(discardTileName, out discardTile);
        tilesetcount = ComputeHand(player);
        tilesetcount[discardTile]++;
        Debug.Log("AFTER INSERT DISCARD TEMP: " + string.Join(", ", tilesetcount));
        return tilesetcount;
    }

    int[] ComputeHand(int player)
    {
        GameObject RightSidePlayerContainer;
        int[] tilesetcount = new int[34];
        switch (player)
        {
            case 1:
                RightSidePlayerContainer = Player1Container;
                break;
            case 2:
                RightSidePlayerContainer = Player2Container;
                break;
            case 3:
                RightSidePlayerContainer = Player3Container;
                break;
            case 4:
                RightSidePlayerContainer = Player4Container;
                break;
            default:
                RightSidePlayerContainer = null;
                Debug.Log("Invalid player number");
                break;
        }
        for (int i = 0; i < RightSidePlayerContainer.transform.childCount; i++)
        {
            string tileindexstring = RightSidePlayerContainer.transform.GetChild(i).gameObject.name;
            int tileindex;
            int.TryParse(tileindexstring, out tileindex);
            tilesetcount[tileindex]++;
        }
        Debug.Log("COMPUTE HAND: " + string.Join(", ", tilesetcount));

        return tilesetcount;
    }

    public void NextPlayer()
    {
        switch (CurrentPlayer)
        {
            case 1:
                CurrentPlayer = 2;
                break;
            case 2:
                CurrentPlayer = 3;
                break;
            case 3:
                CurrentPlayer = 4;
                break;
            case 4:
                CurrentPlayer = 1;
                break;
            default:
                Debug.Log("Invalid player number");
                break;
        }
        Debug.Log("PLAYER: " + CurrentPlayer);
        turncount++;
    }

    int GetNextPlayer()
    {
        int temp;
        switch (CurrentPlayer)
        {
            case 1:
                temp = 2;
                Debug.Log("TempNextPlayer: " + temp);
                return temp;
            case 2:
                temp = 3;
                Debug.Log("TempNextPlayer: " + temp);
                return temp;
            case 3:
                temp = 4;
                Debug.Log("TempNextPlayer: " + temp);
                return temp;
            case 4:
                temp = 1;
                Debug.Log("TempNextPlayer: " + temp);
                return temp;
            default:
                Debug.Log("Invalid player number");
                return temp = -99;
        }
    }

    public void RearrangeCards(GameObject CardContainer)
    {
        List<GameObject> tiles = new List<GameObject>();

        for (int i = 0; i < CardContainer.transform.childCount; i++)
        {
            tiles.Add(CardContainer.transform.GetChild(i).gameObject);
            //Debug.Log(CardContainer.transform.GetChild(i).gameObject.name);
        }

        tiles = tiles.OrderBy(tile => tile.name).ToList();
        int index = 0;
        foreach (GameObject tile in tiles)
        {
            tile.transform.SetSiblingIndex(index);
            //Debug.Log(tile.name);
            index++;
        }
    }

    void DiscardMostRightTile()
    {
        switch (CurrentPlayer)
        {
            case 1:
                Player1Container.transform.GetChild(Player1Container.transform.childCount - 1).gameObject.transform.SetParent(DiscardContainer.transform);
                break;
            case 2:
                Player2Container.transform.GetChild(Player2Container.transform.childCount - 1).gameObject.transform.SetParent(DiscardContainer.transform);
                break;
            case 3:
                Player3Container.transform.GetChild(Player3Container.transform.childCount - 1).gameObject.transform.SetParent(DiscardContainer.transform);
                break;
            case 4:
                Player4Container.transform.GetChild(Player4Container.transform.childCount - 1).gameObject.transform.SetParent(DiscardContainer.transform);
                break;
            default:
                Debug.Log("Invalid player number");
                break;
        }
    }

    void DrawCardFromWall()
    {
        Debug.Log("TILE CONTAINER CHILD COUNT from DrawCardFromWall() is" + TileContainer.transform.childCount);
        if (TileContainer.transform.childCount < 8)
        {
            readyGame = 0;
        }
        else
        {
            Debug.Log("Draw From Wall");
            switch (CurrentPlayer)
            {
                case 1:
                    TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player1Container.transform, false);
                    freshdrawtile = Player1Container.transform.GetChild(Player1Container.transform.childCount - 1).gameObject.name;
                    Player1Container.transform.GetChild(Player1Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    RearrangeCards(Player1Container);
                    break;
                case 2:
                    TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player2Container.transform, false);
                    freshdrawtile = Player2Container.transform.GetChild(Player2Container.transform.childCount - 1).gameObject.name;
                    Player2Container.transform.GetChild(Player2Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    RearrangeCards(Player2Container);
                    break;
                case 3:
                    TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player3Container.transform, false);
                    freshdrawtile = Player3Container.transform.GetChild(Player3Container.transform.childCount - 1).gameObject.name;
                    Player3Container.transform.GetChild(Player3Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    RearrangeCards(Player3Container);
                    break;
                case 4:
                    TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player4Container.transform, false);
                    freshdrawtile = Player4Container.transform.GetChild(Player4Container.transform.childCount - 1).gameObject.name;
                    Player4Container.transform.GetChild(Player4Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    RearrangeCards(Player4Container);
                    break;
                default:
                    Debug.Log("Invalid player number");
                    break;
            }
        }

    }

    void DrawCardFromBackWall()
    {
        if (TileContainer.transform.childCount < 8)
        {
            readyGame = 0;
        }
        else {        
            Debug.Log("Draw From Wall");
            switch (CurrentPlayer)
            {
                case 1:
                    TileContainer.transform.GetChild(TileContainer.transform.childCount - 1).gameObject.transform.SetParent(Player1Container.transform, false);
                    freshdrawtile = Player1Container.transform.GetChild(Player1Container.transform.childCount - 1).gameObject.name;
                    Player1Container.transform.GetChild(Player1Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    RearrangeCards(Player1Container);
                    break;
                case 2:
                    TileContainer.transform.GetChild(TileContainer.transform.childCount - 1).gameObject.transform.SetParent(Player2Container.transform, false);
                    freshdrawtile = Player2Container.transform.GetChild(Player2Container.transform.childCount - 1).gameObject.name;
                    Player2Container.transform.GetChild(Player2Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    RearrangeCards(Player2Container);
                    break;
                case 3:
                    TileContainer.transform.GetChild(TileContainer.transform.childCount - 1).gameObject.transform.SetParent(Player3Container.transform, false);
                    freshdrawtile = Player3Container.transform.GetChild(Player3Container.transform.childCount - 1).gameObject.name;
                    Player3Container.transform.GetChild(Player3Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    RearrangeCards(Player3Container);
                    break;
                case 4:
                    TileContainer.transform.GetChild(TileContainer.transform.childCount - 1).gameObject.transform.SetParent(Player4Container.transform, false);
                    freshdrawtile = Player4Container.transform.GetChild(Player4Container.transform.childCount - 1).gameObject.name;
                    Player4Container.transform.GetChild(Player4Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    RearrangeCards(Player4Container);
                    break;
                default:
                    Debug.Log("Invalid player number");
                    break;
            } 
        }


    }

    void DisplayChowOptions(int[][] meldsets)
    {
        GameObject overlayPrefab = Instantiate(OverlayPanelPrefab, GameCanvas.transform);
        overlayPrefab.transform.Find("PlayerLabel").GetComponent<Text>().text = "PLAYER " + GetNextPlayer().ToString();
        GameObject nothanksButton = Instantiate(OptionsButtonPrefab, overlayPrefab.transform);
        nothanksButton.name = "OptionButton" + "0";
        nothanksButton.GetComponentInChildren<Text>().text = "No thanks";

        for (int i = 0; i < 3; i++)
        {
            if (meldsets[i] == null) { break; }

            Debug.Log(meldsets[i][0] + "-" + meldsets[i][1] + "-" + meldsets[i][2]);
        }
        //spawn option buttons
        for (int i = 0; i < 3; i++)
        {
            if (meldsets[i] == null) { break; }
            else
            {
                GameObject optionButton = Instantiate(OptionsButtonPrefab, overlayPrefab.transform);
                int temp = i + 1;
                optionButton.name = "OptionButton" + temp.ToString();
                optionButton.GetComponentInChildren<Text>().text = meldsets[i][0] + "-" + meldsets[i][1] + "-" + meldsets[i][2];
            }
        }
    }

    void DisplayPongGangOptions(bool[] ponggangset) {
        GameObject overlayPrefab = Instantiate(OverlayPanelPrefab, GameCanvas.transform);
        overlayPrefab.transform.Find("PlayerLabel").GetComponent<Text>().text = "PLAYER " + pongplayer.ToString();
        GameObject nothanksButton = Instantiate(OptionsButtonPrefab, overlayPrefab.transform);
        nothanksButton.name = "OptionButton" + "0";
        nothanksButton.GetComponentInChildren<Text>().text = "No thanks";

        if (ponggangset[0] == true) {
            GameObject optionButton = Instantiate(OptionsButtonPrefab, overlayPrefab.transform);
            optionButton.name = "OptionButton" + "1";
            optionButton.GetComponentInChildren<Text>().text = "PONG";
        }
        if (ponggangset[1] == true)
        {
            GameObject optionButton = Instantiate(OptionsButtonPrefab, overlayPrefab.transform);
            optionButton.name = "OptionButton" + "2";
            optionButton.GetComponentInChildren<Text>().text = "GANG";
        }
    }

    void DisplayPlayerGangOptions() {
        GameObject overlayPrefab = Instantiate(OverlayPanelPrefab, GameCanvas.transform);
        overlayPrefab.transform.Find("PlayerLabel").GetComponent<Text>().text = "PLAYER " + CurrentPlayer.ToString();
        GameObject nothanksButton = Instantiate(OptionsButtonPrefab, overlayPrefab.transform);
        nothanksButton.name = "OptionButton" + "0";
        nothanksButton.GetComponentInChildren<Text>().text = "No thanks";

        GameObject optionButton = Instantiate(OptionsButtonPrefab, overlayPrefab.transform);
        optionButton.name = "OptionButton" + "1";
        optionButton.GetComponentInChildren<Text>().text = "GANG";
    }

    void DestroyOptionButtons()
    {
        //for (int i = 0; i < 3; i++)
        for (int i = 0; i < 4; i++)
        {
            GameObject optionbutton = GameObject.Find("OptionButton" + i);
            //if the button exist then destroy it
            if (optionbutton)
            {
                Destroy(optionbutton);
                Debug.Log(optionbutton.name + "has been destroyed.");
            }
        }

        GameObject overlaypanel = GameObject.Find("OverlayPanel(Clone)");
        //if the button exist then destroy it
        if (overlaypanel)
        {
            Destroy(overlaypanel);
            Debug.Log(overlaypanel + "has been destroyed.");

        }
    }

    int[][] CheckForChow()
    {
        int discardTile = GetDiscardTile();
        int[] playerHand = new int[34];
        playerHand = ComputeHand(GetNextPlayer());
        int meldsetindex = 0;
        //int[][] possiblemeldsets = new int[][] {
        //     new int[3],
        //     new int[3],
        //     new int[3]
        // };
        int[][] possiblemeldsets = new int[3][];
        Debug.Log("CHECK FOR CHOW ENTERED");

        if (discardTile < 27)
        {
            //add discard tile to hand temporarily
            playerHand[discardTile]++;

            int countShift;
            bool alrChecked;
            int suit = (int)Mathf.Floor(discardTile / 9);

            Debug.Log("Discard Tile: " + discardTile);

            Debug.Log("Suit of Discard: " + (suit));

            if (discardTile % 9 == 0) //This is only for shift pattern 1,2,3. Ignore i value
            {
                //for (int i = 0 + 9 * suit; i < 1 + 9 * suit; i++)
                //{
                    int i = 0 + 9 * suit;
                    countShift = 0;
                    alrChecked = false;
                    for (int j = i; j < i + 3; j++)
                    {
                        if (playerHand[j] > 0)
                        {
                            countShift++;
                        }
                        if (countShift == 3 && alrChecked == false)
                        {
                            //activate chow button
                            alrChecked = true;
                            Debug.Log("CAN CHOW: " + i + " - " + (i + 1) + " - " + (i + 2));
                            possiblemeldsets[meldsetindex] = new int[] { i, i + 1, i + 2 };
                            meldsetindex++;
                        }
                    }
                //}
            }
            else if (discardTile % 9 == 1)
            {
                for (int i = 0 + 9 * suit; i < 2 + 9 * suit; i++)
                {
                    countShift = 0;
                    alrChecked = false;
                    for (int j = i; j < i + 3; j++)
                    {
                        if (playerHand[j] > 0)
                        {
                            countShift++;
                        }
                        if (countShift == 3 && alrChecked == false)
                        {
                            //activate chow button
                            alrChecked = true;
                            Debug.Log("CAN CHOW: " + i + " - " + (i + 1) + " - " + (i + 2));
                            possiblemeldsets[meldsetindex] = new int[] { i, i + 1, i + 2 };
                            meldsetindex++;
                        }
                    }
                }
            }
            else if (discardTile % 9 == 7)
            {
                for (int i = 5 + 9 * suit; i < 7 + 9 * suit; i++)
                {
                    countShift = 0;
                    alrChecked = false;
                    for (int j = i; j < i + 3; j++)
                    {
                        if (playerHand[j] > 0)
                        {
                            countShift++;
                        }
                        if (countShift == 3 && alrChecked == false)
                        {
                            //activate chow button
                            alrChecked = true;
                            Debug.Log("CAN CHOW: " + i + " - " + (i + 1) + " - " + (i + 2));
                            possiblemeldsets[meldsetindex] = new int[] { i, i + 1, i + 2 };
                            meldsetindex++;
                        }
                    }
                }
            }
            else if (discardTile % 9 == 8)
            {
                //for (int i = 6 + 9 * suit; i < 7 + 9 * suit; i++)
                //{
                int i = 6 + 9 * suit;
                countShift = 0;
                alrChecked = false;
                for (int j = i; j < i + 3; j++)
                {
                    if (playerHand[j] > 0)
                    {
                        countShift++;
                    }
                    if (countShift == 3 && alrChecked == false)
                    {
                        //activate chow button
                        alrChecked = true;
                        Debug.Log("CAN CHOW: " + i + " - " + (i + 1) + " - " + (i + 2));
                        possiblemeldsets[meldsetindex] = new int[] { i, i + 1, i + 2 };
                        meldsetindex++;
                    }
                }
                //}
            }
            else if (discardTile % 9 > 1 && discardTile % 9 < 7)
            {
                for (int i = discardTile - 2; i < discardTile + 1; i++) //only need to check for wan tong suo
                {
                    countShift = 0;
                    alrChecked = false;
                    for (int j = i; j < i + 3; j++)
                    {
                        if (playerHand[j] > 0)
                        {
                            countShift++;
                        }
                        if (countShift == 3 && alrChecked == false)
                        {
                            //activate chow button
                            alrChecked = true;
                            Debug.Log("CAN CHOW: " + i + " - " + (i + 1) + " - " + (i + 2));
                            possiblemeldsets[meldsetindex] = new int[] { i, i + 1, i + 2 };
                            meldsetindex++;
                        }
                    }
                }
            }

            if (possiblemeldsets[0] == null)
            {

                Debug.Log("NO MELD SETS");
                return null;

            }

            //debugging
            for (int i = 0; i < 3; i++)
            {
                if (possiblemeldsets[i] != null)
                {
                    Debug.Log(" FINAL Meld Set " + i + " :" + possiblemeldsets[i][0] + "-" + possiblemeldsets[i][1] + "-" + possiblemeldsets[i][2]);
                }
            }
            //end of debugging            
            return possiblemeldsets;

        }

        return null;

    }

    bool CheckForPlayerGang()
    {
        Debug.Log("CHECKING FOR PLAYER: " + CurrentPlayer);
        int[] playerHand = new int[34];
        playerHand = ComputeHand(CurrentPlayer);
        //Debug.Log("PLAYER" + CurrentPlayer + " HAND" + string.Join(",", playerHand));

        for (int i = 0; i < 34; i++) {
            if (playerHand[i] == 4)
            {
                gangplayer = CurrentPlayer;
                isInExposed = false;
                return true;
            }
        }

        GameObject PlayerExposedContainer;
        switch (CurrentPlayer)
        {
            case 1:
                PlayerExposedContainer = Player1ExposedContainer;
                break;
            case 2:
                PlayerExposedContainer = Player2ExposedContainer;
                break;
            case 3:
                PlayerExposedContainer = Player3ExposedContainer;
                break;
            case 4:
                PlayerExposedContainer = Player4ExposedContainer;
                break;
            default:
                PlayerExposedContainer = null;
                Debug.Log("Invalid player number");
                break;
        }
        Debug.Log("MELD SETS IN EXPOSED: " + PlayerExposedContainer.transform.childCount);

        if (PlayerExposedContainer.transform.childCount != 0)
        {
            foreach (Transform setcontainer in PlayerExposedContainer.transform)
            {
                if (setcontainer.GetChild(0).gameObject.name == freshdrawtile)
                {
                    if (setcontainer.GetChild(1).gameObject.name == freshdrawtile)
                    {
                        gangplayer = CurrentPlayer;
                        isInExposed = true;
                        return true;

                    }

                }
            }
        }

        return false;
    }

    bool[] CheckForPongGang() {
        bool[] ponggang = new bool[2];
        ponggang[0] = false;
        ponggang[1] = false;
        for (int i = 1; i < 5; i++)
        {
            if (CurrentPlayer != i)
            {
                Debug.Log("CHECKING FOR PLAYER: " + i);
                int discardIndex = GetDiscardTile();
                Debug.Log("DISCARD TILE CHECKING: " + discardIndex);
                int[] playerHand = new int[34];
                playerHand = ComputeHand(i);
                //Debug.Log("PLAYER" + i + " HAND" + playerHand);
                Debug.Log("PLAYER" + i + " HAND" + string.Join(",", playerHand));

                if (playerHand[discardIndex] > 1)
                {
                    ponggang[0] = true;
                    pongplayer = i;
                    if (playerHand[discardIndex] == 3)
                    {
                        ponggang[1] = true;
                        gangplayer = i;
                    }
                    Debug.Log("PONG: " + ponggang[0] + " GANG: " + ponggang[1]);
                    i = 5;
                }
                Debug.Log("PONG: " + ponggang[0] + " GANG: " + ponggang[1]);
            }
        }
        return ponggang;
    }

    public int[] checkTileSet(int[] handSet)
    {
        int[] sCount = shiftFirstCheck(handSet);
        int[] mCount = meldFirstCheck(handSet);
        int[] sbCount = shiftFirstCheckBackwards(handSet);
        int[] mbCount = meldFirstCheckBackwards(handSet);
        Debug.Log("meldFirst Count: " + mCount[0] + " meldFirst Eye: " + mCount[1]);
        Debug.Log("shiftFirst Count: " + sCount[0] + " shiftFirst Eye: " + sCount[1]);
        Debug.Log("shiftbFirst Count: " + sbCount[0] + " shiftbFirst Eye: " + sbCount[1]);
        Debug.Log("meldbFirst Count: " + mbCount[0] + " meldbFirst Eye: " + mbCount[1]);

        int[][] allCount = new int[4][];
        allCount[0] = sCount;
        allCount[1] = mCount;
        allCount[2] = sbCount;
        allCount[3] = mbCount;

        //check for eye
        for (int i = 0; i < 4; i++)
        {
            Debug.Log("EYE COUNT FOR Count Set " + i + "in checkTileSet is" + allCount[i][1]);
            if (allCount[i][1] != 1)
            {
                allCount[i] = new int[] { 0, 0 };
                Debug.Log("Count Set " + i + "in checkTileSet is OUT");
            }
        }

        //check for set count
        int[] highestCount = allCount[0];
        for (int i = 1; i < 4; i++)
        {
            if (allCount[i][0] > highestCount[0])
            {
                highestCount = allCount[i];
            }
        }
        return highestCount;
    }

    int[] meldFirstCheck(int[] playerHand)
    {
        int[] tempHand = new int[34];
        Array.Copy(playerHand, tempHand, 34);
        int[] totalCount = new int[2] { 0, 0 };

        for (int i = 0; i < 34; i++) //Check for pong
        {
            if (tempHand[i] == 3)
            {
                tempHand[i] -= 3;
                totalCount[0] += 1;
            }
        }
        for (int x = 0; x < 3; x++) //To keep track of the 3 suits and check for shifts
        {
            for (int i = 0 + 9 * x; i < 7 + 9 * x; i++) //only need to check for wan tong suo
            {
                int countShift = 0;
                for (int j = i; j < i + 3; j++)
                {
                    if (tempHand[j] > 0)
                    {
                        countShift++;
                        if (countShift == 3)
                        {
                            for (int k = 0; k < 3; k++)
                            {
                                tempHand[i + k] -= 1;
                            }
                            i = -1;
                            totalCount[0] += 1;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        for (int i = 0; i < 34; i++) //Check for eye
        {
            if (tempHand[i] == 2)
            {
                tempHand[i] -= 2;
                totalCount[1] += 1;
            }
        }

        return totalCount;
    }

    int[] meldFirstCheckBackwards(int[] playerHand)
    {
        int[] tempHand = new int[34];
        Array.Copy(playerHand, tempHand, 34);
        int[] totalCount = new int[2] { 0, 0 };

        for (int i = 0; i < 34; i++) //Check for pong
        {
            if (tempHand[i] == 3)
            {
                tempHand[i] -= 3;
                totalCount[0] += 1;
            }
        }
        for (int x = 0; x < 3; x++) //To keep track of the 3 suits and check for shifts
        {
            for (int i = 8 + 9 * x; i > 1 + (9 * x); i--) //check chow - only need to check for wan tong suo
            {
                int countShift = 0;
                for (int j = i; j > i - 3; j--)
                {
                    if (tempHand[j] > 0)
                    {
                        countShift++;
                        if (countShift == 3)
                        {
                            for (int k = 2; k > -1; k--)
                            {
                                tempHand[i - k] -= 1;
                            }
                            i += 1;
                            totalCount[0] += 1;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        for (int i = 0; i < 34; i++) //Check for eye
        {
            if (tempHand[i] == 2)
            {
                tempHand[i] -= 2;
                totalCount[1] += 1;
            }
        }

        return totalCount;
    }

    int[] shiftFirstCheck(int[] playerHand)
    {
        int[] tempHand = new int[34];
        Array.Copy(playerHand, tempHand, 34);
        //temp hand shall be remaining tiles in the player's container (not exposed)
        int[] totalCount = new int[2] { 0, 0 };

        for (int x = 0; x < 3; x++) //To keep track of the 3 suits and check for shifts
        {
            for (int i = 0 + 9 * x; i < 7 + 9 * x; i++) //only need to check for wan tong suo
            {
                int countShift = 0;
                for (int j = i; j < i + 3; j++)
                {
                    if (tempHand[j] > 0)
                    {
                        countShift++;
                        if (countShift == 3)
                        {
                            for (int k = 0; k < 3; k++)
                            {
                                tempHand[i + k] -= 1;
                            }
                            i = -1;
                            totalCount[0] += 1;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        for (int i = 0; i < 34; i++) //Check for pong
        {
            if (tempHand[i] == 3)
            {
                tempHand[i] -= 3;
                totalCount[0] += 1;
            }
        }
        for (int i = 0; i < 34; i++) //Check for eye
        {
            if (tempHand[i] == 2)
            {
                tempHand[i] -= 2;
                totalCount[1] += 1;
            }
        }
        return totalCount;
    }

    int[] shiftFirstCheckBackwards(int[] playerHand)
    {
        int[] tempHand = new int[34];
        Array.Copy(playerHand, tempHand, 34);
        //temp hand shall be remaining tiles in the player's container (not exposed)
        int[] totalCount = new int[2] { 0, 0 };

        for (int x = 0; x < 3; x++) //To keep track of the 3 suits and check for shifts
        {
            for (int i = 8 + 9 * x; i > 1 + (9 * x); i--) //check chow - only need to check for wan tong suo
            {
                int countShift = 0;
                for (int j = i; j > i - 3; j--)
                {
                    if (tempHand[j] > 0)
                    {
                        countShift++;
                        if (countShift == 3)
                        {
                            for (int k = 2; k > -1; k--)
                            {
                                tempHand[i - k] -= 1;
                            }
                            i += 1;
                            totalCount[0] += 1;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        for (int i = 0; i < 34; i++) //Check for pong
        {
            if (tempHand[i] == 3)
            {
                tempHand[i] -= 3;
                totalCount[0] += 1;
            }
        }
        for (int i = 0; i < 34; i++) //Check for eye
        {
            if (tempHand[i] == 2)
            {
                tempHand[i] -= 2;
                totalCount[1] += 1;
            }
        }
        return totalCount;
    }

    bool CheckForHu(int[] playerHand)
    {
        int[] tempHand = new int[34];
        Array.Copy(playerHand, tempHand, 34);
        Debug.Log("CheckForHu tempHand = " + string.Join(", ", tempHand));
        int[] tempCount = checkTileSet(tempHand);
        int concealedtiles = GetConcealedTiles(tempHand);
        Debug.Log("concealed tiles count = " + concealedtiles);
        int requiredmelds = 0;
        switch (concealedtiles)
        {
            case 14:
                requiredmelds = 4;
                break;
            case 11:
                requiredmelds = 3;
                break;
            case 8:
                requiredmelds = 2;
                break;
            case 5:
                requiredmelds = 1;
                break;
            case 2:
                requiredmelds = 0;
                break;
            default:
                requiredmelds = 99;
                break;
        }
        Debug.Log("CHECK FOR HU ENTERED, CURRENT REQUIRED MELDS: " + requiredmelds);
        Debug.Log("CURRENT EYE COUNT: " + tempCount[1]);
        if (tempCount[0] == requiredmelds && tempCount[1] == 1)
        {
            Debug.Log("CAN HU");
            return true;
        }
        else
        {
            Debug.Log("CANNOT HU");
            return false;
        }
    }

    int GetConcealedTiles(int[] playerHand)
    {
        Debug.Log("GetConcealedTiles received: " + string.Join(", ", playerHand));
        int totaltiles = 0;
        for (int i = 0; i < 34; i++)
        {
            totaltiles = totaltiles + playerHand[i];
        }
        Debug.Log("TOTAL CONCEALED TILES: " + totaltiles);
        return totaltiles;
    }

    void AssignNextPlayer() {
        CurrentPlayer = pongplayer;
        Debug.Log("PLAYER: " + CurrentPlayer);
        turncount++;
    }

    void ExecuteChow(int[][] meldsets)
    {
        int discardIndex = GetDiscardTile();
        int[] chosenmeld = new int[3];
        int[] notdiscardtile = new int[2];

        Debug.Log("EXECUTE CHOW DISCARD INDEX: " + discardIndex);
        //identify which tile isnt discard tile
        switch (btnSelected)
        {
            case 1:
                chosenmeld = meldsets[0];
                playerchow = true;
                break;
            case 2:
                chosenmeld = meldsets[1];
                playerchow = true;
                break;
            case 3:
                chosenmeld = meldsets[2];
                playerchow = true;
                break;
            default:
                Debug.Log("Invalid Execute Chow");
                break;
        }

        NextPlayer();
        GameObject PlayerContainer;
        GameObject PlayerExposedContainer;
        switch (CurrentPlayer)
        {
            case 1:
                PlayerContainer = Player1Container;
                PlayerExposedContainer = Player1ExposedContainer;
                break;
            case 2:
                PlayerContainer = Player2Container;
                PlayerExposedContainer = Player2ExposedContainer;
                break;
            case 3:
                PlayerContainer = Player3Container;
                PlayerExposedContainer = Player3ExposedContainer;
                break;
            case 4:
                PlayerContainer = Player4Container;
                PlayerExposedContainer = Player4ExposedContainer;
                break;
            default:
                PlayerContainer = null;
                PlayerExposedContainer = null;
                Debug.Log("Invalid player number");
                break;
        }

        GameObject meldsetcontainer = Instantiate(MeldSetContainerPrefab, PlayerExposedContainer.transform);

        for (int i = 0; i < 3; i++)
        {
            string tilename;
            if (chosenmeld[i].ToString().Length == 1) { tilename = "0" + chosenmeld[i].ToString(); }
            else { tilename = chosenmeld[i].ToString(); }

            Debug.Log("Tile Name is: " + tilename);
            if (chosenmeld[i] != discardIndex)
            {

                PlayerContainer.transform.Find(tilename).gameObject.transform.SetParent(PlayerExposedContainer.transform.GetChild(PlayerExposedContainer.transform.childCount - 1).transform);
            }
            else
            {
                Debug.Log("DISCARDED TILE NAME: " + DiscardContainer.transform.GetChild(DiscardContainer.transform.childCount - 1).gameObject.name);
                DiscardContainer.transform.GetChild(DiscardContainer.transform.childCount - 1).gameObject.transform.SetParent(PlayerExposedContainer.transform.GetChild(PlayerExposedContainer.transform.childCount - 1).transform);
            }
        }

        playerchow = true;
        btnSelected = -99;
        Debug.Log("CHOSEN MELD: " + chosenmeld[0] + "-" + chosenmeld[1] + "-" + chosenmeld[2]);
    }

    void ExecutePlayerGang() {
        int gangtile = -99;
        string gangtilename;
        GameObject PlayerContainer;
        GameObject PlayerExposedContainer;
        switch (CurrentPlayer)
        {
            case 1:
                PlayerContainer = Player1Container;
                PlayerExposedContainer = Player1ExposedContainer;
                break;
            case 2:
                PlayerContainer = Player2Container;
                PlayerExposedContainer = Player2ExposedContainer;
                break;
            case 3:
                PlayerContainer = Player3Container;
                PlayerExposedContainer = Player3ExposedContainer;
                break;
            case 4:
                PlayerContainer = Player4Container;
                PlayerExposedContainer = Player4ExposedContainer;
                break;
            default:
                PlayerContainer = null;
                PlayerExposedContainer = null;
                Debug.Log("Invalid player number");
                break;
        }

        if (isInExposed)
        {
            isInExposed = false;
            foreach (Transform setcontainer in PlayerExposedContainer.transform)
            {
                if (setcontainer.GetChild(0).gameObject.name == freshdrawtile){
                    PlayerContainer.transform.Find(freshdrawtile).gameObject.transform.SetParent(setcontainer);
                }
            }

        }
        else { 
            int[] playerHand = new int[34];
            playerHand = ComputeHand(CurrentPlayer);
            GameObject meldsetcontainer = Instantiate(MeldSetContainerPrefab, PlayerExposedContainer.transform);
            for (int i = 0; i < 34; i++)
            {
                if (playerHand[i] == 4)
                {
                    gangtile = i;
                    break;
                }
            }

            Debug.Log("Gang Tile Converting to string: " + gangtile.ToString());
            if (gangtile.ToString().Length == 1) { gangtilename = "0" + gangtile.ToString(); }
            else { gangtilename = gangtile.ToString(); }
            Debug.Log("Tile Name is: " + gangtilename);

            for (int i = 0; i < 4; i++) {
                PlayerContainer.transform.Find(gangtilename).gameObject.transform.SetParent(PlayerExposedContainer.transform.GetChild(PlayerExposedContainer.transform.childCount - 1).transform);
            }
        }
        DrawCardFromBackWall();
    }

    void ExecutePongGang() {
        int discardIndex = GetDiscardTile();
        int times;
        AssignNextPlayer();
        GameObject PlayerContainer;
        GameObject PlayerExposedContainer;
        switch (CurrentPlayer)
        {
            case 1:
                PlayerContainer = Player1Container;
                PlayerExposedContainer = Player1ExposedContainer;
                break;
            case 2:
                PlayerContainer = Player2Container;
                PlayerExposedContainer = Player2ExposedContainer;
                break;
            case 3:
                PlayerContainer = Player3Container;
                PlayerExposedContainer = Player3ExposedContainer;
                break;
            case 4:
                PlayerContainer = Player4Container;
                PlayerExposedContainer = Player4ExposedContainer;
                break;
            default:
                PlayerContainer = null;
                PlayerExposedContainer = null;
                Debug.Log("Invalid player number");
                break;
        }
        GameObject meldsetcontainer = Instantiate(MeldSetContainerPrefab, PlayerExposedContainer.transform);

        switch (btnSelected)
        {
            case 1:
                times = 2;
                playerponggang = true;
                break;
            case 2:
                times = 3;
                playerponggang = true;
                playergang = true;
                break;
            default:
                times = 0;
                Debug.Log("Invalid Execute PongGang");
                break;
        }

        if (times == 0) { return; } 
        Debug.Log("GOING TO SHIFT TILES FOR PLAYER " + CurrentPlayer + " AND TAKE TILE " + discardIndex);
        DiscardContainer.transform.GetChild(DiscardContainer.transform.childCount - 1).gameObject.transform.SetParent(PlayerExposedContainer.transform.GetChild(PlayerExposedContainer.transform.childCount - 1).transform);

        for (int i = 0; i < times; i++)
        {
            string tilename;
            Debug.Log("Discard Tile Converting to string: " + discardIndex);
            if (discardIndex.ToString().Length == 1) { tilename = "0" + discardIndex.ToString(); }
            else { tilename = discardIndex.ToString(); }
            Debug.Log("Tile Name is: " + tilename);
            PlayerContainer.transform.Find(tilename).gameObject.transform.SetParent(PlayerExposedContainer.transform.GetChild(PlayerExposedContainer.transform.childCount - 1).transform);
        }
    }
}

//public class Player {
//    public int seat;
//    public int[] playerHandConcealed;
//    public int[] playerHandExposed;
//    //public int bonusTai;
//    //public bool isWaiting;

//    public Player(int currentIndex) {
//        this.seat = currentIndex;
//    }

//    public int[] getPlayerHandConcealed() { 
        
//    }

//    public int[] setPlayerHandConcealed()
//    {

//    }

//    public int[] getPlayerHandExposed()
//    {

//    }

//    public int[] setPlayerHandExposed()
//    {

//    }
//}

//public class GameState {
//    public int currentWind;
//    public bool isReady;
//    public bool inGame;
//}