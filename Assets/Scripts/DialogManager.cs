using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [SerializeField] GameObject dialogBox; // Diyalog kutusu UI objesi
    [SerializeField] Text dialogText; // Diyalog metnini tutan UI öðesi
    [SerializeField] int lettersPerSecond; // Yazý animasyonu hýzý

    public event Action OnShowDialog; // Diyalog açýldýðýnda tetiklenen olay
    public event Action OnHideDialog; // Diyalog kapandýðýnda tetiklenen olay
    public static DialogManager Instance { get; private set; } // Singleton eriþimi

    private void Awake()
    {
        Instance = this; // Tek bir DialogManager olmasýný saðlýyoruz
    }

    int currentLine = 0; // Þu anda hangi satýrda olduðumuzu takip eden deðiþken
    Dialog dialog; // Mevcut diyalog verisi
    bool isTyping; // Yazý animasyonu devam ediyor mu?

    public IEnumerator ShowDialog(Dialog dialog)
    {
        yield return new WaitForEndOfFrame();
        OnShowDialog?.Invoke(); // Diyalog baþladýðýný bildir

        this.dialog = dialog;
        dialogBox.SetActive(true); // Diyalog kutusunu aç
        StartCoroutine(TypeDialog(dialog.Lines[0])); // Ýlk satýrý yazdýrmaya baþla
    }

    public void HandleUpdate()
    {
        // Kullanýcý "F" tuþuna bastýðýnda ve yazý tamamlanmýþsa ilerle
        if (Input.GetKeyUp(KeyCode.F) && !isTyping)
        {
            ++currentLine;
            if (currentLine < dialog.Lines.Count)
            {
                StartCoroutine(TypeDialog(dialog.Lines[currentLine])); // Sonraki satýra geç
            }
            else
            {
                dialogBox.SetActive(false); // Diyalog kutusunu kapat
                currentLine = 0; // Satýr sýfýrla
                OnHideDialog?.Invoke(); // Diyalog bittiðini bildir
            }
        }
    }

    public IEnumerator TypeDialog(string line)
    {
        isTyping = true;
        dialogText.text = ""; // Önce metni temizle

        foreach (var letter in line.ToCharArray())
        {
            dialogText.text += letter; // Harf harf ekle
            yield return new WaitForSeconds(1f / lettersPerSecond); // Animasyon hýzý
        }

        isTyping = false;
    }
}
