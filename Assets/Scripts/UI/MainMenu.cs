using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    private struct QuaternionPair
    {
        public Quaternion Yaw;
        public Quaternion Pitch;
    }

    public Collider playButtonCollider;
    public Collider exitButtonCollider;
    public Collider helpCollider;

    public GameObject InstructionsPanel;

    public Transform yawTransform;
    public Transform pitchTransform;

    private Quaternion startingYawRotation;
    private Quaternion startingPitchRotation;

    public float yawOffset = 0;
    public float pitchOffset = 0;
    public Image fadeImage;

    void Start()
    {
        startingYawRotation = yawTransform.localRotation;
        startingPitchRotation = pitchTransform.localRotation;
        InstructionsPanel.SetActive(false);
    }

    QuaternionPair GetLookAtQuaternions(Vector3 target)
    {
        Vector3 direction = target - yawTransform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        Vector3 euler = lookRotation.eulerAngles;
        return new QuaternionPair
        {
            Yaw = Quaternion.Euler(0, 0, yawOffset - euler.y),
            Pitch = Quaternion.Euler(0, pitchOffset - euler.x, 0)
        };
    }

    void LookAt(QuaternionPair lookQuaternions)
    {
        yawTransform.localRotation = Quaternion.Slerp(
            yawTransform.localRotation,
            startingYawRotation * lookQuaternions.Yaw,
            Time.deltaTime * 5.0f
        );

        pitchTransform.localRotation = Quaternion.Slerp(
            pitchTransform.localRotation,
            startingPitchRotation * lookQuaternions.Pitch,
            Time.deltaTime * 5.0f
        );
    }

    void Update()
    {
        RaycastHit hit;
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);

        if (hit.collider != null && hit.collider == playButtonCollider)
        {
            QuaternionPair lookQuaternions = GetLookAtQuaternions(playButtonCollider.transform.position);
            LookAt(lookQuaternions);
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Play Button Clicked");
                StartCoroutine(LoadScene());
            }
            InstructionsPanel.SetActive(false);
        }
        else if (hit.collider != null && hit.collider == exitButtonCollider)
        {
            QuaternionPair lookQuaternions = GetLookAtQuaternions(exitButtonCollider.transform.position);
            LookAt(lookQuaternions);
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Exit Button Clicked");
                Application.Quit();
            }
            InstructionsPanel.SetActive(false);
        }
        else if (hit.collider != null && hit.collider == helpCollider)
        {
            QuaternionPair lookQuaternions = GetLookAtQuaternions(helpCollider.transform.position);
            LookAt(lookQuaternions);
            InstructionsPanel.SetActive(true);
        }
        else {
            QuaternionPair lookQuaternions = new QuaternionPair
            {
                Yaw = startingYawRotation,
                Pitch = startingPitchRotation
            };
            LookAt(lookQuaternions);
            InstructionsPanel.SetActive(false);
        }
    }

    IEnumerator LoadScene()
    {
        float fadeDuration = 2.0f;
        float timer = 0.0f;
        Color initialColor = Color.clear;
        Color targetColor = Color.black;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeImage.color = Color.Lerp(initialColor, targetColor, timer / fadeDuration);
            yield return new WaitForSeconds(0.01f);
        }

        // Load new scene
        Debug.Log("Loading Main Scene...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Area1");
    }
}
