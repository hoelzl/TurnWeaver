using UnityEngine;
using System.Text;

namespace Dialogue
{
    /// <summary>
    /// Simple singleton to log dialogue lines and choices.
    /// </summary>
    public class DialogueLogManager : MonoBehaviour
    {
        public static DialogueLogManager Instance { get; private set; }

        [SerializeField] private bool logToConsole = true;
        // TODO: Add options for logging to file or UI panel

        private readonly StringBuilder _sessionLog = new();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Optional persistence
        }

        public void StartNewLogSession()
        {
            _sessionLog.Clear();
             if (logToConsole) Debug.Log("--- Dialogue Log Session Started ---");
        }

        public void LogLine(string line)
        {
            string formattedLine = $"{line}";
            _sessionLog.AppendLine(formattedLine);
            if (logToConsole) Debug.Log($"[Dialogue] {formattedLine}");
        }

        public void LogChoice(string choiceText, string speaker = "Player")
        {
             string formattedChoice = $"{speaker} chooses: {choiceText}";
             _sessionLog.AppendLine(formattedChoice);
             if (logToConsole) Debug.Log($"[Dialogue] > {choiceText}"); // Simpler console log for choice
        }

        // Optional: Log the full transcript at the end
        public void LogSessionTranscript()
        {
            if (logToConsole)
            {
                 Debug.Log("--- Dialogue Log Session End ---");
                 Debug.Log(_sessionLog.ToString());
                 Debug.Log("--------------------------------");
            }
            // Write to file etc.
        }

        public string GetCurrentSessionLog()
        {
            return _sessionLog.ToString();
        }
    }
}
