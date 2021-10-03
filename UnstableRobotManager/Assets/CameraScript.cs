using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraScript : MonoBehaviour
{
    [Header("Camera")]
    public Camera cam;
    public float StabilityFOVChange = 0;
    
    [Header("Screen shake")]
    public float Trauma;
    public float MaxAngleX;
    public float MaxAngleY;
    public float MaxAngleZ;
    public float MaxOffset;
    public float TraumaDecrease;
    public float MinTrauma = 0;


    float angleX;
    float angleY;
    float angleZ;
    float shake;
    float offsetX;
    float offsetY;
    float offsetZ;
    float dragSpeed;
    Vector3 startLocalPos;

    private float defaultCamSize;

    

    
    public static CameraScript instance;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        startLocalPos = transform.localPosition;
        Trauma = 0;

        defaultCamSize = cam.orthographicSize;
    }


    void Update()
    {
        //Camera shake
        if (Trauma < MinTrauma)
            Trauma = MinTrauma;

        shake = Mathf.Pow(Trauma, 2);
        angleX = MaxAngleX * shake * Random.Range(-1, 2);
        angleY = MaxAngleY * shake * Random.Range(-1, 2);
        angleZ = MaxAngleZ * shake * Random.Range(-1, 2);


        offsetX = MaxOffset * shake * Random.Range(-1, 2);
        offsetY = MaxOffset * shake * Random.Range(-1, 2);
        offsetZ = MaxOffset * shake * Random.Range(-1, 2);

        transform.localEulerAngles = new Vector3(angleX, angleY, angleZ);
        transform.localPosition = startLocalPos + new Vector3(offsetX, offsetY, offsetZ);

        if (Trauma > 0)
            Trauma -= TraumaDecrease * Time.deltaTime;
        else
            Trauma = 0;


        cam.orthographicSize = defaultCamSize + StabilityFOVChange;
    }



    public void Shake(float value)
    {
        //DeveloperConsoleMain.WriteToScreen("AddShake +" + value.ToString(), "AddShake" + Time.time, 2);
        Trauma += value;

        if (Trauma > 1)
        {
            Trauma = 1;
        }
    }
}
