using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Core.Scripts.UI.LoadMenu
{
    public class LoadObjectsMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _buttonPrefab;
        [SerializeField] private Transform _content;
        [SerializeField] private Button _refreshButton;

        private const string URL = Config.URL + "/archive/names";

        private void Start()
        {
            StartCoroutine(FetchStringsFromApi());
            _refreshButton.onClick.AddListener(RefreshList);
        }
        
        IEnumerator FetchStringsFromApi()
        {
            UnityWebRequest www = UnityWebRequest.Get(URL);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
                yield break;
            }

            string text = www.downloadHandler.text;
            if (text == "\"\"")
            {
                yield break;
            }

            string[] receivedNames = text.Substring(1, text.Length - 2).Split(",");
            
            for (int i = _content.childCount - 1; i >= 0; i--)
            {
                Destroy(_content.GetChild(i).gameObject);
            }

            foreach (var item in receivedNames)
            {
                GameObject spawnedButton = Instantiate(_buttonPrefab, _content);
                spawnedButton.GetComponentInChildren<TextMeshProUGUI>().text = item;
            }
        }
        
        private void RefreshList()
        {
            StartCoroutine(FetchStringsFromApi());
        }
    }
}
