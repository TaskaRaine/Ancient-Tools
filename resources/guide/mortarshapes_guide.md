# Custom Mortar Shapes Guide

## Introduction
Do you want to see your own mods grindable items in the mortar instead of an ugly default shape? You can do that! 

This guide works with Ancient Tools 1.2.1 and later.

## Here's How
A few steps is all it takes. 

0: The mortar, by design, only accepts ITEMS, not blocks. Additionally, an item will only be placed in the mortar if it has 'grindedprops'.

1: Import the mortar shape found in ancienttools/shapes/block/mortar into VS ModelCreator. 

2: Import or create your own shape independant of the mortar heirarchy.

3: In your mod assets folder create an 'ancienttools' folder alongside your own mod assets folder.

4: Create another folder called 'shapes' within the 'ancienttools' folder.

5. The shapes folder should have additional folders nested withing as follows: block/mortar/resourceshapes. See the below image for an idea of what you should have.

![Screenshot](ancienttoolsfolderstructure.png)

6. Back in the model creator, remove the mortar shape then export your resource shape into the resourceshapes folder. IMPORTANT: The file name MUST be named a specific way. The name will always begin with 'resource', then must be followed up by the domain that the resource comes from, followed by the resources specific code path. As such: 'resource_domain_firstcodepart_secondcodepart.json'. If I wanted to create a shape for rice grain then the file name would be 'resource_game_grain_rice.json'.

7. Remove any textures references at the top of the shape file. Ensure that your shape faces have a unique "texture" value, though. "texture": "#yourmod_resource" for example.

8. Make sure that you have a texture file for the resource shape somewhere. Either in your own assets somewhere, or even in an ancienttools/textures folder would be fine.

9. In your own assets folder, ensure that you have created a 'patches' folder. You will again need a nested folder structure as follows:

![Screenshot](patchesfolderstructure.png)
    
10. Create a mortar.json file with the following structure:
        
        [
                {
                        op: "add",
                        path: "/textures/yourmod_resource",
                        file: "ancienttools:blocktypes/mortar",
                        value: {
                                base: "yourmodid:block/stone/rock/bauxite4"
                        }
                }
        ]

'yourmod_resource' will be that value you input into the shape file texture location in step 7.
'yourmodid:block/stone/rock/bauxite4' will differ depending on where the texture you want to use for the shape is located.
                
That should really be it. Your shape/texture should now show up in the mortar anytime you place your resource inside. 
If you have any questions, need further guidance, or if you feel this guide needs better explanation please contact Taska on Discord and let her know.

An example mod that works with the mortar can be found here: https://www.dropbox.com/s/6ai9t7x7x8xzmfg/testaddshape.zip?dl=0
Search for 'testgrindable' ingame to find the item.