﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class IntroState : State
{
    public VideoClip introClip;

    private bool introLoaded;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        introLoaded = false;
        GM.video.PlayVideo(introClip, false);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        if (GM.video.player.isPrepared)
            introLoaded = true;

        if (introLoaded && !GM.video.player.isPlaying)
        {
            Debug.Log("NOT PLAYING");
            animator.SetTrigger("NextState");
        }
    }

}