using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public void OnStartButtonClicked()
    {
        // �ε� ������ �̵�
        SceneManager.LoadScene("LoadingScene");
    }

    public void OnExitButtonClicked()
    {
        // ���� ����
        Application.Quit();
    }

    public void OnResetDataButtonClicked()
    {
        SaveSystem.DeletePlayerData();
    }
}
