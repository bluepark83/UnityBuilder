using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text_version;
    // Start is called before the first frame update
    void Start()
    {
        if ( _text_version != null)
            _text_version.text = Application.version;
    }
}
