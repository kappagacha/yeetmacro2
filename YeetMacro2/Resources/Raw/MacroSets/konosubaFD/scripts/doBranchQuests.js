// Do all branch quests. See options for cabbage hunt party name
const loopPatterns = [patterns.titles.home, patterns.titles.branchEvent, patterns.titles.branch, patterns.branchEvent.explosionWalk.chant.disabled, patterns.titles.party];
const daily = dailyManager.GetDaily();
if (daily.doBranchQuests.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}
let evnt;

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.branchEvent.rewardGained });
	switch (loopResult.Path) {
		case 'titles.home':
			logger.info('doBranchQuests: check others notification');
			const othersNotificationResult = macroService.PollPattern(patterns.others.notification, { TimoutMs: 2_000 });
			if (othersNotificationResult.IsSuccess) {
				macroService.PollPattern(patterns.others.notification, { DoClick: true, ClickPattern: patterns.others.branch.notification, PredicatePattern: patterns.titles.branch });
			} else {
				if (macroService.IsRunning) {
					daily.doBranchQuests.done.IsChecked = true;
				}
				return;
			}
			break;
		case 'titles.branch':
			logger.info('doBranchQuests: pick quest');
			const eventResult = macroService.PollPattern([patterns.branchEvent.cabbageHunting, patterns.branchEvent.explosionWalk, patterns.branchEvent.pitAPatBox, patterns.branchEvent.swimsuit], { DoClick: true, PredicatePattern: patterns.titles.branchEvent });
			evnt = eventResult.Path;
			logger.debug('event: ' + evnt);
			break;
		case 'titles.branchEvent':
			logger.info('doBranchQuests: start');
			macroService.PollPattern([patterns.branchEvent.prepare, patterns.branchEvent.start], { DoClick: true, ClickPattern: patterns.branchEvent.noVoices, PredicatePattern: [patterns.branchEvent.explosionWalk.chant.disabled, patterns.titles.party, patterns.branchEvent.skip2] });
			sleep(1000);
			
			if (evnt === 'branchEvent.pitAPatBox') {
				const boxes = ['simpleWoodenBox', 'veryHeavyBox', 'woodenBox', 'clothPouch'].map(option => patterns.branchEvent.pitAPatBox[option]);
				macroService.PollPattern(patterns.branchEvent.skip2, { DoClick: true, PredicatePattern: boxes });
				macroService.PollPattern(boxes, { DoClick: true, ClickPattern: [patterns.branchEvent.rewardGained, patterns.branchEvent.noVoices, patterns.branchEvent.skip2, patterns.branchEvent.prompt.ok], PredicatePattern: patterns.titles.home });
			} else if (evnt === 'branchEvent.swimsuit') {
				//const options = [patterns.branchEvent.swimsuit.megumin, patterns.branchEvent.swimsuit.wiz, patterns.branchEvent.swimsuit.vanir, patterns.branchEvent.swimsuit.arue];
				//macroService.PollPattern(patterns.branchEvent.skip2, { DoClick: true, PredicatePattern: options });
				//macroService.PollPattern(boxes, { DoClick: true, ClickPattern: [patterns.branchEvent.rewardGained, patterns.branchEvent.noVoices, patterns.branchEvent.skip2, patterns.branchEvent.prompt.ok], PredicatePattern: patterns.titles.home });
			}
			break;
		case 'branchEvent.explosionWalk.chant.disabled':
			logger.info('doBranchQuests: explosion walk');
			const optionNumber = 1 + Math.floor(Math.random() * 4);
			logger.debug('option ' + optionNumber);
			macroService.PollPattern(patterns.branchEvent.explosionWalk['option' + optionNumber], { DoClick: true, PredicatePattern: patterns.branchEvent.explosionWalk.chant.enabled });
			macroService.PollPattern(patterns.branchEvent.explosionWalk.chant.enabled, { DoClick: true, ClickPattern: [patterns.battle.next, patterns.branchEvent.skip], PredicatePattern: patterns.titles.home });
			break;
		case 'titles.party':
			logger.info('doBranchQuests: select party');
			const targetPartyName = settings.doBranchQuests.cabbageHunting.party.Value;
			logger.debug(`targetPartyName: ${targetPartyName}`);
			selectParty(targetPartyName);
			sleep(500);
			macroService.PollPattern(patterns.battle.begin, { DoClick: true, ClickPattern: [patterns.battleArena.newHighScore, patterns.battleArena.rank], PredicatePattern: patterns.battle.report });
			macroService.PollPattern(patterns.battle.next, { DoClick: true, ClickPattern: [patterns.battleArena.newHighScore, patterns.battleArena.rank], PredicatePattern: patterns.titles.home });
			break;
	}

	sleep(1_000);
}