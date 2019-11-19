
// Display debug log
#define SHOWDEBUGLOG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class define : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if SHOWDEBUGLOG
        Debug.Log("DEBUG ON");
#endif
    }
}
