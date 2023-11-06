function Player::addDisease(%pl, %disease, %stage)
{
	if (!isObject(%disease) || !isObject(%cl = %pl.client))
		return;
	%disease = %disease.getID();
	if (!isObject(%cl.diseaseInfo))
		%cl.diseaseInfo = new SimSet();
	%now = getSimTime();
	if (!%cl.diseaseInfo.isMember(%disease))
	{
		%cl.diseaseInfo.stage[%disease] = %stage;
		%cl.diseaseInfo.add(%disease);
		%disease.onPlayerInfected(%pl);
		%cl.diseaseInfo.lastTick[%disease] = %now;
		talk(%cl.name @ " caught " @ %disease.getName() @ "!");
	}
	else
		%cl.diseaseInfo.stage[%disease] = getMax(%cl.diseaseInfo.stage[%disease], %stage);
	%pl.updateMovePenalty();
	if (!isEventPending(%pl.diseaseTick))
		%pl.diseaseTick = %pl.schedule(50, "diseaseTick", %now);
}

function Player::cureDisease(%pl, %disease)
{
	if (!isObject(%disease) || !isObject(%cl = %pl.client)
		|| !isObject(%cl.diseaseInfo) || !%cl.diseaseInfo.isMember(%disease))
		return;
	%disease = %disease.getID();
	%cl.diseaseInfo.remove(%disease);
	%cl.diseaseInfo.stage[%disease] = 0;
	%disease.onPlayerConditionImprove(%pl, 1);
	%pl.updateMovePenalty();
}

function Player::cureAllDiseases(%pl)
{
	if (!isObject(%cl = %pl.client) || !isObject(%cl.diseaseInfo))
		return;
	%dCount = %cl.diseaseInfo.getCount();
	for (%i = 0; %i < %dCount; %i++)
		%pl.cureDisease(%cl.diseaseInfo.getObject(0));
}

function Player::diseaseTick(%pl, %last)
{
	cancel(%pl.diseaseTick);
	if (!isObject(%cl = %pl.client))
		return;
	if (!isObject(%cl.diseaseInfo))
		%cl.diseaseInfo = new SimSet();
	%time = vectorDist(%last, %now = getSimTime()) / 1000;
	%dCount = %cl.diseaseInfo.getCount();
	%cl.diseaseInfo.sunResist = 0;
	for (%i = 0; %i < %dCount; %i++)
	{
		%disease = %cl.diseaseInfo.getObject(%i);
		%disease.onPlayerDiseaseTick(%pl, %time);
		%stage = %cl.diseaseInfo.stage[%disease];
		if (%stage <= 0)
			continue;
		%cl.diseaseInfo.sunResist += %disease.sunResistBase + %disease.sunResistPerStage * (%stage - 1);
		%tickTime = vectorDist(%cl.diseaseInfo.lastTick[%disease], %now) / 1000;
		if (%tickTime >= %disease.conditionTickLen)
		{
			%cl.diseaseInfo.lastTick[%disease] = %now;
			%posChance = %disease.basePosChance + %disease.perStagePosChance * (%stage - 1)
				+ %disease.stagePosChance[%stage] + %disease.resistPosChance * %cl.diseaseInfo.resist[%disease];
			%negChance = %disease.baseNegChance + %disease.perStageNegChance * (%stage - 1)
				+ %disease.stageNegChance[%stage] + %disease.resistNegChance * %cl.diseaseInfo.resist[%disease];
			%posWin = getRandom() < %posChance;
			%negWin = getRandom() < %negChance;
			if (%posWin && !%negWin)
			{
				%cl.diseaseInfo.stage[%disease]--;
				if (%cl.diseaseInfo.stage[%disease] <= 0)
				{
					if (%cl.diseaseInfo.immune[%disease] < %disease.maxImmunity)
						%cl.diseaseInfo.immune[%disease]++;
				}
				else
				{
					if (getRandom() < %disease.resistIncOnImprove
						&& %cl.diseaseInfo.resist[%disease] < %disease.maxResistance)
						%cl.diseaseInfo.resist[%disease]++;
				}
				%pl.updateMovePenalty();
				%disease.onPlayerConditionImprove(%pl, %cl.diseaseInfo.stage[%disease] <= 0);
				if (%cl.diseaseInfo.stage[%disease] <= 0)
					continue;
			}
			else if (%negWin && !%posWin)
			{
				%cl.diseaseInfo.stage[%disease]++;
				if (%cl.diseaseInfo.stage[%disease] <= %disease.stages)
				{
					if (getRandom() < (%disease.resistIncOnWorsen + %disease.resistOnWorsenStage[%cl.diseaseInfo.stage[%disease]])
						&& %cl.diseaseInfo.resist[%disease] < %disease.maxResistance)
						%cl.diseaseInfo.resist[%disease]++;
				}
				%pl.updateMovePenalty();
				%disease.onPlayerConditionWorsen(%pl, %cl.diseaseInfo.stage[%disease] > %disease.stages);
				if (!isObject(%pl) || %pl.getState() $= "DEAD")
					break;
				if (%cl.diseaseInfo.stage[%disease] <= 0)
					continue;
			}
		}
		%sCount = %disease.numSymptoms;
		for (%j = 0; %j < %sCount; %j++)
		{
			%symptom = %disease.symptom[%j];
			%bChance = %disease.symptomChance[%j] + %disease.symptomChancePerStage[%j] * (%stage - 1);
			%chance = 1 - mPow(1 - %bChance, %time);
			if (getRandom() < %chance)
				%disease.onPlayerSymptom(%pl, %symptom);
		}
	}
	if (%dCount == 0)
		return;
	%pl.diseaseTick = %pl.schedule(50, "diseaseTick", %now);
}

function Player::updateMovePenalty(%pl)
{
	if (!isObject(%cl = %pl.client))
		return;
	if (!isObject(%cl.diseaseInfo))
		return;
	%dCount = %cl.diseaseInfo.getCount();
	for (%i = 0; %i < %dCount; %i++)
	{
		%disease = %cl.diseaseInfo.getObject(%i);
		%stage = %cl.diseaseInfo.stage[%disease];
		if (%stage <= 0)
			continue;
		%dPenalty = %disease.baseMovePenalty + %disease.stageMovePenalty * (%stage - 1) + %disease.stageMovePenalty[%stage];
		%penalty += %dPenalty;
	}
	%speed = getMax(1 - %penalty, 0);
	%data = %pl.getDatablock();
	%pl.setMaxForwardSpeed(%data.maxForwardSpeed * %speed);
	%pl.setMaxBackwardSpeed(%data.maxBackwardSpeed * %speed);
	%pl.setMaxSideSpeed(%data.maxSideSpeed * %speed);
	%pl.setMaxCrouchForwardSpeed(%data.maxForwardCrouchSpeed * %speed);
	%pl.setMaxCrouchBackwardSpeed(%data.maxBackwardCrouchSpeed * %speed);
	%pl.setMaxCrouchSideSpeed(%data.maxSideCrouchSpeed * %speed);
}

function infectiousZone(%disease, %pos, %radius, %infectChance, %infectFalloff, %lineOfEffect, %lingerTime, %lingerFalloff, %startTime, %prev)
{
	%now = getSimTime();
	if (%startTime $= "")
		%startTime = %now;
	%deltaS = vectorDist(%now, %startTime) / 1000;
	if (%deltaS >= %lingerTime)
		return;
	%deltaT = vectorDist(%now, %prev) / 1000;
	%count = ClientGroup.getCount();
	for (%i = 0; %i < %count; %i++)
	{
		%cl = ClientGroup.getObject(%i);
		%pl = %cl.player;
		if (!isObject(%pl))
			continue;
		%loc = %pl.getHackPosition();
		if (%lineOfEffect)
		{
			%mask = $Typemasks::fxBrickObjectType;
			%ray = containerRaycast(%pos, %loc, %mask);
			if (isObject(%ray))
				continue;
		}
		%dist = vectorDist(%pos, %loc);
		if (%dist > %radius)
			continue;
		%rValue = 1 - (%dist / %radius);
		%rValue = mPow(%rValue, %infectFalloff);
		%lValue = 1 - (%deltaS / %lingerTime);
		%lValue = mPow(%lValue, %lingerFalloff);
		%chance = mPow(%infectChance * %rValue * %lValue, %deltaT);
		if (getRandom() <= %chance)
			%pl.addDisease(%disease, 1);
	}
	schedule(1000, 0, "infectiousZone", %disease, %pos, %radius, %infectChance, %infectFalloff, %lineOfEffect, %lingerTime, %lingerFalloff, %startTime, %now);
}

package EOTW_Disease
{
	function Player::delete(%pl)
	{
		if (isObject(%cl.diseaseInfo))
			%cl.diseaseInfo.delete();
		Parent::delete(%pl);
	}
};
activatePackage("EOTW_Disease");