using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Displays the run upgrade choice and final result screens.
/// Scene navigation is reported to the Bootstrap through events.
/// </summary>
public sealed class RunUiView : MonoBehaviour
{
    [Header("Upgrade Selection")]
    [SerializeField] private GameObject upgradeSelectionPanel;
    [SerializeField] private Button[] upgradeButtons = new Button[3];
    [SerializeField] private TMP_Text[] upgradeTitles = new TMP_Text[3];
    [SerializeField] private TMP_Text[] upgradeDescriptions = new TMP_Text[3];

    [Header("Run Result")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultTitle;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    private readonly UpgradeDefinition[] _shownOptions = new UpgradeDefinition[3];
    private UnityAction[] _upgradeButtonHandlers;
    private UnityAction _restartHandler;
    private UnityAction _mainMenuHandler;

    public event Action<UpgradeDefinition> UpgradeSelected;
    public event Action RestartRequested;
    public event Action MainMenuRequested;

    private void Awake()
    {
        if (upgradeButtons != null)
        {
            _upgradeButtonHandlers = new UnityAction[upgradeButtons.Length];

            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                int optionIndex = i;
                if (upgradeButtons[i] == null)
                    continue;

                _upgradeButtonHandlers[i] = () => SelectUpgrade(optionIndex);
                upgradeButtons[i].onClick.AddListener(_upgradeButtonHandlers[i]);
            }
        }

        _restartHandler = () => RestartRequested?.Invoke();
        _mainMenuHandler = () => MainMenuRequested?.Invoke();

        if (restartButton != null)
            restartButton.onClick.AddListener(_restartHandler);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(_mainMenuHandler);

        HideUpgradeSelection();
        HideRunResult();
    }

    public void ShowUpgradeOptions(IReadOnlyList<UpgradeDefinition> options)
    {
        if (options == null || options.Count != 3)
        {
            Debug.LogError("RunUiView requires exactly three upgrade options.", this);
            return;
        }

        if (upgradeSelectionPanel == null
            || upgradeButtons == null || upgradeTitles == null || upgradeDescriptions == null
            || upgradeButtons.Length < 3 || upgradeTitles.Length < 3 || upgradeDescriptions.Length < 3)
        {
            Debug.LogError("RunUiView is missing one or more upgrade UI references.", this);
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            UpgradeDefinition option = options[i];
            if (option == null || upgradeButtons[i] == null
                || upgradeTitles[i] == null || upgradeDescriptions[i] == null)
            {
                Debug.LogError($"Upgrade option UI at index {i} is not configured.", this);
                return;
            }

            _shownOptions[i] = option;
            upgradeTitles[i].text = option.Name;
            upgradeDescriptions[i].text = option.Description;
            upgradeButtons[i].interactable = true;
        }

        HideRunResult();
        upgradeSelectionPanel.SetActive(true);
    }

    public void HideUpgradeSelection()
    {
        if (upgradeSelectionPanel != null)
            upgradeSelectionPanel.SetActive(false);
    }

    public void ShowRunResult(bool playerWon)
    {
        HideUpgradeSelection();

        if (resultTitle != null)
            resultTitle.text = playerWon ? "ПОБЕДА!" : "ПОРАЖЕНИЕ";

        if (resultPanel != null)
            resultPanel.SetActive(true);
    }

    public void HideRunResult()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    private void SelectUpgrade(int index)
    {
        if (index < 0 || index >= _shownOptions.Length)
            return;

        UpgradeDefinition selected = _shownOptions[index];
        if (selected == null)
            return;

        HideUpgradeSelection();
        UpgradeSelected?.Invoke(selected);
    }

    private void OnDestroy()
    {
        if (upgradeButtons != null && _upgradeButtonHandlers != null)
        {
            for (int i = 0; i < upgradeButtons.Length && i < _upgradeButtonHandlers.Length; i++)
            {
                if (upgradeButtons[i] != null && _upgradeButtonHandlers[i] != null)
                    upgradeButtons[i].onClick.RemoveListener(_upgradeButtonHandlers[i]);
            }
        }

        if (restartButton != null && _restartHandler != null)
            restartButton.onClick.RemoveListener(_restartHandler);

        if (mainMenuButton != null && _mainMenuHandler != null)
            mainMenuButton.onClick.RemoveListener(_mainMenuHandler);

        UpgradeSelected = null;
        RestartRequested = null;
        MainMenuRequested = null;
    }
}
