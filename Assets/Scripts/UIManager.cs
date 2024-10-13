using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    private static UIManager _instance;

    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("UIManager is null");
            }
            return _instance;
        }
    }

    //Timer
    [SerializeField] private float _timeRemaining;
    [SerializeField] private bool _timerIsActive;
    [SerializeField] private TMP_Text _timerText;

    [SerializeField] private TMP_Text _score;

    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        
    }

    private void Update()
    {
        if (_timerIsActive == true)
        {
            if (_timeRemaining > 0)
            {
                _timeRemaining -= Time.deltaTime;
                UpdateTimer(_timeRemaining);
            }
            else if (_timeRemaining <= 0)
            {
                Debug.Log("Out of time");
                _timerIsActive = false;
            }
        }
    }

    //Update the timer text
    public void UpdateTimer(float currentTime)
    {
        currentTime += 1;

        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);

        _timerText.text = string.Format("{0:0} : {1:00}", minutes, seconds);
    }

}