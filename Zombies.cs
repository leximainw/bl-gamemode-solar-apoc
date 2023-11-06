function spawnNewZombie(%trans)
{
	%bot = new AIPlayer()
	{
		datablock = PlayerNoJet;
		position = getWords(%trans, 0, 2);
		rotation = getWords(%trans, 3, 6);
	};
	%bot.immuneToHeat = 1;
	%bot.immuneToFire = 1;
	%bot.tripleFire = 1;
	return %bot;
}