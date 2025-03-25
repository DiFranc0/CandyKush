using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ProgressBar : MonoBehaviour
{
[Header("Configuração da barra de progresso")]
[SerializeField] private Image fillBeckImg;
[SerializeField] private float smoothTime = 0.5f;

[Header("Progresso do jogador")]
[SerializeField] private float currentProgress = 0f;

private float targetProgress;

    // Start is called before the first frame update
    void Start()
    {
        fillBeckImg.fillAmount = 0f;
        targetProgress = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        fillBeckImg.fillAmount = Mathf.MoveTowards(fillBeckImg.fillAmount, targetProgress, smoothTime * Time.deltaTime);
    }
    
    public void AddProgress(float amount)
    {
        currentProgress += amount;
        
        currentProgress = Mathf.Clamp(currentProgress, 0f, 100f);
        
        targetProgress = currentProgress / 100f;
    }
    
    public void ResetProgress()
    {
        currentProgress = 0f;
        targetProgress = 0f;
        fillBeckImg.fillAmount = 0f;
    }
    
    public bool IsComplete()
    {
        return currentProgress >= 100f;
    }
}
