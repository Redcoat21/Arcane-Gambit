using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButtonHandler : MonoBehaviour
{
    public WeaponManager weaponManager;

    public void OnPlayButtonClicked()
    {
        SelectedWeaponStorage.selectedWeapon = weaponManager.CurrentWeapon;
        Debug.Log(SelectedWeaponStorage.selectedWeapon);
        SceneManager.LoadScene("EtdgLevelGenerationScene 1");
    }
}
