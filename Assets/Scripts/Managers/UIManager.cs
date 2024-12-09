using System;
using System.Collections.Generic; 
using Drony.Entities;
using UnityEngine;
using Interpreter;
using Utility;

/// <summary>
/// Singleton class <c>UIManager</c> manages logic for UI
/// </summary>
public class UIManager 
{
    private static UIManager _instance;
    private UIManager() { }
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new UIManager(); 
            }
            return _instance;
        }
    }
}

