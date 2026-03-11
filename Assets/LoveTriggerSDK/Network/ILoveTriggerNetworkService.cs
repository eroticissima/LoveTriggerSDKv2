using System.Threading.Tasks;

namespace LTSystem.Network
{
    using LTSystem.Core;

    /// <summary>
    /// Contract for the network consent and execution service.
    /// Implement this interface to swap between Photon Fusion,
    /// Mirror, or any other networking backend without touching
    /// SDK consumers.
    /// </summary>
    public interface ILoveTriggerNetworkService
    {
        /// <summary>True when the local player is connected and the service is ready.</summary>
        bool IsConnected { get; }

        /// <summary>Network player ref of the local player.</summary>
        ulong LocalPlayerRef { get; }

        /// <summary>
        /// Sends a consent request to the target player and awaits their response.
        /// Returns true if the target accepted within the timeout window.
        /// </summary>
        Task<bool> RequestConsentAsync(ulong targetPlayerRef, LoveTriggerRequest request, float timeoutSeconds = 15f);

        /// <summary>
        /// Sends the acceptance or rejection response back to the initiator.
        /// Called on the receiving player's side.
        /// </summary>
        void RespondToConsent(ConsentResponseData response);

        /// <summary>
        /// Broadcasts trigger execution start to all clients in the room.
        /// Called once consent is confirmed.
        /// </summary>
        void BroadcastTriggerExecution(TriggerExecutionData data);

        /// <summary>
        /// Broadcasts trigger completion to all clients in the room.
        /// </summary>
        void BroadcastTriggerCompletion(TriggerCompletionData data);
    }
}
