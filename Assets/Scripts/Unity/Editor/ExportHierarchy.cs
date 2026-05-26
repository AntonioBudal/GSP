using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;

public class ExportHierarchy
{
    [MenuItem("Tools/Export Scene Hierarchy")]
    static void Export()
    {
        string path = "Hierarchy.txt";

        using (StreamWriter writer = new StreamWriter(path))
        {
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                WriteObject(root, writer, 0);
            }
        }

        Debug.Log("Hierarchy exported!");
    }

    static void WriteObject(GameObject obj, StreamWriter writer, int depth)
    {
        writer.WriteLine(new string(' ', depth * 2) + "- " + obj.name);

        foreach (Transform child in obj.transform)
        {
            WriteObject(child.gameObject, writer, depth + 1);
        }
    }
}