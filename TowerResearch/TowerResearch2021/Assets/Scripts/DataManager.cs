using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

//need these for file writing
using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;

public class DataManager : MonoBehaviour
{


    /**
     * 
     * This script manages all data collection in the program
     * It is stored in a plaintext file, but tab separated to it can easily be copied into a spreadsheet.
     * 
     * Data is collected both when the user hits submit (entering their choices, rotation, )
     * and when the towers have finished falling
     * 
     * 
     * 
     */

    public static DataManager Instance { get; private set; }

    public StabilityManager stability;
    public ZoneScript direction;
    public ButtonManager2 submit;
    public BaseController rotation;
    Button[] buttons;
    public GameManager gm;

    public int zone;
    public int zone2;
    public float stable;
    float time;
    public int epochs;
    public float yRotFinal;

    //these are the blocks that the user can play with
    public GameObject vertBlock, vert2;
    public GameObject horBlock, ho2;
    Vector3 vertStart, vert2start, hoStart, ho2start;

    // Rotation data
    public class RotationEvent
    {
        public float YRotationAngle { get; set; }
        public bool IsClockwise { get; set; }
        public DateTime Timestamp { get; set; }

        // Constructor
        public RotationEvent(float yRotationAngle, bool isClockwise, DateTime timestamp)
        {
            GameObject baseObject = GameObject.Find("Base");
            yRotationAngle = baseObject.transform.rotation.eulerAngles.y;
            YRotationAngle = yRotationAngle;
            IsClockwise = isClockwise;
            timestamp = DateTime.Now;
            Timestamp = timestamp;
        }

        // Override ToString() method to provide a string representation of the rotation event
        public override string ToString()
        {
            return $"YRotationAngle: {YRotationAngle}, Clockwise: {IsClockwise}, Timestamp: {Timestamp}";
        }
    }

    public List<RotationEvent> rotationEvents = new List<RotationEvent>();

    private StringBuilder combinedCenterOfMassLogs = new StringBuilder();

    //collect data from the UI
    //add data to the "spreadsheet"
    //the data files are just .txt files, but they are pre-formatted and can easily just be copied and pasted into excel/google sheets with correct formatting

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        epochs = 0;
        gm = GameManager.instance;
        buttons = direction.returnButtons();
        gm.Invoke("buildTower", .7f);
        resetData();

        vertStart = vertBlock.transform.position;
        hoStart = horBlock.transform.position;
        vert2start = vert2.transform.position;
        ho2start = ho2.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //make sure at least one zone is selected - can't submit if no zones are selected
        //the user does not have to touch stability in order to submit. only select one zone
        //so we have to make sure they are focusing on stability and not forgetting it.
        if (direction.returnDataChoice()[0] != -1)
        {
            submit.readyToSubmit = true;
        }
        else
        {
            submit.readyToSubmit = false;
        }
        if (!stability.slider.IsInteractable())
        {
            //if we can't use the slider, we can't use anything else
            rotation.resetButton.interactable = false;
            rotation.spinClockwise.interactable = false;
            rotation.spinCounter.interactable = false;
            submit.readyToSubmit = false;
        }

        if(gm.blocksIndex >= 300)
        {
            //reset the trial index each time we finish an epoch (50 trials)
            gm.blocksIndex = 0;
            epochs++;
            
        }
        if (epochs == 2 && gm.blocksIndex != 0)
        {
            //end the experiment
            UnityEngine.SceneManagement.SceneManager.LoadScene("EndScene");
        }

    }


    /// <summary>
    /// Logs a rotation event
    /// </summary>
    /// <param name="yRotationAngle">The y-axis rotation angle to log.</param>
    /// <param name="isClockwise">Indicates if the rotation was clockwise.</param>
    public void LogRotationEvent(float yRotationAngle, bool isClockwise)
    {
        rotationEvents.Add(new RotationEvent(yRotationAngle, isClockwise, DateTime.UtcNow));
        Debug.Log($"Logged rotation: {yRotationAngle}, Clockwise: {isClockwise}, Time: {DateTime.UtcNow}");
    }



    int towerCount = 0;

    /// <summary>
    /// collect the user's inputs based on tower observations
    /// </summary>
    public void submitData()
    {
        if (submit.readyToSubmit) //ie: at least one zone is selected
        {
            GameObject baseObject = GameObject.Find("Base");

            //collect values and reset rotation
            zone = direction.returnDataChoice()[0];
            zone2 = direction.returnDataChoice()[1];
            stable = stability.slider.value;
            yRotFinal = baseObject.transform.rotation.eulerAngles.y;
            rotation.ResetRotation();

            Debug.Log("Data saved");
            Debug.Log("Zone: " + zone + " Stability: " + stable);
            Debug.Log("Y Rotation: " + yRotFinal);
            // Log rotation data
            foreach (RotationEvent rotationEvent in rotationEvents)
            {
                Debug.Log($"Rotation: {rotationEvent.YRotationAngle}, Clockwise: {rotationEvent.IsClockwise}, Time: {rotationEvent.Timestamp}");
            }
            var directoryPath = @"D:\VR_tower\IntPhysTowersExperimentData\";
            var screenshotName = "Screenshot_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_Tower" + towerCount +".png";
            var screenshotPath = Path.Combine(directoryPath, screenshotName);
            ScreenCapture.CaptureScreenshot(screenshotPath);
            Debug.Log($"Screenshot captured and saved as {screenshotPath}");


            //collect the starting position of each block

            Rigidbody[] rbList = baseObject.GetComponentsInChildren<Rigidbody>();
            startingPositions = new Vector3[rbList.Length];
            for (int i = 0; i < rbList.Length; i++)
            {
                startingPositions[i] = rbList[i].position;
            }

            direction.setZoneChoice(new int[] { -1, -1 });

            foreach (Button i in buttons)
            {
                i.interactable = false;
            }
            stability.slider.interactable = false;

            //activate physics in 1.1 seconds, then collect the truth in 6 seconds
            //I've found these times to feel the most natural and consistant in terms of towers having finished their physics interactions.
            gm.Invoke("towerFall", 1.1f);
            Invoke("collectTruth", 6.0f);
            
            //once to delete the old tower, once to spawn in the new one.
            //has to work this way because it is coded that way...
            //there is a break between the deletion and spawning because
            //if the new tower is spawned to soon, the algortithm will not work properly
            //this is dependent on processing speed, so it happens occasionally during runtime anyways.
            //see bug report for more information.
            gm.Invoke("buildTower", 6.1f);
            gm.Invoke("buildTower", 6.25f);
            Invoke("resetData", 6.26f);

            //reset the position of the playable blocks
            vertBlock.transform.position = vertStart;
            horBlock.transform.position = hoStart;
            vert2.transform.position = vert2start;
            ho2.transform.position = ho2start;
            vertBlock.SetActive(false);
            horBlock.SetActive(false);
            ho2.SetActive(false);
            vert2.SetActive(false);


            towerCount++;
        }




    }


    /// <summary>
    /// Calculates the center of mass for combinations of blocks stacked on the base from top to bottom.
    /// </summary>
    public List<Vector3> CalculateStackedCentersOfMass()
    {
        GameObject baseObject = GameObject.Find("Base");
        Rigidbody[] allRigidbodies = baseObject.GetComponentsInChildren<Rigidbody>();
        var sortedRbs = allRigidbodies.OrderByDescending(rb => rb.transform.position.y).ToArray();

        List<Vector3> combinedCenterOfMass = new List<Vector3>();
        

        for (int i = 1; i <= sortedRbs.Length; i++)
        {
            Vector3 com = CalculateCombinedCenterOfMass(sortedRbs, i);
            combinedCenterOfMass.Add(com);
            combinedCenterOfMassLogs.AppendLine($"Center of mass for the top {i} blocks: {com}");
        }

        Debug.Log("Combined Center Of Mass: " + combinedCenterOfMassLogs.ToString());
        return combinedCenterOfMass;
    }


    /// <summary>
    /// Calculates the combined center of mass for a specified number of blocks.
    /// </summary>
    private Vector3 CalculateCombinedCenterOfMass(Rigidbody[] rigidbodies, int count)
    {
        Vector3 combinedCenterOfMass = Vector3.zero;
        float totalMass = 0f;

        for (int i = 0; i < count; i++)
        {
            combinedCenterOfMass += rigidbodies[i].worldCenterOfMass * rigidbodies[i].mass;
            totalMass += rigidbodies[i].mass;
        }

        if (totalMass > 0)
        {
            combinedCenterOfMass /= totalMass;
        }

        return combinedCenterOfMass;
    }





    Vector3 drawAvg;
    Vector3 centerOfMass;
    Vector3 furthestXZ;
    int[] zoneMajority;
    Vector3[] finalPositions;

    /// <summary>
    /// Collect data based on the end of the tower falling
    /// </summary>
    public void collectTruth()
    {

        //grab the correct values after physics have been activated

        GameObject baseObject = GameObject.Find("Base");
        Rigidbody[] rbList = baseObject.GetComponentsInChildren<Rigidbody>();
        int numKids = baseObject.transform.childCount;

        Vector3 avgPos = new Vector3(0, 0, 0);
        centerOfMass = new Vector3(0, 0, 0);
        furthestXZ = new Vector3(0, 0, 0);
        float mass = 0;
        zoneMajority = new int[5] {0, 0, 0, 0, 0};
        finalPositions = new Vector3[rbList.Length];


        finalPositions = new Vector3[rbList.Length];
        for (int i = 0; i < rbList.Length; i++)
        {
            finalPositions[i] = rbList[i].position;
        }

        foreach (Rigidbody rb in rbList)
        {
            avgPos += rb.position;
            centerOfMass += rb.position * rb.mass;
            mass += rb.mass;

            //calculate the furthest block from the center
            if(rb.position.x * rb.position.x + rb.position.z * rb.position.z > furthestXZ.x * furthestXZ.x + furthestXZ.z + furthestXZ.z)
            {
                furthestXZ = rb.position;
            }

            //calculate majority zone.
            //this tests if it's still over the baseblock

            RaycastHit hit;
            if(Physics.Raycast(rb.position, new Vector3(0, -1, 0), out hit, 15f, LayerMask.GetMask("Data")));
            {
                try
                {
                    Debug.Log(rb.name + " hit " + hit.collider.name);

                    //add them to the array. Base, red, blue, green, yellow
                    if (hit.collider.name == "BaseDataCube")
                    {
                        zoneMajority[0]++;
                    }
                    if (hit.collider.name == "RedCube")
                    {
                        zoneMajority[1]++;
                    }
                    if (hit.collider.name == "BlueCube")
                    {
                        zoneMajority[2]++;
                    }
                    if (hit.collider.name == "GreenCube")
                    {
                        zoneMajority[3]++;
                    }
                    if (hit.collider.name == "YellowCube")
                    {
                        zoneMajority[4]++;
                    }


                }
                catch (System.Exception)
                {
                    Debug.Log("Couldnt do the thing");
                    //throw;
                    //this shouldnt happen, but there were issues without it
                }
            }




        }


        int? maxVal = null; //nullable so this works even if you have all super-low negatives
        int majorityIndex = -1;
        for (int i = 0; i < zoneMajority.Length; i++)
        {
            int thisNum = zoneMajority[i];
            if (!maxVal.HasValue || thisNum >= maxVal.Value)
            {
                maxVal = thisNum;
                majorityIndex = i;
            }
        }

        avgPos /= numKids;

        centerOfMass /= mass;
        

        Debug.Log("Average Position of Blocks: " + avgPos);
        Debug.Log("Center of Mass is: " + centerOfMass);
        Debug.Log("Furthest Block is: " + furthestXZ);
        Debug.Log("Majority layout: " + majorityIndex);
        //Debug.DrawRay(avgPos, transform.up);
        drawAvg = avgPos;


        WriteToFile();

    }

    Vector3[] startingPositions;
    /// <summary>
    /// Reset the experiment data vestibules for the next trial
    /// </summary>
    public void resetData()
    {

        //reset the rest of the data collection UI
        stability.slider.interactable = true;
        foreach (Button i in buttons)
        {
            i.OnDeselect(null);
            i.interactable = true;
        }

        //direction.setZoneChoice(-1);

        stability.slider.value = 4;

        rotation.resetButton.interactable = true;
        rotation.spinClockwise.interactable = true;
        rotation.spinCounter.interactable = true;
        time = Time.time;

        vertBlock.SetActive(true);
        horBlock.SetActive(true);
        vert2.SetActive(true);
        ho2.SetActive(true);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(drawAvg, drawAvg + new Vector3(0, 3, 0));
        Gizmos.DrawSphere(centerOfMass, .1f);
        Gizmos.DrawCube(furthestXZ, new Vector3(.1f, .1f, .1f));
    }


    /// <summary>
    /// Add the data to the .txt file in a new line
    /// </summary>
    public void WriteToFile()
    {

        //this works pretty well, check the data files if you are confused about anything and it should clear things up for you

        float timeToSubmit = Time.time - time;
        Debug.Log(timeToSubmit);
        //write the file. first need to check and see if the file exists.
        List<Vector3> combinedCenterOfMass = CalculateStackedCentersOfMass();

        //float[] dataToWrite = new float[] { zone, stable, drawAvg,  };
        int[] userData = new int[] {zone, (int)stable};
        var fileName = gm.userName + "Data.txt";
        string path = @"D:\VR_tower\IntPhysTowersExperimentData\" + fileName;

        if (!File.Exists(path))
        {
            Debug.Log(fileName + " does not exist, initializing document");
            using (StreamWriter sw = File.CreateText(path))
            {
                //write the header for the datablockCentersString
                sw.WriteLine(fileName);
                sw.WriteLine("Zone1 \tZone2 \tStability \tTime \tAvg Pos \tFurthest \tCenterOfMass \tY Rotation \tMajority \tStart Pos \tFinal Pos \t combinedCenterOfMass \tRotationEvent");
            }
        }

        string startPosString = "{";
        string finalPosString = "{";
        for(int rb = 0; rb < startingPositions.Length; rb++)
        {
            startPosString += startingPositions[rb].ToString();
            finalPosString += finalPositions[rb].ToString();

            if(rb < startingPositions.Length - 1)
            {
                startPosString += ", ";
                finalPosString += ", ";
            }
            else
            {
                startPosString += "}";
                finalPosString += "}";
            }
        }

        string majorityString = String.Format("[{0}, {1}, {2}, {3}, {4}]", zoneMajority[0], zoneMajority[1], zoneMajority[2], zoneMajority[3], zoneMajority[4]);

        // rotation log 
        string rotationEventsString = "{";
        for (int i = 0; i < rotationEvents.Count; i++)
        {
            rotationEventsString += rotationEvents[i].ToString();
            if (i < rotationEvents.Count - 1)
            {
                rotationEventsString += ", ";
            }
        }
        rotationEventsString += "}";

        // combinedCenterOfMass Log
        string combinedCenterOfMassString = "{";
        for (int i = 0; i < combinedCenterOfMass.Count; i++)
        {
            combinedCenterOfMassString += combinedCenterOfMass[i].ToString();
            if (i < combinedCenterOfMass.Count - 1)
            {
                combinedCenterOfMassString += ", ";
            }
        }
        combinedCenterOfMassString += "}";



        Debug.Log("Appending " + fileName);
        using (StreamWriter sw = File.AppendText(path))
        {
            //preformatted so that it can be pasted into a spreadsheet easily.
            sw.WriteLine(zone + "\t" + zone2 + "\t" + stable + "\t" + timeToSubmit + "\t" + drawAvg.ToString() + "\t" + furthestXZ.ToString() + "\t" + centerOfMass.ToString() + "\t" + yRotFinal + "\t" + majorityString + 
                "\t" + startPosString + "\t" + finalPosString + "\t" + combinedCenterOfMassString + "\t" + rotationEventsString);
        }

        // Clear rotation events after logging
        rotationEvents.Clear();
        combinedCenterOfMassLogs.Clear();
    }
}
