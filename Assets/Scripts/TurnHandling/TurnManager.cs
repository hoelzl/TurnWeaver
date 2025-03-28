using System.Collections;
using System.Collections.Generic;
using Interaction.Objects;
using Player;
using UnityEngine;

namespace TurnHandling
{
    public enum TurnState { PlayerTurn, EnemyTurn, Transitioning }

    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private float turnTransitionDelay = 0.5f;

        private TurnState _currentState = TurnState.Transitioning;
        private readonly List<NPC> _activeNPCs = new List<NPC>();
        private PlayerController _player;

        // Singleton instance
        public static TurnManager Instance { get; private set; }

        // Event for turn changes
        public delegate void TurnChangeHandler(TurnState newState);
        public event TurnChangeHandler OnTurnChanged;

        private void Awake()
        {
            // Simple singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            _player = FindAnyObjectByType<PlayerController>();
            _activeNPCs.AddRange(FindObjectsByType<NPC>(FindObjectsSortMode.None));

            // Start the game with player's turn after a short delay
            StartCoroutine(StartFirstTurn());
        }

        private IEnumerator StartFirstTurn()
        {
            yield return new WaitForSeconds(0.5f);
            StartPlayerTurn();
        }

        public void StartPlayerTurn()
        {
            _currentState = TurnState.PlayerTurn;
            OnTurnChanged?.Invoke(_currentState);
            _player?.StartTurn();

            Debug.Log("Player Turn Started");
        }

        public void EndPlayerTurn()
        {
            // Only allow ending turn during player's turn
            if (_currentState != TurnState.PlayerTurn)
                return;

            _currentState = TurnState.Transitioning;
            OnTurnChanged?.Invoke(_currentState);
            _player?.EndTurn();

            StartCoroutine(ProcessEnemyTurn());
        }

        private IEnumerator ProcessEnemyTurn()
        {
            yield return new WaitForSeconds(turnTransitionDelay);

            _currentState = TurnState.EnemyTurn;
            OnTurnChanged?.Invoke(_currentState);

            Debug.Log("Enemy Turn Started");

            // Process each NPC's turn
            foreach (NPC npc in _activeNPCs)
            {
                // Process NPC logic here
                yield return new WaitForSeconds(0.5f);
            }

            // After all NPCs have moved, transition back to player turn
            yield return new WaitForSeconds(turnTransitionDelay);
            StartPlayerTurn();
        }

        public TurnState GetCurrentTurnState()
        {
            return _currentState;
        }

        // For registering/unregistering NPCs dynamically
        public void RegisterNPC(NPC npc)
        {
            if (!_activeNPCs.Contains(npc))
                _activeNPCs.Add(npc);
        }

        public void UnregisterNPC(NPC npc)
        {
            _activeNPCs.Remove(npc);
        }
    }
}
