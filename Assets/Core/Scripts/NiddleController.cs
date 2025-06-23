using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NiddleController : MonoBehaviour
{
    [SerializeField] private GameObject _topNiddlePart;
    [SerializeField] private Transform _bottomNiddlePart;
    private GameObject _currenTopNiddlePart;

    public void SetTopNidllePart()
    {
        Destroy(_currenTopNiddlePart);
        _currenTopNiddlePart = Instantiate(_topNiddlePart, _bottomNiddlePart.position, _bottomNiddlePart.rotation);
    }
}
