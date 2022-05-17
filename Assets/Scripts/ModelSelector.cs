using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ModelType
{
    None,
    SIS,
    SIR,
    SEIR,
    SEIRD
}

public class ModelSelector : MonoBehaviour
{
    public ModelType modelType = ModelType.None;

    public SIR SIS_GameObject;
    public SIR SIR_GameObject;
    public SEIR SEIR_GameObject;
    public SEIRD SEIRD_GameObject;

    private void Start()
    {
    }

    [Button(Editor=false)]
    public void StartSimulation()
    {
        switch (modelType)
        {
            case ModelType.SIS:
                if(SIS_GameObject != null)
                    SIS_GameObject.StartSimulation();
                break;
            case ModelType.SIR:
                SIR_GameObject.StartSimulation();
                break;
            case ModelType.SEIR:
                SEIR_GameObject.StartSimulation();
                break;
            case ModelType.SEIRD:
                SEIRD_GameObject.StartSimulation();
                break;
            default:
                break;
        }
    }

    private void Update()
    {
        switch (modelType)
        {
            case ModelType.SIS:
                if (SIS_GameObject != null)
                    SIS_GameObject.gameObject.SetActive(true);
                if (SIR_GameObject != null)
                    SIR_GameObject.gameObject.SetActive(false);
                if (SEIR_GameObject != null)
                    SEIR_GameObject.gameObject.SetActive(false);
                if (SEIRD_GameObject != null)
                    SEIRD_GameObject.gameObject.SetActive(false);
                break;
            case ModelType.SIR:
                if (SIS_GameObject != null)
                    SIS_GameObject.gameObject.SetActive(false);
                if (SIR_GameObject != null)
                    SIR_GameObject.gameObject.SetActive(true);
                if (SEIR_GameObject != null)
                    SEIR_GameObject.gameObject.SetActive(false);
                if (SEIRD_GameObject != null)
                    SEIRD_GameObject.gameObject.SetActive(false);
                break;
            case ModelType.SEIR:
                if (SIS_GameObject != null)
                    SIS_GameObject.gameObject.SetActive(false);
                if (SIR_GameObject != null)
                    SIR_GameObject.gameObject.SetActive(false);
                if (SEIR_GameObject != null)
                    SEIR_GameObject.gameObject.SetActive(true);
                if (SEIRD_GameObject != null)
                    SEIRD_GameObject.gameObject.SetActive(false);
                break;
            case ModelType.SEIRD:
                if (SIS_GameObject != null)
                    SIS_GameObject.gameObject.SetActive(false);
                if (SIR_GameObject != null)
                    SIR_GameObject.gameObject.SetActive(false);
                if (SEIR_GameObject != null)
                    SEIR_GameObject.gameObject.SetActive(false);
                if (SEIRD_GameObject != null)
                    SEIRD_GameObject.gameObject.SetActive(true);
                break;
            default:
                Debug.Log("Invalid Model");
                break;
        }
    }
}
