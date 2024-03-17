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
        
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            List<string> enabledObjects = GetEnabledObjects();
            DeleteObjects(enabledObjects);
        }

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
           
            string directoryPath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            filePath = Path.Combine(directoryPath, "UserData", "furnitureDelete.ini");

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


        
        private void CreateDefaultFile()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Title
                writer.WriteLine("# Furniture Delete Configuration");
                writer.WriteLine("# Setting the value to 1 enables deleting the object, 0 disables it.");

                // Object options with comments
                writer.WriteLine("#The desk");
                writer.WriteLine("Desk=1");
                writer.WriteLine();
                writer.WriteLine("#The Bed");
                writer.WriteLine("Bed=1");
                writer.WriteLine();
                writer.WriteLine("#The Bookshelf");
                writer.WriteLine("Bookshelf=1");
                writer.WriteLine();
                writer.WriteLine("#The Shelf");
                writer.WriteLine("Shelf=1");
                writer.WriteLine();
                writer.WriteLine("#The Dresser");
                writer.WriteLine("Dresser=1");
                writer.WriteLine();
                writer.WriteLine("#The Nightstand");
                writer.WriteLine("Nightstand=1");
                writer.WriteLine();
                writer.WriteLine("#The TV Stand");
                writer.WriteLine("Rack=1");
                writer.WriteLine();
                writer.WriteLine("#Collision Fixes");
                writer.WriteLine("#Shadow caster");
                writer.WriteLine("ShadowCaster=1");
                writer.WriteLine("#Shadow");
                writer.WriteLine("Shadow=1");
                writer.WriteLine();
                writer.WriteLine("#Debug allows left clicking to print all objects & MiddleMouse to delete object your looking at and print the objects name");
                writer.WriteLine("Debug=0");
                writer.WriteLine();

                writer.WriteLine("# To add more enable Debug to get the GameObject name then add new line like this 'ObjectName=1' 1 means delete 0 means dont delete");
            }
            LoggerInstance.Msg("Created Config File");
        }

        private void PrintAllGameObjects()
        {
            GameObject mainCamera = Camera.main.gameObject;

            if (mainCamera == null)
            {
                LoggerInstance.Msg("Main camera not found!");
                return;
            }

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

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (debug && Input.GetMouseButtonUp(0)) // Check if left mouse button is released
            {
                PrintAllGameObjects();
            }
            if (debug && Input.GetMouseButtonUp(2)) // Check if middle mouse button is released
            {
                DeleteObjectPlayerWasLookingAt();
            }

        }

        public void Load(int index)
        {
            LoggerInstance.Msg($"New leveld loaded: {index}");
        }

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
