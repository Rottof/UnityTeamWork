using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventManager : MonoBehaviour
{
    [SerializeField] GameObject eventPanel;
    [SerializeField] Button Button1;
    [SerializeField] Button Button2;
    [SerializeField] Button Button3;
    [SerializeField] TMP_Text resultText;
    [SerializeField] TMP_Text countdownText;
    [SerializeField] float countdownSeconds = 3f;
    [SerializeField] float closeDelay = 0.5f;

    // shipController ship;
    // ShootBullet shooter;

    void Start()
    {
        if (eventPanel == null) eventPanel = gameObject;
        eventPanel.SetActive(false); 
    }    

    //     var playerGo = GameObject.FindWithTag("Player");
    //     if (playerGo != null)
    //     {
    //         ship = playerGo.GetComponent<shipController>();
    //         shooter = playerGo.GetComponent<ShootBullet>();
    //         if (shooter == null) shooter = playerGo.GetComponentInChildren<ShootBullet>(true);
    //     }
    //     if (ship == null)
    //     {
    //         ship = FindObjectOfType<shipController>();
    //         if (ship != null)
    //         {
    //             shooter = ship.GetComponent<ShootBullet>();
    //             if (shooter == null) shooter = ship.GetComponentInChildren<ShootBullet>(true);
    //         }
    //     }

    //     if (Button1 != null)
    //     {
    //         Button1.onClick.RemoveAllListeners();
    //         Button1.onClick.AddListener(OnHealthUp);
    //     }
    //     if (Button2 != null)
    //     {
    //         Button2.onClick.RemoveAllListeners();
    //         Button2.onClick.AddListener(OnRandomEffect);
    //     }
    //     if (Button3 != null)
    //     {
    //         Button3.onClick.RemoveAllListeners();
    //         Button3.onClick.AddListener(OnSpeedUp);
    //     }
    // }

    // void OnHealthUp()
    // {
    //     if (ship != null) ship.TakeDamage(-20f);
    //     if (resultText != null) resultText.text = "Your health + 20";
    //     StartCloseDelay();
    // }

    // void OnRandomEffect()
    // {
    //     int r = Random.Range(0, 4);
    //     if (r == 0)
    //     {
    //         if (shooter != null) shooter.IncreaseFireRatePercent(0.10f);
    //         if (resultText != null) resultText.text = "Your fire rate + 10%";
    //     }
    //     else if (r == 1)
    //     {
    //         if (ship != null) ship.TakeDamage(5f);
    //         if (resultText != null) resultText.text = "Your health - 5";
    //     }
    //     else if (r == 2)
    //     {
    //         if (shooter != null) shooter.EnableScatter(true);
    //         if (resultText != null) resultText.text = "Your fire: scatter 3-way";
    //     }
    //     else
    //     {
    //         if (shooter != null) shooter.IncreaseFireRatePercent(0.10f);
    //         if (resultText != null) resultText.text = "Your fire rate + 10%";
    //     }
    //     StartCloseDelay();
    // }

    // void OnSpeedUp()
    // {
    //     if (ship != null) ship.speed *= 1.10f;
    //     if (resultText != null) resultText.text = "Your speed + 10%";
    //     StartCloseDelay();
    // }

    // void ClosePanel()
    // {
    //     if (eventPanel != null) eventPanel.SetActive(false);
    //     Time.timeScale = 1f;
    //     StopCountdown();
    //     if (countdownText != null) countdownText.text = "";
    // }

    // public void BeginCountdown()
    // {
    //     StopCountdown();
    //     if (eventPanel != null && !eventPanel.activeSelf) return;
    //     StartCoroutine(CountdownRoutine());
    // }

    // void StopCountdown()
    // {
    //     StopAllCoroutines();
    // }

    // IEnumerator CountdownRoutine()
    // {
    //     float remaining = Mathf.Max(0f, countdownSeconds);
    //     while (remaining > 0f)
    //     {
    //         if (countdownText != null) countdownText.text = Mathf.CeilToInt(remaining).ToString();
    //         yield return new WaitForSecondsRealtime(1f);
    //         remaining -= 1f;
    //     }
    //     if (countdownText != null) countdownText.text = "";
    //     ClosePanel();
    // }

    // void StartCloseDelay()
    // {
    //     StopCountdown();
    //     StartCoroutine(CloseAfterDelay());
    // }

    // IEnumerator CloseAfterDelay()
    // {
    //     yield return new WaitForSecondsRealtime(closeDelay);
    //     ClosePanel();
    // }
}
