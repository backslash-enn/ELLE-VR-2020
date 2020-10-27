using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class MoveCamera : MonoBehaviour
{
    
    public GameObject myName;
    public GameObject myCube;

    //Camera myCam;
    public float myfloat;
    public Vector3 myVec;
    public string[] myArr;
    public TMP_Text curName;
    public MeshRenderer myMesh;
    public int curIdx = 0;
    public Material[] myMaterials;

    // Start is called before the first frame update
    void Start()
    {
        //myCam = GetComponent<Camera>();
        curName = myName.GetComponent<TMP_Text>();
        curName.text = myArr[curIdx];

        myMesh = myCube.GetComponent<MeshRenderer>();
        myMesh.material = myMaterials[curIdx];

    }
    public void Dummy()
    {
        curName.text = myArr[(++curIdx % myArr.Length)];
        myMesh.material = myMaterials[curIdx % myMaterials.Length];
        
    }

    // Update is called once per frame
    void Update()
    {
        //myCam.fieldOfView += Time.deltaTime * 3f;
        if (Input.GetButton("Mine"))
            transform.position += (myfloat * Time.deltaTime) * Vector3.forward;
    
        if (Input.GetKey(KeyCode.DownArrow)) 
            transform.position += (myfloat * Time.deltaTime) * Vector3.back;

        transform.position += new Vector3(Input.GetAxis("Horizontal") , 0, 0) * myfloat * Time.deltaTime;


       
    }
    
}
