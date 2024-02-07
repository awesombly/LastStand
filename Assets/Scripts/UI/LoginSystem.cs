using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoginSystem : MonoBehaviour
{
    public TMP_InputField email;
    public TMP_InputField password;

    private void Awake()
    {
        if( password != null )
            password.contentType = TMP_InputField.ContentType.Password;
    }
}
