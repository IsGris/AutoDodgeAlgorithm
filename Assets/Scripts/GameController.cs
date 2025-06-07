using UnityEngine;

public class GameController : MonoBehaviour
{
    public Camera mainCamera;
    public AutoDodgeAlgorithm dodgeAlgorithm;
    
    private string timeScaleMultiplier = "1";
    private bool IsGamePaused = true;


    private void Start()
    {
        if (IsGamePaused)
            Time.timeScale = 0;
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 400), "Game Controller", GUI.skin.window);

        GUILayout.Label("Red text - amount of bullets that is collided with bot");
        GUILayout.Label("Green text - amount of bullets that is collided with player");
        GUILayout.Label("White text - timer");

        if (GUILayout.Button("Toggle game pause state"))
            if (IsGamePaused)
            {
                IsGamePaused = false;
                Time.timeScale = 1;
            } else
            {
                IsGamePaused = true;
                Time.timeScale = 0;
            }

        GUILayout.Label("Time Scale Multiplier(dont set to high values, can skip collision of bullets and player):");
        timeScaleMultiplier = GUILayout.TextField(timeScaleMultiplier);
        
        if (GUILayout.Button("Apply"))
        {
            float timeScaleMultiplierFl = 1f;
            if (float.TryParse(timeScaleMultiplier, out timeScaleMultiplierFl))
            {
                Debug.Log("Set time scale to: " + timeScaleMultiplierFl.ToString());
                Time.timeScale = timeScaleMultiplierFl;
            } else
            {
                Debug.LogError("Can't parse float value: " + timeScaleMultiplier);
            }
        }

        if (GUILayout.Button("Switch view to bot"))
        {
            mainCamera.transform.position = new(0, mainCamera.transform.position.y, mainCamera.transform.position.z);
        }

        if (GUILayout.Button("Switch view to player"))
        {
            mainCamera.transform.position = new(30, mainCamera.transform.position.y, mainCamera.transform.position.z);
        }

        if (GUILayout.Button("toggle weight color for each cell on grid"))
        {
            dodgeAlgorithm.DisplayWeightColor = !dodgeAlgorithm.DisplayWeightColor;
        }

        if (GUILayout.Button("toggle weight text for each cell on grid"))
        {
            dodgeAlgorithm.DisplayWeightText = !dodgeAlgorithm.DisplayWeightText;
        }

        GUILayout.EndArea();
    }
}
