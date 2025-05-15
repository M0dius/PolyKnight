# PolyKnight

## Overview
PolyKnight is a 3D action-adventure game where players explore dungeons, battle goblins, and collect treasures. Navigate through multiple levels, defeat enemies including special bosses like the Goblin King, and unlock gates using keys found throughout the game.

## Game Features

### Player Mechanics
- **Movement**: Fluid character movement using a custom character controller
- **Combat**: Melee attack system with damage mechanics
- **Health System**: Player health with damage feedback and death handling
- **Inventory**: Collect keys and coins throughout your adventure

### Enemy Types
- **Regular Goblins**: Standard enemies with basic AI and attack patterns
- **Goblin Prince**: Mid-tier enemy with blue skin, silver crown, and enhanced abilities
- **Goblin King**: Boss enemy with red skin, gold crown with ruby gem, and special abilities including minion summoning and rage mode

### Level Design
- **Multiple Levels**: 6 distinct levels with increasing difficulty
- **Level Gates**: Locked gates requiring keys to progress
- **Traps**: Hazards that damage the player
- **Treasure Chests**: Containers that can be opened to obtain coins

### Game Systems
- **Coin Collection**: Gather coins from defeated enemies and chests
- **Audio System**: Background music and sound effects for actions and events
- **UI Elements**: Health display, coin counter, and interactive menus
- **Save System**: Progress tracking across levels

## Controls
- **WASD**: Movement
- **Mouse**: Camera control
- **Left Click**: Attack
- **E**: Interact with objects (chests, gates)
- **Esc**: Pause menu

## Technical Details

### Scripts
- **Player Scripts**: CharacterController, PlayerAttack, PlayerHealth
- **Enemy Scripts**: GoblinAI, GoblinPrinceAI, GoblinKingAI
- **Interaction Scripts**: ChestInteraction, KeyItem, LevelGate
- **System Scripts**: GameManager, AudioManager, CoinManager

### Assets
- Custom 3D models and animations
- Particle effects for blood and special abilities
- Sound effects and background music

## Development Team
- Mohammed Taha 202200948
- Abdullah Bakhsh 202201356
- Ali Juma 202200673
- Hussain Abdulnabi 202200487
- Mohamed AlAlawi 202202097
- Ahmed Abdulla 202002156

## Installation
1. Clone the repository
2. Open the project in Unity (version 2021.3 or later recommended)
3. Open File -> Build And Run -> Choose a Name and Start
