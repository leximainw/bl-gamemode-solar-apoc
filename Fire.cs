function fxDtsBrick::burnLoop(%brick, %last)
{
	cancel(%brick.burnLoop);
	if(%brick.burnedFuel == 0)
		%brick.setEmitter(BurnEmitterB);
	%data = %brick.getDatablock();
	%volume = %data.brickSizeX * %data.brickSizeY * %data.brickSizeZ;
	%time = vectorDist(%last, %now = getSimTime()) / 1000;
	%brick.burnedFuel += %time;
	if(%brick.burnedFuel >= mSqrt(%volume))
	{ %brick.dontRefund = 1; if(%brick.material $= "Dead_Plant") %brick.trulyDont = 1; %brick.killBrick(); return; }
	if(mAbs(%brick.angleID - 2) == 1)
	{
		%sizeX = %data.brickSizeY / 2;
		%sizeY = %data.brickSizeX / 2;
	}
	else
	{
		%sizeX = %data.brickSizeX / 2;
		%sizeY = %data.brickSizeY / 2;
	}
	%sizeZ = %data.brickSizeZ / 5;
	%box = vectorAdd(%sizeX SPC %sizeY SPC %sizeZ, "0.05 0.05 0.05");
	initContainerBoxSearch(%brick.getPosition(), %box, $Typemasks::fxBrickAlwaysObjectType);
	while(isObject(%test = containerSearchNext()))
		if(%test != %brick && !isEventPending(%test.burnLoop) && %test.isFlammable)
		{
			%testDB = %test.getDatablock();
			%testVol = %testDB.brickSizeX * %testDB.brickSizeY * %testDB.brickSizeZ;
			%sizeDiff = %testVol / %volume; %ignite = !getRandom(0, mFloor(%sizeDiff * 80) + 20);
			if(%ignite) %test.burnLoop(getSimTime());
		}
	if($EOTW::Day >= 38)
	{
		%mask = $Typemasks::fxBrickObjectType;
		initContainerBoxSearch(%pos = %brick.getPosition(), %box, $Typemasks::PlayerObjectType);
		while(isObject(%pl = containerSearchNext()))
		{
			if(!isObject(containerRaycast(%pos, %pl.getHackPosition(), %mask)))
			{
				%pl.burningTime += 0.02;
				if(!isEventPending(%pl.burnLoop))
					%pl.burnLoop(getSimTime());
			}
		}
	}
	%brick.burnLoop = %brick.schedule(48, "burnLoop", %now);
}

function Player::burnLoop(%pl, %last)
{
	cancel(%pl.burnLoop);
	if(!isObject(%pl) || %pl.getState() $= "DEAD") return;
	if(!isEventPending(%pl.EOTW_FireTrail))
		%pl.EOTW_FireTrail();
	%mask = $Typemasks::fxBrickObjectType;
	initContainerRadiusSearch(%pos = %pl.getHackPosition(), 10, $Typemasks::PlayerObjectType);
	while(isObject(%this = containerSearchNext()))
		if(vectorDist(%pos, %loc = %this.getHackPosition()) <= 4 && !isObject(containerRaycast(%pos, %loc, %mask)) && !%this.immuneToFire)
		{ %sum += %this.burningTime; %obj[-1+%objs++] = %this; } %avg = %sum / %objs;
	if(%objs > 1 && %avg > 0.05) for(%i=0;%i<%objs;%i++) { %obj[%i].burningTime = %avg; if(!isEventPending(%obj[%i].burnLoop)) %obj[%i].schedule(16, "burnLoop", getSimTime()); }
	%time = vectorDist(%last, %now = getSimTime()) / 1000;
	if(%time > %pl.burningTime)
	{
		%dmg = %pl.burningTime * 5;
		%pl.burningTime = 0;
	}
	else
	{
		%dmg = %time * 5;
		%pl.burningTime -= %time;
	}
	if(!%pl.immuneToBurn)
	{
		//%pl.setDamageLevel(%pl.getDamageLevel() + %dmg);
		//%pl.setDamageFlash(0.25);
		//if(%pl.getDamageLevel() >= %pl.getDatablock().maxDamage)
		//	%pl.damage(0, 0, 10000, $DamageType::BurnedToDeath);
		
		%pl.damage(0, 0, %dmg, $DamageType::BurnedToDeath);
		%pl.setDamageFlash(0.25);
	}
	if(%pl.burningTime == 0) { cancel(%pl.EOTW_FireTrail); return; }
	%pl.burnLoop = %pl.schedule(16, "burnLoop", %now);
}

function Player::EOTW_FireTrail(%pl)
{
	cancel(%pl.EOTW_FireTrail);
	if(!isObject(%pl) || %pl.getState() $= "DEAD") return;
	%flame = new ParticleEmitterNode()
	{
		datablock = GenericEmitterNode;
		emitter = BurnEmitterA;
		position = %pl.position;
	};
	%flame.delete = %flame.schedule(500, "delete");
	%pl.EOTW_FireTrail = %pl.schedule(100, "EOTW_FireTrail");
}