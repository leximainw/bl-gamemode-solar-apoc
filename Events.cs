registerInputEvent("fxDtsBrick", "OnSunAlert", "Self fxDtsBrick");
registerInputEvent("fxDtsBrick", "OnSunrise", "Self fxDtsBrick");
registerInputEvent("fxDtsBrick", "OnSunset", "Self fxDtsBrick");

registerInputEvent("fxDtsBrick", "OnTimeMatch", "Self fxDtsBrick\tPlayer Player\tClient GameConnection\tMinigame Minigame");
registerInputEvent("fxDtsBrick", "OnTimeMismatch", "Self fxDtsBrick\tPlayer Player\tClient GameConnection\tMinigame Minigame");
registerOutputEvent("fxDtsBrick", "CheckTime", "int 0 1439\tint 0 1439", 0);

function fxDtsBrick::CheckTime(%brick, %time0, %time1)
{
	if (%time0 <= %time1)
		%match = %time0 <= $EOTWTimeTick && $EOTWTimeTick <= %time1;
	else
		%match = %time0 > $EOTWTimeTick || $EOTWTimeTick > %time1;
	if(%match)
		%brick.processInputEvent("OnTimeMatch", $InputTarget_Client);
	else
		%brick.processInputEvent("OnTimeMismatch", $InputTarget_Client);
}

function triggerSunAlert()
{
	triggerInput("OnSunAlert");
}

function triggerSunrise()
{
	triggerInput("OnSunrise");
}

function triggerSunset()
{
	triggerInput("OnSunset");
}

function triggerInput(%input)
{
	%groups = MainBrickgroup.getCount();
	for (%i = 0; %i < %groups; %i++)
	{
		%group = MainBrickgroup.getObject(%i);
		%bricks = %group.getCount();
		for (%j = 0; %j < %bricks; %j++)
		{
			%match = 0;
			%brick = %group.getObject(%j);
			for (%k = 0; %k < %brick.numEvents; %k++)
				if (%brick.eventInput[%k] $= %input)
				{
					%match = 1;
					break;
				}
			if (%match)
			{
				$InputTarget_Self = %brick;
				%brick.processInputEvent(%input, %group.client);
			}
		}
	}
}

function clearIllegalEvents()
{
	unregisterOutputEvent("fxDtsBrick", "setItem");		//Players shoot projectiles at others.
	unregisterOutputEvent("fxDtsBrick", "spawnExplosion");	//Griefers try to lag the server by mass spawning explosions.
	unregisterOutputEvent("fxDtsBrick", "spawnItem");	//Players shoot projectiles at others.
	unregisterOutputEvent("fxDtsBrick", "spawnProjectile");	//Players shoot projectiles at others.
	
	unregisterOutputEvent("GameConnection", "incScore");	//Useless except to brag about meaningless points.
	
	unregisterOutputEvent("Player", "addHealth");		//This is a survival-based gamemode. No healing.
	unregisterOutputEvent("Player", "changeDatablock");	//This is a survival-based gamemode. No jets.
	unregisterOutputEvent("Player", "clearTools");		//Griefers use this to clear others' tools.
	unregisterOutputEvent("Player", "instantRespawn");	//Griefers use this to make death traps.
	unregisterOutputEvent("Player", "kill");		//Griefers use this to make death traps.
	unregisterOutputEvent("Player", "setHealth");		//This is a survival-based gamemode. No healing.
	unregisterOutputEvent("Player", "setPlayerScale");	//Players abuse this to get out of needing to build a reasonably-sized shelter.
	unregisterOutputEvent("Player", "spawnExplosion");	//Griefers try to lag the server by mass spawning explosions.
	unregisterOutputEvent("Player", "spawnProjectile");	//Players shoot projectiles at others.
}
