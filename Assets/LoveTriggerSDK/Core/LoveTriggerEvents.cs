using System;
using UnityEngine;

namespace LTSystem.Core
{
    // ─────────────────────────────────────────────────────
    //  LoveTriggerEvents.cs
    //  All network-serializable consent structs and runtime
    //  event data. These are the messages that flow through
    //  NetworkedLoveTriggerManager's consent gate.
    // ─────────────────────────────────────────────────────

    /// <summary>Sent from initiator to target to request consent.</summary>
    [Serializable]
    public struct ConsentRequestData
    {
        /// <summary>Unique ID for this request — used to correlate the response.</summary>
        public string RequestID;
        /// <summary>ID of the trigger being requested.</summary>
        public string TriggerID;
        /// <summary>Network player ref of the initiating player.</summary>
        public ulong InitiatorPlayerRef;
        /// <summary>Display name of the trigger shown in the target's UI.</summary>
        public string TriggerDisplayName;
        /// <summary>UTC timestamp when the request was created.</summary>
        public long TimestampUtc;
    }

    /// <summary>Sent from target back to initiator accepting or declining.</summary>
    [Serializable]
    public struct ConsentResponseData
    {
        /// <summary>Must match ConsentRequestData.RequestID.</summary>
        public string RequestID;
        /// <summary>True if the target accepted.</summary>
        public bool Accepted;
        /// <summary>Network player ref of the responding player.</summary>
        public ulong ResponderPlayerRef;
    }

    /// <summary>Broadcast when a trigger begins executing on all clients.</summary>
    [Serializable]
    public struct TriggerExecutionData
    {
        public string TriggerID;
        public ulong InitiatorPlayerRef;
        public ulong PartnerPlayerRef;
        public long TimestampUtc;
    }

    /// <summary>Broadcast when a trigger sequence has fully completed.</summary>
    [Serializable]
    public struct TriggerCompletionData
    {
        public string TriggerID;
        public bool CompletedNormally;
        public string AbortReason; // null if CompletedNormally
    }

    // ── Static event bus ──────────────────────────────────
    /// <summary>
    /// Static event bus for SDK-wide trigger lifecycle events.
    /// Subscribe from any system without direct component references.
    /// </summary>
    public static class LoveTriggerEventBus
    {
        /// <summary>Fired when a consent request is received by the local player.</summary>
        public static event Action<ConsentRequestData>   OnConsentRequestReceived;

        /// <summary>Fired when a consent response is received by the initiator.</summary>
        public static event Action<ConsentResponseData>  OnConsentResponseReceived;

        /// <summary>Fired when a trigger begins executing (post-consent).</summary>
        public static event Action<TriggerExecutionData> OnTriggerExecutionStarted;

        /// <summary>Fired when a trigger sequence ends for any reason.</summary>
        public static event Action<TriggerCompletionData> OnTriggerCompleted;

        public static void RaiseConsentRequestReceived(ConsentRequestData data)    => OnConsentRequestReceived?.Invoke(data);
        public static void RaiseConsentResponseReceived(ConsentResponseData data)  => OnConsentResponseReceived?.Invoke(data);
        public static void RaiseTriggerExecutionStarted(TriggerExecutionData data) => OnTriggerExecutionStarted?.Invoke(data);
        public static void RaiseTriggerCompleted(TriggerCompletionData data)       => OnTriggerCompleted?.Invoke(data);
    }
}
