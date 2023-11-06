function addSymptom(%obj)
{
	if (!isObject(%obj) || %obj.class !$= "SymptomSO" || %obj.name $= "")
		return;
	if (!isObject(SymptomGroup))
		new SimSet(SymptomGroup);
	%group = SymptomGroup;
	%group.elem[%obj.name] = %obj;
	%group.add(%obj);
}

function SymptomSO::onSymptom(%this, %pl) { }

if (isObject(Symptom_Cough))
	Symptom_Cough.delete();
new ScriptObject(Symptom_Cough)
{
	class = "SymptomSO";
	name = "cough";
};
function Symptom_Cough::onSymptom(%this, %pl, %disease)
{
	%pos = %disease.symptomPos[%this];
	%radius = %disease.symptomData[%pos, 0];
	%infectChance = %disease.symptomData[%pos, 1];
	%infectFalloff = %disease.symptomData[%pos, 2];
	%lineOfEffect = %disease.symptomData[%pos, 3];
	%lingerTime = %disease.symptomData[%pos, 4];
	%lingerFalloff = %disease.symptomData[%pos, 5];
	infectiousZone(%disease, %pl.getHackPosition(), %radius, %infectChance, %infectFalloff, %lineOfEffect, %lingerTime, %lingerFalloff);
	servercmdE(%pl.client, "coughs.");
	%pl.playThread(0, "plant");
}
addSymptom(Symptom_Cough);

if (isObject(Symptom_Sneeze))
	Symptom_Sneeze.delete();
new ScriptObject(Symptom_Sneeze)
{
	class = "SymptomSO";
	name = "sneeze";
};
function Symptom_Sneeze::onSymptom(%this, %pl, %disease)
{
	%pos = %disease.symptomPos[%this];
	%radius = %disease.symptomData[%pos, 0];
	%infectChance = %disease.symptomData[%pos, 1];
	%infectFalloff = %disease.symptomData[%pos, 2];
	%lineOfEffect = %disease.symptomData[%pos, 3];
	%lingerTime = %disease.symptomData[%pos, 4];
	%lingerFalloff = %disease.symptomData[%pos, 5];
	infectiousZone(%disease, %pl.getHackPosition(), %radius, %infectChance, %infectFalloff, %lineOfEffect, %lingerTime, %lingerFalloff);
	servercmdE(%pl.client, "sneezes.");
	%pl.playThread(0, "plant");
	%pl.playThread(1, "plant");
}

if (isObject(Symptom_Miasma))
	Symptom_Miasma.delete();
new ScriptObject(Symptom_Miasma)
{
	class = "SymptomSO";
	name = "miasma";
};
function Symptom_Miasma::onSymptom(%this, %pl, %disease)
{
	%pos = %disease.symptomPos[%this];
	%radius = %disease.symptomData[%pos, 0];
	%infectChance = %disease.symptomData[%pos, 1];
	%infectFalloff = %disease.symptomData[%pos, 2];
	%lineOfEffect = %disease.symptomData[%pos, 3];
	%lingerTime = %disease.symptomData[%pos, 4];
	%lingerFalloff = %disease.symptomData[%pos, 5];
	%emitter = new ParticleEmitterNode()
	{
		datablock = GenericEmitterNode;
		emitter = FogEmitterA;
		position = %pl.getHackPosition();
	};
	%emitter.setColor("0.8 1 0.5");
	%emitter.schedule((%lingerTime * 1000) | 0, "delete");
	infectiousZone(%disease, %pl.getHackPosition(), %radius, %infectChance, %infectFalloff, %lineOfEffect, %lingerTime, %lingerFalloff);
}


function addDisease(%obj)
{
	if (!isObject(%obj) || %obj.class !$= "DiseaseSO" || %obj.name $= "")
		return;
	if (!isObject(DiseaseGroup))
		new SimSet(DiseaseGroup);
	%group = DiseaseGroup;
	%group.elem[%obj.name] = %obj;
	%group.add(%obj);
}

function DiseaseSO::addSymptom(%this, %symptom, %chance, %chancePerStage,
	%data0, %data1, %data2, %data3, %data4,
	%data5, %data6, %data7, %data8, %data9)
{
	%pos = %this.numSymptoms++ - 1;
	%this.symptom[%pos] = %symptom;
	%this.symptomPos[%symptom] = %pos;
	if (%chance < 0 && %chancePerStage < 0)
	{
		%chance = 0;
		%chancePerStage = 0;
		%this.symptomOnDeath = %symptom;
	}
	%this.symptomChance[%pos] = %chance;
	%this.symptomChancePerStage[%pos] = %chancePerStage;
	for (%i = 0; %i < 10 && %data[%i] !$= ""; %i++)
		%this.symptomData[%pos, %i] = %data[%i];
	return %this;
}

function DiseaseSO::onPlayerInfected(%this, %pl) { }
function DiseaseSO::onPlayerConditionWorsen(%this, %pl, %terminal)
{
	if (%terminal)
		if (%this.isTerminal)
		{
			if (isObject(%this.symptomOnDeath))
				%this.symptomOnDeath.onSymptom(%pl, %this);
			%pl.cureAllDiseases();
			%pl.kill();
		}
		else
			%pl.client.diseaseInfo.stage[%this.getID()] = %this.stages;
}
function DiseaseSO::onPlayerConditionImprove(%this, %pl, %cured)
{
	if (%cured && !%this.isRetrovirus)
		%pl.cureDisease(%this);
}
function DiseaseSO::onPlayerSymptom(%this, %pl, %symptom)
{
	%symptom.onSymptom(%pl, %this);
}
function DiseaseSO::onPlayerDiseaseTick(%this, %pl, %time) { }

if (isObject(Disease_CommonCold))
	Disease_CommonCold.delete();
new ScriptObject(Disease_CommonCold)
{
	class = "DiseaseSO";
	name = "Common Cold";
	
	// getting better/worse
	stages = 5;
	isTerminal = 1;
	conditionTickLen = 15;
	basePosChance = 0.00;
	baseNegChance = 0.06;
	perStagePosChance = 0.03;
	perStageNegChance = 0.01;
	
	// resistance and immunity
	// resistance helps you beat a disease once you get it
	// immunity helps you not catch a disease before you get it
	maxResistance = 10;
	clearResistanceOnPlague = 1;
	resistPosChance = 0.004;
	resistNegChance = -0.004;
	resistIncOnImprove = 1;
	resistOnWorsenStage3 = 0.25;
	resistOnWorsenStage4 = 0.5;
	resistOnWorsenStage5 = 0.75;
	maxImmunity = 5;
	clearImmunityOnPlague = 1;
	immuneResistRate = 0.0001;
	immuneResistBurst = 0.2;
	
	// movement penalty for this illness
	baseMovePenalty = 0.10;
	stageMovePenalty = 0.05;
	
	// passive infection weight
	infectLineOfSight = 1;
	infectRefDist = 8;
	infectFalloff = 2;
	infectBaseChance = 0.0002;
	infectChancePerStage = 0.0001;
};
Disease_CommonCold
.addSymptom(Symptom_Cough, 0.005, 0.01, 8, 0.02, 2, 1, 15, 2)
.addSymptom(Symptom_Sneeze, 0.001, 0.002, 12, 0.12, 1, 1, 60, 1)
.addSymptom(Symptom_Miasma, -1, -1, 20, 0.2, 2, 1, 120, 1);
addDisease(Disease_CommonCold);

if (isObject(Disease_Granite))
	Disease_Granite.delete();
new ScriptObject(Disease_Granite)
{
	class = "DiseaseSO";
	name = "Granite Germination";
	
	// getting better/worse
	stages = 20;
	conditionTickLen = 15;
	baseNegChance = 1;
	alwaysCureOnDeath = 1;
	
	// movement penalty for this illness
	baseMovePenalty = 0.05;
	stageMovePenalty = 0.05;
	
	// passive infection weight
	infectLineOfSight = 1;
	infectRefDist = 8;
	infectFalloff = 2;
	infectBaseChance = 0.0001;
	infectChancePerStage = 0.00002;
	infectChanceStage20 = 0.00102;
	
	// benefits
	sunResistBase = 0.05;
	sunResistPerStage = 0.05;
};
function Disease_Granite::onPlayerInfected(%this, %pl)
{
	%this.updateClientAvatar(%pl.client);
	DiseaseSO::onPlayerInfected(%this, %pl);
}
function Disease_Granite::onPlayerConditionWorsen(%this, %pl, %terminal)
{
	%this.updateClientAvatar(%pl.client);
	DiseaseSO::onPlayerConditionWorsen(%this, %pl, %terminal);
}
function Disease_Granite::onPlayerConditionImprove(%this, %pl, %cured)
{
	%this.updateClientAvatar(%pl.client);
	DiseaseSO::onPlayerConditionWorsen(%this, %pl, %cured);
}
function Disease_Granite::updateClientAvatar(%this, %cl)
{
	%cl.graniteOverride = 1;
	if (%cl.diseaseInfo.stage[%this.getID()] <= 0)
	{
		%cl.lastUpdateBodyColorsTime = 0;
		%cl.applyBodyColors();
		%cl.graniteOverride = 0;
		return;
	}
	%lerp = %cl.diseaseInfo.stage[%this.getID()] / 20;
	%accent = %cl.accentColor;
	%cl.accentColor = %this.grayify(%accent, %lerp);
	%chest = %cl.chestColor;
	%cl.chestColor = %this.grayify(%chest, %lerp);
	%hat = %cl.hatColor;
	%cl.hatColor = %this.grayify(%hat, %lerp);
	%head = %cl.headColor;
	%cl.headColor = %this.grayify(%head, %lerp);
	%hip = %cl.hipColor;
	%cl.hipColor = %this.grayify(%hip, %lerp);
	%larm = %cl.larmColor;
	%cl.larmColor = %this.grayify(%larm, %lerp);
	%lhand = %cl.lhandColor;
	%cl.lhandColor = %this.grayify(%lhand, %lerp);
	%lleg = %cl.llegColor;
	%cl.llegColor = %this.grayify(%lleg, %lerp);
	%pack = %cl.packColor;
	%cl.packColor = %this.grayify(%pack, %lerp);
	%rarm = %cl.rarmColor;
	%cl.rarmColor = %this.grayify(%rarm, %lerp);
	%rhand = %cl.rhandColor;
	%cl.rhandColor = %this.grayify(%rhand, %lerp);
	%rleg = %cl.rlegColor;
	%cl.rlegColor = %this.grayify(%rleg, %lerp);
	%secondPack = %cl.secondPackColor;
	%cl.secondPackColor = %this.grayify(%secondPack, %lerp);
	%cl.lastUpdateBodyColorsTime = 0;
	%cl.applyBodyColors();
	%cl.accentColor = %accent;
	%cl.chestColor = %chest;
	%cl.hatColor = %hat;
	%cl.headColor = %head;
	%cl.hipColor = %hip;
	%cl.larmColor = %larm;
	%cl.lhandColor = %lhand;
	%cl.llegColor = %lleg;
	%cl.packColor = %pack;
	%cl.rarmColor = %rarm;
	%cl.rhandColor = %rhand;
	%cl.rlegColor = %rleg;
	%cl.secondPackColor = %secondPack;
	%cl.graniteOverride = 0;
}
function Disease_Granite::grayify(%this, %color, %lerp)
{
	return vectorLerp(%color, %this.getGrayColor(%color), %lerp) SPC getWord(%color, 3);
}
function Disease_Granite::getGrayColor(%this, %color)
{
	%value = getWord(%color, 0) * 0.2126 + getWord(%color, 1) * 0.7152 + getWord(%color, 2) * 0.0722;
	return %value SPC %value SPC %value SPC getWord(%color, 3);
}
addDisease(Disease_Granite);