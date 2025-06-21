using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Attachment;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class LoadObjectButton : MonoBehaviour
{
    [SerializeField] private Vector3 _startPositionOnTable;
    [SerializeField] private Material _loadedObjectMaterial;

    private const string URL_BASE = Config.URL + "/archive?name=";

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    public void OnButtonClick()
    {
        Debug.Log($"suka merzkiy {this.gameObject.name}");
        StartCoroutine(DownloadFile());
    }

    private IEnumerator DownloadFile()
    {
        UnityWebRequest www = UnityWebRequest.Get(URL_BASE + gameObject.GetComponentInChildren<TextMeshProUGUI>().text);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("request suka");
            byte[] stlData = www.downloadHandler.data;
            List<Mesh> meshes = STLReader.Read(stlData);
            List<MeshFilter> meshFilters = new List<MeshFilter>();
            List<GameObject> objectSeparated = new List<GameObject>();
            foreach (Mesh mesh in meshes)
            {
                SpawnMesh(mesh, out MeshFilter meshFilter, out GameObject obj);
                meshFilters.Add(meshFilter);
                objectSeparated.Add(obj);
            }

            GameObject colliderParent = new GameObject("Collider");
            GameObject combinedObj = MeshCombiner.CombineMeshes(
                meshFilters, colliderParent.transform, _startPositionOnTable, _loadedObjectMaterial);
            
            SetupObject(colliderParent, combinedObj);
            

            foreach (var obj in objectSeparated)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
        }
    }

    private void SpawnMesh(Mesh mesh, out MeshFilter meshFilter, out GameObject obj)
    {
        obj = new GameObject("LoadedObjectMesh");
        meshFilter = obj.AddComponent<MeshFilter>();
        obj.AddComponent<MeshRenderer>();

        meshFilter.mesh = mesh;
    }

    private void SetupObject(GameObject colliderParent, GameObject combinedObject)
    {
        GameObject loadedObjectParent = new GameObject("LoadedObjectParent");
        colliderParent.transform.SetParent(loadedObjectParent.transform);
        
        GameObject spawn = GameObject.FindWithTag("Spawn");
        loadedObjectParent.transform.SetParent(spawn.transform);
        loadedObjectParent.transform.position = _startPositionOnTable;
        
        //colliderParent.layer = LayerMask.NameToLayer(Config.DESTRUCTABLE_LAYER_MASK);
        //loadedObjectParent.layer = LayerMask.NameToLayer(Config.DESTRUCTABLE_LAYER_MASK);
        loadedObjectParent.transform.localScale *= 0.001f;
        /*loadedObjectParent.AddComponent<Rigidbody>()
            .AddComponent<Grabbable>()
            .AddComponent<HandGrabInteractable>()
            .AddComponent<GrabInteractable>();
        Grabbable grabbable = loadedObjectParent.GetComponent<Grabbable>();
        loadedObjectParent.GetComponent<GrabInteractable>().InjectOptionalPointableElement(grabbable);
        loadedObjectParent.GetComponent<GrabInteractable>().InjectRigidbody(loadedObjectParent.GetComponent<Rigidbody>());
        loadedObjectParent.GetComponent<HandGrabInteractable>().InjectRigidbody(loadedObjectParent.GetComponent<Rigidbody>());
        loadedObjectParent.GetComponent<HandGrabInteractable>().InjectOptionalPointableElement(grabbable);
       
        loadedObjectParent.GetComponent<Rigidbody>().useGravity = false;
        loadedObjectParent.GetComponent<Rigidbody>().isKinematic = true;
        loadedObjectParent.AddComponent<GrabbableWithName>(); 
        */


        loadedObjectParent.AddComponent<XRGrabInteractable>();

        colliderParent.AddComponent<BoxCollider>();
        colliderParent.GetComponent<BoxCollider>().isTrigger = true;
        combinedObject.AddComponent<BoxCollider>();


        colliderParent.GetComponent<BoxCollider>().center = new Vector3(0, 0, 0);
        colliderParent.GetComponent<BoxCollider>().size = combinedObject.GetComponent<BoxCollider>().size;

        loadedObjectParent.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        loadedObjectParent.GetComponent<Rigidbody>().isKinematic = true;


        /*
        string objectType = string.Empty;
        TextMeshProUGUI textMesh = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        if (textMesh.text[^1] == 'I')
        {
            objectType = "МРТ";
        }
        else if (textMesh.text[^1] == 'T')
        {
            objectType = "КТ";
        }
        loadedObjectParent.GetComponent<GrabbableWithName>().RussianName = $"Загруженный объект {objectType}";
        */
        combinedObject.GetComponent<BoxCollider>().enabled = false;

        loadedObjectParent.GetComponent<XRGrabInteractable>().colliders.Add(loadedObjectParent.GetComponentInChildren<BoxCollider>());
    }

}
