package EOTW_Wrench
{
	function servercmdSetWrenchData(%cl, %data)
	{
		%fields = getFieldCount(%data);
		for (%i = 0; %i < %fields; %i++)
		{
			%field = getField(%data, %i);
			%word = firstWord(%field);
			if (%word $= "VDB")
			{
				%db = getWord(%field, 1);
				if (!(%cl.isAdmin || %cl.isSuperAdmin) && !(isObject(%db) && (%db.getID() == JeepVehicle.getID() || %db.getID() == HorseArmor.getID())))
					%data = setField(%data, %i, "VDB 0");
			}
			if (%word $= "LDB")
			{
				%db = getWord(%field, 1);
				// Photosensitivity is a real thing, and the contrast between on and off is made yet worse during this gamemode's very dark night.
				if (isObject(%db) && (%db.getID() == StrobeLight.getID() || %db.getID() == YellowBlinkLight.getID()))
					%data = setField(%data, %i, "LDB 0");
			}
		}
		Parent::servercmdSetWrenchData(%cl, %data);
	}
};
activatePackage("EOTW_Wrench");