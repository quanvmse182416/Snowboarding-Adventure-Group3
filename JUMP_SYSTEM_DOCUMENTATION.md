# Jump System Implementation

## Overview
The jump system has been completely refactored to meet the following requirements:
- **Unlimited jumps when grounded** (only limited by cooldown)
- **Maximum 3 jumps when airborne**
- **Backward jumps with S/Down Arrow keys**
- **Reliable ground detection and jump reset**

## How It Works

### Ground vs Air Jump Logic
1. **When Grounded (`isGrounded = true`)**:
   - Player can jump unlimited times
   - Only restriction is the cooldown timer (`jumpCooldown`)
   - Jump counter is **not** incremented for ground jumps
   - Jump counter is reset to 0 when landing

2. **When Airborne (`isGrounded = false`)**:
   - Player is limited to 3 air jumps maximum
   - Each air jump increments the `currentJumps` counter
   - Once `currentJumps >= maxJumps`, no more air jumps allowed

### Key Methods

#### `CanJump()`
- **Grounded**: Returns `true` if cooldown has passed (unlimited jumps)
- **Airborne**: Returns `true` if `currentJumps < maxJumps` AND cooldown has passed

#### `PerformJump()`
- Only increments `currentJumps` when `!isGrounded` (air jumps only)
- Ground jumps don't count toward the air jump limit

#### `OnGroundEnter()`
- Called by `GroundDetector` when player touches ground
- Resets `currentJumps = 0`
- Sets `isGrounded = true`

#### `OnGroundExit()`
- Called by `GroundDetector` when player leaves ground
- Sets `isGrounded = false`

## Features

### Normal Jumps
- **Space Bar**: Performs upward jump
- **Force**: Configurable via `jumpForce`
- **Cooldown**: Configurable via `jumpCooldown` (default 0.1s)

### Backward Jumps
- **S Key or Down Arrow**: Performs backward jump
- **Enabled/Disabled**: Configurable via `enableBackwardJump`
- **Force**: Configurable via `backwardJumpForce`
- **Direction**: Configurable via `backwardJumpRatio` and `backwardUpwardRatio`

### Ground Detection
- Uses a separate `GroundChecker` GameObject with `CircleCollider2D`
- Positioned at `groundCheckOffset` relative to player
- Uses `GroundDetector` script to trigger `OnGroundEnter`/`OnGroundExit`

## Testing the System

### Manual Testing
1. **Test Unlimited Ground Jumps**:
   - Stand on ground
   - Rapidly press Space Bar
   - Should be able to jump as many times as desired (with cooldown)

2. **Test Limited Air Jumps**:
   - Jump off a platform
   - While in air, press Space Bar multiple times
   - Should be limited to 3 air jumps, then no more jumps until landing

3. **Test Jump Reset on Landing**:
   - Use all 3 air jumps while airborne
   - Land on ground
   - Should immediately be able to jump unlimited times again

### Using JumpSystemTester
Add the `JumpSystemTester` script to your player GameObject for easier testing:

- **Tab**: Display current jump system status
- **G**: Force test grounded state (unlimited jumps)
- **A**: Force test airborne state (3 jump limit)
- **R**: Reset jump counter manually

### Debug Information
The system provides extensive debug logging:
- Jump attempts and results
- Ground state changes
- Jump counter updates
- Velocity information

## Configuration

### Inspector Settings
All key parameters are exposed in the Inspector:

#### Jump Settings
- `jumpForce`: Base jump force (default: 15)
- `maxJumps`: Maximum air jumps (default: 3)
- `jumpCooldown`: Minimum time between jumps (default: 0.1s)
- `maxJumpVelocity`: Cap on upward velocity (default: 20)

#### Backward Jump Settings
- `enableBackwardJump`: Enable/disable backward jumps
- `backwardJumpForce`: Force for backward jumps
- `backwardJumpRatio`: Backward movement component (0.6 = 60% backward)
- `backwardUpwardRatio`: Upward movement component (0.8 = 80% upward)

#### Ground Detection
- `groundCheckRadius`: Size of ground detection area
- `groundCheckOffset`: Position of ground detector relative to player
- `groundLayerMask`: Which layers count as ground

## Removed Features
The following overcomplicated features have been removed:
- Automatic jump reset based on time/velocity
- "Stuck" detection and forced unstuck logic
- Periodic jump counter resets
- Physics-based automatic resets

## Context Menu Debug Tools
Right-click on the Jump script in the Inspector for debug options:
- Force ground state
- Reset jump counter
- Test jump reset logic
- Display current status

## Integration Notes
- Works with both Legacy Input System and new Input System
- Compatible with existing `GroundDetector` script
- No changes required to other scripts
- Maintains backward compatibility with existing prefabs
