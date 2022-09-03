using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float jumpForce;
    public float secondJumpForce;
    public float gravityModifier;
    public bool gameOver;

    public GameObject gameOverScreen;
    public TextMeshProUGUI scoreText;
    public ParticleSystem explosionParticle;
    public ParticleSystem dirtParticle;
    public AudioClip jumpSound;
    public AudioClip crashSound;
    private AudioSource playerAudio;
    private Animator playerAnim;
    private Rigidbody playerRb;

    public bool doubleSpeed = false;
    private int jumpCount = 0;
    private int score = 0;
    private float lerpSpeed = 5.0f;


    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        playerAnim = GetComponent<Animator>();
        playerAudio = GetComponent<AudioSource>();
        Physics.gravity *= gravityModifier;
        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(0, 0, 0);
        float journeyLength = Vector3.Distance(startPos, endPos);
        float startTime = Time.time;
        float distanceCovered = (Time.time - startTime) * lerpSpeed;
        float fractionOfJourney = distanceCovered / journeyLength;
        playerAnim.SetFloat("Speed_Multiplier", 0.5f);
        while (fractionOfJourney < 1)
        {
            distanceCovered = (Time.time - startTime) * lerpSpeed;
            fractionOfJourney = distanceCovered / journeyLength;
            transform.position = Vector3.Lerp(startPos, endPos, fractionOfJourney);
            yield return null;
        }
        playerAnim.SetFloat("Speed_Multiplier", 1.0f);
        gameOver = false;
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && !gameOver)
        {
            doubleSpeed = true;
            playerAnim.SetFloat("Speed_Multiplier", 2.0f);
        }
        else if (doubleSpeed)
        {
            doubleSpeed = false;
            playerAnim.SetFloat("Speed_Multiplier", 1.0f);
        }

        if (!gameOver)
        {
            if (doubleSpeed)
            {
                score += 2;
            }
            else
            {
                score++;
            }
            scoreText.text = "Score: " + score.ToString();
        }


        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < 2 && !gameOver)
        {
            jumpCount++;
            playerAudio.PlayOneShot(jumpSound, 1.0f);
            dirtParticle.Stop();
            if(jumpCount == 1)
            {
                playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
            else
            {
                playerRb.AddForce(Vector3.up * secondJumpForce, ForceMode.Impulse);
            }
            playerAnim.SetTrigger("Jump_trig");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            jumpCount = 0;
            dirtParticle.Play();
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            StartCoroutine(GameOver());
        }
    }

    IEnumerator GameOver()
    {
        gameOver = true;
        playerAudio.PlayOneShot(crashSound, 1.0f);
        dirtParticle.Stop();
        explosionParticle.Play();
        playerAnim.SetBool("Death_b", true);
        playerAnim.SetInteger("DeathType_int", 1);
        yield return new WaitForSeconds(1.0f);
        gameOverScreen.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
