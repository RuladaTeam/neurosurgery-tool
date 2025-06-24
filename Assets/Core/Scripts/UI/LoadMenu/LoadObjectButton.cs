using System.Collections;
using System.Dynamic;
using Core.Scripts.MeshCreation;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Core.Scripts.UI.LoadMenu
{
    public class LoadObjectButton : MonoBehaviour
    {
        [SerializeField] private Vector3 _startPositionOnTable;
        [SerializeField] private Material _loadedObjectMaterial;
        [SerializeField] private GameObject _objectMenuPrefab;

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            StartCoroutine(DownloadFile());
        }

        private IEnumerator DownloadFile()
        {
            string objectName = gameObject.GetComponentInChildren<TextMeshProUGUI>().text;
            var vertexRequest = UnityWebRequest.Get(Config.URL_BASE_VERTEX + objectName);
            var trianglesRequest = UnityWebRequest.Get(Config.URL_BASE_TRIANGLES + objectName);
            var colorsRequest = UnityWebRequest.Get(Config.URL_BASE_COLORS + objectName + "&density=" 
                                                    + ColorSchemeChanger.CurrentDensityMode);

            yield return vertexRequest.SendWebRequest();
            yield return trianglesRequest.SendWebRequest();
            yield return colorsRequest.SendWebRequest();

            if (vertexRequest.result != UnityWebRequest.Result.Success
                || trianglesRequest.result != UnityWebRequest.Result.Success
                || colorsRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(vertexRequest.error);
                Debug.Log(trianglesRequest.error);
                Debug.Log(colorsRequest.error);
                yield break;
            }
            
            byte[] vertexData = vertexRequest.downloadHandler.data;
            byte[] trianglesData = trianglesRequest.downloadHandler.data;
            byte[] colorsData = colorsRequest.downloadHandler.data;

            GameObject spawnedObject = MeshCreator.Read(vertexData, trianglesData, colorsData, _loadedObjectMaterial);

            SetupObject(spawnedObject);
        }

        private void SetupObject(GameObject obj)
        {
            obj.tag = Config.LOADED_OBJECT_TAG;
            obj.layer = LayerMask.NameToLayer(Config.LOADED_OBJECT_LAYER);
            obj.name = gameObject.GetComponentInChildren<TextMeshProUGUI>().text;
            obj.transform.localScale *= 0.002f;
            obj.transform.rotation = Quaternion.Euler(new Vector3(180, 180, 0));
        
            obj.transform.position = Vector3.up;
            obj.AddComponent<BoxCollider>();
            
            LoadedObject loadedObjectComponent = obj.AddComponent<LoadedObject>();
            obj.AddComponent<XRGrabInteractable>().useDynamicAttach = true;
            BoxCollider hoverCollider = obj.AddComponent<BoxCollider>();
            GameObject objectMenu = GameObject.Instantiate(_objectMenuPrefab, obj.transform);
            loadedObjectComponent.Init(hoverCollider, objectMenu);
            
            obj.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            obj.GetComponent<Rigidbody>().isKinematic = true;
        }
    }
}
