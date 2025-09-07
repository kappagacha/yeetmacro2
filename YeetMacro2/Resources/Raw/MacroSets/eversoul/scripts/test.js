const targetSoul = macroService.ClonePattern(settings.doManageTown.bond.targetSoul.Value, {
	X: 275,
	Y: 80,
	Width: 1005,
	Height: 900,
	Path: 'settings.doManageTown.bond.targetSoul',
	OffsetType: 'DockLeft',
	BoundsCalcType: 'FillWidth'
});

//return targetSoul;
macroService.PollPattern(targetSoul, { PredicatePattern: patterns.lobby.level });

