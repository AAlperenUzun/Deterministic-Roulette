using System;
using System.Collections.Generic;
using Roulette.Core;
using UnityEngine;

namespace Roulette.Game
{
    [DefaultExecutionOrder(-100)]
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("New game")]
        [SerializeField] private long _startingBalance = 1000;
        [SerializeField] private int[] _chipDenominations = { 1, 5, 25, 100, 500 };
        [SerializeField] private RouletteType _defaultTable = RouletteType.European;

        [Header("Persistence")]
        [SerializeField] private bool _persistenceEnabled = true;
        [SerializeField] private bool _autoSaveEachRound = true;

        private SaveService _saveService;
        private System.Random _rng;

        public GameContext Context { get; private set; }
        public IReadOnlyList<int> ChipDenominations => _chipDenominations;
        public long StartingBalance => _startingBalance;

        public event Action GameReset;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _rng = new System.Random();
            _saveService = new SaveService(new FileSaveStorage());
            Context = LoadOrCreate();
            HookAutoSave();
        }

        private GameContext LoadOrCreate()
        {
            if (_persistenceEnabled && _saveService.HasSave && _saveService.TryLoad(out SaveData data))
                return _saveService.Restore(data, _rng);
            return new GameContext(_defaultTable, _startingBalance, _rng);
        }

        private void HookAutoSave()
        {
            if (_persistenceEnabled && _autoSaveEachRound)
                Context.SpinResolved += OnSpinResolved;
        }

        private void OnSpinResolved(SpinResolution _) => Save();

        public void Save()
        {
            if (_persistenceEnabled) _saveService.Save(Context);
        }

        public void NewGame()
        {
            Context.NewGame(_startingBalance, _defaultTable);
            if (_persistenceEnabled) _saveService.Delete();
            GameReset?.Invoke();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) Save();
        }

        private void OnApplicationQuit() => Save();

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
