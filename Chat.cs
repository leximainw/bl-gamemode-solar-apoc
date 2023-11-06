package EOTWChat
{
	function servercmdMessageSent(%cl, %msg)
	{
		Parent::servercmdMessageSent(%cl, %msg);
	}
};
activatePackage("EOTWChat");

function servercmdE(%cl, %e0, %e1, %e2, %e3, %e4, %e5, %e6, %e7, %e8, %e9, %e10, %e11, %e12, %e13, %e14, %e15, %e16, %e17)
{
	for (%i = 0; %i < 18; %i++)
		%emote = %emote SPC %e[%i];
	%emote = trim(%emote);
	if (%emote $= "")
		return;
	%emote = stripMLControlChars(%emote);
	messageAll('', "\c3*" @ %cl.getPlayerName() @ "\c6 " @ %emote);
}

function servercmdMe(%cl, %e0, %e1, %e2, %e3, %e4, %e5, %e6, %e7, %e8, %e9, %e10, %e11, %e12, %e13, %e14, %e15, %e16, %e17)
{
	servercmdE(%cl, %e0, %e1, %e2, %e3, %e4, %e5, %e6, %e7, %e8, %e9, %e10, %e11, %e12, %e13, %e14, %e15, %e16, %e17);
}