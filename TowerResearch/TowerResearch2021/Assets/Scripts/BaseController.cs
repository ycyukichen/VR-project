using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseController : MonoBehaviour
{
    private Vector3 rotate;
    public Button spinClockwise;
    public Button spinCounter;
    public Button resetButton;

    // Start is called before the first frame update
    void Start()
    {
        rotate = new Vector3(0, 50, 0);

    }


    /// <summary>
    /// invoked by the rotation buttons in the UI
    /// </summary>  
    public void RotateClockwise()
    {
        float rotationAmount = 30 * Time.deltaTime; // Adjust rotation speed if necessary
        transform.Rotate(0, rotationAmount, 0, Space.Self);
        // Log the rotation with positive angle for clockwise
        DataManager.Instance.LogRotationEvent(rotationAmount, true); // Pass 'true' to indicate clockwise rotation
    }

    /// <summary>
    /// invoked by the rotation buttons in the UI
    /// </summary>  
    public void RotateCounterClockwise()
    {
        float rotationAmount = -30 * Time.deltaTime; // Adjust rotation speed if necessary
        transform.Rotate(0, rotationAmount, 0, Space.Self);
        // Log the rotation with negative angle for counter-clockwise
        DataManager.Instance.LogRotationEvent(rotationAmount, false); // Pass 'false' to indicate counter-clockwise rotation
    }

    /// <summary>
    /// invoked by the rotation buttons in the UI
    /// </summary>  
    public void ResetRotation()
    {
        LeanTween.rotate(this.gameObject, new Vector3(0, 0, 0), 1.0f).setEaseInOutCubic();
        //LeanTween is a library that makes some animations a little bit easier to do in code
        //this one just resets the base to rotation 0 in a cubic manner in 1 second but it has lots of other cool applications in unity
        DataManager.Instance.LogRotationEvent(0, false); // Indicating a reset, direction doesn't matter for reset
    }
}