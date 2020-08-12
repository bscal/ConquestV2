using Conquest;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIGeneratorDebugger : MonoBehaviour
{

    public Text counterText;
    public UnityEngine.UI.Button pauseButton;
    public Text hexInfoText;
    public Text tileObjText;
    public Text movementText;
    public Text plateDataText;
    public Text plateData2Text;

    private Hex m_watchedHex;

    private void Start()
    {
        pauseButton.onClick.AddListener(OnPauseClicked);
    }

    void Update()
    {
        counterText.text = $"{GameManager.Singleton.generator.iterations}/{MapGenerator.numOfIters}";
    }

    private void OnPauseClicked()
    {
        GameManager.Singleton.generator.paused = !GameManager.Singleton.generator.paused;
    }
}
