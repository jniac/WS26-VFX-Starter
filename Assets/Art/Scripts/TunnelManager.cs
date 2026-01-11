using Mono.Cecil;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TunnelManager : MonoBehaviour
{
    public static TunnelManager Instance { get; private set; }

    Transform dollyTransform;

    /// <summary>
    /// Finds and returns the Transform of the Dolly object among root objects in the scene.
    /// </summary>
    /// <returns></returns>
    Transform FindDollyTransform()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = currentScene.GetRootGameObjects();

        foreach (GameObject obj in rootObjects)
        {
            if (obj.name == "Tunnel Dolly")
            {
                return obj.transform;
            }
        }

        return null; // Dolly not found
    }

    void UpdateDollyTransform(Transform dollyTransform)
    {
        if (dollyTransform != null)
        {
            // Example: Move the dolly along the z-axis over time
            dollyTransform.position += new Vector3(0, 0, Time.deltaTime * 5f);
        }
    }

    void OnValidate()
    {
        gameObject.name = "Tunnel Manager";
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // There can only be one instance of TunnelManager
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Update()
    {

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            var scenes = new string[] { "Tunnel-Demo-SectionA", "Tunnel-Demo-SectionB" };
            var currentIndex = System.Array.IndexOf(scenes, SceneManager.GetActiveScene().name);
            var nextIndex = (currentIndex + 1) % scenes.Length;
            SceneManager.LoadScene(scenes[nextIndex]);
        }

        if (dollyTransform == null)
            dollyTransform = FindDollyTransform();

        if (dollyTransform != null)
            UpdateDollyTransform(dollyTransform);
    }
}
