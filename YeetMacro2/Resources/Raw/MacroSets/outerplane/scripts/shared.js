// @raw-script
// @position=1000
function selectTeam(teamSlot, returnCurrentCp) {
	if (!teamSlot || teamSlot === 'Current' || teamSlot < 1) return;

	let currentTeamSlot = getCurrentTeamSlot();
	while (currentTeamSlot?.trim() !== teamSlot) {
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
	logger.info(`currentTeamSlot: ${currentTeamSlot}`);
	while (!currentTeamSlot) {
		currentTeamSlot = macroService.GetText(selectedTeamSlotPattern);
		logger.info(`currentTeamSlot: ${currentTeamSlot}`);
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