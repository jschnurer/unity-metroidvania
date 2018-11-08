using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour {
    enum DialogModes
    {
        Message,
        Choice
    }

    GameObject dialogBox;
    Text dialogText;
    Text continuePrompt;
    bool pausing = false;
    bool paused = false;
    DialogModes mode = DialogModes.Message;
    List<GameObject> choicePositions = new List<GameObject>();
    int choiceIndex = 0;
    bool movedChoice = false;

    public GameObject Choice1Box;
    public GameObject Choice2Box;
    public GameObject Choice3Box;

    public GameObject Choice1BoxTextObj;
    public GameObject Choice2BoxTextObj;
    public GameObject Choice3BoxTextObj;

    public GameObject Choice1PointerLoc;
    public GameObject Choice2PointerLoc;
    public GameObject Choice3PointerLoc;

    public GameObject ChoicePointer;

    Action Choice1Action;
    Action Choice2Action;
    Action Choice3Action;

    public static DialogManager Find()
    {
        return GameObject.Find("DialogManager").GetComponent<DialogManager>();
    }

	void Start () {
        Choice1Box.SetActive(false);
        Choice2Box.SetActive(false);
        Choice3Box.SetActive(false);
        ChoicePointer.SetActive(false);

        dialogBox = GameObject.Find("DialogBox");
        dialogText = GameObject.Find("DialogText").GetComponent<Text>();
        continuePrompt = GameObject.Find("ContinuePrompt").GetComponent<Text>();
        dialogBox.SetActive(false);

        // Starting message when game begins
        ShowMessage("Your ship was struck by an unknown object moving at extremely high velocity. "
            + "The damage to your ship is irreparable without recovering 5 missing components. "
            + "Search the planet to find where they have fallen.");
	}

    public void ShowMessage(string message)
    {
        mode = DialogModes.Message;
        pausing = true;
        Time.timeScale = 0;
        continuePrompt.enabled = true;
        dialogText.text = message;
        dialogBox.SetActive(true);
        GameObject.Find("Player").GetComponent<Player_Move_Prot>().InControl = false;

        DisableChoices();
    }

    public void ShowChoice(string message,
        string choice1,
        Action choice1Action,
        string choice2,
        Action choice2Action,
        string choice3 = null,
        Action choice3Action = null)
    {
        mode = DialogModes.Choice;
        pausing = true;
        Time.timeScale = 0;
        continuePrompt.enabled = false;
        dialogText.text = message;
        dialogBox.SetActive(true);
        GameObject.Find("Player").GetComponent<Player_Move_Prot>().InControl = false;
        choiceIndex = 0;

        choicePositions.Clear();

        if (!string.IsNullOrEmpty(choice1))
        {
            ToggleChoice(true, choice1,
                Choice1Box,
                Choice1BoxTextObj,
                Choice1PointerLoc);

            Choice1Action = choice1Action;
        }
        else
            ToggleChoice(false, string.Empty,
                Choice1Box,
                Choice1BoxTextObj,
                Choice1PointerLoc);

        if (!string.IsNullOrEmpty(choice2))
        {
            ToggleChoice(true, choice2,
                            Choice2Box,
                            Choice2BoxTextObj,
                            Choice2PointerLoc);

            Choice2Action = choice2Action;
        }
        else
            ToggleChoice(false, string.Empty,
                            Choice2Box,
                            Choice2BoxTextObj,
                            Choice2PointerLoc);

        if (!string.IsNullOrEmpty(choice3))
        {
            ToggleChoice(true, choice3,
                            Choice3Box,
                            Choice3BoxTextObj,
                            Choice3PointerLoc);

            Choice3Action = choice3Action;
        }
        else
            ToggleChoice(false, string.Empty,
                            Choice3Box,
                            Choice3BoxTextObj,
                            Choice3PointerLoc);

        ChoicePointer.SetActive(true);
        ChoicePointer.transform.position = choicePositions[choiceIndex].transform.position;
    }

    void ToggleChoice(bool active, string text,
        GameObject choiceBox,
        GameObject choiceBoxTextObj,
        GameObject choiceLoc)
    {
        choiceBox.SetActive(active);
        choiceBoxTextObj.SetActive(active);
        choiceBoxTextObj.GetComponent<Text>().text = text;

        if (active)
            choicePositions.Add(choiceLoc);
    }

    void DisableChoices()
    {
        ToggleChoice(false, null, Choice1Box, Choice1BoxTextObj, Choice1PointerLoc);
        ToggleChoice(false, null, Choice2Box, Choice2BoxTextObj, Choice2PointerLoc);
        ToggleChoice(false, null, Choice3Box, Choice3BoxTextObj, Choice3PointerLoc);
        ChoicePointer.SetActive(false);
    }

    private void Update()
    {
        if (pausing)
        {
            pausing = false;
            paused = true;
            return;
        }

        if (paused && (Input.GetButtonUp("Jump") || Input.GetButtonUp("Start")))
        {
            paused = false;
            dialogBox.SetActive(false);
            Time.timeScale = 1;
            GameObject.Find("Player").GetComponent<Player_Move_Prot>().InControl = true;

            if (mode == DialogModes.Choice)
            {
                HandleChoice();
            }
        }
        else if(paused)
        {
            float y = Input.GetAxis("Vertical");
            bool joyStickUpOrDown = y >= .5 || y <= -.5;

            if (!joyStickUpOrDown && movedChoice)
                movedChoice = false;
            else if(joyStickUpOrDown && !movedChoice)
                HandleChoiceMove(y);
        }
    }

    void HandleChoiceMove(float y)
    {
        if (y >= 0.5)
            choiceIndex++;
        else if (y <= -.5)
            choiceIndex--;

        if (choiceIndex < 0)
            choiceIndex = choicePositions.Count - 1;
        else if (choiceIndex >= choicePositions.Count)
            choiceIndex = 0;

        ChoicePointer.transform.position = choicePositions[choiceIndex].transform.position;
        movedChoice = true;
    }

    void HandleChoice()
    {
        var pointerPos = ChoicePointer.transform.position;

        Action choice = null;

        if (pointerPos == Choice1PointerLoc.transform.position)
            choice = Choice1Action;
        else if (pointerPos == Choice2PointerLoc.transform.position)
            choice = Choice2Action;
        else if (pointerPos == Choice3PointerLoc.transform.position)
            choice = Choice3Action;

        if (choice != null)
        {
            choice();
        }
    }
}
