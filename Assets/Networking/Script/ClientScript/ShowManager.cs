using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class ShowManager : MonoBehaviour
{
    public List<Vector3> CameraPoint = new List<Vector3>();

    public Vector3 GirlEndPoint;
    public Vector3 StudentEndPoint;

    public float GirlSpeed;
    public float StudentSpeed;

    public GameObject Girl;
    public GameObject Student;

    public GameObject GirlSpeech;
    public GameObject StudentSpeech;

    public Animator GirlAnimator;
    public Animator StudentAnimator;

    public float MaxIntensity;
    public float IntensityForce;
    public PostProcessVolume Volume;
    public Bloom Bloom;


    public void Start()
    {
        Camera.main.transform.position = CameraPoint[GameObject.FindObjectOfType<Player>().MyType - 1];

        StartCoroutine(GirlAnimation());
        StartCoroutine(StudentAnimation());

        Volume.profile.TryGetSettings(out Bloom);
    }

    public IEnumerator GirlAnimation()
    {
        yield return new WaitForSeconds(2.5f);

        GirlAnimator.SetBool("RightWalk", true);
        GirlAnimator.SetBool("RightIdle", false);

        while (Girl.transform.position.x != GirlEndPoint.x)
        {
            yield return null;
            Girl.transform.position = Vector3.MoveTowards(Girl.transform.position, GirlEndPoint, GirlSpeed * Time.deltaTime);
        }

        GirlAnimator.SetBool("RightWalk", false);
        GirlAnimator.SetBool("RightIdle", false);

        yield return new WaitForSeconds(1f);
        Girl.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.3f);

        GirlSpeech.SetActive(true);

        yield break;
    }

    public IEnumerator StudentAnimation()
    {
        yield return new WaitForSeconds(2.5f);

        StudentAnimator.SetBool("RightWalk", true);
        StudentAnimator.SetBool("RightIdle", false);

        while (Student.transform.position.x != StudentEndPoint.x)
        {
            yield return null;
            Student.transform.position = Vector3.MoveTowards(Student.transform.position, StudentEndPoint, StudentSpeed * Time.deltaTime);
        }

        StudentAnimator.SetBool("RightWalk", false);
        StudentAnimator.SetBool("RightIdle", false);

        yield return new WaitForSeconds(1f);
        Student.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.3f);

        StudentSpeech.SetActive(true);

        yield return new WaitForSeconds(1.0f);
        while (Bloom.intensity.value <= MaxIntensity)
        {
            yield return null;
            Bloom.intensity.value += IntensityForce * Time.deltaTime;
        }

        yield break;
    }
}
