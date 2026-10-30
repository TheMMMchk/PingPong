using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] private RunUiView runUiView;
    [SerializeField] private float ballSpeed = 6f;
    [SerializeField] private Vector2 ballStartDirection = new Vector2(0.2f, -1f);

    private MoveController playerMoveController;
    private MoveController enemyMoveController;
    private MoveModel _playerMoveModel;
    private MoveModel _enemyMoveModel;
    private BotMoveService _botMoveService;
    private BallService _ballService;
    private BallHitService _ballHitService;
    private BallRoundService _ballRoundService;
    private ScorePresenter _scorePresenter;
    private UpgradeService _upgradeService;
    private RunProgressionService _runProgressionService;

    private bool _waitingForUpgrade;
    private bool _runFinished;

    private void Awake()
    {
        if (runUiView == null)
        {
            Debug.LogError(
                "GameplayBootstrap is missing RunUiView. Open SampleScene and run " +
                "Tools > PingPong > Install Roguelite Run UI, then save the scene.",
                this);
            enabled = false;
            return;
        }

        InitScore();
        InitPlayer();
        InitEnemy();

        _ballService = new BallService(ballPhysicsView, ballSpeed, ballStartDirection);

        BallImpactView impactView = ballHitComponent.GetComponent<BallImpactView>();
        if (impactView == null)
            impactView = ballHitComponent.gameObject.AddComponent<BallImpactView>();

        _upgradeService = new UpgradeService(
            _playerMoveModel, _enemyMoveModel, ballPhysicsView);

        _ballHitService = new BallHitService(
            ballHitComponent, ballPhysicsView, impactView,
            playerMoveView, enemyMoveView, playerPaddleCollider, enemyPaddleCollider,
            leftWallCollider, rightWallCollider,
            () => _upgradeService.PlayerHitControlBonus);

        _ballRoundService = new BallRoundService(
            ballPhysicsView, _ballService, playerPaddleCollider, enemyPaddleCollider,
            _scorePresenter, ballStartDirection, Vector2.zero);

        _runProgressionService = new RunProgressionService(_ballRoundService);
        _runProgressionService.UpgradeSelectionRequested += HandleUpgradeSelectionRequested;
        _runProgressionService.RunFinished += HandleRunFinished;

        runUiView.UpgradeSelected += HandleUpgradeSelected;
        runUiView.RestartRequested += RestartRun;
        runUiView.MainMenuRequested += ReturnToMainMenu;
        runUiView.HideUpgradeSelection();
        runUiView.HideRunResult();

        // All event subscriptions are ready before the first serve begins.
        _ballService.Launch();
    }

    private void Update()
    {
        if (_runFinished || _waitingForUpgrade)
            return;

        playerMoveController.Tick(Time.deltaTime);
        enemyMoveController.Tick(Time.deltaTime);
        _ballRoundService?.Tick(Time.deltaTime);
    }

    private void OnDestroy()
    {
        if (_runProgressionService != null)
        {
            _runProgressionService.UpgradeSelectionRequested -= HandleUpgradeSelectionRequested;
            _runProgressionService.RunFinished -= HandleRunFinished;
            _runProgressionService.Dispose();
        }

        if (runUiView != null)
        {
            runUiView.UpgradeSelected -= HandleUpgradeSelected;
            runUiView.RestartRequested -= RestartRun;
            runUiView.MainMenuRequested -= ReturnToMainMenu;
        }

        _ballHitService?.Dispose();
    }

    private void HandleUpgradeSelectionRequested(int completedMatches)
    {
        _waitingForUpgrade = true;

        if (runUiView == null || _upgradeService == null)
        {
            Debug.LogError("Cannot show upgrade selection: RunUiView or UpgradeService is missing.", this);
            return;
        }

        runUiView.ShowUpgradeOptions(_upgradeService.GetRandomOptions(3));
    }

    private void HandleUpgradeSelected(UpgradeDefinition upgrade)
    {
        if (!_waitingForUpgrade || _runFinished || upgrade == null)
            return;

        _upgradeService.Apply(upgrade);
        runUiView.HideUpgradeSelection();
        _waitingForUpgrade = false;
        _runProgressionService.ContinueAfterUpgrade();
    }

    private void HandleRunFinished(bool playerWon)
    {
        _runFinished = true;
        _waitingForUpgrade = false;

        if (runUiView != null)
            runUiView.ShowRunResult(playerWon);
    }

    private void RestartRun()
    {
        LoadScene("SampleScene");
    }

    private void ReturnToMainMenu()
    {
        LoadScene("MainMenu");
    }

    private void LoadScene(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"Cannot load scene '{sceneName}'. Add it to Build Profiles / Build Settings.",
                this);
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
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
        _enemyMoveModel = new MoveModel(4);
        _botMoveService = new BotMoveService(
            ballPhysicsView, enemyMoveView, leftWallCollider, rightWallCollider);
        enemyMoveController = new MoveController(_enemyMoveModel, enemyMoveView, _botMoveService);
    }

    private void InitPlayer()
    {
        InitPlayerMovement();
        InitPlayerHp();
    }

    private void InitPlayerMovement()
    {
        _playerMoveModel = new MoveModel(4);
        PlayerMoveService moveService = new PlayerMoveService();
        playerMoveController = new MoveController(_playerMoveModel, playerMoveView, moveService);
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
