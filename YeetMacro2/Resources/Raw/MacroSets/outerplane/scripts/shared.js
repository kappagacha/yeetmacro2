// @raw-script
// @position=1000
function selectTeam(teamSlot, returnCurrentCp) {
	if (!teamSlot || teamSlot === 'Current' || teamSlot < 1) return;

	const topLeft = macroService.GetTopLeft();
	const xLocation = topLeft.X + 90;
	let currentTeamSlot = getCurrentTeamSlot();
	while (currentTeamSlot?.trim() !== teamSlot) {
		if (currentTeamSlot > teamSlot) {
			macroService.DoSwipe({ X: xLocation, Y: 200 }, { X: xLocation, Y: 400 });
			sleep(1_000);
		}

		const teamSlotResult = findTeamSlot(teamSlot);
		if (teamSlotResult) {
			macroService.DoClick(teamSlotResult);
			sleep(1_500);
		}
		currentTeamSlot = getCurrentTeamSlot();
	}
	
	if (macroService.IsRunning && returnCurrentCp) {
		const cpText = macroService.GetText(patterns.battle.cp);
		return Number(cpText.slice(0, -4).slice(1) + cpText.slice(-3));
	}
}

function findTeamSlot(teamSlot) {
	const teamSlotCornerResults = macroService.FindPattern(patterns.battle.teamSlotCorner, { Limit: 10 });
	const teams = teamSlotCornerResults.Points.map(p => {
		const teamSlotPattern = macroService.ClonePattern(patterns.battle.teamSlot, { Y: p.Y + 7 });
		return {
			point : p,
			slot: macroService.GetText(teamSlotPattern)
		};
	});
	return teams.find(t => t.slot === teamSlot)?.point;
}

function getCurrentTeamSlot() {
	const selectedTeamSlotResult = macroService.PollPattern(patterns.battle.teamSlotCorner.selected);
	const selectedTeamSlotPattern = macroService.ClonePattern(patterns.battle.teamSlot, { Y: selectedTeamSlotResult.Point.Y + 7 });
	let currentTeamSlot = macroService.GetText(selectedTeamSlotPattern)
	//logger.info(`currentTeamSlot: ${currentTeamSlot}`);
	while (!currentTeamSlot) {
		currentTeamSlot = macroService.GetText(selectedTeamSlotPattern);
		//logger.info(`currentTeamSlot: ${currentTeamSlot}`);
		sleep(100);
	}
	return currentTeamSlot;
}

function selectTeamAndBattle(teamSlot, sweepBattle) {
	selectTeam(teamSlot);
	macroService.PollPattern(patterns.battle.setup.auto, { DoClick: true, PredicatePattern: patterns.battle.setup.repeatBattle });
	if (sweepBattle) {
		macroService.PollPattern(patterns.battle.setup.sweep, { DoClick: true, PredicatePattern: patterns.battle.setup.sweep.ok });
		macroService.PollPattern(patterns.battle.setup.sweep.ok, { DoClick: true, InversePredicatePattern: patterns.battle.setup.sweep.ok });
	} else {
		macroService.PollPattern(patterns.battle.setup.enter, { DoClick: true, PredicatePattern: patterns.battle.setup.enter.ok });
	}
}

function refillStamina(targetStamina) {
	goToLobby();
	let currentStamina = macroService.GetText(patterns.lobby.staminaValue);
	logger.info(`refillStamina to ${targetStamina}. current stamina is ${currentStamina}`);
	if (currentStamina >= targetStamina) {
		return;
	}

	macroService.PollPattern(patterns.lobby.mailbox, { DoClick: true, PredicatePattern: patterns.titles.mailbox });
	macroService.PollPattern(patterns.mailbox.product, { DoClick: true, PredicatePattern: patterns.mailbox.product.selected });
	sleep(1000);

	const targetMailboxItem = patterns.mailbox.stamina;
	while (macroService.IsRunning && currentStamina < targetStamina) {
		macroService.SwipePollPattern(targetMailboxItem, { MaxSwipes: 20, Start: { X: 1400, Y: 900 }, End: { X: 1400, Y: 150 } });
		const staminaResult = macroService.FindPattern(targetMailboxItem);
		const recievePattern = macroService.ClonePattern(patterns.mailbox.receive, { CenterY: staminaResult.Point.Y, Height: 60.0 });
		macroService.PollPattern(recievePattern, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
		macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.titles.mailbox });
		sleep(500);
		currentStamina = macroService.GetText(patterns.mailbox.staminaValue);
		logger.info(`refillStamina to ${targetStamina}. current stamina is ${currentStamina}`);
	}
}