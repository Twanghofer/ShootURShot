using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum PlayState { PLAYER1SHOOTING, PLAYER1MOVING, PLAYER2SHOOTING, PLAYER2MOVING, EXECUTING, FINISHED };

    public bool playAudio;
    public Vector3 player1SpawnPos;
    public Vector3 player2SpawnPos;
    public GameObject playerPrefab;
    public GameObject bulletPrefab;
    public GameObject playerHolo;
    public GameObject startScreen;
    public LineRenderer lineRenderer;
    public AudioManager audioManager;
    public Text button;
    public Text topText;
    public Text bottomText;
    public Text p1ScoreText;
    public Text p2ScoreText;
    public Image player1Sprite;
    public Image player2Sprite;
    public Material lineMat;
    public Material arrowMat;
    public Material player2Mat;
    public Color redColor = new Color(241, 58, 58);
    public Color blueColor = new Color(191, 46, 46);

    public float speed = 5f;
    public float maxSpeed = 20f;
    public float speedFactor = 2.5f;
    public float playerMoveSpeed = 5f;
    public float maxRangeMovement = 5f;
    [HideInInspector] public bool multiplySpeed = false;
    [HideInInspector] public PlayState state = PlayState.PLAYER1SHOOTING;

    private Transform player1;
    private Transform player2;
    private GameObject holoPlayer;
    private Vector3 touchPos;
    private Vector3 lineDir;
    private Vector3 player1Dir;
    private Vector3 player2Dir;
    private Vector3 player1Move;
    private Vector3 player2Move;
    private Touch touch;
    private bool movePlayers;
    private bool checkMusic;
    private int roundCount = 0;
    private int p1score = 0;
    private int p2score = 0;
    private float startSpeed;
    private float player1Speed;
    private float player2Speed;

    private void Awake()
    {
        ResetPlayers();

        p1ScoreText.text = p1score.ToString();
        p2ScoreText.text = p2score.ToString();

        startSpeed = speed;

        lineRenderer.startWidth = 1;
        lineRenderer.endWidth = 1;
    }

    private void Start()
    {
        HardReset();

        if (playAudio)
        {
            audioManager.Play("StartMusic");
        }


        player1Move = player1.position;
        player2Move = player2.position;

        player1Sprite.color = redColor;
        player2Sprite.color = blueColor;
    }

    void Update()
    {
        if (Input.touchCount > 0 )
        {
            if (checkMusic) return;
            touch = Input.GetTouch(0);
            Ray ray = Camera.main.ScreenPointToRay(touch.position);

            if (Physics.Raycast(Camera.main.ScreenToWorldPoint(touch.position), Vector3.down* 100, out RaycastHit hitInfo))
            {
                touchPos = ray.GetPoint(hitInfo.distance);
                touchPos.y = 0.5f;

                if (Physics.Raycast(touchPos, Vector3.down, out RaycastHit scanInfo) && !scanInfo.transform.CompareTag("Stage")) return;

                if (state != PlayState.EXECUTING && touch.phase == TouchPhase.Began)
                {
                    if (playAudio)
                    {
                        audioManager.Play("Click");
                    }
                }

                switch (state)
                {
                    case PlayState.PLAYER1SHOOTING:
                        lineDir = touchPos - player1.position;
                        lineRenderer.SetPosition(0, player1.position);
                        lineRenderer.SetPosition(1, touchPos - (lineDir - (lineDir / lineDir.magnitude) * 2f));
                        break;

                    case PlayState.PLAYER1MOVING:
                        lineDir = touchPos - player1.position;
                        lineRenderer.SetPosition(0, player1.position);
                        if(Physics.Raycast(player1.position, lineDir, out RaycastHit hit))
                        {
                            if (!hit.collider.CompareTag("Player") && Vector3.Distance(player1.position,hit.point)<= Vector3.Distance(player1.position, touchPos)) 
                            {
                                lineRenderer.SetPosition(1, hit.point-((lineDir/lineDir.magnitude)*.5f));
                            }

                            else
                            {
                                lineRenderer.SetPosition(1, touchPos);
                            }
                        }
                       
                        holoPlayer.transform.position = lineRenderer.GetPosition(1);
                        player1Move = holoPlayer.transform.position;
                        break;

                    case PlayState.PLAYER2SHOOTING:
                        lineDir = touchPos - player2.position;
                        lineRenderer.SetPosition(0, player2.position);
                        lineRenderer.SetPosition(1, touchPos - (lineDir - (lineDir / lineDir.magnitude) * 2f));
                        break;

                    case PlayState.PLAYER2MOVING:
                        lineDir = touchPos - player2.position;
                        lineRenderer.SetPosition(0, player2.position);
                        if (Physics.Raycast(player2.position, lineDir, out hit))
                        {
                            if (!hit.collider.CompareTag("Player") && Vector3.Distance(player2.position, hit.point) <= Vector3.Distance(player2.position, touchPos))
                            {
                                lineRenderer.SetPosition(1, hit.point - ((lineDir / lineDir.magnitude) * .5f));
                            }

                            else
                            {
                                lineRenderer.SetPosition(1, touchPos);
                            }
                        }

                        holoPlayer.transform.position = lineRenderer.GetPosition(1);
                        player2Move = holoPlayer.transform.position;
                        break;
                }
            }

            else
            {
                Debug.Log("none hit by raycast");
            }
        }

        if (!checkMusic) return;

        if (!audioManager.ClipPlaying("StartMusic"))
        {
            if (playAudio)
            {
                audioManager.Play("Start");
                audioManager.Play("Music");
            }
            startScreen.SetActive(false);
            checkMusic = false;
        }

    }


    private void FixedUpdate()
    {
        if (!movePlayers) return;

        MovePlayer(player1, player1Move);
        MovePlayer(player2, player2Move);

        if(GameObject.FindGameObjectsWithTag("Bullet").Length == 0)
           
        {
            if (Player1Alive() && Player2Alive())
            {
                SoftReset();
                Debug.Log("No player died, starting new Round..");
            }
            else
            {
                CheckScore();
                ResetPlayers();
                SoftReset();
            }
        }
    }

    public void setVariables()
    {
        multiplySpeed = false;

        //Debug.Log("speed is " + speed);

        switch (state)
        {
            case PlayState.PLAYER1SHOOTING:
                player1Dir = lineDir;
                player1Speed = speed;
                topText.text = "PLAYER 1 POSITIONING";
                button.text = "NEXT PLAYER";
                state = PlayState.PLAYER1MOVING;
                holoPlayer = Instantiate(playerHolo, player1Move, Quaternion.identity);
                holoPlayer.transform.position = player1.position;
                holoPlayer.transform.eulerAngles = new Vector3(90,0,0);
                PrepLineRenderer();
                break;

            case PlayState.PLAYER1MOVING:
                if (Vector3.Distance(holoPlayer.transform.position, player1.position) <= 0.5f) return;
                Destroy(holoPlayer);
                topText.color = blueColor;
                topText.text = "PLAYER 2 SHOOTING";
                button.text = "SHOOT";
                state = PlayState.PLAYER2SHOOTING;
                PrepLineRenderer();
                break;

            case PlayState.PLAYER2SHOOTING:
                player2Dir = lineDir;
                player2Speed = speed;
                topText.text = "PLAYER 2 POSITIONING";
                button.text = "NEXT PLAYER";
                state = PlayState.PLAYER2MOVING;
                holoPlayer = Instantiate(playerHolo, player1Move, Quaternion.identity);
                holoPlayer.transform.position = player2.position;
                holoPlayer.transform.eulerAngles = new Vector3(90, 0, 0);
                PrepLineRenderer();
                break;

            case PlayState.PLAYER2MOVING:
                if (Vector3.Distance(holoPlayer.transform.position, player2.position) <= 0.5f) return;
                Destroy(holoPlayer);
                topText.color = Color.white;
                topText.text = "ARE YOU READY?";
                button.text = "GO!";
                state = PlayState.EXECUTING;
                lineRenderer.enabled = false;
                break;

            case PlayState.EXECUTING:
                topText.text = "PLAYING";
                ExecuteActions();
                state = PlayState.FINISHED;
                break;

            case PlayState.FINISHED:
                Debug.Log("Cant press button when playing");
                break;

        }

        speed = startSpeed;

    }

    public void ExecuteActions()
    {
        ToggleCollider(player1.gameObject);
        ToggleCollider(player2.gameObject);

        ShootBullet(player1,player1Dir, player1Speed, false);
        ShootBullet(player2,player2Dir, player2Speed, true);
        movePlayers = true;

        ToggleCollider(player1.gameObject);
        ToggleCollider(player2.gameObject);
    }

    private void ShootBullet(Transform _player, Vector3 _dir, float _speed, bool changeMat)
    {
        GameObject bullet = Instantiate(bulletPrefab, _player.position, Quaternion.identity);
        if(changeMat)
        {
            bullet.GetComponent<Renderer>().material = player2Mat;
            bullet.GetComponent<Bullet>().bulletImpact.GetComponent<Renderer>().material = player2Mat;
        }
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.AddForce(_dir.normalized * _speed, ForceMode.Impulse);

    }

    private void MovePlayer(Transform _player, Vector3 _movePos)
    {
        if (_player == null) return;
        _player.position = Vector3.MoveTowards(_player.position, _movePos, Time.deltaTime * playerMoveSpeed);
    }

    public void getForce()
    {
        multiplySpeed = true;
        Debug.Log("calling");
    }

    private void PrepLineRenderer()
    {
        lineRenderer.enabled = true;
        
        if (state == PlayState.PLAYER1SHOOTING)
        {
            Vector3 playerDir = player2.position - player1.position;
            lineRenderer.material = arrowMat;
            lineRenderer.SetPosition(0, player1.position);
            lineRenderer.SetPosition(1, player2.position - (playerDir - (playerDir / playerDir.magnitude) * 2f));
        }
        else if (state == PlayState.PLAYER2SHOOTING)
        {
            Vector3 playerDir = player1.position - player2.position;
            lineRenderer.material = arrowMat;
            lineRenderer.SetPosition(0, player2.position);
            lineRenderer.SetPosition(1, player1.position - (playerDir - (playerDir / playerDir.magnitude) * 2f));
        }
        else
        {
            lineRenderer.material = lineMat;
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);
        }
    }

    public void MultiplySpeed()
    {
        speed = Mathf.Lerp(speed,maxSpeed,Time.deltaTime * speedFactor);
    }

    public void ScaleLine()
    {
        Vector3[] linePositions = new Vector3[2];
        lineRenderer.GetPositions(linePositions);
        Vector3 dir = linePositions[1] - linePositions[0];
        lineRenderer.SetPosition(1, linePositions[1]-(dir-(dir/dir.magnitude)*(speed/6f)));

        float colorLerp = (speed / 20);
        if(state== PlayState.PLAYER1SHOOTING)
        {
            lineRenderer.material.color = Color.Lerp(Color.white, redColor, colorLerp);
        }
        else
        {
            lineRenderer.material.color = Color.Lerp(Color.white, blueColor, colorLerp);
        }

    }

    private void ToggleCollider(GameObject _gameObject)
    {
        if (_gameObject == null)
        {
            Debug.LogWarning("Couldnt toggle collider " + _gameObject +"was null");
        return;
        }
        if (_gameObject.GetComponent<Collider>().enabled)
        {
            _gameObject.GetComponent<Collider>().enabled = false;
        }
        else
        {
            _gameObject.GetComponent<Collider>().enabled = true;
        }
    }
       
    private void SoftReset()
    {
        roundCount++;
        
        bottomText.text = "Round " + roundCount;

        state = PlayState.PLAYER1SHOOTING;

        player1Dir = player1SpawnPos;
        player1Move = player1SpawnPos;
        player1Speed = 1f;
        player2Dir = player2SpawnPos;
        player2Move = player2SpawnPos;
        player2Speed = 1f;

        movePlayers = false;

        PrepLineRenderer();
        topText.color = redColor;
        topText.text = "PLAYER 1 SHOOTING";
        button.text = "SHOOT";
    }

    private void ResetPlayers()
    {
        if(!Player1Alive() && !Player2Alive())
        {
            Debug.Log("Both Players were dead, respawning them..");
            player1 = Instantiate(playerPrefab, player1SpawnPos, Quaternion.identity).transform;
            player2 = Instantiate(playerPrefab, player2SpawnPos, Quaternion.identity).transform;
            player2.GetComponent<Renderer>().material = player2Mat;

            player1Move = player1.position;
            player2Move = player2.position;
            movePlayers = false;
        }
        
        else
        {
            Debug.Log("One Player died, resetting positions...");
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach(GameObject player in players)
            {
                Destroy(player);
            }
            player1 = Instantiate(playerPrefab, player1SpawnPos, Quaternion.identity).transform;
            player2 = Instantiate(playerPrefab, player2SpawnPos, Quaternion.identity).transform;
            player2.GetComponent<Renderer>().material = player2Mat;

            player1Move = player1.position;
            player2Move = player2.position;
            movePlayers = false;
        }
    }

    private void CheckScore()
    {
        if (Player1Alive() && !Player2Alive())
        {
            p1score++;
            if (playAudio)
            {
                audioManager.Play("Player1Won");
            }
        }

        else if (!Player1Alive() && Player2Alive())
        {
            p2score++;
            if (playAudio)
            {
                audioManager.Play("Player2Won");
            }
        }

        else
        {
            p1score++;
            p2score++;
            if (playAudio)
            {
                audioManager.Play("Player1Won");
                audioManager.Play("Player2Won");
            }
        }
        p1ScoreText.text = p1score.ToString();
        p2ScoreText.text = p2score.ToString();

        Debug.Log("Game is " + p1score + ":" + p2score);
    }

    private bool Player1Alive()
    {
        if (player1 == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private bool Player2Alive()
    {
        if (player2 == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void HardReset()
    {
        audioManager.Stop("Music");
        startScreen.SetActive(true);
        ResetPlayers();
        SoftReset();
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach(GameObject bullet in bullets)
        {
            Destroy(bullet);
        }

        roundCount =0;
        roundCount++;
        bottomText.text = "Round " + roundCount;

        p1score = 0;
        p2score = 0;
        p1ScoreText.text = p1score.ToString();
        p2ScoreText.text = p2score.ToString();

        checkMusic = true;

        if (playAudio)
        {
            audioManager.Play("StartMusic");
        }
    }
}
