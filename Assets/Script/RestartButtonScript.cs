using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RestartButtonScript : MonoBehaviour
{
    public Button yourButton;

    GameManagerScript gm;

    // Start is called before the first frame update
    void Start()
    {
        Button btn = yourButton.GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick);
        gm = FindObjectOfType<GameManagerScript>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    void TaskOnClick()
    {
        gm.ClearCanvas();
        gm.startGame = true;
    }

}
