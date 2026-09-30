using System.Collections.Generic;
using UnityEngine;

public class SaveableObject : MonoBehaviour
{
    // Общий список всех интерактивных предметов
    public static List<SaveableObject> AllSaveables = new List<SaveableObject>();

    [Header("Уникальный ID объекта")]
    [SerializeField] private string uniqueID;

    private void OnEnable()
    {
        if (!AllSaveables.Contains(this))
            AllSaveables.Add(this);
    }

    private void OnDisable()
    {
        AllSaveables.Remove(this);
    }

    public void SaveState()
    {
        if (string.IsNullOrEmpty(uniqueID)) return;

        // Сохраняем, поднят/активен ли объект
        PlayerPrefs.SetInt(uniqueID + "_Active", gameObject.activeSelf ? 1 : 0);
        // Сохраняем угол поворота (для дверей/рычагов)
        PlayerPrefs.SetFloat(uniqueID + "_RotY", transform.localEulerAngles.y);
    }

    public void LoadState()
    {
        if (string.IsNullOrEmpty(uniqueID)) return;

        if (PlayerPrefs.HasKey(uniqueID + "_Active"))
        {
            bool isActive = PlayerPrefs.GetInt(uniqueID + "_Active") == 1;
            gameObject.SetActive(isActive);
        }

        if (PlayerPrefs.HasKey(uniqueID + "_RotY"))
        {
            float rotY = PlayerPrefs.GetFloat(uniqueID + "_RotY");
            Vector3 rot = transform.localEulerAngles;
            transform.localEulerAngles = new Vector3(rot.x, rotY, rot.z);
        }
    }
}