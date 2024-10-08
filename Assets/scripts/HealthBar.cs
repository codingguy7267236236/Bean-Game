using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;

    private void Awake()
    {
        slider = transform.GetComponent<Slider>();
    }

    public void SetHealth(int health)
    {
        slider.value = health;
    }

    public void SetMax(int max)
    {
        slider.maxValue = max;
        SetHealth(max);
    }
}
