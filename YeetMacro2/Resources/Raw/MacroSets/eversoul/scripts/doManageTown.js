// @position=12
const loopPatterns = [patterns.lobby.level, patterns.town.manageTown.title, patterns.town.manageTown.bond.outingsAvailable, patterns.town.manageTown.huntingGroundManagement.likeIcon];
const daily = dailyManager.GetCurrentDaily();

if (daily.doManageTown.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}
const targetBondLevelIsEnabled = settings.doManageTown.bond.targetBondLevel.IsEnabled;
const targetBondLevel = settings.doManageTown.bond.targetBondLevel.Value;
const targetSoul = macroService.ClonePattern(settings.doManageTown.bond.targetSoul.Value, {
	X: 275,
	Y: 80,
	Width: 1005,
	Height: 900,
	Path: 'settings.outings.target',
	OffsetType: 'DockLeft',
	BoundsCalcType: 'FillWidth'
});

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doManageTown: click town');
			macroService.PollPattern(patterns.lobby.town, { DoClick: true, PredicatePattern: patterns.town.enter });
			sleep(500);
			macroService.PollPattern(patterns.town.manageTown, { DoClick: true, PredicatePattern: patterns.town.manageTown.title });
			break;
		case 'town.manageTown.title':
			logger.info('doManageTown: click info with Offset');
			if (!daily.doManageTown.bond.IsChecked) {
				macroService.ClickPattern(patterns.town.manageTown.bond);
				continue;
			}
			if (!daily.doManageTown.huntingGroundManagement.done.IsChecked) {
				macroService.ClickPattern(patterns.town.manageTown.huntingGroundManagement);
				continue;
			}

			if (macroService.IsRunning) daily.doManageTown.done.IsChecked = true;
			return;
		case 'town.manageTown.bond.outingsAvailable':
			logger.info('doManageTown: do quick outings');
			sleep(500);
			let soulSwipeResult = macroService.PollPattern(targetSoul, { SwipePattern: patterns.town.outings.swipe, TimeoutMs: 15_000 });
			if (!soulSwipeResult.IsSuccess) {
				return 'Unable to find target soul';
			}

			macroService.ClickPattern(targetSoul);
			sleep(500);
			macroService.ClickPattern(targetSoul);

			if (targetBondLevelIsEnabled) {
				sleep(1_000);
				const currentBondLevel = Number.parseInt(macroService.FindText(patterns.town.manageTown.bond.bondLevel)?.replace(/[ ]/g, ''));
				if (currentBondLevel >= Number(targetBondLevel)) {
					throw new Error(`Target bond level reached: ${targetBondLevel}`);
				}
			}

			for (let i = 0; i < 3; i++) {
				macroService.PollPattern(patterns.town.manageTown.bond.quickOuting, { DoClick: true, PredicatePattern: patterns.general.tapTheScreen });
				macroService.PollPattern(patterns.general.tapTheScreen, { DoClick: true, PredicatePattern: [patterns.town.manageTown.bond.outingsAvailable, patterns.town.manageTown.bond.outingsComplete] });
			}

			macroService.PollPattern(patterns.town.manageTown.title, { ClickPattern: patterns.general.back, ClickPredicatePattern: patterns.town.manageTown.bond.outingsAvailable });
			if (macroService.IsRunning) daily.doManageTown.bond.IsChecked = true;
			break;
		case 'town.manageTown.huntingGroundManagement.likeIcon':
			if (!daily.doManageTown.huntingGroundManagement.manageHuntingGround.IsChecked) {
				logger.info('doManageTown: manage hunting ground');
				macroService.PollPattern(patterns.town.manageTown.huntingGroundManagement.manageHuntingGround, { DoClick: true, PredicatePattern: patterns.town.manageTown.sweep });
				macroService.PollPattern(patterns.town.manageTown.sweep, { DoClick: true, PredicatePattern: patterns.town.manageTown.confirm });
				macroService.PollPattern(patterns.town.manageTown.confirm, { DoClick: true, PredicatePattern: patterns.town.manageTown.huntingGroundManagement.likeIcon });
				if (macroService.IsRunning) daily.doManageTown.huntingGroundManagement.manageHuntingGround.IsChecked = true;
			}

			if (!daily.doManageTown.huntingGroundManagement.friends.IsChecked) {
				logger.info('doManageTown: send all likes to friends');
				macroService.PollPattern(patterns.town.manageTown.huntingGroundManagement.friends, { DoClick: true, PredicatePattern: patterns.town.manageTown.huntingGroundManagement.friends.selected });
				macroService.ClickPattern(patterns.town.manageTown.huntingGroundManagement.friends.sendAllLikes);
				if (macroService.IsRunning) daily.doManageTown.huntingGroundManagement.friends.IsChecked = true;
			}

			if (!daily.doManageTown.huntingGroundManagement.guild.IsChecked) {
				logger.info('doManageTown: send all likes to guild');
				macroService.PollPattern(patterns.town.manageTown.huntingGroundManagement.guild, { DoClick: true, PredicatePattern: patterns.town.manageTown.huntingGroundManagement.guild.selected });
				macroService.ClickPattern(patterns.town.manageTown.huntingGroundManagement.friends.sendAllLikes);
				if (macroService.IsRunning) daily.doManageTown.huntingGroundManagement.guild.IsChecked = true;
			}

			if (!daily.doManageTown.huntingGroundManagement.expeditions.IsChecked) {
				logger.info('doManageTown: do expeditions');

				macroService.PollPattern(patterns.town.manageTown.huntingGroundManagement.helpReceivedYesterday, { DoClick: true, PredicatePattern: patterns.town.manageTown.huntingGroundManagement.helpReceivedYesterday.selected });
				doExpeditionSweeps();

				if (!macroService.FindPattern(patterns.town.manageTown.huntingGroundManagement.expedition.done).IsSuccess) {
					macroService.PollPattern(patterns.town.manageTown.huntingGroundManagement.like, { DoClick: true, PredicatePattern: patterns.town.manageTown.huntingGroundManagement.like.selected });
					doExpeditionSweeps();
				}

				if (!macroService.FindPattern(patterns.town.manageTown.huntingGroundManagement.expedition.done).IsSuccess) {
					macroService.PollPattern(patterns.town.manageTown.huntingGroundManagement.helpReceivedYesterday, { DoClick: true, PredicatePattern: patterns.town.manageTown.huntingGroundManagement.helpReceivedYesterday.selected });
					doExpeditionSweeps2();
				}

				if (!macroService.FindPattern(patterns.town.manageTown.huntingGroundManagement.expedition.done).IsSuccess) {
					macroService.PollPattern(patterns.town.manageTown.huntingGroundManagement.like, { DoClick: true, PredicatePattern: patterns.town.manageTown.huntingGroundManagement.like.selected });
					doExpeditionSweeps2();
				}

				if (macroService.IsRunning) daily.doManageTown.huntingGroundManagement.expeditions.IsChecked = true;
			}

			daily.doManageTown.huntingGroundManagement.done.IsChecked =
				daily.doManageTown.huntingGroundManagement.manageHuntingGround.IsChecked &&
				daily.doManageTown.huntingGroundManagement.friends.IsChecked &&
				daily.doManageTown.huntingGroundManagement.guild.IsChecked &&
				daily.doManageTown.huntingGroundManagement.expeditions.IsChecked;
			return;
	}

	sleep(1_000);
}

function doExpeditionSweeps() {
	const recievedHelpResult = macroService.FindPattern(patterns.town.manageTown.huntingGroundManagement.expedition.receivedHelp, { Limit: 10 });
	if (!recievedHelpResult.IsSuccess) return;

	for (let recievedHelp of recievedHelpResult.Points) {
		if (macroService.FindPattern(patterns.town.manageTown.huntingGroundManagement.expedition.done).IsSuccess) return;

		const sweep0Pattern = macroService.ClonePattern(patterns.town.manageTown.huntingGroundManagement.expedition.sweep0, { X: 1706, CenterY: recievedHelp.Y + 63, Width: 60, Height: 50, Padding: 10, PathSuffix: `_${recievedHelp.Y}y` });
		const sweepResult = macroService.PollPattern(sweep0Pattern, { DoClick: true, PredicatePattern: [patterns.town.manageTown.huntingGroundManagement.expedition.sweep, patterns.town.manageTown.huntingGroundManagement.expedition.noSweep] });
		if (sweepResult.PredicatePath === 'town.manageTown.huntingGroundManagement.expedition.sweep') {
			macroService.PollPattern(patterns.town.manageTown.huntingGroundManagement.expedition.sweep, { DoClick: true, PredicatePattern: patterns.town.manageTown.huntingGroundManagement.expedition.sweep.confirm });
			macroService.PollPattern(patterns.town.manageTown.huntingGroundManagement.expedition.sweep.confirm, { DoClick: true, PredicatePattern: patterns.town.manageTown.huntingGroundManagement.likeIcon });
		} else {
			macroService.PollPattern(patterns.town.manageTown.huntingGroundManagement.expedition.cancel, { DoClick: true, PredicatePattern: patterns.town.manageTown.huntingGroundManagement.likeIcon });
		}
	}
}

function doExpeditionSweeps2() {
	const visitResult = macroService.FindPattern(patterns.town.manageTown.huntingGroundManagement.expedition.sweep0, { Limit: 10 });
	const helpReceivedArr = visitResult.Points
		.map(point => macroService.ClonePattern(patterns.town.manageTown.huntingGroundManagement.expedition.numHelpReceived, { CenterY: point.Y, PathSuffix: `_${point.Y}y` }))
		.map(pattern => ({ numHelpReceived: macroService.FindText(pattern), centerY: pattern.Pattern.RawBounds.Center.Y }));

	helpReceivedArr.sort((a, b) => a.numHelpReceived - b.numHelpReceived); // prioritize least helped

	for (const helpReceived of helpReceivedArr) {
		if (macroService.FindPattern(patterns.town.manageTown.huntingGroundManagement.expedition.done).IsSuccess) break;

		const sweep0Pattern = macroService.ClonePattern(patterns.town.manageTown.huntingGroundManagement.expedition.sweep0, { X: 1706, CenterY: helpReceived.centerY, Width: 60, Height: 50, Padding: 10, PathSuffix: `_${helpReceived.centerY}y` });
		const sweepResult = macroService.PollPattern(sweep0Pattern, { DoClick: true, PredicatePattern: [patterns.town.manageTown.huntingGroundManagement.expedition.sweep, patterns.town.manageTown.huntingGroundManagement.expedition.noSweep] });
		if (sweepResult.PredicatePath === 'town.manageTown.huntingGroundManagement.expedition.sweep') {
			macroService.PollPattern(patterns.town.manageTown.huntingGroundManagement.expedition.sweep, { DoClick: true, PredicatePattern: patterns.town.manageTown.huntingGroundManagement.expedition.sweep.confirm });
			macroService.PollPattern(patterns.town.manageTown.huntingGroundManagement.expedition.sweep.confirm, { DoClick: true, PredicatePattern: patterns.town.manageTown.huntingGroundManagement.likeIcon });
		} else {
			macroService.PollPattern(patterns.town.manageTown.huntingGroundManagement.expedition.cancel, { DoClick: true, PredicatePattern: patterns.town.manageTown.huntingGroundManagement.likeIcon });
		}
	}
}