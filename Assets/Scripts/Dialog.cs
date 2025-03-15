
using System.Collections.Generic;
using UnityEngine;

// Bir diyalog içeriðini temsil eden sýnýf
[System.Serializable]
public class Dialog
{
    [SerializeField] List<string> lines; // Diyalog satýrlarýný içeren liste

    public List<string> Lines
    {
        get { return lines; } // Diyalog metnini dýþarýdan eriþilebilir hale getirir
    }
}
