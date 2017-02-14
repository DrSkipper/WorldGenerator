using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitHandler : VoBehavior
{
    public string Destination = "";
    //public ExitInput Input = ExitInput.Exit;

    [System.Serializable]
    public enum ExitInput
    {
        Exit,
        Pause,
        Cancel
    }

    void Update()
    {
        bool pressed = Input.GetKey(KeyCode.Escape);
        /*switch (this.Input)
        {
            default:
            case ExitInput.Exit:
                pressed = MenuInput.Exit();
                break;
            case ExitInput.Pause:
                pressed = MenuInput.Pause();
                break;
            case ExitInput.Cancel:
                pressed = MenuInput.Cancel();
                break;
        }*/

        if (pressed)
        {
            this.Exit();
        }
    }

    public void Exit()
    {
        if (this.Destination != "")
        {
            //ProgressData.LoadFromDisk(true);
            SceneManager.LoadScene(this.Destination);
            Destroy(this.gameObject);
        }
        else
        {
            Application.Quit();
        }
    }
}
