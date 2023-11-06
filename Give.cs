function servercmdGive(%cl, %vict, %v0, %v1, %v2, %v3, %v4, %v5, %v6, %v7, %v8, %v9)
{
	if (%vict $= "sun" || %vict $= "the sun" || (%vict $= "the" && %v0 $= "sun"))
	{
		messageClient(%cl, '', "\c6The Sun is too far away to give anything to, but you likely don't have long to wait.");
		return;
	}
	%targ = findClientByName(%vict);
	if (!isObject(%targ))
	{
		messageClient(%cl, '', "\c6Unable to find client \"" @ %vict @ "\".");
		return;
	}
	for (%i = 0; %i < 10; %i++)
		%value = %value SPC %v[%i];
	%value = trim(%value);
	if (%value $= "")
	{
		messageClient(%cl, '', "\c6You must put in a recipient, and what you want to give.");
		messageClient(%cl, '', "\c6For example, \"\c2/give\c3 Badspot 1000 Stone\c6\".");
		return;
	}
	if (!isMaterial(%value) && findClientByName(%value) == %targ)
	{
		if (%cl == %targ)
			messageClient(%cl, '', "\c6You give you yourself in an extremely confusing series of acrobatics that ultimately ends with you in the same place you started.");
		else
		{
			messageClient(%cl, '', "\c6You give " @ %targ.name @ " themselves. How this action came to take place is a mystery for the ages.");
			messageClient(%targ, '', "\c6" @ %cl.name @ " gave you yourself. How this action came to take place is a mystery for the ages.");
		}
		return;
	}
	else if (%value $= "Blockland")
	{
		if (%cl == %targ)
			messageClient(%cl, '', "\c6You're quite certain you already have that.");
		else
			messageClient(%cl, '', "\c6You have a sneaking suspicion that " @ %targ.name @ " already has that.");
		return;
	}
	else if (%value $= "easter egg" || %value $= "an egg" || %value $= "egg")
	{
		if (%cl == %targ)
			messageClient(%cl, '', "\c6You found one of the easter eggs hidden in /give.");
		else
		{
			messageClient(%cl, '', "\c6You found one of the easter eggs hidden in /give, and gave it to " @ %targ.name @ ".");
			messageClient(%targ, '', "\c6" @ %cl.name @ " found one of the easter eggs hidden in /give, and gave it to you.");
		}
		return;
	}
	else if (%value $= "life" || %value $= "a life")
	{
		if (%cl == %targ)
			messageClient(%cl, '', "\c6You make a token effort to have a life, but continue to play video games instead.");
		else
			messageClient(%cl, '', "\c6You try to give " @ %targ.name @ " a life, but it turns out you don't have one to give.");
		return;
	}
	else if (%value $= "nothing")
	{
		if (%cl == %targ)
			messageClient(%cl, '', "\c6You give yourself nothing. You now have " @ %cl.nothingCount++ @ " nothing.");
		else
		{
			messageClient(%cl, '', "\c6You give " @ %targ.name @ " nothing. They now have " @ %targ.nothingCount++ @ " nothing.");
			messageClient(%targ, '', "\c6" @ %cl.name @ " gave you nothing. You now have " @ %targ.nothingCount @ " nothing.");
		}
		return;
	}
	else if (%value $= "sun" || %value $= "the sun")
	{
		messageClient(%cl, '', "\c6You try to give " @ %targ.name @ " the Sun, but your arm isn't long enough.");
		return;
	}
	%amt = firstWord(%value);
	%mat = restWords(%value);
	if (%amt == 0)
	{
		if (%cl == %targ)
			messageClient(%cl, '', "\c6You give yourself nothing. That was a waste of time." @ (striPos(%amt, "0") == 0 ? "" : " (Did you forget to put in an amount?)"));
		else
			messageClient(%cl, '', "\c6You give " @ %targ.name @ " nothing. That was a waste of time." @ (striPos(%amt, "0") == 0 ? "" : " (Did you forget to put in an amount?)"));
		return;
	}
	else if (!isMaterial(%mat))
	{
		if (%cl == %targ)
			messageClient(%cl, '', "\c6You try to give yourself " @ %amt SPC %mat @ ", but it turns out " @ %mat @ " isn't a thing in the game.");
		else
			messageClient(%cl, '', "\c6You try to give " @ %targ.name SPC %amt SPC %mat @ ", but it turns out " @ %mat @ " isn't a thing in the game.");
		return;
	}
	else if (%amt < 0)
	{
		if (%cl == %targ)
			messageClient(%cl, '', "\c6You steal " @ getMin(-%amt, $EOTW::Material[%cl.bl_id, %mat]) SPC %mat @ " from yourself. You feel like quite an accomplished pickpocket, until you realize what just happened.");
		else
			messageClient(%cl, '', "\c6You try to give " @ %targ.name SPC %amt SPC %mat @ ", but are informed that's called \"stealing\".");
		return;
	}
	else
	{
		if (%cl == %targ)
			messageClient(%cl, '', "\c6You give " @ getMin(%amt, $EOTW::Material[%cl.bl_id, %mat]) SPC %mat @ " to yourself. You feel rather silly.");
		else
		{
			if (%mat $= $SturdiumName)
				%mat = "Sturdium";
			%amt = getMin(%amt, $EOTW::Material[%cl.bl_id, %mat]);
			$EOTW::Material[%cl.bl_id, %mat] -= %amt;
			$EOTW::Material[%targ.bl_id, %mat] += %amt;
			messageClient(%cl, '', "\c6You give " @ %targ.name SPC %amt SPC %mat @ ".");
			messageClient(%targ, '', "\c6" @ %cl.name @ " gave you " @ %amt SPC %mat @ ".");
		}
	}
}