using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadPrefs : MonoBehaviour
{
	[Header("General Setting")]
	[SerializeField] private bool canUse = false;
	[SerializeField] private MenuController menuController;
	
	[Header("Volume Setting")]
	[SerializeField] private TMP_Text volumeTextValue = null;
	[SerializeField] private Slider volumeSlider = null;
	
	[Header("Brightness Setting")]
	[SerializeField] private TMP_Text brightnessTextValue = null;
	[SerializeField] private Slider brightnessSlider = null;
	
	[Header("Quality Level Setting")]
	[SerializeField] private TMP_Dropdown qualityDropdown;
	
	[Header("Fullscreen Setting")]
	[SerializeField] private Toggle fullScreenToggle;
	
	[Header("Sensivity Setting")]
	[SerializeField] private TMP_Text controllerSenTextValue = null;
	[SerializeField] private Slider controllerSenSlider = null;
	
	[Header("Invert Y Setting")]
	[SerializeField] private Toggle invertYToggle = null;
	
	private void Awake()
	{
		if (canUse)
		{
			if (PlayerPrefs.HasKey("masterVolume"))
			{
				float localVolume = PlayerPrefs.GetFloat("masterVolume");
				
				volumeTextValue.text = localVolume.ToString("0.0");
				volumeSlider.value = localVolume;
				AudioListener.volume = localVolume;
			}
			else
			{
				menuController.ResetButton("Audio");
			}
			
			if (PlayerPrefs.HasKey("maserQuality"))
			{
				int localQuality = PlayerPrefs.GetInt("maserQuality");
				qualityDropdown.value = localQuality;
				QualitySettings.SetQualityLevel(localQuality);
			}
			
			if (PlayerPrefs.HasKey("maserFullscreen"))
			{
				int localFullscreen = PlayerPrefs.GetInt("maserFullscreen");
				
				if (localFullscreen == 1)
				{
					Screen.fullScreen = true;
					fullScreenToggle.isOn = true;
				}
				else
				{
					Screen.fullScreen = false;
					fullScreenToggle.isOn = false;
				}
			}
			
			if (PlayerPrefs.HasKey("maserBrightness"))
			{
				float localBrightness = PlayerPrefs.GetFloat("maserBrightness");
				
				brightnessTextValue.text = localBrightness.ToString("0.0");
				brightnessSlider.value = localBrightness;
			}
			
			if (PlayerPrefs.HasKey("maserSen"))
			{
				float localSensivity = PlayerPrefs.GetFloat("maserSen");
				
				controllerSenTextValue.text = localSensivity.ToString("0");
				controllerSenSlider.value = localSensivity;
				menuController.mainControllerSen = Mathf.RoundToInt(localSensivity);
			}
			
			if (PlayerPrefs.HasKey("maserInvertY"))
			{
				if (PlayerPrefs.GetInt("maserInvertY") == 1)
				{
					invertYToggle.isOn = true;
				}
				else
				{
					invertYToggle.isOn = false;
				}
			}
		}
	}
}






