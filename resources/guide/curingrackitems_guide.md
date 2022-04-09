# Custom Curing Rack Items

## Introduction
The Ancient Tools curing rack has been reworked in Ancient Tools 1.4.1 to be more versatile. Now, it can be used to cure and dry using vanilla mechanics, in addition to the custom curing implementation added by Ancient Tools.

You are now able to hang your own mod items on the rack. All you need are the proper attributes/transforms.

## Here's How

1. Give the items that need to be hung an 'onCuringRackProps' object. Both the originally placed version of the item and the transformed version will need this.

2. You will need to define a 'transform' object within the onCuringRackProps object. It's best to start by getting your rotation aligned first and transformed such that the meshes 'spin' around the curing hooks. Then, do fine tuned adjustments using the offset variables. 
The following variables can be set to position your object meshes:

	Scale: Affects the size of the meshes hung on the curing rack.
	`scale: { x: 0.0, y: 0.0, z: 0.0 }`	

	Rotation: Rotates meshes hung on the curing rack. 
	`rotation: { x: 0.0, y: 0.0, z: 0.0}`

	Translation: Can be used to move the meshes on the curing rack.
	`translation: { x: 0.0, y: 0.0, z: 0.0}`

	Depth Offset: Push all the meshes outward from center.
	'depthOffset: 0.0'

	Horizontal Offset: Shift meshes horizontally so that things can align more centrally if needed
	'horizontalOffset: 0.0'

	Vertical Offset: Lift or lower the meshes on the rack
	'verticalOffset: 0.0'

	Rotate Around: The axis to rotate the meshes around. You'll likely want to use the 3D directional vectors of up(shown below), forward, and right.
	`rotateAround: { x: 0, y: 1, z: 0 }

	These next properties aren't strictly necessary, but may be useful under certain circumstances:

	Manual Translation: You can position all meshes on the rack like this, manually. This copies to the second hook automatically.
	`
	manualTranslation: {
		0: { x: 0.1725, y: -0.31, z: 0.087 },
		1: { x: 0.075, y: -0.31, z: -0.013 },
		2: { x: 0.1725, y: -0.31, z: -0.112 },
		3: { x: 0.275, y: -0.31, z: -0.013 },
	}
	`
	Extra Rotation: A separate rotation for the 90 degree turned curing rack
	`extraRotation: { x: 0.0, y: 0.0, z: 0.0 }

	Extra Manual Translation: A separate manual translation for the 90 degree turned curing rack.
	`
	extraManualTranslation: {
		0: { x: 0.16, y: -0.31, z: 0.087 },
		1: { x: 0.075, y: -0.31, z: -0.0025 },
		2: { x: 0.16, y: -0.31, z: -0.1 },
		3: { x: 0.25, y: -0.31, z: -0.0025 },
	}
	`

3. If you're using vanilla mechanics for drying/curing or using it purely for visuals, then you're done! If not, you will need to add some extra data to your object to use the Ancient Tools curing method.
In the onCuringRackProps object, add another object called 'conversionProps'.

4. You will need to set the following variables within conversionProps:

	Total Curing Hours: The time it takes for your object to cure(transform) into another object.
	`totalCuringHours: 0.0`

	Convert Into Itemstack: The item/block that the item will transform into when the total curing hours time has been reached.
	'convertIntoItemstack: "game:redmeat-cured"`
	
	Converted Itemstack Type: Is the converted thing a block or an item?
	`convertedItemstackType: "item"`

5. Have a look at the Ancient Tools files for examples. Specifically, the salted meat for the use of conversionProps or the Wildcraft patch to clippings for the manual translations. There are likely issues with some things here, too... I've only tested for things that I have needed thusfar.
