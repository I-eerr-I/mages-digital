using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;



public class MenuController : MonoBehaviour
{
	[Header("Volume Setting")]
	[SerializeField] private TMP_Text volumeTextValue = null;
	[SerializeField] private Slider volumeSlider = null;
	[SerializeField] private float defaultVolume = 0.2f;
	
	[Header("Gameplay Settings")]
	[SerializeField] private TMP_Text controllerSenTextValue = null;
	[SerializeField] private Slider controllerSenSlider = null;
	[SerializeField] private int defaultSen = 4;
	public int mainControllerSen = 4;
	
	[Header("Toggle Settings")]
	[SerializeField] private Toggle invertYToggle = null;
	
	[Header("Graphics Settings")]
	[SerializeField] private TMP_Text brightnessTextValue = null;
	[SerializeField] private Slider brightnessSlider = null;
	[SerializeField] private int defaultBrightness = 1;
	
	public Light sceneLight;
	
	[Space(10)]
	[SerializeField] private TMP_Dropdown qualityDropdown;
	[SerializeField] private Toggle fullScreenToggle;
	
	
	private int _qualityLevel;
	private bool _isFullScreen;
	private float _brightnessLevel;
	
	//[Header("Confirmation")]
	//[SerializeField] private GameObject confirmationPrompt = null;
	
	[Header("New Project")]
	public string _newGame2;
	public string _newGame3;
	public string _newGame4;
	public string _newGame5;
	public string _newGame6;	
	
	[Header("Resolution Dropdowns")]
	public TMP_Dropdown resolutionDropdown;
	private Resolution[] resolutions;
	
	private void Start()
	{
		resolutions = Screen.resolutions;
		resolutionDropdown.ClearOptions();
		
		List<string> options = new List<string>();
		
		int currentResolutionIndex = 0;
		
		for (int i = 0; i<resolutions.Length; i++)
		{
			string option = resolutions[i].width + " x " + resolutions[i].height;
			options.Add(option);
			
			if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
			{
				currentResolutionIndex = i;
			}
		}
		
		resolutionDropdown.AddOptions(options);
		resolutionDropdown.value = currentResolutionIndex;
		resolutionDropdown.RefreshShownValue();	
	}
	
	public void SetResolution(int resolutionIndex)
	{
		Resolution resolution = resolutions[resolutionIndex];
		Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
	}
	
	public void NewGameDialogYes2()
	{
		SceneManager.LoadScene(_newGame2);
	}
	
	public void NewGameDialogYes3()
	{
		SceneManager.LoadScene(_newGame3);
	}
	
	public void NewGameDialogYes4()
	{
		SceneManager.LoadScene(_newGame4);
	}
	
	public void NewGameDialogYes5()
	{
		SceneManager.LoadScene(_newGame5);
	}
	
	public void NewGameDialogYes6()
	{
		SceneManager.LoadScene(_newGame6);
	}
	
	public void ExitButton()
	{
		Application.Quit();
	}
	
	public void SetVolume(float volume)
	{
		AudioListener.volume = volume;
		volumeTextValue.text = volume.ToString("0.0");
	}
	
	public void VolumeApply()
	{
		PlayerPrefs.SetFloat("maserVolume", AudioListener.volume);
		//StartCoroutine(ConfirmationBox());
	}
	
	public void SetControllerSen(float sensitivity)
	{
		mainControllerSen = Mathf.RoundToInt(sensitivity);
		controllerSenTextValue.text = sensitivity.ToString("0");
	}
	
	public void GameplayApply()
	{
		if (invertYToggle.isOn)
		{
			PlayerPrefs.SetInt("maserInvertY", 1);
		}
		else
		{
			PlayerPrefs.SetInt("maserInvertY", 0);
		}
		
		PlayerPrefs.SetFloat("maserSen", mainControllerSen);
		//StartCoroutine(ConfirmationBox());
	}
	
	public void SetBrightness(float brightness)
	{
		_brightnessLevel = brightness;
		brightnessTextValue.text = brightness.ToString("0.0");
	}
	
	public void SetFullScreen(bool isFullScreen)
	{
		_isFullScreen = isFullScreen;
	}
	
	public void SetQuality(int qualityIndex)
	{
		_qualityLevel = qualityIndex;
	}
	
	public void GraphicsApply()
	{
		PlayerPrefs.SetFloat("maserBrightness", _brightnessLevel);
		sceneLight.intensity = _brightnessLevel;
		
		PlayerPrefs.SetInt("maserQuality", _qualityLevel);
		QualitySettings.SetQualityLevel(_qualityLevel);
		
		PlayerPrefs.SetInt("maserFullscreen", (_isFullScreen ? 1 : 0));
		Screen.fullScreen = _isFullScreen;
		
		//StartCoroutine(ConfirmationBox());
	}
	
	public void ResetButton(string MenuType)
	{
		if (MenuType == "Graphics")
		{
			//Reset brightness
			brightnessSlider.value = defaultBrightness;
			brightnessTextValue.text = defaultBrightness.ToString("0.0");
			
			qualityDropdown.value = 1;
			QualitySettings.SetQualityLevel(1);
			
			fullScreenToggle.isOn = false;
			Screen.fullScreen = false;
			
			Resolution currentResolution = Screen.currentResolution;
			Screen.SetResolution(currentResolution.width, currentResolution.height, Screen.fullScreen);
			resolutionDropdown.value = resolutions.Length;
			
			GraphicsApply();
		}
		
		if (MenuType == "Audio")
		{
			AudioListener.volume = defaultVolume;	
			volumeSlider.value = defaultVolume;
			volumeTextValue.text = defaultVolume.ToString("0.0");
			VolumeApply();
		}
		
		if (MenuType == "Gameplay")
		{
			controllerSenTextValue.text = defaultSen.ToString("0");
			controllerSenSlider.value = defaultSen;
			mainControllerSen = defaultSen;
			invertYToggle.isOn = false;
			GameplayApply();
		}
	}
	
//	public IEnumerator ConfirmationBox()
//	{
		//confirmationPrompt.SetActive(true);
		//yield return new WaitForSeconds(2);
		//confirmationPrompt.SetActive(false);
	//}
}
