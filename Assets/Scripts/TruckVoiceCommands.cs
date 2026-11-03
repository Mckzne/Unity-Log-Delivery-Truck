
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class TruckVoiceCommands : MonoBehaviour
{
    [Header("Existing Command Controller")]
    public TruckCommandController commandController;

    private KeywordRecognizer recognizer;
    private Dictionary<string, Action> commands;

    private void Start()
    {
        Debug.Log("Starting truck voice commands...");

        // Check speech recognition support first.
        if (!PhraseRecognitionSystem.isSupported)
        {
            Debug.LogError(
                "Speech recognition is not supported on this system."
            );
            enabled = false;
            return;
        }

        // Check the command controller.
        if (commandController == null)
        {
            Debug.LogError(
                "Assign TruckCommandController in the Inspector.",
                this
            );
            enabled = false;
            return;
        }

        // Define recognized phrases and their commands.
        commands = new Dictionary<string, Action>(
            StringComparer.OrdinalIgnoreCase
        )
        {
            { "forward", () => SendCommand("forward") },
            { "drive forward", () => SendCommand("forward") },
            { "reverse", () => SendCommand("reverse") },
            { "turn left", () => SendCommand("turn left") },
            { "turn right", () => SendCommand("turn right") },
            { "stop", () => SendCommand("stop") },
            { "brake", () => SendCommand("brake") },
            { "collect log", () => SendCommand("collect log") },
            { "deliver log", () => SendCommand("deliver log") },
            { "return to base", () => SendCommand("return to base") },
            { "truck status", () => SendCommand("status") },
            { "help", () => SendCommand("help") }
        };

        try
        {
            recognizer = new KeywordRecognizer(
                new List<string>(commands.Keys).ToArray(),
                ConfidenceLevel.Medium
            );

            recognizer.OnPhraseRecognized += OnPhraseRecognized;
            recognizer.Start();

            Debug.Log(
                "VOICE SYSTEM STARTED. Recognizer running: " +
                recognizer.IsRunning
            );

            Debug.Log("Say 'forward', 'stop', or 'help'.");
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "Could not start voice recognition: " + ex.Message
            );

            CleanupRecognizer();
            enabled = false;
        }
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log(
            "VOICE DETECTED: " + args.text +
            " | CONFIDENCE: " + args.confidence
        );

        if (commands != null &&
            commands.TryGetValue(args.text, out Action action))
        {
            Debug.Log("VOICE ACCEPTED: " + args.text);
            action.Invoke();
        }
        else
        {
            Debug.LogWarning(
                "Recognized phrase has no matching command: " + args.text
            );
        }
    }

    private void SendCommand(string command)
    {
        if (commandController == null)
        {
            Debug.LogError("TruckCommandController is missing.");
            return;
        }

        Debug.Log("Sending command to truck: " + command);
        commandController.ProcessCommand(command);
    }

    private void CleanupRecognizer()
    {
        if (recognizer == null)
            return;

        recognizer.OnPhraseRecognized -= OnPhraseRecognized;

        if (recognizer.IsRunning)
            recognizer.Stop();

        recognizer.Dispose();
        recognizer = null;
    }

    private void OnDestroy()
    {
        CleanupRecognizer();
    }
}
