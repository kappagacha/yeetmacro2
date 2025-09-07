const targetSoul = macroService.ClonePattern(settings.doManageTown.bond.targetSoul.Value, {
	X: 275,
	Y: 80,
	Width: 1005,
	Height: 900,
	Resolution: { Width: 1920, Height: 1080 },
	Path: 'test.settings.doManageTown.bond.targetSoul',
	OffsetType: 'DockLeft',
	BoundsCalcType: 'FillWidth'
});

//return targetSoul;
macroService.PollPattern(targetSoul, { PredicatePattern: patterns.lobby.level });

