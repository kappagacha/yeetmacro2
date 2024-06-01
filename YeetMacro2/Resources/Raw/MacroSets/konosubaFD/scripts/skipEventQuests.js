// @position=12
// Skip event quests
const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.events, patterns.quest.events.globalMission];
const targetSkipLevel = settings.skipEventQuest.targetLevel.Value ?? 12;

while (macroService.IsRunning) {
	const result = macroService.PollPattern(loopPatterns);
	switch (result.Path) {
		case 'titles.home':
			logger.info('skipEventQuest: click tab quest');
			macroService.ClickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			logger.info('skipEventQuest: click quest events');
			macroService.ClickPattern(patterns.quest.events);
			break;
		case 'quest.events.globalMission':
			logger.info('skipEventQuest: global mission');
			macroService.ClickPattern(patterns.quest.events.globalMission.back);
			break;
		case 'titles.events':
			macroService.PollPattern(patterns.quest.events.quest, { DoClick: true, PredicatePattern: patterns.titles.quest });
			sleep(1_000);
			const questResult = macroService.PollPattern([patterns.quest.events.quest.special, patterns.quest.events.quest]);
			if (!questResult.Path === 'quest.events.quest.special') {
				logger.info('skipEventQuest: main hard quest');
				return;
			}

			logger.info('skipEventQuest: event special quest');

			macroService.PollPattern(patterns.quest.events.quest.normal[targetSkipLevel], { DoClick: true, PredicatePattern: patterns.titles.events });
			macroService.PollPattern(patterns.quest.events.quest.skipMax, { DoClick: true, PredicatePattern: patterns.battle.prepare.disabled });
			macroService.PollPattern(patterns.tickets.use, { DoClick: true, ClickPattern: [patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp, patterns.skipAll.skipComplete], PredicatePattern: patterns.tickets.prompt.ok });
			macroService.PollPattern(patterns.tickets.prompt.ok, { DoClick: true, PredicatePattern: patterns.titles.events });
			sleep(500);
			return;
	}

	sleep(1_000);
}