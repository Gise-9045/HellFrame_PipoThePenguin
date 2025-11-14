using System;
using MoreMountains.Feedbacks;
using UnityEngine;

public class PlayerFeedback : MonoBehaviour
{
    private PlayerMovement playerMovement;
    public MMF_Player jumpStartFeedbacks;
    public MMF_Player landingFeedbacks;
    private void OnEnable()
    {
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Destroy(this);
        }

        playerMovement.OnJump += JumpFeedback;
        playerMovement.OnLand += LandFeedback;
    }

    private void OnDisable()
    {
        playerMovement.OnJump -= JumpFeedback;
        playerMovement.OnLand -= LandFeedback;
    }

    private void JumpFeedback()
    {
        jumpStartFeedbacks?.PlayFeedbacks();
    }
    
    private void LandFeedback()
    {
        landingFeedbacks?.PlayFeedbacks();
    }
}