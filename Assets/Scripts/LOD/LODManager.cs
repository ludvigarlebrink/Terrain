using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System;
using UnityEngine;

namespace Name.Lod
{
    public enum TestMode
    {
        LOD,
        LODFade,
        LODDither,
        NoLOD,
        EmptyScene
    }
}


namespace Name.Lod
{
    public class TestData
    {
        public int numFrames = 0;
        public int numFramesUnder60 = 0;
        public int numFramesUnder30 = 0;
        public int numFramesUnder15 = 0;
        public float frameTime = 0.0f;
        public float worstFrameTime = float.MinValue;
        public float bestFrameTime = float.MaxValue;
        public float testTime = 0.0f;
        public long totalMemory = 0;
        public long lowestMemory = long.MaxValue;
        public long highestMemory = long.MinValue;
    }
}

namespace Name.Lod
{
    public class LODManager : MonoBehaviour
    {

        #region Public Variables
        public float lod0 = 0.5f;
        public float lod1 = 0.25f;
        public float lod2 = 0.125f;
        public float lod3 = 0.075f;
        public float lod4 = 0.01f;
        public float delay = 10.0f;
        public float spacing = 1.0f;
        public float speed = 2.0f;
        public int xSize = 10;
        public int ySize = 10;
        public int zSize = 10;
        public int numLaps = 10;
        public GameObject lodObject = null;
        public GameObject lodFadeObject = null;
        public GameObject lodDitherObject = null;
        public GameObject noLodObject = null;
        public Transform target = null;
        public CameraPath cameraPath = null;
        #endregion

        #region Private Variables
        private bool initialized = true;
        private float counter = 0.0f;
        private int visited = -1;
        private int currentLap = 0;
        private Transform targetPoint;
        private Transform cam = null;
        private TestMode currentMode = TestMode.LOD;

        private TestData lodData = new TestData();
        private TestData lodFadeData = new TestData();
        private TestData lodDitherData = new TestData();
        private TestData noLodData = new TestData();
        private TestData emptyData = new TestData();

        private DateTime localTime = DateTime.Now;
        private string timeString = "";
        #endregion

        #region Private Functions
        private void Start()
        {
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/LODTest"))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/LODTest");
            }

            localTime = DateTime.Now;
            timeString = localTime.Year.ToString() + "-" + localTime.Month.ToString() + "-" + localTime.Day.ToString() + "-" + localTime.Hour.ToString() + "h-" + localTime.Minute.ToString() + "min";

            cam = Camera.main.transform;
            InitTest();
        }

        private void Update()
        {
            // Discard bad frames due to initialization.
            if (initialized)
            {
                switch (currentMode)
                {
                    case TestMode.LOD:
                        ScreenCapture.CaptureScreenshot(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/LODTest/LOD" + timeString + ".png");
                        break;
                    case TestMode.LODFade:
                        ScreenCapture.CaptureScreenshot(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/LODTest/LODFade" + timeString + ".png");
                        break;
                    case TestMode.LODDither:
                        ScreenCapture.CaptureScreenshot(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/LODTest/LODDither" + timeString + ".png");
                        break;
                    case TestMode.NoLOD:
                        ScreenCapture.CaptureScreenshot(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/LODTest/NoLOD" + timeString + ".png");
                        break;
                    case TestMode.EmptyScene:
                        break;
                    default:
                        break;
                }

                initialized = false;
                counter = delay;
                return;
            }

            if (counter > 0.0001f)
            {
                counter -= Time.deltaTime;
                return;
            }

            if (visited >= cameraPath.pathObjs.Count - 1)
            {
                ++currentLap;
                if (currentLap > numLaps)
                {
                    switch (currentMode)
                    {
                        case TestMode.LOD:
                            currentMode = TestMode.LODFade;
                            InitTest();
                            return;
                        case TestMode.LODFade:
                            currentMode = TestMode.LODDither;
                            InitTest();
                            return;
                        case TestMode.LODDither:
                            currentMode = TestMode.NoLOD;
                            InitTest();
                            return;
                        case TestMode.NoLOD:
                            currentMode = TestMode.EmptyScene;
                            InitTest();
                            return;
                        case TestMode.EmptyScene:
                            ExitTest();
                            return;
                        default:
                            return;
                    }
                }

                visited = 0;
            }

            switch (currentMode)
            {
                case TestMode.LOD:
                    UpdateData(lodData);
                    break;
                case TestMode.LODFade:
                    UpdateData(lodFadeData);
                    break;
                case TestMode.LODDither:
                    UpdateData(lodDitherData);
                    break;
                case TestMode.NoLOD:
                    UpdateData(noLodData);
                    break;
                case TestMode.EmptyScene:
                    UpdateData(emptyData);
                    break;
                default:
                    break;
            }

            cam.LookAt(target);
            cam.position = Vector3.MoveTowards(cam.position, targetPoint.position, speed * Time.deltaTime);
            if (Vector3.Distance(cam.position, targetPoint.position) < 0.00001f)
            {
                ++visited;
                targetPoint = cameraPath.pathObjs[visited];
            }
        }

        private void InitTest()
        {
            visited = 0;
            currentLap = 0;
            targetPoint = cameraPath.pathObjs[0];
            cam.position = targetPoint.position;
            cam.LookAt(target);

            for (int i = 0; i < transform.childCount; ++i)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            if (currentMode == TestMode.EmptyScene)
            {
                initialized = true;
                return;
            }

            // Collect garbage.
            GC.Collect();

            for (int x = 0; x < xSize; ++x)
            {
                for (int y = 0; y < ySize; ++y)
                {
                    for (int z = 0; z < zSize; ++z)
                    {
                        GameObject obj = null;
                        LOD[] LODs = null;
                        switch (currentMode)
                        {
                            case TestMode.LOD:
                                obj = Instantiate(lodObject, new Vector3(x * spacing, y * spacing, z * spacing), Quaternion.identity);
                                obj.transform.parent = gameObject.transform;
                                LODs = obj.GetComponent<LODGroup>().GetLODs();
                                LODs[0].screenRelativeTransitionHeight = lod0;
                                LODs[1].screenRelativeTransitionHeight = lod1;
                                LODs[2].screenRelativeTransitionHeight = lod2;
                                LODs[3].screenRelativeTransitionHeight = lod3;
                                LODs[4].screenRelativeTransitionHeight = lod4;

                                obj.GetComponent<LODGroup>().SetLODs(LODs);
                                obj.GetComponent<LODGroup>().RecalculateBounds();
                                break;
                            case TestMode.LODFade:
                                obj = Instantiate(lodFadeObject, new Vector3(x * spacing, y * spacing, z * spacing), Quaternion.identity);
                                obj.transform.parent = gameObject.transform;
                                LODs = obj.GetComponent<LODGroup>().GetLODs();
                                LODs[0].screenRelativeTransitionHeight = lod0;
                                LODs[1].screenRelativeTransitionHeight = lod1;
                                LODs[2].screenRelativeTransitionHeight = lod2;
                                LODs[3].screenRelativeTransitionHeight = lod3;
                                LODs[4].screenRelativeTransitionHeight = lod4;

                                obj.GetComponent<LODGroup>().SetLODs(LODs);
                                obj.GetComponent<LODGroup>().RecalculateBounds();
                                break;
                            case TestMode.LODDither:
                                obj = Instantiate(lodDitherObject, new Vector3(x * spacing, y * spacing, z * spacing), Quaternion.identity);
                                obj.transform.parent = gameObject.transform;
                                LODs = obj.GetComponent<LODGroup>().GetLODs();
                                LODs[0].screenRelativeTransitionHeight = lod0;
                                LODs[1].screenRelativeTransitionHeight = lod1;
                                LODs[2].screenRelativeTransitionHeight = lod2;
                                LODs[3].screenRelativeTransitionHeight = lod3;
                                LODs[4].screenRelativeTransitionHeight = lod4;

                                obj.GetComponent<LODGroup>().SetLODs(LODs);
                                obj.GetComponent<LODGroup>().RecalculateBounds();
                                break;
                            case TestMode.NoLOD:
                                obj = Instantiate(noLodObject, new Vector3(x * spacing, y * spacing, z * spacing), Quaternion.identity);
                                obj.transform.parent = gameObject.transform;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            initialized = true;
        }

        private void UpdateData(TestData data)
        {
            ++data.numFrames;
            data.frameTime += Time.deltaTime;
            if (data.worstFrameTime < Time.deltaTime)
            {
                data.worstFrameTime = Time.deltaTime;
            }

            if (data.bestFrameTime > Time.deltaTime)
            {
                data.bestFrameTime = Time.deltaTime;
            }

            if (Time.deltaTime * 1000.0f > 16.667f)
            {
                ++data.numFramesUnder60;
            }

            if (Time.deltaTime * 1000.0f > 33.333f)
            {
                ++data.numFramesUnder30;
            }

            if (Time.deltaTime * 1000.0f > 66.667f)
            {
                ++data.numFramesUnder15;
            }

            data.testTime += Time.deltaTime;

            long memory = GC.GetTotalMemory(true);

            data.totalMemory += memory;

            if (data.highestMemory < memory)
            {
                data.highestMemory = memory;
            }

            if (data.lowestMemory > memory)
            {
                data.lowestMemory = memory;
            }
        }

        private void ExitTest()
        {
            string text = "-- GENERAL --\n\n";

            text += "Date and time:              " + localTime.ToString(new CultureInfo("en-GB")) + "\n";

            text += "\n";

            text += "LOD LEVELS\n";
            text += "LOD 0:                      " + lod0.ToString() + "\n";
            text += "LOD 1:                      " + lod1.ToString() + "\n";
            text += "LOD 2:                      " + lod2.ToString() + "\n";
            text += "LOD 3:                      " + lod3.ToString() + "\n";
            text += "LOD 4:                      " + lod4.ToString() + "\n";

            text += "\n";

            text += "Number of bunnies:          " + (xSize * ySize * zSize).ToString() + "\n";
            text += "Camera speed:               " + speed.ToString() + "\n";
            text += "Number of laps:             " + numLaps.ToString() + "\n";

            text += "\n\n";

            text += "-- TESTS --\n\n";

            text += "LOD TEST\n";
            text += TextData(lodData);
            text += "\n";

            text += "LOD FADE TEST\n";
            text += TextData(lodFadeData);
            text += "\n";

            text += "LOD DITHER TEST\n";
            text += TextData(lodDitherData);
            text += "\n";

            text += "NO LOD TEST\n";
            text += TextData(noLodData);
            text += "\n";

            text += "EMPTY SCENE TEST\n";
            text += TextData(emptyData);

            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/LODTest/test" + timeString + ".txt", text);
            Debug.Log("DONE!");

            Application.Quit();
        }

        private string TextData(TestData data)
        {
            string text = "";

            // Test time.
            text += "Test time:                  ";
            text += data.testTime.ToString() + " s\n";

            // Frame time.
            text += "Average frame time:         ";
            text += ((data.frameTime / data.numFrames) * 1000).ToString() + " ms\n";
            text += "Best frame time:            ";
            text += (data.bestFrameTime * 1000.0f).ToString() + " ms\n";
            text += "Worst frame time:           ";
            text += (data.worstFrameTime * 1000.0f).ToString() + " ms\n";
            text += "Number of frames:           ";
            text += data.numFrames.ToString() + "\n";
            text += "Number Frames under 60:     ";
            text += data.numFramesUnder60.ToString() + "\n";
            text += "Number Frames under 30:     ";
            text += data.numFramesUnder30.ToString() + "\n";
            text += "Number Frames under 15:     ";
            text += data.numFramesUnder15.ToString() + "\n";
            text += "Frames under 60 percentage: ";
            text += ((data.numFramesUnder60 / (float)data.numFrames) * 100.0f).ToString() + "%\n";
            text += "Frames under 30 percentage: ";
            text += ((data.numFramesUnder30 / (float)data.numFrames) * 100.0f).ToString() + "%\n";
            text += "Frames under 15 percentage: ";
            text += ((data.numFramesUnder15 / (float)data.numFrames) * 100.0f).ToString() + "%\n";

            // Memory.
            text += "Average memory:             ";
            text += (data.totalMemory / data.numFrames).ToString() + " bytes\n";
            text += "Highest memory used:        ";
            text += data.highestMemory.ToString() + " bytes\n";
            text += "Lowest memory used:         ";
            text += data.lowestMemory.ToString() + " bytes\n";
            return text;
        }
        #endregion
    }

}
