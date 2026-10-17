using UnityEngine;

public class GameplayBootstrap : MonoBehaviour
{
    [SerializeField] private HealthView playerHpView;
    [SerializeField] private ScoreView scoreView;
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
    private ScorePresenter _scorePresenter;

    private void Awake()
    {
        InitScore();
        InitPlayer();
        InitEnemy();
        _ballService = new BallService(ballPhysicsView, ballSpeed, ballStartDirection);

        BallImpactView impactView = ballHitComponent.GetComponent<BallImpactView>();
        if (impactView == null)
            impactView = ballHitComponent.gameObject.AddComponent<BallImpactView>();

        _ballHitService = new BallHitService(
            ballHitComponent, ballPhysicsView, impactView,
            playerMoveView, enemyMoveView, playerPaddleCollider, enemyPaddleCollider,
            leftWallCollider, rightWallCollider);

        _ballRoundService = new BallRoundService(
            ballPhysicsView, _ballService, playerPaddleCollider, enemyPaddleCollider,
            _scorePresenter, ballStartDirection, Vector2.zero);
        _ballService.Launch();
    }

    private void OnDestroy() { _ballHitService?.Dispose(); }

    private void Update()
    {
        playerMoveController.Tick(Time.deltaTime);
        enemyMoveController.Tick(Time.deltaTime);
        _ballRoundService?.Tick(Time.deltaTime);
    }

    private void InitScore()
    {
        ScoreModel playerScoreModel = new ScoreModel();
        ScoreModel enemyScoreModel = new ScoreModel();
        _scorePresenter = new ScorePresenter(playerScoreModel, enemyScoreModel, scoreView);
    }

    private void InitEnemy()
    {
        InitEnemyMovement();
        InitEnemyHp();
    }

    private void InitEnemyMovement()
    {
        MoveModel model = new MoveModel(4);
        _botMoveService = new BotMoveService(ballPhysicsView, enemyMoveView, leftWallCollider, rightWallCollider);
        enemyMoveController = new MoveController(model, enemyMoveView, _botMoveService);
    }

    private void InitPlayer()
    {
        InitPlayerMovement();
        InitPlayerHp();
    }

    private void InitPlayerMovement()
    {
        MoveModel model = new MoveModel(4);
        PlayerMoveService moveService = new PlayerMoveService();
        playerMoveController = new MoveController(model, playerMoveView, moveService);
    }

    private void InitEnemyHp()
    {
        HealthModel model = new HealthModel(50);
        HealthPresenter presenter = new HealthPresenter(model);
    }

    private void InitPlayerHp()
    {
        HealthModel model = new HealthModel(50);
        HealthPresenter presenter = new HealthPresenter(model, playerHpView);
        presenter.UpdateHpText();
    }
}
