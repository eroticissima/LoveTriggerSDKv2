# LoveTriggerSDKv2
> The open-source Unity framework powering [Eroticissima](https://www.eroticissima.wtf) — a consent-first, gender-free interaction system for VR multiplayer experiences.

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-ff1a6e.svg)](https://www.gnu.org/licenses/gpl-3.0)
[![Unity](https://img.shields.io/badge/Unity-2023%2B-white.svg)](https://unity.com)
[![Discord](https://img.shields.io/badge/Discord-SDK%20Access-7289da.svg)](https://discord.gg/KTCdKNnvGb)

---

## What is LoveTriggerSDK?

**LoveTriggers (LTs)** are the interaction primitive of Eroticissima — authored motion sequences that two or more avatars can perform together, ranging from simple touches to full explicit content. The SDK provides:

- **Consent-first architecture** — every trigger is a *request*, never a forced action
- **Full VRIK embodiment** — IK blending in/out around every authored sequence
- **Photon Fusion 2 networking** — multiplayer consent handshake with async RPC pipeline  
- **Timeline + Cinemachine** — cinematic encounter sequencing with dynamic track binding
- **Gender-free avatar system** — non-binary body configs at every layer
- **NPC partner AI** — believable hesitation arc, relationship state, audio banks

---

## Requirements

| Dependency | Version |
|---|---|
| Unity | 2023 LTS+ |
| FinalIK (VRIK) | 2.x |
| Photon Fusion | 2.x |
| Cinemachine | 2.x |
| Unity Input System | 1.7+ |
| XR Management | 4.x (for VR) |

---

## Installation

1. Clone or download this repository
2. Copy the `Assets/LoveTriggerSDK/` folder into your Unity project's `Assets/` directory
3. Install required packages via Package Manager (FinalIK via Asset Store, others via Package Manager)
4. Open `Eroticissima > LoveTriggerSDK > Validate SDK Setup` to confirm everything is wired

---

## Quick Start

### 1. Scene Setup
Add these components to a persistent GameObject in your scene:
```
LoveTriggerSystemManager
LoveTriggerDatabase
PlayerSpawnManager
NetworkedLoveTriggerManager (on a NetworkObject)
UniversalInputSystem
```

### 2. Create a LoveTrigger
```
Assets > Create > LoveTriggerSDK > LoveTrigger
```
Fill in:
- `triggerID` — unique string (e.g. `"lt_kiss_soft"`)
- `singleAnimation` — AnimationData with your clip
- `consentType` — `Manual` for multiplayer, `AlwaysAccept` for solo/NPC
- `requiresConsent` — always `true` unless explicitly solo

### 3. Place an InteractableObject
Add `InteractableObject` to any scene GameObject. Assign your trigger list. The full state machine runs automatically:

```
IDLE → DETECTION → PROMPTED → CONSENT_PENDING → EXECUTING → RESTORING → IDLE
```

### 4. For Timeline encounters
Use `EnhancedLoveTriggerSO` and assign a `TimelineAsset`. Name your tracks `Source` and `Target` — binding is dynamic, no hardcoded references needed.

---

## Architecture

```
LoveTriggerSO (authoring)
  → LoveTriggerDatabase (lookup by ID)
    → LoveTriggerRequest (runtime request)
      → NetworkedLoveTriggerManager (consent gate)
        → UniversalAnimationController / InteractableTimelineController (playback)
          → State restored → locomotion returned
```

See the [interactive SDK dashboard](https://www.eroticissima.wtf/sdk.html) for the full architecture reference.

---

## Layer Reference

| Layer | Classes | Namespace |
|---|---|---|
| Data Authoring | `LoveTriggerSO`, `EnhancedLoveTriggerSO` | `LTSystem.Core` |
| Lookup | `LoveTriggerDatabase` | `LTSystem.Core` |
| Runtime | `LoveTriggerRequest`, `LoveTriggerEvents` | `LTSystem.Core` |
| Network / Consent | `ILoveTriggerNetworkService`, `NetworkedLoveTriggerManager` | `LTSystem.Network` |
| Playback | `UniversalAnimationController`, `InteractableTimelineController` | `LTSystem.Animation` |
| Scene Context | `InteractableObject`, `NPCPartnerController` | `LTSystem.Interaction` |
| Platform / Input | `PlatformManager`, `UniversalInputSystem` | `LTSystem.Core` |
| Player Lifecycle | `PlayerConfiguration`, `PlayerFactory`, `PlayerSpawnManager` | `LTSystem.Core` |
| Bootstrap | `LoveTriggerSystemManager` | `LTSystem.Core` |

---

## Core Principles — Non-Negotiables

1. **Consent is infrastructural.** LoveTriggers are requests, not forced actions. `requiresConsent` is never bypassed — not for NPCs, not for any edge case.
2. **Modularity over hacks.** All temporary workarounds are marked `// TODO: REFACTOR`.
3. **VR embodiment fidelity.** VRIK is always restored after sequences. `BlendVRIKIn` lives in `finally {}`.
4. **Gender and body freedom.** All character systems support non-binary, fully customizable avatar expression.
5. **SDK readiness.** Every public API is typed, documented, and usable without reading the source.

---

## Known Technical Debt

| ID | File | Status | Summary |
|---|---|---|---|
| A | `LoveTriggerSystemManager.cs` | ✅ Fixed | Premature `yield break` made init unreachable |
| B | `PlatformDetectionSystem.cs` | ✅ Resolved | Merged into single `PlatformManager` |
| C | `NetworkedLoveTriggerManager.cs` | ✅ Implemented | Consent gate fully wired |
| D | `InteractableObjectFix.cs` | ✅ Eliminated | `SetupUIElements` is now `protected virtual` |
| E | `LoveTriggerSO.cs` | ✅ Deprecated | `animatorClip` marked `[Obsolete]`, `AnimationData` is canonical |

---

## Contributing

Eroticissima is open to contributors — game developers, 3D artists, sound designers, and researchers exploring intimacy in digital spaces.

- Join [Discord](https://discord.gg/KTCdKNnvGb) to request SDK access and meet the team
- Support on [Patreon](https://www.patreon.com/eroticissima) as a Sugar-Creator for early access
- Licensed under **GNU GPL v3** — the core SDK is and will remain free

**Contact:** eroticissima@gmail.com

---

*Eroticissima © Miyö Van Stenis — VR game · art research project · open-source SDK*
