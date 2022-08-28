using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DataHandler : MonoBehaviour
{
    public Transform character;

    private Rigidbody body;
    private TMP_Text m_text;

    private void Awake() {

        body = character.GetComponent<Rigidbody>();
        m_text = GetComponent<TextMeshProUGUI>() ?? gameObject.AddComponent<TextMeshProUGUI>();
        m_text.text = "Start";
    }

    // Update is called once per frame
    void Update() {
        
        if (body.velocity.magnitude > 0.001) {

            m_text.text = $"Position:\n{body.position}\nVelocity:\n{body.velocity}";
        }
    }
}
