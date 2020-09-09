using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnvironmentMangerScript : MonoBehaviour {
    // Start is called before the first frame update

    public Material Day, Sunset, Night;

    private Material currentMaterial;

    void Start() {
        int hour = DateTime.Now.Hour;
        if (hour < 17 && hour > 8) {
            RenderSettings.skybox = Day;
        } else if (hour >= 17 && hour < 19) {
            RenderSettings.skybox = Sunset;
        } else {
            RenderSettings.skybox = Night;
        }
        currentMaterial = RenderSettings.skybox;
    }

    void OnApplicationFocus(bool focus) {
        if (focus) {
            int hour = DateTime.Now.Hour;
            Material newMaterial;
            if (hour < 17 && hour > 8) {
                newMaterial = Day;
            } else if (hour >= 17 && hour < 19) {
                newMaterial = Sunset;
            } else {
                newMaterial = Night;
            }
            if (newMaterial != currentMaterial) {
                RenderSettings.skybox = newMaterial;
                currentMaterial = newMaterial;
            }
        }
    }
}
