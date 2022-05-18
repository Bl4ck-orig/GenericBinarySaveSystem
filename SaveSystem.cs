using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

/// <summary>
/// Used for saving, loading any serializable object to save within Unity.
/// Uses the Application.persistentDataPath as path and stores the objects as .dat files.
/// 
/// inspired by https://www.youtube.com/watch?v=XOjd_qU2Ido&t=238s
/// 
/// 27.10.2021 - Bl4ck?
/// </summary>
public static class SaveSystem<T>
{
    #region Private Variables -----------------------------------------------------------------
    private static bool IsActive;
    private static float maxIterations = 15f;
    private static float operations = 0f;
    #endregion -----------------------------------------------------------------

    #region Saving -----------------------------------------------------------------
    /// <summary>
    /// Tries to save an object.
    /// </summary>
    /// <param name="_dataToSafe">The desired data to safe</param>
    /// <param name="_fileName">The name used for saving</param>
    public static void TrySaving(T _dataToSafe, string _fileName)
    {
        Debug.Log("Saving data which is " + _dataToSafe.ToString());

        // Tries to save until the operationTimer exceeds the maximal time
        while (operations < maxIterations)
        {
            if (!IsActive)
            {
                Save(_dataToSafe, _fileName);
                operations = 0;

                return;
            }
            operations++;
        }
        operations = 0;

        Debug.LogError("Saving was not possible! Maximal amount of iterations reached.");
    }

    /// <summary>
    /// Saves some data using BinaryFormatter.
    /// </summary>
    /// <param name="_dataToSafe">The desired data to safe</param>
    /// <param name="_fileName">The name used for saving</param>
    private static void Save(T _dataToSafe, string _fileName)
    {
        IsActive = true;
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/" + _fileName + ".dat";
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, _dataToSafe);
        stream.Close();
        IsActive = false;
    }
    #endregion -----------------------------------------------------------------

    #region Loading -----------------------------------------------------------------
    /// <summary>
    /// Loads all found files starting with the specified name.
    /// </summary>
    /// <param name="_fileName">The desired name</param>
    /// <returns>All files in a list that start with the desired name</returns>
    public static List<T> LoadAll(string _fileName)
    {
        List<T> output = new List<T>();
        DirectoryInfo directoryInfo = new DirectoryInfo(Application.persistentDataPath);
        foreach (FileInfo file in directoryInfo.GetFiles())
        {
            if (file.Name.StartsWith(_fileName))
                output.Add(TryLoading(file.Name.TrimEnd(file.Extension.ToCharArray())));
        }
        return output;
    }

    /// <summary>
    /// Try to load a file with a desired name.
    /// </summary>
    /// <param name="_fileName">Name of the file</param>
    /// <returns>An object of containing the file</returns>
    public static T TryLoading(string _fileName)
    {
        T data = default;
        if (Exists(_fileName))
        {
            // As long as there is no data loaded and the operationTimer does not exceed the limit
            while (data == null && operations < maxIterations)
            {
                // Only operates if it is not active
                if (!IsActive)
                {
                    data = Load(_fileName);
                    Debug.Log("Loading file " + _fileName + " with data which is:  " + data);
                }
                operations++;
            }
            if (data == null) 
                Debug.LogError("Failing to load file " + _fileName + "! Maximal amount of iterations reached.");
        }
        else
            Debug.LogError("No SaveFile found at " + _fileName + "!");
        operations = 0f;
        return data;
    }

    /// <summary>
    /// Loads the specific file using BinaryFormatter.
    /// </summary>
    /// <param name="_fileName">Name of the file</param>
    /// <returns>The loaded object</returns>
    private static T Load(string _fileName)
    {
        IsActive = true;
        string path = Application.persistentDataPath + "/" + _fileName + ".dat";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            T data =(T) formatter.Deserialize(stream);
            stream.Close();
            IsActive = false;
            return data;
        }
        else
        {
            IsActive = false;
            return default;
        }
    }
    #endregion -----------------------------------------------------------------

    #region Deleting -----------------------------------------------------------------
    /// <summary>
    /// Tries to delete a file.
    /// </summary>
    /// <param name="_fileName">The desired filename</param>
    public static void TryDeleting(string _fileName)
    {
        Debug.Log("Deleting data called "+ _fileName);
        while (operations < maxIterations)
        {
            // Only operates if it is not active
            if (!IsActive)
            {
                Delete(_fileName);
                operations = 0;

                return;
            }
            operations++;

        }
        operations = 0;
        Debug.LogError("Deleting savefile was not possible! Maximal amount of iterations reached.");
    }

    /// <summary>
    /// Deletes a specific file.
    /// </summary>
    /// <param name="_fileName">Name of the file</param>
    private static void Delete(string _fileName)
    {
        IsActive = true;
        string path = Application.persistentDataPath + "/" + _fileName + ".dat";
        if (File.Exists(path))
        {
            Debug.Log("Deleting " + path);
            File.Delete(path);
        }
        else
            Debug.LogError("File not found in " + path);
        IsActive = false;
    }
    #endregion -----------------------------------------------------------------

    #region Clarifictaion -----------------------------------------------------------------
    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    /// <param name="_fileName">Name of the file</param>
    /// <returns>True if it exists</returns>
    public static bool Exists(string _fileName) => File.Exists(Application.persistentDataPath + "/" + _fileName + ".dat");
    #endregion -----------------------------------------------------------------
}
