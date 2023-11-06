function EOTW_SetColors()
{
	$EOTW::Colors::Dead	= getClosestColor("0.60 0.60 0.30 1.00");   //153 153 077
	$EOTW::Colors::Glass	= getClosestColor("1.00 1.00 1.00 0.50");   //128 128 128
	$EOTW::Colors::Grass	= getClosestColor("0.05 0.75 0.15 1.00");   //013 191 038
	$EOTW::Colors::Metal	= getClosestColor("0.35 0.35 0.40 1.00");   //089 089 102
	$EOTW::Colors::Stone	= getClosestColor("0.75 0.75 0.80 1.00");   //191 191 204
	$EOTW::Colors::Sturdium	= getClosestColor("0.20 0.20 0.25 1.00");   //051 051 064
	$EOTW::Colors::Vine	= getClosestColor("0.05 0.35 0.15 1.00");   //013 089 038
	$EOTW::Colors::Wood	= getClosestColor("0.50 0.35 0.15 1.00");   //128 089 038
}
EOTW_SetColors();

datablock fxDtsBrickData(brickResourceSpawnData : brick32x32fData)
{
	category = "Special";
	subcategory = "Interactive";
	uiName = "Resource Spawn";
	costScale = 20;
};

function brickResourceSpawnData::onPlant(%data, %brick)
{
	if (!isObject(ResourceSpawnGroup))
		new SimSet(ResourceSpawnGroup);
	ResourceSpawnGroup.add(%brick);
}

function brickResourceSpawnData::onLoadPlant(%data, %brick)
{
	if (!isObject(ResourceSpawnGroup))
		new SimSet(ResourceSpawnGroup);
	ResourceSpawnGroup.add(%brick);
}

package EOTW_Materials
{
	function Armor::onTrigger(%data, %this, %trig, %tog)
	{
		if(isEventPending($EnvMasterLoop) && isObject(%cl = %this.client))
		{
			if(%trig == 0 && !(isObject(%this.getMountedImage(0)) && %tog))
			{
				if(%tog)
				{
					%eye = %this.getEyePoint();
					%dir = %this.getEyeVector();
					%for = %this.getForwardVector();
					%face = getWords(vectorScale(getWords(%for, 0, 1), vectorLen(getWords(%dir, 0, 1))), 0, 1) SPC getWord(%dir, 2);
					%mask = $Typemasks::fxBrickAlwaysObjectType | $Typemasks::TerrainObjectType;
					%ray = containerRaycast(%eye, vectorAdd(%eye, vectorScale(%face, 5)), %mask, %this);
					if(isObject(%hit = firstWord(%ray)) && %hit.getClassName() $= "fxDtsBrick" && %hit.isCollectable)
					{
						if(%hit.beingCollected)
							%cl.centerPrint("<color:FFFFFF>Someone is already collecting that material brick!", 3);
						else
						{
							%hit.beingCollected = 1;
							%hit.cancelCollecting = %hit.schedule(48, "cancelCollecting");
							%this.collectLoop(%hit, getSimTime());
							%cl.material = %hit.material;
							%this.client.showMaterials();
						}
					}
				}
				else if(isEventPending(%this.collectLoop))
					cancel(%this.collectLoop);
			}
			else if(%trig == 4 && ((isObject(%image = %this.getMountedImage(0)) && %image.getName() $= "BrickImage") || isEventPending(%this.collectLoop)))
			{
				if(%tog)
					switch$(%cl.material)
					{
						case "Dead_Plant": %cl.material = "Glass";
						case "Glass": %cl.material = "Grass";
						case "Grass": %cl.material = "Metal";
						case "Metal": %cl.material = "Stone";
						case "Stone":
							if($EOTW::Day >= 40) %cl.material = "Sturdium";
							else %cl.material = "Vine";
						case "Sturdium": %cl.material = "Vine";
						case "Vine": %cl.material = "Wood";
						case "Wood": %cl.material = "Glass";
					}
					%cl.showMaterials();
			}
		}
		Parent::onTrigger(%data, %this, %trig, %tog);
	}
};
activatePackage("EOTW_Materials");

function fxDtsBrick::cancelCollecting(%brick)
{ %brick.beingCollected = 0; }

function GameConnection::showMaterials(%cl)
{
	if(%cl.material $= "")
		%cl.material = "Wood";
	%amt = $EOTW::Material[%cl.bl_id, %cl.material] + 0;
	%sturdium = ($EOTW::Day >= 53 ? "Sturdium" : "???");
	%timePrefix = "\c3Time\c6: " @ $LastEOTWTime @ "     ";
	switch$(%cl.material)
	{
		case "Dead_Plant":	%cl.bottomPrint(%timePrefix @ "<color:99994D>Dead Plant\c6: STOP HACKING MY GAME.        \c7Next material: MISSINGNO   (Jet to cycle)", -1);
		case "Glass":	%cl.bottomPrint(%timePrefix @ "<color:808080>Glass\c6: " @ %amt @ "        \c7Next material: Grass   (Jet to cycle)", -1);
		case "Grass":	%cl.bottomPrint(%timePrefix @ "<color:0EBF26>Grass\c6: " @ %amt @ "        \c7Next material: Metal   (Jet to cycle)", -1);
		case "Metal":	%cl.bottomPrint(%timePrefix @ "<color:595966>Metal\c6: " @ %amt @ "        \c7Next material: Stone   (Jet to cycle)", -1);
		case "Stone":	%cl.bottomPrint(%timePrefix @ "<color:BFBFCC>Stone\c6: " @ %amt @ "        \c7Next material: " @ ($EOTW::Day >= 40 ? %sturdium : "Vine") @ "   (Jet to cycle)", -1);
		case "Sturdium":%cl.bottomPrint(%timePrefix @ ($EOTW::Day >= 53 ? "<color:333340>" @ $SturdiumName : "<color:FFFFFF>???") @ "\c6: " @ %amt @ "        \c7Next material: Vine   (Jet to cycle)", -1);
		case "Vine":	%cl.bottomPrint(%timePrefix @ "<color:0E5926>Vine\c6: " @ %amt @ "        \c7Next material: Wood   (Jet to cycle)", -1);
		case "Wood":	%cl.bottomPrint(%timePrefix @ "<color:805926>Wood\c6: " @ %amt @ "        \c7Next material: Glass   (Jet to cycle)", -1);
	}
}

function Player::collectLoop(%pl, %brick, %start)
{
	cancel(%pl.collectLoop);
	if(!isObject(%cl = %pl.client) || %pl.getState() $= "DEAD") return;
	if(!isObject(%brick) || %brick.isDead()) return;
	%eye = %pl.getEyePoint();
	%dir = %pl.getEyeVector();
	%for = %pl.getForwardVector();
	%face = getWords(vectorScale(getWords(%for, 0, 1), vectorLen(getWords(%dir, 0, 1))), 0, 1) SPC getWord(%dir, 2);
	%mask = $Typemasks::fxBrickAlwaysObjectType | $Typemasks::TerrainObjectType;
	%ray = containerRaycast(%eye, vectorAdd(%eye, vectorScale(%face, 5)), %mask, %this);
	if(isObject(%hit = firstWord(%ray)) && %hit == %brick)
	{
		cancel(%brick.cancelCollecting);
		if((%time=vectorDist(%start, getSimTime())) >= %brick.collectTime)
		{
			if(%brick.forceVolume !$= "")
				%volume = %brick.forceVolume;
			else
			{
				%data = %brick.getDatablock();
				%volume = %data.brickSizeX * %data.brickSizeY * %data.brickSizeZ;
			}
			%old = $EOTW::Material[%cl.bl_id, %brick.material];
			$EOTW::Material[%cl.bl_id, %brick.material] = %volume + %old;
			if(%brick.material $= "Sturdium") %mat = ($EOTW::Day < 53 ? "???" : $SturdiumName); else %mat = %brick.material;
			%cl.centerPrint("<br><color:FFFFFF>Collected a gatherable "@%mat@" brick.<br>100% complete.", 3);
			%brick.isCollectable = 0; %brick.killbrick(); %cl.showMaterials();
		}
		else
		{
			%brick.cancelCollecting = %brick.schedule(48, "cancelCollecting");
			%pl.collectLoop = %pl.schedule(16, "collectLoop", %brick, %start);
			if(%brick.material $= "Sturdium") %mat = ($EOTW::Day < 53 ? "???" : $SturdiumName); else %mat = %brick.material;
			%progress = mFloatLength(%time/%brick.collectTime*100,0);
			if(%cl.gatherProgress != %progress)
			{
				%cl.centerPrint("<br><color:FFFFFF>Collecting a gatherable "@%mat@" brick.<br>"@%progress@"% complete.", 3);
				%cl.gatherProgress = %progress;
			}
		}
	}
}

function GatherableLoop()
{
	cancel($GatherableLoop);
	if($EOTW::Day < 35) for(%i=0;%i<1;%i++) %mat[-1+%mats++] = "Glass";
	if($EOTW::Day < 5) for(%i=0;%i<2;%i++) %mat[-1+%mats++] = "Grass";
	if($EOTW::Day < 45) for(%i=0;%i<5;%i++) %mat[-1+%mats++] = "Metal";
	if($EOTW::Day < 25) for(%i=0;%i<7;%i++) %mat[-1+%mats++] = "Stone";
	if($EOTW::Day >= 40 && ($EOTW::Day < 55 || $Pref::Server::SolarApoc::InfiniteMode)) for(%i=0;%i<1;%i++) %mat[-1+%mats++] = "Sturdium";
	if($EOTW::Day >= 65) for(%i=0;%i<1;%i++) %mat[-1+%mats++] = "Clear Sturdium";
	if($EOTW::Day < 15) for(%i=0;%i<2;%i++) %mat[-1+%mats++] = "Vine";
	if($EOTW::Day < 15) for(%i=0;%i<10;%i++) %mat[-1+%mats++] = "Wood";
	if(!isObject(Gatherables) || Gatherables.getCount() < 500)
		%mat = %mat[getRandom(0, 99)]; if(%mat !$= "") spawnGatherable(%mat);
	$GatherableLoop = schedule(48, 0, "GatherableLoop");
}

function isMaterial(%mat)
{
	%_glass = 1;
	%_grass = 1;
	%_metal = 1;
	%_stone = 1;
	%_vine = 1;
	%_wood = 1;
	%_[$SturdiumName] = 1;
	%_clear[" " @ $SturdiumName] = 1;
	return %_[%mat];
}

function spawnGatherable(%mat)
{
	%tierGrass = 0;
	%tierVine = 0;
	%tierWood = 1;
	%tierStone = 2;
	%tierGlass = 2;
	%tierMetal = 3;
	%tierSturdium = 4;
	%tier["Clear Sturdium"] = 4;
	if (isObject(ResourceSpawnGroup) && ResourceSpawnGroup.getCount() != 0)
	{
		%count = ResourceSpawnGroup.getCount();
		for (%i = 0; %i < %count; %i++)
		{
			%spawn = ResourceSpawnGroup.getObject(%i);
			if (%tier[%spawn.material] < %tier[%mat])
				continue;
			%spawn[%spawns++ - 1] = %spawn;
		}
		if (%count != 0)
		{
			%rsChance = (mCos(mClampF($EOTW::Day - 20, 0, 100) / 100 * 3.1415926535) + 1) / 2;
			%rs = getRandom() < %rsChance;
		}
	}
	if (!%rs)
		%pos = (getRandom(-200, 199) / 2 + 0.25) SPC (getRandom(-200, 199) / 2 + 0.25) SPC 0.1;
	else
	{
		%spawn = %spawn[getRandom(0, %spawns - 1)];
		%pos = vectorAdd(%spawn.getPosition(), (getRandom(-16, 15) / 2 + 0.25) SPC (getRandom(-16, 15) / 2 + 0.25) SPC 0.2);
	}
	%data = EOTW_CreateBrick(EnvMaster, brick1x1FData, %pos, $EOTW::Colors["::"@%mat]); %brick = getField(%data, 0);
	if(getField(%data, 1)) { %brick.delete(); return; }
	%brick.material = %mat;
	switch$(%mat)
	{
		case "Glass":
			%brick.collectTime = 10000;
			%brick.destroyOnDay = 35;
			%brick.isCollectable = 1;
			%brick.forceVolume = 20;
		case "Grass":
			%brick.collectTime = 500;
			%brick.destroyOnDay = 5;
			%brick.isCollectable = 1;
			%brick.forceVolume = 5;
		case "Metal":
			%brick.collectTime = 15000;
			%brick.destroyOnDay = 45;
			%brick.isCollectable = 1;
			%brick.forceVolume = 100;
		case "Stone":
			%brick.collectTime = 5000;
			%brick.destroyOnDay = 25;
			%brick.isCollectable = 1;
			%brick.forceVolume = 125;
		case "Sturdium":
			%brick.collectTime = 30000;
			%brick.destroyOnDay = 55;
			%brick.isCollectable = 1;
			%brick.forceVolume = 50;
		case "Clear Sturdium":
			%brick.collectTime = 30000;
			%brick.destroyOnDay = 60;
			%brick.isCollectable = 1;
			%brick.forceVolume = 20;
		case "Vine":
			%brick.collectTime = 1000;
			%brick.destroyOnDay = 15;
			%brick.isCollectable = 1;
			%brick.forceVolume = 5;
		case "Wood":
			%brick.collectTime = 2000;
			%brick.destroyOnDay = 15;
			%brick.isCollectable = 1;
			%brick.forceVolume = 1000;
	}
	if(!isObject(Gatherables))
		MainBrickgroup.add(new SimGroup(Gatherables) { bl_id = 1337; name = "God"; });
	Gatherables.add(%brick);
}

//%brick.canBeFireballed   // - - Implement this
//%brick.collectTime (in MS)
//%brick.destroyOnDay   //Collectable spawns only; don't apply this to built bricks
//%brick.forceVolume
//%brick.isBreakable   // - - - - Implement this
//%brick.isCollectable
//%brick.isFlammable   // - - - - Implement this
//%brick.isIgnitable   // - - - - Implement this
//%brick.material
//%brick.smashable   // - - - - - Implement this
//%brick.unrecolorable   // - - - Implement this