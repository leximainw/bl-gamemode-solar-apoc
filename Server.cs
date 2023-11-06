//AddDamageType("BurnedToDeath", '%1 burned to death!', '%1 burned to death!', 1, 1);
//AddDamageType("EOTWFireball", '%1 was hit by a meteor!', '%1 was hit by a meteor!', 1, 1);
//AddDamageType("Sun", '%1 got fried by the Sun!', '%1 got fried by the Sun!', 1, 1);
AddDamageType("BurnedToDeath", '', '', 1, 1);
AddDamageType("EOTWFireball", '', '', 1, 1);
AddDamageType("Sun", '', '', 1, 1);

function getSturdiumName()
{
	if (getRandom() < 0.1)
	{
		%element = getRandomElementName();
		return strUpr(getSubStr(%element, 0, 1)) @ strLwr(getSubStr(%element, 1, strLen(%element) - 1));
	}
	%name[-1+%names++] = "Sturdium";
	%name[-1+%names++] = "Adminium";
	%name[-1+%names++] = "Eternium";
	return %name[getRandom(0, %names - 1)];
}

function getElements()
{
	if ($ElementList $= "")
	{
		%file = new FileObject();
		%file.openForRead("Add-Ons/Gamemode_Solar_Apoc/ElementList.txt");
		%list = %file.readLine();
		%file.close();
		%file.delete();
		$ElementList = strReplace(%list, ",", "\t");
	}
	return $ElementList;
}

function isElementName(%name)
{
	return striPos("\t" @ getElements() @ "\t", "\t" @ %name @ "\t") != -1;
}

function getRandomElementName()
{
	%elements = getElements();
	%fields = getFieldCount(%elements);
	return getField(%elements, getRandom(0, %fields - 1));
}

if($SturdiumName $= "") $SturdiumName = getSturdiumName();

if($Pref::EOTW::AllowWrench $= "")
	$Pref::EOTW::AllowWrench = 1;

package EOTW
{
	function fxDtsBrick::setColor(%brick, %color)
	{ if(!%brick.unrecolorable && (getWord(getColorIDTable(%color), 3) > 0.95 || (%brick.material $= "Glass" || %brick.material $= ""))) Parent::setColor(%brick, %color); }
	
	function servercmdEnvGui_SetVar(%cl, %var, %arg)
	{
		if(!%cl.environMaster)
			if(!$EnvGuiTesting)
			{
				%cl.centerPrint("<color:FF8000>Only the Environment Master bot can edit these settings.", 15);
				return;
			}
			else
				echo(%var SPC %arg);
		Parent::servercmdEnvGui_SetVar(%cl, %var, %arg);
	}
	
	function servercmdPlantBrick(%cl)
	{
		if(isEventPending($EnvMasterLoop) && !%cl.builderMode)
		{
			if(isObject(%pl = %cl.player) && isObject(%temp = %pl.tempbrick))
			{
				%data = %temp.getDatablock();
				if(firstWord(%data.uiName) $= "Coffin") return;
				%volume = %data.brickSizeX * %data.brickSizeY * %data.brickSizeZ;
				if (%data.costScale !$= "")
					%volume *= %data.costScale;
				if (%cl.infBuildMode)
					%volume = 0;
				//if(%data.uiName $= "Global Storage") %volume = 0;
				if($EOTW::Material[%cl.bl_id, %mat = %cl.material] < %volume)
					%cl.centerPrint("<color:FF0000>Whoops!<br>\c6You don't have enough "@(%cl.material$="Sturdium"?($EOTW::Day >= 53 ? getSturdiumName() : "???"):%cl.material)@" to place that brick!", 3);
				else if((%mat $= "Dead_Plant" || %mat $= "Grass" || %mat $= "Vine") && %data.getName() !$= "brick1x1FData")
					%cl.centerPrint("<color:FF0000>Whoops!<br>\c6Plants must be a 1x1F brick.", 3);
				else
				{
					%brick = Parent::servercmdPlantBrick(%cl); if(!isObject(%brick)) return %brick;
					$EOTW::Material[%cl.bl_id, %mat] = $EOTW::Material[%cl.bl_id, %mat] - %volume;
					%brick.material = %mat; %brick.setColor($EOTW::Colors["::"@(%mat$="Dead_Plant"?"Dead":%mat)]);
					switch$(%mat)
					{
						case "Dead_Plant":
							%brick.canBeFireballed = 1;
							%brick.isBreakable = 1;
							%brick.isFlammable = 1;
							%brick.isIgnitable = 1;
							%brick.unrecolorable = 1;
							%brick.setColliding(0);
							%brick.burnLoop(getSimTime());
							%cl.centerPrint("<color:FFFFFF>Well done, Sally Mc Smarty Pants. You found a way to build Dead Plant.", 3);
						case "Glass":
							%brick.canBeFireballed = 1;
							%brick.isBreakable = 1;
							%brick.smashable = 1;
						case "Grass":
							%brick.canBeFireballed = 1;
							%brick.isBreakable = 1;
							%brick.isFlammable = 1;
							%brick.unrecolorable = 1;
							%brick.setColliding(0);
							%brick.schedule(48, "EOTW_GrowGrass", 0, 0, 0, getSimTime());
						case "Metal":
							%brick.isBreakable = 1;
						case "Stone":
							%brick.canBeFireballed = 1;
							%brick.isBreakable = 1;
						case "Vine":
							%brick.canBeFireballed = 1;
							%brick.isBreakable = 1;
							%brick.isFlammable = 1;
							%brick.unrecolorable = 1;
							%brick.setColliding(0);
							%brick.schedule(48, "EOTW_GrowVine", 0, 0, 0, getSimTime());
						case "Wood":
							%brick.canBeFireballed = 1;
							%brick.isBreakable = 1;
							%brick.isFlammable = 1;
							%brick.isIgnitable = 1;
					}
					%cl.showMaterials();
				}
			}
		}
		else Parent::servercmdPlantBrick(%cl);
	}
	
	function servercmdSetWrenchData(%cl, %data)
	{
		if(isEventPending($EnvMasterLoop))
		{
			%item = getWord(getField(%data, 4), 1);
			%allowedItem[-1+%allowedItems++] = "Hammer ";
			%allowedItem[-1+%allowedItems++] = "Wrench";
			%allowedItem[-1+%allowedItems++] = "Wand";
			%allowedItem[-1+%allowedItems++] = "Key Blue";
			%allowedItem[-1+%allowedItems++] = "Key Green";
			%allowedItem[-1+%allowedItems++] = "Key Red";
			%allowedItem[-1+%allowedItems++] = "Key Yellow";
			for(%i=0;%i<%allowedItems;%i++)
				if(%item.uiName $= %allowedItem[%i])
					%allow = 1;
			if(!%allow) %data = setField(%data, 4, "IDB 0");	//People shoot projectiles at others.
		}
		Parent::servercmdSetWrenchData(%cl, %data);
	}
	
	function fxDtsBrick::onRemove(%brick)
	{
		if(!%brick.dontRefund)
		{
			%data = %brick.getDatablock();
			%volume = %data.brickSizeX * %data.brickSizeY * %data.brickSizeZ;
			if (%data.costScale !$= "")
				%volume *= %data.costScale;
			$EOTW::Material[%brick.getGroup().bl_id, %brick.material] += %volume;
		}
		Parent::onRemove(%brick);
	}
	
	function GameConnection::createPlayer(%cl, %trans)
	{
	//	if($EOTW::HasDied[%cl.bl_id])
	//	{
	//		if(isObject(%cl.getControlObject()))
	//			%cl.chatMessage("\c6You have died. Please wait for the next round to try again.");
	//		else { (%cam = %cl.camera).setTransform(%trans); %cl.setControlObject(%cam); }
	//		return;
	//	}
	//	else if($EOTW::Day >= 10 && $EOTW::IsDay && %cl.brickgroup.spawnBrickCount == 0)
	//	{
	//		if(isObject(%cl.getControlObject()))
	//			%cl.chatMessage("\c6You can't spawn during the day; you could end up losing your one life instantly. Please be patient.");
	//		else { (%cam = %cl.camera).setTransform(%trans); %cl.setControlObject(%cam); }
	//		return;
	//	}
		Parent::createPlayer(%cl, %trans);
		if(isEventPending($EnvMasterLoop) && isObject(%pl = %cl.player))
		{
			%pl.spawnTime = 0;
			%pl.setShapeNameDistance(0);
			%pl.setDatablock(PlayerNoJet);
			%pl.clearTools();
			%tool[-1+%tools++] = HammerItem.getID();
			if(%cl.isSuperAdmin || $Pref::EOTW::AllowWrench) %tool[-1+%tools++] = WrenchItem.getID();
			%tool[-1+%tools++] = PrintGun.getID();
			for(%i=0;%i<%tools;%i++)
			{
				%pl.tool[%i] = %tool[%i];
				messageClient(%cl, 'MsgItemPickup', '', %i, %tool[%i], 1);
			}
			%pl.diseaseTick(getSimTime());
			%pl.updateMovePenalty();
		}
		if (isObject(%cl.diseaseInfo))
		{
			%dCount = %cl.diseaseInfo.getCount();
			for (%i = 0; %i < %dCount; %i++)
			{
				%disease = %cl.diseaseInfo.getObject(%dCount - (%i + 1));
				if (%disease.alwaysCureOnDeath)
					%pl.cureDisease(%disease);
			}
		}
	}
	
	function GameConnection::onDeath(%cl, %a, %b, %damageType, %d)
	{
		Parent::onDeath(%cl, %a, %b, %damageType, %d);
		if(%damageType == $DamageType::BurnedToDeath)
			messageClient(%cl, '', "\c0"@%cl.name@" burned to death!");
		else if(%damageType == $DamageType::EOTWFireball)
			messageClient(%cl, '', "\c0"@%cl.name@" was hit by a meteor!");
		else if(%damageType == $DamageType::Sun)
			messageClient(%cl, '', "\c0"@%cl.name@" got fried by the Sun!");
		$EOTW::HasDied[%cl.bl_id] = 1;
	}
	
	function servercmdCreateMinigame(%cl, %a, %b, %c, %d, %e, %f, %g, %h, %i, %j)
	{ if(!isEventPending($EnvMasterLoop)) Parent::servercmdCreateMinigame(%cl, %a, %b, %c, %d, %e, %f, %g, %h, %i, %j); }
	
	function GameConnection::applyBodyColors(%cl)
	{
		if (%cl.graniteOverride)
			Parent::applyBodyColors(%cl);
		else
			Disease_Granite.updateClientAvatar(%cl);
	}
};
activatePackage("EOTW");

function EnvMasterLoop(%time, %day, %color, %colInc, %colTarg, %size)
{
	cancel($EnvMasterLoop);
	if($Overloaded)
	{
		$EnvMasterLoop = schedule((100 * ($SampleRate + 1)), 0, "EnvMasterLoop", %time, %day, %color, %colInc, %colTarg, %size);
		return;
	}
	%time += $TimeSync;
	$TimeSync = 0;
	if(%time >= 360)
	{
		%time = 0;
		%day++;
		if(isObject(Gatherables)) for(%i=0;%i<Gatherables.getCount();%i++)
			if((%brick = Gatherables.getObject(%i)).destroyOnDay <= getMin(%day, $Pref::Server::SolarApoc::InfiniteMode ? 50 : 999999)) %brick.killBrick();
		%size += 0.1;
		if(%size > 5)
			%size = 5;
		if(%day >= 61 && !$Pref::Server::SolarApoc::InfiniteMode)
		{
			%groups = MainBrickgroup.getCount();
			for(%i=0;%i<%groups;%i++)
			{
				%group = MainBrickgroup.getObject(%i);
				%bricks = %group.getCount();
				for(%j=0;%j<%bricks;%j++)
					%group.getObject(%j).dontRefund = 1;
			}
			$SturdiumName = getSturdiumName();
			//deleteVariables("$EOTW::*");
			Gatherables.delete(); cancel($GatherableLoop);
			EOTW_SetColors();
			cancel($EnvMasterLoop);
			%groups = MainBrickgroup.getCount();
			for(%i=0;%i<%groups;%i++)
			{
				%group = MainBrickgroup.getObject(%i);
				%bricks = %group.getCount();
				for(%j=0;%j<%bricks;%j++)
				{
					%brick = %group.getObject(%j);
					if(%brick.material !$= "")
						%brick.killBrick();
				}
			}
			servercmdEnvMaster(EnvMaster);
			messageAll('MsgAdminForce', "\c3Environment Master\c6: Congratulations, survivors! \c2Starting a new round!");
			for(%i=0;%i<ClientGroup.getCount();%i++) if((%cl = ClientGroup.getObject(%i)).hasSpawnedOnce) %cl.instantRespawn(); return;
		}
		switch(%day)
		{
			case 5: %msg = " Environment Notice: Grass seeds will no longer spawn for collection.";
			case 7: %msg = " Environment Notice: Grass will now die if directly exposed to the sun.";
			case 10: %msg = " Environment Warning: It is no longer safe to scavenge during the day.";
			case 14: %msg = " Environment Notice: Vines will now die if directly exposed to the sun.";
			case 15: %msg = " Environment Notice: Wood and vine seeds will no longer spawn for collection.";
			case 20: %msg = " Environment Warning: Wood and dead plants will now ignite if exposed to the sun.";
			case 25: %msg = " Environment Notice: Stone will no longer spawn for collection.";
			case 30: %msg = " Environment Warning: Meteors are raining from the sky; collisions may severely damage structures.";
			case 35: %msg = " Environment Notice: Glass will no longer spawn for collection.";
			case 37: %msg = " Environment Notice: Grass will now die even when indoors.";
			case 38: %msg = " Environment Warning: Players can now catch fire if exposed to the sun, fire, or other burning players.";
		//	case 40: %msg = " Environment Warning: Zombies immune to the immense heat have been spotted heading this way.";   //Unimplemented
			case 41: %msg = " Environment Warning: Ignitable materials will now catch fire even when indoors.";
			case 42: %msg = " Environment Notice: Life, the Universe, and everything.";
		//	case 43: %msg = " Environment Warning: The heat-proof zombies have arrived.";   //Unimplemented
			case 45: %msg = " Environment Notice: Metal will no longer spawn for collection.";
			case 47: %msg = " Environment Notice: Vines will now die even when indoors.";
			case 48: %msg = " Environment Notice: Players no longer regenerate over time.";
			case 50: %msg = " Environment Warning: The heat has reached critical levels; no known materials can withstand it.";
			case 53: %msg = " Environment Notice: A new material has been discovered. " @ (isElementName($SturdiumName) ? "The general public has taken to calling it " @ $SturdiumName @ ", much to the bafflement and irritation of scientists the world over" : "It has been given the temporary name of " @ $SturdiumName) @ ".";
			case 55: if (!$Pref::Server::SolarApoc::InfiniteMode) %msg = " Environment Notice: "@$SturdiumName@" will no longer spawn for collection.";
			case 57: if (!$Pref::Server::SolarApoc::InfiniteMode) %msg = " Environment Notice: No material except for "@$SturdiumName@" can withstand the heat, even when indoors.";
			case 58: if (!$Pref::Server::SolarApoc::InfiniteMode) %msg = " Environment Notice: "@$SturdiumName@" will no longer withstand the heat.";
		}
		messageAll('', "\c3Environment Master\c6: The sun rises on Day "@%day@"."@%msg);
		%color = vectorAdd(%color, %colInc);
		if(vectorDist(%color, %colTarg) == 0)
		{
			switch$(%colTarg)
			{
				case "1 0 0":
					if(%colInc $= "0.1 0 0")
					{
						%colInc = "0 0.1 0";
						%colTarg = "1 1 0";
					}
					else
					{
						%colInc = "0 0 0";
						%colTarg = "0 0 0";
					}
				case "1 1 0":
					%colInc = "-0.1 0 0";
					%colTarg = "0 1 0";
				case "0 1 0":
					%colInc = "0 0 0.1";
					%colTarg = "0 1 1";
				case "0 1 1":
					%colInc = "0 -0.1 0";
					%colTarg = "0 0 1";
				case "0 0 1":
					%colInc = "0.1 0.1 0";
					%colTarg = "1 1 1";
				case "1 1 1":
					%colInc = "0 0 0";
					%colTarg = "0 0 0";
			}
		}
		servercmdEnvGui_SetVar(EnvMaster, "SunFlareColor", %color);
	}
	$EOTW::Day = %day; $EOTW::Time = %time;
	if(%time == 180)
	{
		messageAll('', "\c3Environment Master\c6: The sun sets on Day "@%day@"."@(%day >= 10 ? " It is now safe to scavenge." : ""));
		servercmdEnvGui_SetVar(EnvMaster, "SunFlareColor", "0 0 0");
	}
	if(%time > 180)
	{
		$EOTW::IsDay = 0; %flare = 0;
		%realA = 1 - (mAbs(%time - 90) / 120);
		%realB = 1 - (mAbs(%time - 450) / 120);
		%realFlare = getMax(%realA, %realB);
		if(%realFlare < 0) %realFlare = 0;
	}
	else
	{
		$EOTW::IsDay = 1;
		%flare = 1 - (mAbs(%time - 90) / 120);
		%realFlare = %flare;
		if(%day >= 20)
		{
			for(%i=0;%i<mCeil(getBrickCount()/2000);%i++)
			{
				%groups = MainBrickgroup.getCount();
				for(%j=0;%j<%groups;%j++)
					%bricks[%j] = MainBrickgroup.getObject(%j).getCount() + %bricks[%j - 1];
				%highest = %bricks[%groups - 1];
				%select = getRandom(1, %highest);
				while(%bricks[%sel] < %select) %sel++;
				while(MainBrickgroup.getObject(%sel).getCount() == 0 && %sel < %groups) %sel++;
				if(%sel < %groups)
				{
					%group = MainBrickgroup.getObject(%sel);
					%bricks = %group.getCount();
					if(%bricks != 0)
					{
						%brick = %group.getObject(getRandom(0, %bricks - 1));
						if(%brick.isIgnitable)
						{
							if(%day >= 41) %brick.burnLoop(getSimTime());
							else
							{
								%pi = 3.14159265; %val = (%time / 180) * %pi;
								%ang = ($EnvGuiServer::SunAzimuth / 180) * %pi;
								%dir = vectorScale(mSin(%ang) * mCos(%val) SPC mCos(%ang) * mCos(%val) SPC mSin(%val), 512);
								%ray = containerRaycast(vectorAdd(%pos = %brick.getPosition(), %dir), %pos, $Typemasks::fxBrickAlwaysObjectType | $Typemasks::StaticShapeObjectType);
								if(!isObject(%hit = firstWord(%ray)) || %hit == %brick)
									%brick.burnLoop(getSimTime());
							}
						}
					}
				}
			}
		}
		if(%day >= 30)
		{
			%pi = 3.14159265; %val = (%time / 180) * %pi;
			%ang = ($EnvGuiServer::SunAzimuth / 180) * %pi;
			%dir = vectorScale(mSin(%ang) * mCos(%val) SPC mCos(%ang) * mCos(%val) SPC mSin(%val), 512);
			%hit = getRandom(-1000, 1000) SPC getRandom(-1000, 1000) SPC 0;
			%proj = new Projectile()
			{
				datablock = EOTWFireballProjectile;
				initialPosition = vectorAdd(%hit, vectorScale(%dir, 1));
				initialVelocity = vectorScale(%dir, -100);
			};
		}
		if(%day >= 50)
		{
			if(mAbs(%time * 10 - mFloor(%time * 10)) <= 0.05)
			{
				for(%i=0;%i<mCeil(getBrickCount()/(1000/(%day-49)));%i++)
				{
					%groups = MainBrickgroup.getCount();
					for(%j=0;%j<%groups;%j++)
						%bricks[%j] = MainBrickgroup.getObject(%j).getCount() + %bricks[%j - 1];
					%highest = %bricks[%groups - 1];
					%select = getRandom(1, %highest);
					while(%bricks[%sel] < %select) %sel++;
					while(MainBrickgroup.getObject(%sel).getCount() == 0 && %sel < %groups) %sel++;
					%group = MainBrickgroup.getObject(%sel);
					%bricks = %group.getCount();
					if(%bricks != 0)
					{
						%brick = %group.getObject(getRandom(0, %bricks - 1));
						if(%brick.isBreakable || (%day >= 58 && !$Pref::Server::SolarApoc::InfiniteMode))
						{
							if(%day >= 57 && %brick.isBreakable && !$Pref::Server::SolarApoc::InfiniteMode) %brick.killBrick();
							else
							{
								%pi = 3.14159265; %val = (%time / 180) * %pi;
								%ang = ($EnvGuiServer::SunAzimuth / 180) * %pi;
								%dir = vectorScale(mSin(%ang) * mCos(%val) SPC mCos(%ang) * mCos(%val) SPC mSin(%val), 512);
								%ray = containerRaycast(vectorAdd(%pos = %brick.getPosition(), %dir), %pos, $Typemasks::fxBrickAlwaysObjectType | $Typemasks::StaticShapeObjectType);
								if(!isObject(%hit = firstWord(%ray)) || %hit == %brick)
									%brick.killBrick();
							}
						}
					}
				}
			}
		}
	}
	%pi = 3.14159265;
	%val = (%time / 180) * %pi;
	%ang = ($EnvGuiServer::SunAzimuth / 180) * %pi;
	%dir = vectorScale(mSin(%ang) * mCos(%val) SPC mCos(%ang) * mCos(%val) SPC mSin(%val), 512);
	%pos[-1+%posCount++] = "0 0 0";
	for(%i=0;%i<ClientGroup.getCount();%i++)
		if(isObject(%pl = ClientGroup.getObject(%i).player))
		%pos[-1+%posCount++] = %pl.getPosition();
	for(%i=0;%i<%posCount;%i++)
	{
		initContainerRadiusSearch(%pos[%i], 240, $Typemasks::PlayerObjectType);
		while(isObject(%obj = containerSearchNext()))
		{
			if(%obj.getState() !$= "DEAD" && !%hasHarmed[%obj])
			{
				%hasHarmed[%obj] = 1;
				%hit = containerRaycast(vectorAdd(%pos = %obj.getHackPosition(), %dir), %pos, $Typemasks::fxBrickObjectType | $Typemasks::StaticShapeObjectType);
				if(!isObject(%hit) && %size >= 1 && %flare != 0)
				{
					if(%obj.getDamagePercent() >= 1)
						%obj.damage(0, 0, 10000, $DamageType::Sun);
					else
					{
						%damage = ((%size / 4) * ($SampleRate + 1) * 5);
						%damage *= 1 - (%obj.client.diseaseInfo.sunResist);
						%damage = %obj.getDamageLevel() + %damage;
						if(%damage >= 100 && !%obj.immuneToSun)
							%obj.damage(0, 0, 10000, $DamageType::Sun);
						else if (%obj.client.diseaseInfo.sunResist < 1)
						{
							if(!%obj.immuneToSun)
							{
								%obj.setDamageLevel(%damage);
								%obj.setDamageFlash(0.25);
							}
							if(%day >= 38 && !%obj.immuneToFire)
							{
								%burnTime = (%day / 50) * ($SampleRate + 1) * (%obj.tripleFire ? 3 : 1);
								%burnTime *= 1 - (%obj.client.diseaseInfo.sunResist);
								%obj.burningTime += %burnTime;
								if(!isEventPending(%obj.burnLoop))
									%obj.burnLoop(getSimTime());
							}
						}
						if(isObject(%cl = %obj.client) && !%obj.immuneToSun && %obj.client.diseaseInfo.sunResist < 1)
							%cl.centerPrint("<color:FF8000>Get into a shaded area!<br>"@%obj.getHealthText(), 2);
						if(%obj.immuneToSun && %obj.deimmuneTime > 0)
						{
							%obj.deimmuneTime -= (100 * ($SampleRate + 1));
							if(%obj.deimmuneTime <= 0)
							{
								%obj.deimmuneTime = 0;
								%obj.immuneToSun = 0;
								%obj.immuneToFire = 0;
								%obj.immuneToBurn = 0;
								%obj.immuneToMeteors = 0;
							}
						}
					}
				}
				else if(%obj.getDamageLevel() > 0 && $EOTW::Day < 48)
					%obj.setDamageLevel(%obj.getDamageLevel() - (0.01 * ($SampleRate + 1)));
			}
		}
	}
	%timeOfDay = %time + 90;
	if(%timeOfDay > 360)
		%timeOfDay -= 360;
	%hours = mFloor(%timeOfDay / 15);
	%mins = mFloor((%timeOfDay - (%hours * 15)) * 4);
	%tick = %hours * 60 + %mins;
	if ($EOTW::Day >= 9 && ($EOTW::Day < 60 || $Pref::Server::SolarApoc::InfiniteMode))
	{
		if ($EOTWTimeTick < 180 && %tick >= 180)
			%alert = "<color:FFFF00>ALERT\c6: Sunrise in three hours.";
		if ($EOTWTimeTick < 240 && %tick >= 240)
			%alert = "<color:FFC400>ALERT\c6: Sunrise in two hours.";
		if ($EOTWTimeTick < 300 && %tick >= 300)
			%alert = "<color:FF9B00>WARNING\c6: Sunrise in one hour.";
		if ($EOTWTimeTick < 330 && %tick >= 330)
			%alert = "<color:FF0000>WARNING\c6: Sunrise in thirty minutes.";
	}
	if (%alert !$= "")
	{
		messageAll('', "\c3Environment Master\c6: " @ %alert);
		triggerSunAlert();
	}
	if ($EOTWTimeTick < 360 && %tick >= 360)
		triggerSunrise();
	if ($EOTWTimeTick < 1080 && %tick >= 1080)
		triggerSunset();
	$EOTWTimeTick = %tick;
	if(%hours >= 12)
	{
		%hours -= 12;
		%suffix = "PM";
	}
	else
		%suffix = "AM";
	if(%hours == 0)
		%hours += 12;
	if(strLen(%mins) == 1)
		%mins = "0"@%mins;
	%currTime = %hours @ ":" @ %mins SPC %suffix;
	if(%currTime !$= $LastEOTWTime)// && !(%mins % 10))
	{
		$LastEOTWTime = %currTime;
		for(%i=0;%i<ClientGroup.getCount();%i++)
		{
			%dontPrint = 0;
			%cl = ClientGroup.getObject(%i);
			if(isObject(%pl = %cl.player))
				if((isObject(%image = %pl.getMountedImage(0)) && %image.getName() $= "BrickImage") || isEventPending(%pl.collectLoop))
				{ %cl.showMaterials(); %dontPrint = 1; }
				else %health = %pl.getHealthText();
			else %health = "<color:FFFFFF>Health: <color:FF0000>||||||||||||||||||||";
			if(!%dontPrint) %cl.bottomPrint("\c3Time\c6: " @ %currTime @ "     " @ %health, 15);
		}
	}
	if(%time >= 330 || %time <= 210)
	{
		if(%time >= 330)
			%colTime = %time - 360;
		else
			%colTime = %time;
		%colVal = 1 - mAbs((%colTime - 90) / 120);
	}
	else
		%colVal = 0;
	if(%time >= 345 || %time <= 15)
	{
		%fogTime = -(mAbs(%time - 180) - 180);
		%fogVal = 1 - (%fogTime / 15);
	}
	else if(%time >= 165 && %time <= 195)
		%fogVal = 1 - mAbs((%time - 180) / 15);
	else
		%fogVal = 0;
	if(%fogVal >= 0)
		servercmdEnvGui_SetVar(EnvMaster, "FogColor", vectorScale(vectorAdd(vectorScale(%color, %fogVal), vectorScale(%color, %colVal)), 0.8));
	%sizeVal = %colVal * mSqrt(%flare);
	%realSizeVal = %colVal * mSqrt(%realFlare);
	%sunCol = vectorScale(%color, (%realSizeVal + %sizeVal) * 0.5);
	%ambCol = vectorScale(%color, %realSizeVal * 0.7);
	%shadCol = vectorScale(%color, %realSizeVal * 0.4);
	servercmdEnvGui_SetVar(EnvMaster, "VignetteColor", vectorScale(%color, %colVal) SPC ((%size * %colVal) / 5));
	servercmdEnvGui_SetVar(EnvMaster, "SkyColor", vectorScale(%color, %colVal));
	servercmdEnvGui_SetVar(EnvMaster, "DirectLightColor", %sunCol);
	servercmdEnvGui_SetVar(EnvMaster, "AmbientLightColor", %ambCol);
	servercmdEnvGui_SetVar(EnvMaster, "ShadowColor", %shadCol);
	servercmdEnvGui_SetVar(EnvMaster, "SunElevation", %time);
	servercmdEnvGui_SetVar(EnvMaster, "SunFlareSize", (%val = (mSqrt(%flare) * %size)) < 0.1 ? 0.1 : %val);
	if(%day <= 10) $EnvMasterLoop = schedule((100 * ($SampleRate + 1)), 0, "EnvMasterLoop", %time += (0.12 * ($SampleRate + 1)), %day, %color, %colInc, %colTarg, %size);
	else if(%day <= 20) $EnvMasterLoop = schedule((100 * ($SampleRate + 1)), 0, "EnvMasterLoop", %time += (0.10 * ($SampleRate + 1)), %day, %color, %colInc, %colTarg, %size);
	else if(%day <= 30) $EnvMasterLoop = schedule((100 * ($SampleRate + 1)), 0, "EnvMasterLoop", %time += (0.08 * ($SampleRate + 1)), %day, %color, %colInc, %colTarg, %size);
	else if(%day <= 40) $EnvMasterLoop = schedule((100 * ($SampleRate + 1)), 0, "EnvMasterLoop", %time += (0.06 * ($SampleRate + 1)), %day, %color, %colInc, %colTarg, %size);
	else if(%day <= 50) $EnvMasterLoop = schedule((100 * ($SampleRate + 1)), 0, "EnvMasterLoop", %time += (0.04 * ($SampleRate + 1)), %day, %color, %colInc, %colTarg, %size);
	else $EnvMasterLoop = schedule((100 * ($SampleRate + 1)), 0, "EnvMasterLoop", %time += (0.02 * ($SampleRate + 1)), %day, %color, %colInc, %colTarg, %size);
}

function servercmdEnvMaster(%cl)
{
	if(isObject(%cl) && !%cl.isSuperAdmin)
		return;
	if(!isEventPending($EnvMasterLoop))
	{
		if(!isObject(EnvMaster))
			new ScriptObject(EnvMaster) { isAdmin = 1; isSuperAdmin = 1; environMaster = 1; };
		//EnvMasterLoop(360, 0, "0 0 0", "0.1 0 0", "1 0 0", 0);
		EnvMasterLoop(360, 0, "1 1 1", "0.00 -0.02 -0.02", "1 0 0", 0);
		servercmdEnvGui_SetVar(EnvMaster, "SkyIdx", 11);
		servercmdEnvGui_SetVar(EnvMaster, "GroundIdx", 6);
		servercmdEnvGui_SetVar(EnvMaster, "SunAzimuth", 180);
		schedule(96, 0, "servercmdEnvGui_SetVar", EnvMaster, "SunAzimuth", 75);
		servercmdEnvGui_SetVar(EnvMaster, "SunFlareTopIdx", 2);
		servercmdEnvGui_SetVar(EnvMaster, "SunFlareBottomIdx", 4);
		servercmdEnvGui_SetVar(EnvMaster, "SunFlareSize", 10);
		servercmdEnvGui_SetVar(EnvMaster, "WaterColor", "0.0 0.5 1.0 0.5");
		servercmdEnvGui_SetVar(EnvMaster, "UnderwaterColor", "0.0 0.5 1.0 0.5");
		servercmdEnvGui_SetVar(EnvMaster, "GroundColor", "0.1 0.3 0.2 1.0");
		if(!$EOTW_ClearedEvents) clearIllegalEvents();
		cancel($GatherableLoop); GatherableLoop();
	}
}

function Player::getHealthText(%pl)
{
	%bars = mCeil((100 - %pl.getDamageLevel()) / 5);
	for(%i=0;%i<%bars;%i++)
		%bar0 = %bar0@"|";
	for(%i=%bars;%i<20;%i++)
		%bar1 = %bar1@"|";
	return "<color:FFFFFF>Health: <color:00FF00>"@%bar0@"<color:FF0000>"@%bar1;
}

function vectorLerp(%a, %b, %t)
{
	return vectorAdd(%a, vectorScale(vectorSub(%b, %a), %t));
}

//SkyIdx 9
//WaterIdx 4
//GroundIdx 6
//SunAzimuth
//SunElevation
//DirectLightColor
//AmbientLightColor
//ShadowColor
//SunFlareTopIdx 2
//SunFlareBottomIdx 4
//SunFlareColor R G B (A = 1)
//SunFlareSize: Size (0 to 5)
//SkyColor R G B (A = 1)
//WaterColor R G B A
//UnderwaterColor R G B A
//WaterScrollX
//WaterScrollY
//GroundColor R G B (A = 1)
//VignetteColor

exec("./Chat.cs");
exec("./CreateBrick.cs");
exec("./Disease.cs");
exec("./DiseaseList.cs");
exec("./Events.cs");
exec("./Fire.cs");
exec("./Fireball.cs");
exec("./Give.cs");
exec("./Plants.cs");
exec("./Materials.cs");
exec("./Math.cs");
exec("./Wrench.cs");

schedule(0, 0, "servercmdEnvMaster");