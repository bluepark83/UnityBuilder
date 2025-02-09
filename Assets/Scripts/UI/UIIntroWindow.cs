using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIIntroWindow : MonoBehaviour
{
    [SerializeField] private Slider ProgressSlider;
    [SerializeField] private TMP_Text Text_Percent;

    private void OnEnable()
    {
        GameManager.ProgressEvent += OnProgressSlider;
        GameManager.CompletionEvent += OnCompletionEvent;
    }

    private void OnDisable()
    {
        GameManager.ProgressEvent -= OnProgressSlider;
        GameManager.CompletionEvent += OnCompletionEvent;
    }

    void OnProgressSlider(float value)
    {
        if ( ProgressSlider != null) 
            ProgressSlider.value = value;
        
        if ( Text_Percent != null)
            Text_Percent.text = value * 100 + "%";
    }

    void OnCompletionEvent(bool isComplete)
    {
        if ( ProgressSlider != null) 
            ProgressSlider.value = 1f;
        
        if ( Text_Percent != null)
            Text_Percent.text = "100%";
    }
}
