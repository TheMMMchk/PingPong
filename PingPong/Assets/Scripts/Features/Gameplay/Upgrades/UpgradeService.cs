using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeService
{
    private const float PlayerSpeedMultiplier = 1.15f;
    private const float BallSpeedMultiplier = 0.9f;
    private const float EnemySpeedMultiplier = 0.85f;
    private const float HitControlBonusPerPick = 0.05f;
    private const float MaxPlayerHitControlBonus = 0.15f;

    private static readonly UpgradeDefinition[] AllUpgrades =
    {
        new UpgradeDefinition(
            UpgradeId.FastPaddle,
            "Быстрая ракетка",
            "Скорость движения игрока увеличивается на 15%."),
        new UpgradeDefinition(
            UpgradeId.SlowBall,
            "Медленный шар",
            "Скорость мяча уменьшается на 10%."),
        new UpgradeDefinition(
            UpgradeId.HitControl,
            "Контроль удара",
            "Движение ракетки сильнее влияет на направление мяча."),
        new UpgradeDefinition(
            UpgradeId.WeakenedOpponent,
            "Ослабленный соперник",
            "Скорость движения бота уменьшается на 15%.")
    };

    private readonly MoveModel _playerMoveModel;
    private readonly MoveModel _enemyMoveModel;
    private readonly BallPhysicsView _ballPhysicsView;
    private readonly System.Random _random = new System.Random();
    private float _playerHitControlBonus;

    public float PlayerHitControlBonus => _playerHitControlBonus;

    public UpgradeService(MoveModel playerMoveModel, MoveModel enemyMoveModel,
        BallPhysicsView ballPhysicsView)
    {
        _playerMoveModel = playerMoveModel
            ?? throw new ArgumentNullException(nameof(playerMoveModel));
        _enemyMoveModel = enemyMoveModel
            ?? throw new ArgumentNullException(nameof(enemyMoveModel));
        _ballPhysicsView = ballPhysicsView
            ?? throw new ArgumentNullException(nameof(ballPhysicsView));
    }

    /// <summary>
    /// Returns a random set of unique options. The count is clamped to the
    /// available number of upgrade types, so it can never contain duplicates.
    /// </summary>
    public List<UpgradeDefinition> GetRandomOptions(int count = 3)
    {
        int optionCount = Mathf.Clamp(count, 0, AllUpgrades.Length);
        var available = new List<UpgradeDefinition>(AllUpgrades);

        // Partial Fisher-Yates shuffle: only the requested number of entries moves.
        for (int i = 0; i < optionCount; i++)
        {
            int selectedIndex = _random.Next(i, available.Count);
            UpgradeDefinition selected = available[selectedIndex];
            available[selectedIndex] = available[i];
            available[i] = selected;
        }

        return available.GetRange(0, optionCount);
    }

    /// <summary>
    /// Applies one upgrade. Repeated choices stack only within the documented caps.
    /// </summary>
    public void Apply(UpgradeDefinition upgrade)
    {
        if (upgrade == null)
            throw new ArgumentNullException(nameof(upgrade));

        switch (upgrade.Id)
        {
            case UpgradeId.FastPaddle:
                _playerMoveModel.ApplySpeedMultiplier(PlayerSpeedMultiplier);
                break;

            case UpgradeId.SlowBall:
                _ballPhysicsView.ApplySpeedMultiplier(BallSpeedMultiplier);
                break;

            case UpgradeId.HitControl:
                _playerHitControlBonus = Mathf.Clamp(
                    _playerHitControlBonus + HitControlBonusPerPick,
                    0f,
                    MaxPlayerHitControlBonus);
                break;

            case UpgradeId.WeakenedOpponent:
                _enemyMoveModel.ApplySpeedMultiplier(EnemySpeedMultiplier);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(upgrade), upgrade.Id,
                    "Неизвестный идентификатор улучшения.");
        }
    }
}
