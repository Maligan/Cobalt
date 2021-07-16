using System.Collections;
using System.Collections.Generic;
using System.Text;
using Cobalt.Net;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Text))]
public class Stats : MonoBehaviour
{
    private Text _text;

    [TextArea]
    [SerializeField]
    private string _pattern;
    private Dictionary<string, string> _values = new Dictionary<string, string>();

    private void Awake()
    {
        _text = GetComponent<Text>();
    }

    public void Set(string key, int value)
    {
        _values["{" + key + "}"] = value.ToString();
    }

    private IEnumerator Start()
    {
        while (true)
        {
            _values["{fps.current}"] = (1f / Time.smoothDeltaTime).ToString("f0");
            _values["{fps.time}"] = (Time.smoothDeltaTime * 1000).ToString("f1");

            var builder = new StringBuilder(_pattern);
            foreach (var pair in _values)
                builder.Replace(pair.Key, pair.Value);

            _text.text = builder.ToString();

            yield return new WaitForSecondsRealtime(0.25f);
        }
    }
}
