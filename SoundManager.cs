﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using UnityEngine.UI;

namespace Spaces {
    public class SoundManager : MonoBehaviourPun {

        PhotonVoiceNetwork PunVoice;

        Recorder recorder;

        private bool hasActivated = false;


        public GameObject SliderComponent;

        public GameObject UnMutedButton;
        public GameObject MutedButton;
        int publicRoom;


        void Start() {
            publicRoom = PlayerPrefs.GetInt("isInPublicWorld");
            GameObject Voice = GameObject.FindGameObjectWithTag("Voice");
            PunVoice = Voice.GetComponent<PhotonVoiceNetwork>();
        }

        public void Mute() {
            UnMutedButton.SetActive(false);
            MutedButton.SetActive(true);
            PunVoice.PrimaryRecorder.TransmitEnabled = false;
        }

        public void UnMute() {
            UnMutedButton.SetActive(true);
            MutedButton.SetActive(false);
            PunVoice.PrimaryRecorder.TransmitEnabled = true;
        }


        void Update() {
            if (PunVoice.ClientState == Photon.Realtime.ClientState.Joined && (!hasActivated)) {
                if (publicRoom == 1) {
                    recorder = PunVoice.PrimaryRecorder;
                    PunVoice.PrimaryRecorder.TransmitEnabled = false;
                    Destroy(this);
                    return;
                }
                hasActivated = true;
                UnMutedButton.SetActive(true);
                recorder = PunVoice.PrimaryRecorder;
            } 
            // if (recorder) {
            //     if (recorder.LevelMeter != null){
            //         if (recorder.LevelMeter.CurrentAvgAmp > 0.01f) {
            //             float percentagePower = (recorder.LevelMeter.CurrentAvgAmp / 0.13f);
            //             slider.value = percentagePower;
            //         }
            //     } else {
            //         slider.value = 0;
            //     }
            // }
        }
    }
}
