using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public void OnStartButtonClicked()
    {
        // 로딩 씬으로 이동
        SceneManager.LoadScene("LoadingScene");
    }

    public void OnExitButtonClicked()
    {
        // 게임 종료
        Application.Quit();
    }

    public void OnResetDataButtonClicked()
    {
        SaveSystem.DeletePlayerData();
    }
}
