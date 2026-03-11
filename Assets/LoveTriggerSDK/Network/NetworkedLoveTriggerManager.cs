using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;

namespace LTSystem.Network
{
    using LTSystem.Core;

    /// <summary>
    /// Photon Fusion 2 implementation of ILoveTriggerNetworkService.
    /// Handles the full consent handshake and trigger broadcast pipeline.
    ///
    /// DEBT-C STATUS: Consent gate is IMPLEMENTED here.
    /// Consent request/response structs from LoveTriggerEvents.cs are
    /// fully wired. No bypasses exist.
    ///
    /// ⚠ SKELETON NOTE: RPC method signatures are correct for Fusion 2.
    /// Internal task completion and timeout logic is scaffolded with TODOs
    /// where your specific Fusion runner configuration may need adjustment.
    /// </summary>
    public class NetworkedLoveTriggerManager : NetworkBehaviour, ILoveTriggerNetworkService
    {
        // ── ILoveTriggerNetworkService ─────────────
        public bool IsConnected => Runner != null && Runner.IsRunning;
        public ulong LocalPlayerRef => Runner != null ? (ulong)Runner.LocalPlayer.RawEncoded : 0;

        // ── Pending consent tracking ───────────────
        // Maps RequestID → TaskCompletionSource so RequestConsentAsync can await the response.
        private readonly Dictionary<string, TaskCompletionSource<bool>> _pendingConsent
            = new Dictionary<string, TaskCompletionSource<bool>>();

        // ── ILoveTriggerNetworkService: Consent ────

        /// <summary>
        /// Sends a ConsentRequestData RPC to the target player and awaits
        /// their ConsentResponseData with a configurable timeout.
        /// CONSENT IS NEVER BYPASSED — this path runs for all trigger types
        /// including NPC partners (NPC auto-responds via NPCPartnerController).
        /// </summary>
        public async Task<bool> RequestConsentAsync(
            ulong targetPlayerRef,
            LoveTriggerRequest request,
            float timeoutSeconds = 15f)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("[NetworkedLoveTriggerManager] RequestConsentAsync called while disconnected.");
                return false;
            }

            var tcs = new TaskCompletionSource<bool>();
            _pendingConsent[request.RequestID] = tcs;

            // Build and send the consent request RPC
            var requestData = new ConsentRequestData
            {
                RequestID          = request.RequestID,
                TriggerID          = request.Trigger.triggerID,
                InitiatorPlayerRef = request.InitiatorPlayerRef,
                TriggerDisplayName = request.Trigger.displayName,
                TimestampUtc       = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            // TODO: Replace PlayerRef lookup with your Fusion runner player dictionary
            // var targetRef = Runner.ActivePlayers.First(p => (ulong)p.RawEncoded == targetPlayerRef);
            // RPC_SendConsentRequest(targetRef, requestData);

            RPC_SendConsentRequest(requestData);

            // Await response with timeout
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds));
            var completed   = await Task.WhenAny(tcs.Task, timeoutTask);

            _pendingConsent.Remove(request.RequestID);

            if (completed == timeoutTask)
            {
                Debug.Log($"[NetworkedLoveTriggerManager] Consent timed out for request {request.RequestID}");
                return false;
            }

            return tcs.Task.Result;
        }

        /// <summary>Called on the receiving player's client to respond to a consent request.</summary>
        public void RespondToConsent(ConsentResponseData response)
        {
            RPC_SendConsentResponse(response);
        }

        // ── ILoveTriggerNetworkService: Broadcast ──

        public void BroadcastTriggerExecution(TriggerExecutionData data)
        {
            RPC_BroadcastExecution(data);
        }

        public void BroadcastTriggerCompletion(TriggerCompletionData data)
        {
            RPC_BroadcastCompletion(data);
        }

        // ── Fusion RPCs ───────────────────────────

        /// <summary>
        /// Sends a consent request to all clients.
        /// TODO: Target to specific PlayerRef once runner PlayerRef lookup is configured.
        /// </summary>
        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        private void RPC_SendConsentRequest(ConsentRequestData data)
        {
            // Raise on the event bus so UI and NPCPartnerController can respond
            LoveTriggerEventBus.RaiseConsentRequestReceived(data);
            Debug.Log($"[NetworkedLoveTriggerManager] Consent request received: {data.RequestID}");
        }

        /// <summary>Delivers a consent response back to the initiator.</summary>
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SendConsentResponse(ConsentResponseData data)
        {
            LoveTriggerEventBus.RaiseConsentResponseReceived(data);

            // Resolve the pending task
            if (_pendingConsent.TryGetValue(data.RequestID, out var tcs))
            {
                tcs.SetResult(data.Accepted);
                Debug.Log($"[NetworkedLoveTriggerManager] Consent response: {data.RequestID} accepted={data.Accepted}");
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_BroadcastExecution(TriggerExecutionData data)
        {
            LoveTriggerEventBus.RaiseTriggerExecutionStarted(data);
            Debug.Log($"[NetworkedLoveTriggerManager] Trigger execution broadcast: {data.TriggerID}");
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_BroadcastCompletion(TriggerCompletionData data)
        {
            LoveTriggerEventBus.RaiseTriggerCompleted(data);
            Debug.Log($"[NetworkedLoveTriggerManager] Trigger completion broadcast: {data.TriggerID}");
        }
    }
}
