using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening; // Ensure DoTween is installed and imported

public class EndingController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] EndingTexts;

    void Start()
    {
        // Start the sequence when the game begins
        PlayEndingSequence();
    }

    private void PlayEndingSequence()
    {
        // Ensure texts are fully transparent at the start
        foreach (var text in EndingTexts)
        {
            text.alpha = 0;
        }

        // Create a sequence using DoTween
        Sequence endingSequence = DOTween.Sequence();

        for (int i = 0; i < EndingTexts.Length; i++)
        {
            int index = i; // Capture the current index for the lambda expression

            // Add animations for each EndingText
            endingSequence.AppendCallback(() => EndingTexts[index].gameObject.SetActive(true))
                           .Append(EndingTexts[index].DOFade(1, 1f)) // Fade in over 1 second
                           .AppendInterval(2f)               // Stay visible for 2 seconds
                           .Append(EndingTexts[index].DOFade(0, 1f)) // Fade out over 1 second
                           .OnComplete(() => EndingTexts[index].gameObject.SetActive(false)); // Disable after fade out
        }

        // Start the sequence
        endingSequence.Play();
    }
}