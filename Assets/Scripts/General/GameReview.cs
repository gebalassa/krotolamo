//using Google.Play.Review;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameReview : MonoBehaviour
{
    [SerializeField] float timeBeforeShowPrompt = 1.25f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ShowReviewPrompt());
    }

    private IEnumerator ShowReviewPrompt()
    {
        int gameSessions = GameController.GetPlaySessions();
        bool reviewPromptWasDisplayed = GameController.ReviewPromptWasDisplayed();
        yield return null;

        // Revisar si prompt ya fue mostrado
        /*if (!reviewPromptWasDisplayed && gameSessions >= JankenUp.Reviews.minSessionsBeforePrompt)
        {
            yield return new WaitForSeconds(timeBeforeShowPrompt);

            if (Application.platform == RuntimePlatform.Android)
            {
                var reviewManager = new ReviewManager();
                var requestFlowOperation = reviewManager.RequestReviewFlow();
                requestFlowOperation.Completed += playReviewInfoAsync => {
                    if (playReviewInfoAsync.Error == ReviewErrorCode.NoError)
                    {
                        var playReviewInfo = playReviewInfoAsync.GetResult();
                        var launchFlowOperation = reviewManager.LaunchReviewFlow(playReviewInfo);
                        launchFlowOperation.Completed += launchFlowOperationInfo => {
                            if (launchFlowOperation.Error == ReviewErrorCode.NoError) GameController.SetReviewPromptWasDisplayed();
                        };
                    }
                };
            }
        }
        else
        {
            yield return null;
        }*/

    }

}
