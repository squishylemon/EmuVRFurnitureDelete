using MelonLoader;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace EmuVRFurnitureDelete
{
    public class EmuVRFurnitureDeleteC : MelonMod
    {
        private string filePath;
        private bool debug = false; // Set this to true to enable debug mode

        // Called when a scene is loaded
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            List<string> enabledObjects = GetEnabledObjects();
            DeleteObjects(enabledObjects);
        }

        // Called when the application starts
        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            // Set file path
            string directoryPath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            filePath = Path.Combine(directoryPath, "UserData", "furnitureDelete.ini");

            // If config file doesn't exist, create it
            if (!File.Exists(filePath))
            {
                CreateDefaultFile();
                debug = isdebugEnabled();
                List<string> enabledObjects = GetEnabledObjects();
                DeleteObjects(enabledObjects);
            }
            else
            {
                debug = isdebugEnabled();
                List<string> enabledObjects = GetEnabledObjects();
                DeleteObjects(enabledObjects);
            }
        }

        // Check if debug mode is enabled in the config file
        private bool isdebugEnabled()
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Trim() == "Debug=1")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // Create default config file
        private void CreateDefaultFile()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Write title and comments
                writer.WriteLine("# Furniture Delete Configuration");
                writer.WriteLine("# Setting the value to 1 enables deleting the object, 0 disables it.");

                // Write object options with comments
                writer.WriteLine("#The desk");
                writer.WriteLine("Desk=1");
                // More objects...

                // Write debug option
                writer.WriteLine("#Debug allows left clicking to print all objects & MiddleMouse to delete object your looking at and print the objects name");
                writer.WriteLine("Debug=0");

                // Write instruction for adding more objects
                writer.WriteLine("# To add more enable Debug to get the GameObject name then add new line like this 'ObjectName=1' 1 means delete 0 means dont delete");
            }
            LoggerInstance.Msg("Created Config File");
        }

        // Print all game objects
        private void PrintAllGameObjects()
        {
            // Get main camera
            GameObject mainCamera = Camera.main.gameObject;

            if (mainCamera == null)
            {
                LoggerInstance.Msg("Main camera not found!");
                return;
            }

            // Find all objects in the scene
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

            // Create a list to hold GameObject-distance pairs
            List<(GameObject, float)> objectDistances = new List<(GameObject, float)>();

            // Calculate distances and add pairs to the list
            foreach (GameObject obj in allObjects)
            {
                MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    float distanceToCamera = Vector3.Distance(obj.transform.position, mainCamera.transform.position);
                    objectDistances.Add((obj, distanceToCamera));
                }
            }

            // Sort the list based on distances
            objectDistances.Sort((x, y) => x.Item2.CompareTo(y.Item2));

            // Print the sorted list
            foreach ((GameObject obj, float distance) in objectDistances)
            {
                LoggerInstance.Msg($"{obj.name}: Has Mesh, Distance to Main Camera: {distance:F2}");
            }
        }

        // Get list of enabled objects from config file
        private List<string> GetEnabledObjects()
        {
            List<string> enabledObjects = new List<string>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2 && parts[1] == "1")
                    {
                        enabledObjects.Add(parts[0]);
                    }
                }
            }

            return enabledObjects;
        }

        // Delete specified objects
        private void DeleteObjects(List<string> objectNames)
        {
            foreach (string name in objectNames)
            {
                GameObject objToDelete = GameObject.Find(name);
                if (objToDelete != null)
                {
                    GameObject.Destroy(objToDelete);
                }
            }
        }

        // Called every frame
        public override void OnUpdate()
        {
            base.OnUpdate();
            // If debug mode is enabled and left mouse button is released, print all game objects
            if (debug && Input.GetMouseButtonUp(0))
            {
                PrintAllGameObjects();
            }
            // If debug mode is enabled and middle mouse button is released, delete object player is looking at
            if (debug && Input.GetMouseButtonUp(2))
            {
                DeleteObjectPlayerWasLookingAt();
            }
        }

        // Log when a new level is loaded
        public void Load(int index)
        {
            LoggerInstance.Msg($"New level loaded: {index}");
        }

        // Delete the object the player is looking at
        private void DeleteObjectPlayerWasLookingAt()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject objToDelete = hit.collider.gameObject;
                if (objToDelete != null)
                {
                    MeshFilter meshFilter = objToDelete.GetComponent<MeshFilter>();
                    if (meshFilter != null && meshFilter.sharedMesh != null)
                    {
                        LoggerInstance.Msg($"Deleted Object: {objToDelete.name}");
                        GameObject.Destroy(objToDelete);
                    }
                }
            }
        }
    }
}
