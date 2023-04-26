# Custom Mortar Shapes Guide

## Introduction
Do you want to see your own mods grindable items in the mortar instead of an ugly default shape? You can do that! 

As of Ancient Tools 1.5.2, collectibles now use a 'mortarProperties' attribute to control various aspects of a collectible within a mortar.

Required properties are as follows: 

shapePath: The location of a shape created specifically for the mortar. Import the mortar into VSModelCreator and fit your shape into it, delete the mortar, then export. Make sure to include the mod domain in the shape path!
    Example: 
        shapePath: "ancienttools:shapes/block/mortar/resourceshapes/resource_bricklayers_frit_baked"

texturePaths: An array of textures for your shape. Each array item must have a code string, and a path string. The code should match a texture code within the above mortar shape. Make sure to include the mod domain in the texture path!
    Example: 
        texturePaths: [{ code: "frit", path: "bricklayers:item/resource/frit/baked/{type}" }],

groundStack: The collectible that an item within the mortar will grind into. This can be different from the vanilla groundedStack used with the quern! Again, don't forget the domain!
    Example: 
        groundStack: { type: "item", code: "bricklayers:frit-ground-{type}" },

resultQuantity: The number of items that will be given upon successful grind.
