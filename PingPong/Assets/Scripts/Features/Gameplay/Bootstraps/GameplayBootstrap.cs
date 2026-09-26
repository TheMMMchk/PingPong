using System.Runtime.CompilerServices;
using UnityEngine;

public class GameplayBootstrap : MonoBehaviour
{
    [SerializeField] private HealthView playerHpView;
    [SerializeField] private ScoreView playerScoreView;

    [SerializeField] private MoveView playerMoveView;
    [SerializeField] private MoveView enemyMoveView;

    [SerializeField] private BallHitComponent ballHitComponent;


    private MoveController playerMoveController;
    private MoveController enemyMoveController;
    private BotMoveService _botMoveService;


    private void Awake()
    {


        InitPlayer();
        InitEnemy();
        BallHitService ballHitService = new BallHitService(ballHitComponent);
    }

    private void Update()
    {
        playerMoveController.Tick(Time.deltaTime);
        enemyMoveController.Tick(Time.deltaTime);
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
        _botMoveService = new BotMoveService();
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

    private static void InitEnemyScore()
    {
        ScoreModel enemyScoreModel = new ScoreModel();
        ScorePresenter enemyScorePresenter = new ScorePresenter(enemyScoreModel);
    }

    private void InitPlayerScore()
    {
        ScoreModel scoreModel = new ScoreModel();
        ScorePresenter scorePresenter = new ScorePresenter(scoreModel, playerScoreView);

        scorePresenter.UpdateScoreText();   
    }

    private void InitEnemyHp()
    {
        HealthModel enemyHpModel = new HealthModel(50);

        HealthPresenter emenyHpPresenter = new HealthPresenter(enemyHpModel);
    }

    private void InitPlayerHp()
    {
        HealthModel playerHpModel = new HealthModel(50);

        HealthPresenter playerHpPresenter = new HealthPresenter(playerHpModel, playerHpView);

        playerHpPresenter.UpdateHpText();
    }


}
