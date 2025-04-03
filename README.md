# TurnWeaver - Unity Ink Integration Example

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

TurnWeaver serves as a practical example project demonstrating how to integrate Inkle's Ink narrative scripting language
with common RPG systems within the Unity engine.

**Please Note:** This project is currently in an **early stage of development** and is **incomplete**. While the core
interaction between Ink and the RPG systems (Inventory, Quests, Dialogue) is functional, many aspects are not fully
built out. It is intended more as a learning resource or starting point than a production-ready framework.

**GitHub Repository:** [https://github.com/hoelzl/TurnWeaver](https://github.com/hoelzl/TurnWeaver)

## Core Features Demonstrated

* **Ink Dialogue:** Integration with Ink for branching conversations.
* **RPG Systems:** Basic Inventory, Quest, and Interaction systems.
* **Ink ↔ System Communication:** Examples of how Ink scripts can query and modify game state (inventory, quests) via
  external functions.
* **UI Toolkit:** User interface built using Unity's UI Toolkit.

## Getting Started

### Prerequisites

* **Unity:** A recent version of the Unity Hub and Unity Editor (currently V6.42f1).
* **Git:** A Git client (like Git Bash, GitHub Desktop) to clone the repository.
* **Ink Integration:** The Ink integration package is included in the project sources, so you don't have to install it.

### Installation

1. **Clone the Repository:**
   Open your terminal or Git client and run:
   ```bash
   git clone [https://github.com/hoelzl/TurnWeaver.git](https://github.com/hoelzl/TurnWeaver.git)
   ```
   Alternatively, download the project as a ZIP file from the GitHub page.

2. **Open in Unity Hub:**
    * Launch Unity Hub.
    * Click "Open" or "Add project from disk".
    * Navigate to the cloned `TurnWeaver` directory and select it.
    * Unity will import the project. This might take a few minutes.

### Running the Project

1. Once the project is open in the Unity Editor, locate the main scene (`Assets/Scenes/SampleScene`).
2. Open the main scene.
3. Press the **Play** button at the top of the Unity Editor.
4. Interact with the NPCs:
    - Click to move or interact with the highlighted character
    - `I` opens the inventory
    - `J` opens the quest list
    - You can rotate the camera by holding the right mouse button and zoom in/out using the mouse wheel

## Modifying the Game

### Modifying Stories (Ink)

* **Location:** Dialogue scripts are written in the Ink language and stored as `.ink` files in
  `Assets/Data/Ink Dialogue/`.
* **Editing:** You can edit `.ink` files using any text editor or dedicated tools like Inky.
* **Structure:** The system uses an `InkStoryManager` to handle story assets and an `InkDialogueController` to manage
  sessions. NPCs link to stories via the `NPCInkData` component, which specifies the story key, flow name, and starting
  path.
* **Compilation:** The Ink Unity Integration package automatically compiles `.ink` files into `.json` assets (e.g.,
  `default_story.json`) which are used at runtime. Ensure this package is installed correctly.
* **Functions:** You can call C# functions from Ink using the `EXTERNAL` keyword in Ink and binding them in
  `InkFunctionRegistry.cs`.
* A number of example functions are available. You can find the list in `Assets/Data/Ink Dialogue/functions.ink`.
* You can reference game objects via ID. 
  * The map of all objects currently available via ID is in `Assets/Data/Ink Dialogue/unique_ids.ink`
  * All assets with an `UniqueId` component can be referenced.
  * To generate the list in `unique_ids.ink` use the editor tool in the `Tools/Generate Unique ID List`.

### Adding New Items

1. **Create Item Definition (ScriptableObject):**
    * In the Unity Editor, navigate to `Assets/Data/Inventory/Items/` (or a subfolder).
    * Right-click -> Create -> Inventory -> Item / Equipment / Consumable.
    * Configure the properties (name, description, icon, weight, value, stackability, category, specific stats) in the
      Inspector.
    * Ensure the `Unique Item Name` is distinct if the display `Item Name` might be shared. It can be left empty if `Item Name` is already unique.
2. **Add to Item Database:**
    * Select the `Item Database` asset located at `Assets/Data/Inventory/Item Database.asset`.
    * Use the custom editor button "Find and Populate All Items" to automatically scan the project and add your new
      item (and others) to the database. Alternatively, you can manually add it to the `allItems` list in the Inspector.
3. **Use the Item:** You can now add the item to:
    * `RPGInventory` components (via the `initialItems` list or `InventoryPresetSO`).
    * `InventoryPresetSO` assets (in `Assets/Data/Inventory/Presets/`) for setting up initial inventories or shop stock.
    * Reward systems or other game logic by referencing its `UniqueItemName` and using `RPGInventory` methods like
      `AddItem`.
    * You can add items to an inventory, remove them from an inventory or get the number of items a user has by using `add_item()`, `remove_item()` or `count_item()` from ink scripts.
    * These methods take the unique ID of the owner of the inventory component, the unique item name, and for add/remove the number of items.

### Adding New Quests

1. **Create Quest Definition (ScriptableObject):**
    * Navigate to `Assets/Data/Quests/`.
    * Right-click -> Create -> Quests -> Quest Definition.
    * Configure the `QuestSO` asset:
        * Set a unique `Quest Short Name` (used for scripting/Ink).
        * Set the display `Quest Name`.
        * Define the `Tasks` by adding elements to the list. Each task needs a unique `Task Short Name` within the quest
          and a display `Task Name`.
2. **Register with QuestManager:**
    * Select the GameObject in your scene that holds the `QuestManager` component.
    * Add your new `QuestSO` asset to the `Quests` list in the `QuestManager`'s Inspector.
3. **Triggering & Updating Quests:**
    * Use Ink functions (bound in `InkFunctionRegistry`) like `set_quest_status` and `set_task_status` to start quests
      and update task progress from dialogues.
    * Implement game logic (e.g., defeating an enemy, collecting an item, reaching a location) that calls
      `QuestManager.Instance.SetTaskStatus(...)` or `QuestManager.Instance.SetQuestStatus(...)`.
    * From ink you can use `set_quest_status()`, `get_quest_status()`, `set_task_status`, and `get_task_status()`.
    * The `QuestLogLayer` UI automatically reflects the status changes tracked by the `QuestManager`.

## Technical Details & Architecture (for Programmers)

This section provides a more detailed look at the implemented features and the underlying code structure.

### Detailed Features

* **Player Controller:** Handles input and movement using Unity's Input System and NavMeshAgent. Basic interaction
  triggering.
* **Interaction System:** Generic system for interacting with objects and NPCs (`IInteractable`, `InteractionOptionSO`,
  `InteractionManager`). Supports highlighting interactable objects.
* **Inventory System:** Robust inventory (`RPGInventory`) supporting stackable/non-stackable items (`ItemSO`,
  `ItemStack`), equipment (`Equipment`, `EquipmentItemSO`), consumables (`ConsumableItemSO`), currency, weight limits,
  and slots. Includes shops (`Shop`) and inventory presets (`InventoryPresetSO`). Uses an `ItemDatabase` for item
  management.
* **Dialogue System:** Integrated with Inkle's Ink (`.ink` files in `Assets/Data/Ink Dialogue/`). Features an
  `InkStoryManager`, `InkDialogueController`, `StoryProcessor` for handling story flow, choices, and external function
  calls (`InkFunctionRegistry`). NPC-specific data is managed via `NPCInkData`.
* **Quest System:** Manage quests (`QuestSO`) and tasks (`QuestTask`) with status tracking via the `QuestManager`.
* **UI System:** Modular UI built with UI Toolkit, managed by `UILayerManager` and `UIManager`. Includes layers for
  interactions, descriptions, inventory, item details, shops, quantity selection, dialogue, and quests.
* **Turn Management:** Basic turn handling framework (`TurnManager`).
* **Core Utilities:** Includes a `UniqueId` system for referencing scene objects.
* **Editor Tools:** Custom editors for Inventory (`InventoryEditor`), Presets (`InventoryPresetEditor`), Item Database (
  `ItemDatabaseEditor`), and a tool to generate Unique ID lists (`UniqueIdListGenerator`).

### System Architecture Overview

The project is organized into several key namespaces/folders within `Assets/Scripts/`:

* **Core:** Basic utilities like the `UniqueId` system for scene object referencing.
* **Input:** Handles player input using Unity's Input System via `RPGInputActions` and `PlayerInputHandler`.
* **Player:** Contains `PlayerController` (main coordinator), `PlayerMovement` (using NavMeshAgent), and
  `PlayerInteractionController`.
* **Interaction:** Defines interfaces (`IInteractable`, `IInteractionOption`, `IInteractionSource`), ScriptableObjects
  for options (`InteractionOptionSO`), and the `InteractionManager` to orchestrate interactions. Includes highlighting.
  Specific interaction types (Examine, Open, InkDialogue, Inventory, Shop, Transfer) are implemented as
  `InteractionOptionSO` derivatives.
* **Inventory:** Core logic in `RPGInventory`. Item definitions use `ItemSO` and its subclasses (`EquipmentItemSO`,
  `ConsumableItemSO`). Stacks are managed by `ItemStack`. `Equipment` handles equipped items. `Shop` provides trading
  logic. `ItemDatabase` stores all item definitions. `InventoryPresetSO` allows defining reusable item collections.
* **Dialogue (Ink):** Relies heavily on the Ink runtime. `InkStoryManager` loads and manages Ink `Story` objects.
  `InkDialogueController` initiates and coordinates dialogue sessions. `StoryProcessor` steps through the Ink story
  content and choices. `NPCInkData` configures NPC dialogue behavior. `InkFunctionRegistry` binds C# functions for Ink
  EXTERNAL calls.
* **Quests:** Defines quest structure (`QuestSO`, `QuestTask`) and status enums (`QuestStatus`, `TaskStatus`).
  `QuestManager` tracks the runtime state of all quests.
* **TurnHandling:** Contains the `TurnManager` for managing player and potentially NPC turns.
* **UI:** Built using UI Toolkit. `UILayerManager` handles a stack of UI layers. `UIManager` provides static methods to
  easily show specific layers (e.g., `UIManager.ShowInventory(...)`). Individual UI screens are implemented as
  subclasses of `UILayer` (e.g., `InventoryLayer`, `DialogueUILayer`, `QuestLogLayer`).
* **Editor:** Contains custom editor scripts to enhance usability in the Unity Editor, such as for the `RPGInventory`,
  `InventoryPresetSO`, and `ItemDatabase`.

## License

This project is licensed under the **MIT License**. See the LICENSE file (if included in the repository)
or [https://opensource.org/licenses/MIT](https://opensource.org/licenses/MIT) for details.

## Copyright

Copyright (c) Dr. Matthias Hölzl
