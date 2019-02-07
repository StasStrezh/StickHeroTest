using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameController : MonoBehaviour
{
    #region const
    private const float FALL_STICK_SPEED = 4.5f;
    private const float HERO_SPEED = 3f;
    private const float LEFT_MARGIN = -2.5f;
    private const float MAX_STICK_SCALE = 550f;

    private Vector3 SPAWN_VECTOR = new Vector3(5f, -3f, 0f);
    #endregion


    #region SerializeField
    [SerializeField]
    private Text scoreLabel;
    [SerializeField]
    private Text bestScoreLabel;
    [SerializeField]
    private GameObject points;
    [SerializeField]
    private GameObject restartDialog;
    [SerializeField]
    private GameObject startDialoge;
    [SerializeField]
    private GameObject stickPrefab;
    [SerializeField]
    public GameObject hero;
    [SerializeField]
    private AudioManager audioManager;
    #endregion


    #region private_fields
    private Animator heroAnimator;
    private GameObject stick;
    private Transform prevColumn;
    private Transform nextColumn;
    private Transform currentColumn;
    private Transform heroTransform;
    private Transform rootHero;


    private bool isRotateStick = false;
    private bool isHit = false;
    private bool isReady = true;

    private float angle = 0f;
    private float heroPosX;
    private float heroPosY = -0.524f;

    private int groundLayer;
    private int bonusLayer;
    private int stickLayer;

    private Vector3 currentColPosition;
    private Vector3 nextColPosition;
    private Vector3 prevColPosition;
    private Vector3 heroPosition;
    #endregion


    #region public_fields
    public bool isComplete = false;
    public bool isApply = false;
    #endregion


    #region property
    private HeroState StateAnimation
    {
        get { return (HeroState)heroAnimator.GetInteger("State"); }
        set { heroAnimator.SetInteger("State", (int)value); }
    }
    #endregion


    #region methods
    void Start()
    {
        bonusLayer = LayerMask.GetMask("Bonus");
        groundLayer = LayerMask.GetMask("Ground");
        stickLayer = LayerMask.GetMask("Stick");

        heroAnimator = hero.GetComponent<Animator>();
        heroTransform = hero.transform;
        rootHero = heroTransform.Find("root");

        currentColumn = GameObject.Find("Col1").GetComponent<Transform>();
        nextColumn = GameObject.Find("Col2").GetComponent<Transform>();
        prevColumn = GameObject.Find("Col").GetComponent<Transform>();

        heroPosition = new Vector3(-3f, heroPosY, 0f);
        currentColPosition = new Vector3(-3f, -3f, 0f);
        prevColPosition = new Vector3(-6f, -3f, 0f);

        nextColumn.Find("Column").transform.localScale = new Vector3(Random.Range(1f, 2f), 1, 1);
        nextColPosition = new Vector3(Random.Range(0.5f, 1.5f), -3f, 0);
    }


    void Update()
    {
        if (isReady && isApply && !isComplete)
        {
            ButtonTup();
        }
        if (isRotateStick)
        {
            StickRotate();
        }

        if (isComplete)
        {
            currentColumn.position = Vector3.Lerp(currentColumn.position, currentColPosition, HERO_SPEED * Time.deltaTime);
            nextColumn.position = Vector3.Lerp(nextColumn.position, nextColPosition, HERO_SPEED * Time.deltaTime);
            prevColumn.position = Vector3.Lerp(prevColumn.position, prevColPosition, HERO_SPEED * Time.deltaTime);
            heroTransform.position = Vector3.Lerp(heroTransform.position, heroPosition, HERO_SPEED * Time.deltaTime);
        }

        if (currentColumn.position.x < LEFT_MARGIN)
        {
            isComplete = !isComplete;
            SwapColumn();
            isReady = true;
        }
    }


    /// <summary>
    /// What happens when we push mouse button
    /// </summary>
    void ButtonTup()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (stick == null)
            {
                audioManager.Play(AudioManager.AudioState.StickGrow);
                stick = Instantiate<GameObject>(stickPrefab);
                stick.transform.position = prevColumn.Find("Column").transform.Find("Right_Col").transform.position;
            }
        }

        if (stick != null)
        {
            if (Input.GetMouseButton(0))
            {
                if (stick.transform.localScale.y < MAX_STICK_SCALE)
                {
                    stick.transform.localScale += new Vector3(0, 4f, 0);
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                audioManager.Play(AudioManager.AudioState.Stop);
                isRotateStick = true;
                isReady = false;
            }
        }
    }


    /// <summary>
    /// Rotating stick
    /// </summary>
    void StickRotate()
    {
        stick.transform.Rotate(0, 0, -FALL_STICK_SPEED);
        angle += FALL_STICK_SPEED;

        if (angle >= 90)
        {
            angle = 0;
            isRotateStick = false;
            isHit = HasBeenHit();
            StartCoroutine(MoveToPosition(heroPosition.x));
        }
    }


    /// <summary>
    /// Method for swaping columns
    /// </summary>
    void SwapColumn()
    {
        Transform temp = currentColumn;
        currentColumn = nextColumn;
        nextColumn = prevColumn;
        prevColumn = temp;

        nextColumn.position = SPAWN_VECTOR;
        nextColumn.Find("Column").transform.localScale = new Vector3(Random.Range(1f, 2f), 1, 1);
        float topRange = 2.5f - nextColumn.Find("Column").transform.localScale.x;
        nextColPosition = new Vector3(Random.Range(-1.7f, topRange), -3, 0);
    }


    /// <summary>
    /// Method where we check whether we hit next column with our stick or not
    /// </summary>
    bool HasBeenHit()
    {
        Vector3 stickTopPos = stick.transform.Find("TopPosition").transform.position;
        var info = Physics2D.Raycast(stickTopPos, Vector2.down, 0.2f, groundLayer | bonusLayer);
        var col = info.collider;
        if (col != null)
        {
            if (col.tag == "Bonus")
            {
                audioManager.Play(AudioManager.AudioState.Bonus);
                Scoremanager.score += 2;
            }
            else if (col.tag == "Ground")
            {
                audioManager.Play(AudioManager.AudioState.FallStick);
                Scoremanager.score += 1;
            }
            heroPosX = currentColumn.transform.position.x;
            heroPosition = new Vector3(heroPosX, heroPosY, 0);
            return true;
        }
        audioManager.Play(AudioManager.AudioState.FallStick);
        heroPosX = stick.transform.Find("TopPosition").transform.position.x;
        heroPosition = new Vector3(heroPosX, heroPosY, 0);
        return false;
    }


    /// <summary>
    /// Moving to next column and checking whether we hit next column or not
    /// </summary>
    /// <param name="targetPosX"></param>
    IEnumerator MoveToPosition(float targetPosX)
    {
        while (heroTransform.position.x < targetPosX)
        {
            StateAnimation = HeroState.Run;
            var info = Physics2D.Raycast(rootHero.position, Vector2.down, 0.2f, groundLayer | stickLayer | bonusLayer);
            var col = info.collider;
            if (col != null)
            {
                heroTransform.Translate(Vector3.right * HERO_SPEED * Time.deltaTime);
            }
            yield return null;
        }
        if (!isHit)
        {
            stick.GetComponent<Rigidbody2D>().isKinematic = false;
            stick.transform.Find("stick").GetComponent<Collider2D>().isTrigger = true;
            heroTransform.GetComponent<Collider2D>().isTrigger = true;
        }
        else
        {
            StateAnimation = HeroState.Idle;
            audioManager.Play(AudioManager.AudioState.Score);
            heroPosition = new Vector3(-3f, heroPosY, 0f);
            isComplete = true;
            Destroy(stick);
        }
    }


    /// <summary>
    /// GameOver screen
    /// </summary>
    void GameOver()
    {

        heroPosition = new Vector3(-3f, heroPosY, 0f);

        if (Scoremanager.score > PlayerPrefs.GetInt("Score"))
        {
            PlayerPrefs.SetInt("Score", Scoremanager.score);
        }

        scoreLabel.text = "Score : " + Scoremanager.score.ToString();
        bestScoreLabel.text = "Best : " + PlayerPrefs.GetInt("Score").ToString();
        isApply = false;
        restartDialog.SetActive(true);
        Destroy(stick);
        points.SetActive(false);
        Scoremanager.score = 0;
    }


    /// <summary>
    /// Trigger when we didn't hit next column
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter2D(Collider2D other)
    {
        StateAnimation = HeroState.Idle;
        audioManager.Play(AudioManager.AudioState.Death);
        heroTransform.GetComponent<Collider2D>().isTrigger = false;
        heroTransform.position = new Vector3(currentColumn.position.x, heroPosY, 0);
        GameOver();
    }
}
#endregion


public enum HeroState
{
    Idle,
    Run
}

