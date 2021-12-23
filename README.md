## Ancient Tools

 A collection of useful tools from ancient times.

 ![Screenshot](resources/images/mortars.png)

### Description

 Hello and welcome to the GitHub repository for Ancient Tools. This project is a mod for the survival adventure game Vintage Story which adds an assortment of immersive, interactive tools to the game.

### Releases
#### Vintage Story 1.16.0+
[Version 1.3.0](https://github.com/TaskaRaine/Ancient-Tools/releases/download/1.3.0/AncientTools_V1.3.0.zip)

#### Vintage Story 1.15.0+
[Version 1.2.4](https://github.com/TaskaRaine/Ancient-Tools/releases/download/1.2.4/AncientTools_V1.2.4.zip)

[Version 1.2.3](https://github.com/TaskaRaine/Ancient-Tools/releases/download/1.2.3/AncientTools_V1.2.3.zip)

[Version 1.2.2](https://github.com/TaskaRaine/Ancient-Tools/releases/download/1.2.2/AncientTools_V1.2.2.zip)

[Version 1.2.1](https://github.com/TaskaRaine/Ancient-Tools/releases/download/1.2.1/AncientTools_V1.2.1.zip)

[Version 1.2.0](https://github.com/TaskaRaine/Ancient-Tools/releases/download/1.2.0/AncientTools_V1.2.0.zip)

[Version 1.1.0](https://github.com/TaskaRaine/Ancient-Tools/releases/download/1.1.0/AncientTools_V1.1.0.zip)

[Version 1.0.3](https://github.com/TaskaRaine/Ancient-Tools/releases/download/1.0.3/AncientTools_V1.0.3.zip)

[Version 1.0.2](https://github.com/TaskaRaine/Ancient-Tools/releases/download/1.0.2/AncientTools_V1.0.2.zip)

#### Vintage Story 1.14.10
[Version 1.0.1](https://github.com/TaskaRaine/Ancient-Tools/releases/download/1.0.1/AncientTools_V1.0.1.zip)

[Version 1.0.0](https://github.com/TaskaRaine/Ancient-Tools/releases/download/1.0.0/AncientTools_V1.0.0.zip)

### Change Log

#### Version 1.3.0
--Brain Tanning
Added a whole new technique for creating leather inspired by Native American techniques, Brain Tanning.

Added a stretching frame block that can be used to scrape soaked hides. All hides can be stretched on the frame for decoration.

Added a hide water sack block that is used to convert raw hide into soaked hide.

Added two new hide types required for the brain tanning process, a 'brained hide' and a 'smoked hide'.

All wild animals have the chance to drop a new item when killed, a brain!

Added a new liquid, braining solution, crafted from brain and water in a barrel.

--Bark
Craft logs with an adze in the crafting grid should now return the correct number of bark pieces.

--Adze
Fixed a crash related to block selection while the adze is in use.

#### Version 1.2.4
The bark recipe output count is now based on the config variable BarkPerLog.

#### Version 1.2.3
Implemented various configuration options for Ancient Tools, including the ability to disable bark bread and salves. Other options can be seen in the config file that generates on first load of this version.

Carry Capacity 'Carryable' behaviour is now only attached to bark baskets when Carry Capacity is actually installed.

#### Version 1.2.2
Salve containers now have world interaction information that should help clarify the salve creation process.

The tooltip shown when beeswax is inserted into an empty salve container should now show correct information.

Bark baskets now are properly named immediately after crafting.

Fixed an issue where bark and beeswax could be combined to create barkoil, instead of bark and fat as intended.

#### Version 1.2.1
--Adze(and related)
Stripped logs now only drop the ud directional variant when broken. 

Created a compatibility recipe for Building+ derbarked wooden supports. They can be crafted directly by using stripped logs.

Added the molds to the Ancient Tools creative tab.

--Curing Rack(and related)
Fixed an issue where the curing hook GUI image hid the number of hooks in the stack.

Improved the block interaction tooltips for the curing rack.

#### Version 1.2.0
--Adze(and related)
Added an adze tool(with multiple variants) that can strip bark from trees.

Added stone(created through knapping) and metal(created through smithing/smelting) adze heads that can be used to create adze tools.

Added a stripped log block with variants matching all tree types. These stripped logs can used for decorative purposes, chiseled, used for firewood, or planks.

Added bark for each tree type. These have various crafting purposes, and can be used as a firestarter/kindling in place of firewood. Short burn time.

Birch and pine bark can be ground down into flour, then crafted into a dough that can then be baked into breads with the clay oven.

Healing salves made from oils extracted from birch/pine bark can be crafted for significant healing over time. There's a detailed guide in-game describing this process.

All bark can be used to create cute little bark baskets that can store items. Equivalent to the reed basket. Carry Capacity compatible.

--Curing Rack
Hopefully fixed a text issue in the curing rack blockinfo.

--Mortar and Pestle
Added shapes for pine bark and birch bark

#### Version 1.1.0
---Curing Rack
Implements a salted meat item that is intended to be cured on a rack. Redmeat, poultry and bushmeant. Can be cooked.

Implements a curing rack block made of any wood type that can be visually extended across multple blocks and/or hung with the proper support.

Bone curing hooks can be crafted and placed on the rack, then meat can be added.

Rack meat perish rate is modified based on the environment in which it is placed... will need a controlled environment to properly cure.

Meats attached to the rack will cure over 20 ingame days 

---Mortar and Pestle
Fixed a rare crash that occurred when an item rots in the mortar, and is then attempted to be ground down.

Pestle rotation now syncs upon starting and finishing a grind.

#### Version 1.0.3
Particle colours should now sync between all players.

FPS while interacting with the mortar is vastly improved.

The pestle while in the mortar has its own renderer which means the animation is far smoother.

Pestle animation should be seen by all players(rotation is not synced, however).


#### Version 1.0.2
Compiled mod to be compatible with Vintage Story 1.15.0

New grindable items introduced in Vintage Story 1.15.0 are grindable in the mortar(previously implemented, but inaccessible)

Fixed an exploit where players could insert old foods into the mortar then pull out fresh food. 


#### Version 1.0.1
Adds a chalk stone shape for the mortar, forgotten with the first release.

The mortar now finds shapes based on the the FULL item code, not just first and last code parts.


#### Version 1.0.0
Initial implementation of the mortar/pestle.
