﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class AttractState : State {

    public int numberOfItems;
    public int currentItem = 0;

    public float displayTime;


    //STATE BASE FUNCTIONS

    override public void OnStateEnter(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        //Call the inherited functions to get the helper variable
        base.OnStateEnter(animator, info, layerIndex);
        
        helper.attract.SetActive(true);

        numberOfItems = GM.Instance.data.attractItems.Count;

        ShowCurrentItem();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        helper.attract.SetActive(false);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        if (Input.anyKeyDown)
            animator.SetTrigger("NextState");

        displayTime -= Time.deltaTime;

        //Do stuff when the delay is over
        if (displayTime < 0)
        {
            //When playing video, only switch if video is done
            if (GetCurrentItem().type == AttractItem.AttractItemType.Video)
            {
                if (!GM.Instance.video.player.isPlaying)
                    ShowNextItem();
            }

            //On text or image, just go to the next one
            else
            {
                ShowNextItem();
            }
        }
    }


    //STATE FUNCTIONS

    /// <summary>
    /// This shows the current attract item
    /// </summary>
    private void ShowCurrentItem()
    {
        if (GetCurrentItem() != null)
        {
            switch (GetCurrentItem().type)
            {
                case AttractItem.AttractItemType.Image:
                    DisplayImage();
                    return;

                case AttractItem.AttractItemType.Text:
                    DisplayText();
                    return;

                case AttractItem.AttractItemType.Video:
                    DisplayVideo();
                    return;
            }
        }

        else
        {
            GM.Instance.logger.Debug("Attract: Item Number does not exist.");
        }
    }

    private void ShowNextItem()
    {
        currentItem++;

        if (currentItem > numberOfItems - 1)
            currentItem = 0;

        ShowCurrentItem();
    }

    private void DisplayImage()
    {
        //Turn off unused gameobjects
        GM.Instance.video.StopVideo();
        helper.attractText.gameObject.SetActive(false);

        //Display the image
        helper.attractImage.gameObject.SetActive(true);
        helper.attractImage.sprite = GetCurrentItem().sprite;
        displayTime = GetCurrentItem().displayTime;
    }

    private void DisplayVideo()
    {
        //Turn off unused GameObjects
        helper.attractImage.gameObject.SetActive(false);
        helper.attractText.gameObject.SetActive(false);

        //Play the video!
        GM.Instance.video.PlayVideo(GetCurrentItem().pathToItem, false, true);
        displayTime = 1; //just need a second to get the video player running.
    }

    private void DisplayText()
    {
        //Turn off unused gameObjects
        helper.attractImage.gameObject.SetActive(false);

        //Play background video, and display text
        GM.Instance.video.PlayVideo(GM.Instance.data.launcherBackground, true, false);
        helper.attractText.text = GetCurrentItem().text;
        displayTime = GetCurrentItem().displayTime;
    }

    private AttractItem GetCurrentItem()
    {
        return GM.Instance.data.attractItems[currentItem];
    }
}
