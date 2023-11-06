function fxDtsBrick::EOTW_GrowGrass(%brick, %darkTime, %growTime, %fryTime, %last)
{
	if(!isObject(%brick) || %brick.isDead()) return;
	if(!isObject(PlantGroup)) new SimSet(PlantGroup);
	if(!PlantGroup.isMember(%brick)) PlantGroup.add(%brick);
	%time = vectorDist(%last, %now = getSimTime()) / 1000;
	%pi = 3.14159265; %val = ($EOTW::Time / 180) * %pi;
	%ang = ($EnvGuiServer::SunAzimuth / 180) * %pi;
	%dir = vectorScale(mSin(%ang) * mCos(%val) SPC mCos(%ang) * mCos(%val) SPC mSin(%val), 512);
	%depth = 0;
	for(%i=0;%i<3;%i++)
	{
		%ray = containerRaycast(vectorAdd(%pos = %brick.getPosition(), %dir), %pos, $Typemasks::fxBrickAlwaysObjectType, %brick, %hit0, %hit1, %hit2);
		if(isObject(%hit = firstWord(%ray)) && %hit != %brick)
			if(getWord(getColorIDTable(%hit.getColorID()), 3) > 0.95 && %hit.isRendering() && getWord(%name = %hit.getDatablock().uiName, getWordCount(%name) - 1) !$= "Window") { %dark = 1; %depth = 1; }
			else { %hit[%i] = %hit; %depth = 1; }
	}
	if($EOTW::Time > 180)
	{
		%darkTime += %time;
		%growTime = 0;
		%fryTime = 0;
	}
	else if((%depth == 0 && $EOTW::Day >= 7) || $EOTW::Day >= 37)
	{
		%darkTime = 0;
		%growTime = 0;
		%fryTime += %time;
		if(%fryTime >= 5)
		{
			%brick.dontRefund = 1;
			%brick.isIgnitable = 1;
			%brick.unrecolorable = 0;
			%brick.setColor($EOTW::Colors::Dead);
			%brick.material = "Dead_Plant";
			%brick.unrecolorable = 1;
			return;
		}
	}
	else if(!%dark)
	{
		%darkTime = 0;
		%growTime += %time;
		%fryTime = 0;
		if(%growTime >= 15)
		{
			%growTime -= 15;
			%dir = getField("0.0 0.5 0.0	0.5 0.0 0.0	0.0 -0.5 0.0	-0.5 0.0 0.0", getRandom(0, 3));
			if(isObject(%ray = containerRaycast(%pos = %brick.getPosition(), %spawn = vectorAdd(%pos, %dir), $Typemasks::fxBrickAlwaysObjectType)))
			{
				if(%ray.material !$= "Dead_Plant" && %ray.material !$= "Grass" && %ray.material !$= "Vine" && %ray.isColliding())
				if(!isObject(containerRaycast(%pos, %high = vectorAdd(%pos, "0.0 0.0 0.2"), $Typemasks::fxBrickAlwaysObjectType)))
					if(!isObject(containerRaycast(%high, %spawn = vectorAdd(%high, %dir), $Typemasks::fxBrickAlwaysObjectType)))
						%spawnPos = %spawn;
			}
			else if(isObject(%ray = containerRaycast(%spawn, %low = vectorAdd(%spawn, "0.0 0.0 -0.2"), $Typemasks::fxBrickAlwaysObjectType)) && %ray.isColliding())
			{ if(%ray.material !$= "Dead_Plant" && %ray.material !$= "Grass" && %ray.material !$= "Vine") %spawnPos = %spawn; }
			else if(isObject(%ray = containerRaycast(%low, %bottom = vectorAdd(%low, "0.0 0.0 -0.2"), $Typemasks::fxBrickAlwaysObjectType)) && %ray.isColliding())
			{ if(%ray.material !$= "Dead_Plant" && %ray.material !$= "Grass" && %ray.material !$= "Vine") %spawnPos = %low; }
			if(%spawnPos !$= "")
			{
				%data = EOTW_CreateBrick(%brick.getGroup(), %brick.getDatablock(), %spawnPos, $EOTW::Colors::Grass);
				%newBrick = getField(%data, 0);
				if(getField(%data, 1))
					%newBrick.delete();
				else
				{
					%newBrick.canBeFireballed = 1;
					%newBrick.isBreakable = 1;
					%newBrick.isFlammable = 1;
					%newBrick.material = "Grass";
					%newBrick.unrecolorable = 1;
					%newBrick.setColliding(0);
					%newBrick.EOTW_GrowGrass(0, getRandom(0, 1000) / 1000, 0, %now);
				}
			}
		}
	}
	else
	{
		%darkTime += %time;
		%growTime = 0;
		%fryTime = 0;
	}
	if(%darkTime >= 2100)
	{
		%brick.dontRefund = 1;
		%brick.isIgnitable = 1;
		%brick.unrecolorable = 0;
		%brick.setColor($EOTW::Colors::Dead);
		%brick.material = "Dead_Plant";
		%brick.unrecolorable = 1;
		return;
	}
	%brick.growthLoop = %brick.schedule(16 * mCeil(PlantGroup.getCount() / 10), "EOTW_GrowGrass", %darkTime, %growTime, %fryTime, %now);
}

function fxDtsBrick::EOTW_GrowVine(%brick, %darkTime, %growTime, %fryTime, %last)
{
	if(!isObject(%brick) || %brick.isDead()) return;
	if(!isObject(PlantGroup)) new SimSet(PlantGroup);
	if(!PlantGroup.isMember(%brick)) PlantGroup.add(%brick);
	%time = vectorDist(%last, %now = getSimTime()) / 1000;
	%pi = 3.14159265; %val = ($EOTW::Time / 180) * %pi;
	%ang = ($EnvGuiServer::SunAzimuth / 180) * %pi;
	%dir = vectorScale(mSin(%ang) * mCos(%val) SPC mCos(%ang) * mCos(%val) SPC mSin(%val), 512);
	%depth = 0;
	for(%i=0;%i<3;%i++)
	{
		%ray = containerRaycast(vectorAdd(%pos = %brick.getPosition(), %dir), %pos, $Typemasks::fxBrickAlwaysObjectType, %brick, %hit0, %hit1, %hit2);
		if(isObject(%hit = firstWord(%ray)) && %hit != %brick)
			if(getWord(getColorIDTable(%hit.getColorID()), 3) > 0.95 && %hit.isRendering() && getWord(%name = %hit.getDatablock().uiName, getWordCount(%name) - 1) !$= "Window") { %dark = 1; %depth = 1; }
			else { %hit[%i] = %hit; %depth = 1; }
	}
	if($EOTW::Time > 180)
	{
		%darkTime += %time;
		%growTime = 0;
		%fryTime = 0;
	}
	else if((%depth == 0 && $EOTW::Day >= 14) || $EOTW::Day >= 47)
	{
		%darkTime = 0;
		%growTime = 0;
		%fryTime += %time;
		if(%fryTime >= 5)
		{
			%brick.dontRefund = 1;
			%brick.isIgnitable = 1;
			%brick.unrecolorable = 0;
			%brick.setColor($EOTW::Colors::Dead);
			%brick.material = "Dead_Plant";
			%brick.unrecolorable = 1;
			return;
		}
	}
	else if(!%dark)
	{
		%darkTime = 0;
		%growTime += %time;
		%fryTime = 0;
		initContainerBoxSearch(%pos = %brick.getPosition(), "0.55 0.55 0.25", $Typemasks::fxBrickAlwaysObjectType);
		while(isObject(%test = containerSearchNext()))
			if(%test.material $= "Wood" && %test.isColliding())
				%wooden = 1;
		if(%growTime >= (%wooden ? 5 : 15))
		{
			%growTime -= 3;
			%dir = getField("0.0 0.5 0.0	0.5 0.0 0.0	0.0 -0.5 0.0	-0.5 0.0 0.0	0.0 0.0 0.2	0.0 0.0 -0.2", getRandom(0, 5));
			if(!isObject(%ray = containerRaycast(%pos, %spawn = vectorAdd(%pos, %dir), $Typemasks::fxBrickAlwaysObjectType)) && getWord(%spawn, 2) > 0.05)
			{
				initContainerBoxSearch(%spawn, "0.55 0.55 0.25", $Typemasks::fxBrickAlwaysObjectType);
				while(isObject(%test = containerSearchNext()))
					if(%test.material !$= "Dead_Plant" && %test.material !$= "Grass" && %test.material !$= "Vine" && %test.isColliding())
						%supported = 1;
				if(%supported)
				{
					%data = EOTW_CreateBrick(%brick.getGroup(), %brick.getDatablock(), %spawn, $EOTW::Colors::Vine);
					%newBrick = getField(%data, 0);
					if((%error = getField(%data, 1)) && %error != 2)
						%newBrick.delete();
					else
					{
						%growTime -= 12;
						%newBrick.canBeFireballed = 1;
						%newBrick.isBreakable = 1;
						%newBrick.isFlammable = 1;
						%newBrick.material = "Vine";
						%newBrick.unrecolorable = 1;
						%newBrick.setColliding(0);
						%newBrick.EOTW_GrowVine(0, getRandom(0, 1000) / 1000, 0, %now);
					}
				}
			}
		}
	}
	else
	{
		%darkTime += %time;
		%growTime = 0;
		%fryTime = 0;
	}
	if(%darkTime >= 6000)
	{
		%brick.dontRefund = 1;
		%brick.isIgnitable = 1;
		%brick.unrecolorable = 0;
		%brick.setColor($EOTW::Colors::Dead);
		%brick.material = "Dead_Plant";
		%brick.unrecolorable = 1;
		return;
	}
	%brick.growthLoop = %brick.schedule(16 * mCeil(PlantGroup.getCount() / 10), "EOTW_GrowVine", %darkTime, %growTime, %fryTime, %now);
}