using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class WorkNodeAdder : MonoBehaviour
{
    public MasterAI ai;

    public static WorkNodeAdder instance;

    private void OnEnable()
    {
        instance = this;
    }


#if UNITY_EDITOR
    [MenuItem("CustomMethods/Find All Work Nodes")]
#endif
    public static void FindAllNodes()
    {
        instance.ai.CivilWorkNodes = new List<WorkNode>();
        instance.ai.SecurityWorkNodes = new List<WorkNode>();

        GameObject[] AllNodes = GameObject.FindGameObjectsWithTag("WorkNode");
        foreach (GameObject n in AllNodes)
        {
            WorkNode nscript = n.GetComponent<WorkNode>();

            if (nscript.IsCivil)
                instance.ai.CivilWorkNodes.Add(nscript);
            else
                instance.ai.SecurityWorkNodes.Add(nscript);
        }

    }
}
