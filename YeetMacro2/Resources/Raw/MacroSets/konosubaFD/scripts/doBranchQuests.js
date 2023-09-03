let done = false;
const loopPatterns = [patterns.titles.home, patterns.titles.branchEvent, patterns.titles.branch, patterns.branchEvent.explosionWalk.chant.disabled, patterns.titles.party];
let event;

while (macroService.IsRunning && !done) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'titles.home':
			logger.info('doBranchQuests: check others notification');
			const othersNotificationResult = macroService.FindPattern(patterns.others.notification);
			if (othersNotificationResult.IsSuccess) {
				macroService.PollPattern(patterns.others.notification, { DoClick: true, PredicatePattern: patterns.titles.branch });
				//macroService.PollPattern(patterns.others.branch.notification, { DoClick: true, PredicatePattern: patterns.titles.branch });
			}
			else {
				done = true;
			}
			break;
		case 'titles.branch':
			logger.info('doBranchQuests: pick quest');
			const eventResult = macroService.PollPattern([patterns.branchEvent.cabbageHunting, patterns.branchEvent.explosionWalk, patterns.branchEvent.pitAPatBox], { DoClick: true, PredicatePattern: patterns.titles.branchEvent });
			event = eventResult.Path;
			logger.debug('event: ' + event);
			result = event;
			break;
		case 'titles.branchEvent':
			logger.info('doBranchQuests: start');
			macroService.PollPattern([patterns.branchEvent.prepare, patterns.branchEvent.start], { DoClick: true, ClickPattern: patterns.branchEvent.pitAPatBox.noVoices, PredicatePattern: [patterns.branchEvent.explosionWalk.chant.disabled, patterns.titles.party, patterns.branchEvent.pitAPatBox.skip] });
			sleep(1000);
			
			if (event === 'branchEvent.pitAPatBox') {
				const boxes = ['simpleWoodenBox', 'veryHeavyBox', 'woodenBox', 'clothPouch'].map(option => patterns.branchEvent.pitAPatBox[option]);
				macroService.PollPattern(patterns.branchEvent.pitAPatBox.skip, { DoClick: true, PredicatePattern: boxes });
				macroService.PollPattern(boxes, { DoClick: true, ClickPattern: [patterns.branchEvent.pitAPatBox.rewardGained, patterns.branchEvent.pitAPatBox.noVoices, patterns.branchEvent.pitAPatBox.skip, patterns.branchEvent.prompt.ok], PredicatePattern: patterns.titles.home });
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
			const targetPartyName = settings.party.cabbageHunting.props.value;
			logger.debug(`targetPartyName: ${targetPartyName}`);
			if (targetPartyName === 'recommendedElement') {
				selectPartyByRecommendedElement(isBossMulti ? -425 : 0);	// Recommended Element icons are shifted by 425 to the left of expected location
			}
			else {
				if (!(selectParty(targetPartyName))) {
					result = `targetPartyName not found: ${targetPartyName}`;
					done = true;
					break;
				}
			}
			sleep(500);
			macroService.PollPattern(patterns.battle.begin, { DoClick: true, PredicatePattern: patterns.battle.report });
			macroService.PollPattern(patterns.battle.next, { DoClick: true, ClickPattern: [patterns.battleArena.newHighScore, patterns.battleArena.rank], PredicatePattern: patterns.titles.home });
			break;
	}

	sleep(1_000);
}
logger.info('Done...');