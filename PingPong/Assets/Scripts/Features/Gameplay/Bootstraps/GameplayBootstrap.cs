using UnityEngine;

public class GameplayBootstrap : MonoBehaviour
{
    [SerializeField] private HealthView playerHpView;

    [SerializeField] private MoveView playerMoveView;
    [SerializeField] private MoveView enemyMoveView;

    [SerializeField] private BallHitComponent ballHitComponent;
    [SerializeField] private BallPhysicsView ballPhysicsView;
    [SerializeField] private Collider2D playerPaddleCollider;
    [SerializeField] private Collider2D enemyPaddleCollider;
    [SerializeField] private Collider2D leftWallCollider;
    [SerializeField] private Collider2D rightWallCollider;
    [SerializeField] private float ballSpeed = 6f;
    [SerializeField] private Vector2 ballStartDirection = new Vector2(0.2f, -1f);

    private MoveController playerMoveController;
    private MoveController enemyMoveController;
    private BotMoveService _botMoveService;
    private BallService _ballService;
    private BallHitService _ballHitService;
    private BallRoundService _ballRoundService;
    private ScorePresenter _playerScorePresenter;
    private ScorePresenter _enemyScorePresenter;

    private void Awake()
    {
        InitPlayer();
        InitEnemy();

        _ballService = new BallService(
            ballPhysicsView,
            ballSpeed,
            ballStartDirection);

        _ballHitService = new BallHitService(
            ballHitComponent,
            ballPhysicsView,
            playerPaddleCollider,
            enemyPaddleCollider,
            leftWallCollider,
            rightWallCollider);

        _ballRoundService = new BallRoundService(
            ballPhysicsView,
            _ballService,
            playerPaddleCollider,
            enemyPaddleCollider,
            _playerScorePresenter,
            _enemyScorePresenter,
            ballStartDirection,
            Vector2.zero);

        _ballService.Launch();
    }

    private void OnDestroy()
    {
        _ballHitService?.Dispose();
    }

    private void Update()
    {
        playerMoveController.Tick(Time.deltaTime);
        enemyMoveController.Tick(Time.deltaTime);
        _ballRoundService?.Tick(Time.deltaTime);
    }

    private void InitEnemy()
    {
        InitEnemyMovement();
        InitEnemyHp();
        InitEnemyScore();
    }

    private void InitEnemyMovement()
    {
        MoveModel model = new MoveModel(4);
        _botMoveService = new BotMoveService(
            ballPhysicsView,
            enemyMoveView,
            leftWallCollider,
            rightWallCollider);

        enemyMoveController = new MoveController(model, enemyMoveView, _botMoveService);
    }

    private void InitPlayer()
    {
        InitPlayerMovement();
        InitPlayerHp();
        InitPlayerScore();
    }

    private void InitPlayerMovement()
    {
        MoveModel model = new MoveModel(4);
        PlayerMoveService moveService = new PlayerMoveService();
        playerMoveController = new MoveController(model, playerMoveView, moveService);
    }

    private void InitEnemyScore()
    {
        ScoreModel enemyScoreModel = new ScoreModel();
        _enemyScorePresenter = new ScorePresenter(enemyScoreModel);
    }

    private void InitPlayerScore()
    {
        ScoreModel playerScoreModel = new ScoreModel();
        _playerScorePresenter = new ScorePresenter(playerScoreModel);
    }

    private void InitEnemyHp()
    {
        HealthModel enemyHpModel = new HealthModel(50);
        HealthPresenter enemyHpPresenter = new HealthPresenter(enemyHpModel);
    }

    private void InitPlayerHp()
    {
        HealthModel playerHpModel = new HealthModel(50);
        HealthPresenter playerHpPresenter = new HealthPresenter(playerHpModel, playerHpView);
        playerHpPresenter.UpdateHpText();
    }
}