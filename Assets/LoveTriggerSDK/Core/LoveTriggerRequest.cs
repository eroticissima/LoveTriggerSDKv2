using System;
using UnityEngine;

namespace LTSystem.Core
{
    /// <summary>
    /// Runtime representation of a trigger request created when a player
    /// selects a LoveTrigger. Passed through the consent gate and on to
    /// the animation/Timeline controllers.
    /// </summary>
    public class LoveTriggerRequest
    {
        // ── Identity ──────────────────────────────
        /// <summary>Unique ID for this specific runtime request.</summary>
        public string RequestID { get; }

        /// <summary>The trigger definition being requested.</summary>
        public LoveTriggerSO Trigger { get; }

        // ── Participants ──────────────────────────
        /// <summary>GameObject of the initiating player/avatar.</summary>
        public GameObject Initiator { get; }

        /// <summary>GameObject of the partner player/avatar. Null for solo triggers.</summary>
        public GameObject Partner { get; set; }

        // ── Network ───────────────────────────────
        /// <summary>Network player ref of initiator (Photon Fusion ulong).</summary>
        public ulong InitiatorPlayerRef { get; set; }

        /// <summary>Network player ref of partner. 0 for solo / NPC.</summary>
        public ulong PartnerPlayerRef { get; set; }

        /// <summary>
        /// Matches the RequestID used in ConsentRequestData / ConsentResponseData
        /// for server-side correlation.
        /// </summary>
        public string NetworkRequestID => RequestID;

        // ── State ─────────────────────────────────
        /// <summary>Current lifecycle state of this request.</summary>
        public EncounterState State { get; set; } = EncounterState.Idle;

        /// <summary>UTC timestamp when this request was created.</summary>
        public long CreatedAtUtc { get; }

        // ── Constructor ───────────────────────────
        public LoveTriggerRequest(LoveTriggerSO trigger, GameObject initiator, GameObject partner = null)
        {
            RequestID    = Guid.NewGuid().ToString();
            Trigger      = trigger ?? throw new ArgumentNullException(nameof(trigger));
            Initiator    = initiator;
            Partner      = partner;
            CreatedAtUtc = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        // ── Helpers ───────────────────────────────
        /// <summary>True if this trigger requires a partner and one is assigned.</summary>
        public bool HasPartner => Partner != null;

        /// <summary>True if consent must be obtained before execution.</summary>
        public bool RequiresConsent => Trigger.requiresConsent
                                    && Trigger.consentType == ConsentType.Manual;

        public override string ToString() =>
            $"[LoveTriggerRequest] ID={RequestID} Trigger={Trigger.triggerID} State={State}";
    }
}
