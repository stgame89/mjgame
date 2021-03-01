using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionButtonScript : MonoBehaviour
{
    public Button yourButton;

    GameManagerScript gm;

    // Start is called before the first frame update
    void Start()
    {
        Button btn = yourButton.GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick);
        gm = FindObjectOfType<GameManagerScript>();
        gm.responded = false;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void TaskOnClick()
    {
        gm.responded = true;
        int.TryParse(this.name.Substring(name.Length - 1, 1), out gm.btnSelected);
        Debug.Log(gm.btnSelected);
    }

}
