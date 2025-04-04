using UnityEngine;
using System.Collections;

public class SettingsAnimator : MonoBehaviour
{
    public GameObject settingsCanvas;
    public Animator animator;

    public void OpenSettings()
    {
        settingsCanvas.SetActive(true);
        animator.SetTrigger("Show");
    }

    public void CloseSettings()
    {
        animator.SetTrigger("Hide");
        StartCoroutine(DisableAfterAnimation(0.4f));
    }

    IEnumerator DisableAfterAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        settingsCanvas.SetActive(false);
    }
}