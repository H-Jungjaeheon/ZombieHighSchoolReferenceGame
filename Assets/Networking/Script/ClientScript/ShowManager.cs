using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class ShowManager : MonoBehaviour
{
    [Header("ServerState")]
    public static bool ServerStart = false;

    public bool BloomOut = false;
    public bool WaitPlayer = false;
    public bool GirlEnd = false;
    public bool StudentEnd = false;

    [Header("Camera")]
    public List<Vector3> CameraPoint = new List<Vector3>();
    public float CameraSize = 10.0f;
    public float CameraMoveSpeed = 5.0f;

    [Header("Object")]
    public GameObject Bokdo;
    public GameObject GirlRoom;
    public GameObject StudentRoom;

    [Header("Move Point")]
    public Vector3 GirlEndPoint;
    public Vector3 StudentEndPoint;
    public Vector3 GirlStartPoint;
    public Vector3 StudentStartPoint;
    public Vector3 GirlBokdoStartPoint;
    public Vector3 StudentBokdoStartPoint;
    public Vector3 GirlBokdoEndPoint;
    public Vector3 StudentBokdoEndPoint;

    [Header("Player Attribute")]
    public float GirlSpeed;
    public float StudentSpeed;

    [Header("Player")]
    public GameObject Girl;
    public GameObject Student;

    [Header("Speech Sleep")]
    public GameObject GirlSpeech;
    public GameObject StudentSpeech;

    [Header("Speech Wow")]
    public GameObject GirlSpeechWow;
    public GameObject StudentSpeechWow;

    [Header("Speech Happy")]
    public GameObject GirlSpeechHappy;
    public GameObject StudentSpeechHappy;

    [Header("Animator")]
    public Animator GirlAnimator;
    public Animator StudentAnimator;

    [Header("Postprocess")]
    public float MaxIntensity;
    public float MinIntensity;
    public float IntensityForce;
    public PostProcessVolume Volume;
    public Bloom Bloom;
    public Vignette Vignette;


    public void Start()
    {
        Camera.main.transform.position = CameraPoint[GameObject.FindObjectOfType<Player>().MyType - 1];
        Volume.profile.TryGetSettings(out Bloom);
        Volume.profile.TryGetSettings(out Vignette);

        if (ServerStart == false)
        {
            StartCoroutine(StartGirlAnimation());
            StartCoroutine(StartStudentAnimation());

            ServerStart = true;
        }

        else
        {
            StartCoroutine(EndGirlAnimation());
            StartCoroutine(EndStudentAnimation());

            ServerStart = false;
        }
    }

    public IEnumerator StartGirlAnimation()
    {
        GirlRightIdle();

        yield return new WaitForSeconds(2.5f);

        GirlRightWalk();

        while (Girl.transform.position.x != GirlEndPoint.x)
        {
            yield return null;
            Girl.transform.position = Vector3.MoveTowards(Girl.transform.position, GirlEndPoint, GirlSpeed * Time.deltaTime);
        }

        GirlBackIdle();

        yield return new WaitForSeconds(1f);
        Girl.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.3f);

        GirlSpeech.SetActive(true);

        yield break;
    }

    public IEnumerator StartStudentAnimation()
    {
        StudentRightIdle();

        yield return new WaitForSeconds(2.5f);

        StudentRightWalk();

        while (Student.transform.position.x != StudentEndPoint.x)
        {
            yield return null;
            Student.transform.position = Vector3.MoveTowards(Student.transform.position, StudentEndPoint, StudentSpeed * Time.deltaTime);
        }

        StudentBackIdle();

        yield return new WaitForSeconds(1f);
        Student.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.3f);

        StudentSpeech.SetActive(true);
        yield return new WaitForSeconds(1.0f);

        while(Camera.main.transform.position.y != CameraPoint[2].y && Camera.main.orthographicSize < CameraSize)
        {
            yield return null;

            Camera.main.transform.position = Vector3.MoveTowards(Camera.main.transform.position, CameraPoint[2], CameraMoveSpeed * Time.deltaTime);
            if(Camera.main.orthographicSize < CameraSize)
            {
                Camera.main.orthographicSize += CameraMoveSpeed * Time.deltaTime;
            }
        }
        yield return new WaitForSeconds(1.3f);

        while (Bloom.intensity.value <= MaxIntensity)
        {
            yield return null;
            Bloom.intensity.value += IntensityForce * Time.deltaTime;
        }

        //TODO: Fix
        SceneManager.LoadScene("EndShowScene");
        yield break;
    }

    public IEnumerator EndGirlAnimation()
    {
        Girl.SetActive(true);

        while(BloomOut == false)
        {
            yield return null;
        }

        GirlRightIdle();
        yield return new WaitForSeconds(0.5f);
        GirlLeftIdle();
        yield return new WaitForSeconds(0.5f);
        GirlRightIdle();
        yield return new WaitForSeconds(0.5f);
        GirlFrontIdle();
        yield return new WaitForSeconds(0.5f);
        GirlSpeechWow.SetActive(true);
        yield return new WaitForSeconds(0.7f);
        GirlSpeechWow.SetActive(false);
        GirlLeftWalk();

        while (Girl.transform.position.x != GirlStartPoint.x)
        {
            yield return null;
            Girl.transform.position = Vector3.MoveTowards(Girl.transform.position, GirlStartPoint, GirlSpeed * Time.deltaTime);
        }

        GirlBackIdle();

        yield return new WaitForSeconds(0.8f);

        Girl.gameObject.SetActive(false);

        while (WaitPlayer == false)
            yield return null;

        GirlLeftWalk();
        while (Girl.transform.position.x != GirlBokdoEndPoint.x)
        {
            yield return null;
            Girl.transform.position = Vector3.MoveTowards(Girl.transform.position, GirlBokdoEndPoint, GirlSpeed * Time.deltaTime);
        }

        GirlEnd = true;
        GirlLeftIdle();


        yield break;
    }

    public IEnumerator EndStudentAnimation()
    {
        Student.SetActive(true);

        yield return new WaitForSeconds(1.0f);
        while (Bloom.intensity.value >= MinIntensity)
        {
            yield return null;
            Bloom.intensity.value -= IntensityForce * Time.deltaTime;
        }

        BloomOut = true;

        StudentRightIdle();
        yield return new WaitForSeconds(0.5f);
        StudentLeftIdle();
        yield return new WaitForSeconds(0.5f);
        StudentRightIdle();
        yield return new WaitForSeconds(0.5f);
        StudentFrontIdle();
        yield return new WaitForSeconds(0.5f);
        StudentSpeechWow.SetActive(true);
        yield return new WaitForSeconds(0.7f);
        StudentSpeechWow.SetActive(false);
        StudentLeftWalk();

        while (Student.transform.position.x != StudentStartPoint.x)
        {
            yield return null;
            Student.transform.position = Vector3.MoveTowards(Student.transform.position, StudentStartPoint, StudentSpeed * Time.deltaTime);
        }

        StudentBackIdle();

        yield return new WaitForSeconds(0.8f);

        Student.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.8f);

        UIManager.Instance.FadeImage.gameObject.SetActive(true);
        UIManager.Instance.FadeState = Fade.In;
        yield return new WaitForSeconds(2.5f);

        Camera.main.gameObject.transform.position = CameraPoint[0];

        GirlLeftIdle();
        Girl.transform.position = GirlBokdoStartPoint;
        Girl.SetActive(true);
        StudentRightIdle();
        Student.transform.position = StudentBokdoStartPoint;
        Student.SetActive(true);

        GirlRoom.SetActive(false);
        StudentRoom.SetActive(false);
        Bokdo.SetActive(true);

        UIManager.Instance.FadeState = Fade.Out;
        yield return new WaitForSeconds(2.5f);

        WaitPlayer = true;

        StudentRightWalk();
        while(Student.transform.position.x != StudentBokdoEndPoint.x)
        {
            yield return null;
            Student.transform.position = Vector3.MoveTowards(Student.transform.position, StudentBokdoEndPoint, StudentSpeed * Time.deltaTime);
        }

        StudentEnd = true;
        StudentRightIdle();

        while (GirlEnd == false && StudentEnd == false)
            yield return null;

        yield return new WaitForSeconds(0.8f);
        GirlSpeechHappy.SetActive(true);
        StudentSpeechHappy.SetActive(true);
        yield return new WaitForSeconds(0.5f);

        UIManager.Instance.FadeState = Fade.In;

        yield break;
    }

    #region GirlAnimator
    private void GirlFrontIdle()
    {
        GirlAnimator.SetBool("RightIdle", false);
        GirlAnimator.SetBool("RightWalk", false);
        GirlAnimator.SetBool("LeftIdle", false);
        GirlAnimator.SetBool("LeftWalk", false);
        GirlAnimator.SetBool("FrontIdle", true);
        GirlAnimator.SetBool("BackIdle", false);
    }

    private void GirlBackIdle()
    {
        GirlAnimator.SetBool("RightIdle", false);
        GirlAnimator.SetBool("RightWalk", false);
        GirlAnimator.SetBool("LeftIdle", false);
        GirlAnimator.SetBool("LeftWalk", false);
        GirlAnimator.SetBool("FrontIdle", false);
        GirlAnimator.SetBool("BackIdle", true);
    }

    private void GirlLeftIdle()
    {
        GirlAnimator.SetBool("RightIdle", false);
        GirlAnimator.SetBool("RightWalk", false);
        GirlAnimator.SetBool("LeftIdle", true);
        GirlAnimator.SetBool("LeftWalk", false);
        GirlAnimator.SetBool("FrontIdle", false);
        GirlAnimator.SetBool("BackIdle", false);
    }

    private void GirlRightIdle()
    {
        GirlAnimator.SetBool("RightIdle", true);
        GirlAnimator.SetBool("RightWalk", false);
        GirlAnimator.SetBool("LeftIdle", false);
        GirlAnimator.SetBool("LeftWalk", false);
        GirlAnimator.SetBool("FrontIdle", false);
        GirlAnimator.SetBool("BackIdle", false);
    }

    private void GirlLeftWalk()
    {
        GirlAnimator.SetBool("RightIdle", false);
        GirlAnimator.SetBool("RightWalk", false);
        GirlAnimator.SetBool("LeftIdle", false);
        GirlAnimator.SetBool("LeftWalk", true);
        GirlAnimator.SetBool("FrontIdle", false);
        GirlAnimator.SetBool("BackIdle", false);
    }

    private void GirlRightWalk()
    {
        GirlAnimator.SetBool("RightIdle", false);
        GirlAnimator.SetBool("RightWalk", true);
        GirlAnimator.SetBool("LeftIdle", false);
        GirlAnimator.SetBool("LeftWalk", false);
        GirlAnimator.SetBool("FrontIdle", false);
        GirlAnimator.SetBool("BackIdle", false);
    }
    #endregion

    #region StudentAnimator
    private void StudentFrontIdle()
    {
        StudentAnimator.SetBool("RightIdle", false);
        StudentAnimator.SetBool("RightWalk", false);
        StudentAnimator.SetBool("LeftIdle", false);
        StudentAnimator.SetBool("LeftWalk", false);
        StudentAnimator.SetBool("FrontIdle", true);
        StudentAnimator.SetBool("BackIdle", false);
    }

    private void StudentBackIdle()
    {
        StudentAnimator.SetBool("RightIdle", false);
        StudentAnimator.SetBool("RightWalk", false);
        StudentAnimator.SetBool("LeftIdle", false);
        StudentAnimator.SetBool("LeftWalk", false);
        StudentAnimator.SetBool("FrontIdle", false);
        StudentAnimator.SetBool("BackIdle", true);
    }

    private void StudentLeftIdle()
    {
        StudentAnimator.SetBool("RightIdle", false);
        StudentAnimator.SetBool("RightWalk", false);
        StudentAnimator.SetBool("LeftIdle", true);
        StudentAnimator.SetBool("LeftWalk", false);
        StudentAnimator.SetBool("FrontIdle", false);
        StudentAnimator.SetBool("BackIdle", false);
    }

    private void StudentRightIdle()
    {
        StudentAnimator.SetBool("RightIdle", true);
        StudentAnimator.SetBool("RightWalk", false);
        StudentAnimator.SetBool("LeftIdle", false);
        StudentAnimator.SetBool("LeftWalk", false);
        StudentAnimator.SetBool("FrontIdle", false);
        StudentAnimator.SetBool("BackIdle", false);
    }

    private void StudentLeftWalk()
    {
        StudentAnimator.SetBool("RightIdle", false);
        StudentAnimator.SetBool("RightWalk", false);
        StudentAnimator.SetBool("LeftIdle", false);
        StudentAnimator.SetBool("LeftWalk", true);
        StudentAnimator.SetBool("FrontIdle", false);
        StudentAnimator.SetBool("BackIdle", false);
    }

    private void StudentRightWalk()
    {
        StudentAnimator.SetBool("RightIdle", false);
        StudentAnimator.SetBool("RightWalk", true);
        StudentAnimator.SetBool("LeftIdle", false);
        StudentAnimator.SetBool("LeftWalk", false);
        StudentAnimator.SetBool("FrontIdle", false);
        StudentAnimator.SetBool("BackIdle", false);
    }
    #endregion
}
