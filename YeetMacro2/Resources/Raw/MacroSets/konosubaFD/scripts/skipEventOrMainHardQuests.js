// @position=12
// Skip event quests
const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.events, patterns.quest.events.globalMission];
const targetSkipLevel = settings.skipEventOrMainHardQuests.targetEventLevel.Value ?? 12;

while (macroService.IsRunning) {
	const result = macroService.PollPattern(loopPatterns, { ClickPattern: [patterns.general.next, patterns.general.close] });
	switch (result.Path) {
		case 'titles.home':
			logger.info('skipEventOrMainHardQuests: click tab quest');
			macroService.ClickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			logger.info('skipEventOrMainHardQuests: click quest events');
			macroService.ClickPattern(patterns.quest.events);
			break;
		case 'quest.events.globalMission':
			logger.info('skipEventOrMainHardQuests: global mission');
			macroService.ClickPattern(patterns.quest.events.globalMission.back);
			break;
		case 'titles.events':
			macroService.PollPattern(patterns.quest.events.quest, { DoClick: true, PredicatePattern: patterns.titles.quest });
			sleep(1_000);
			const questResult = macroService.PollPattern([patterns.quest.events.quest.special, patterns.quest.events.main]);
			if (questResult.Path === 'quest.events.quest.special') {
				logger.info('skipEventOrMainHardQuests: event special quest');

				macroService.PollPattern(patterns.quest.events.quest.normal[targetSkipLevel], { DoClick: true, PredicatePattern: patterns.titles.events });
				macroService.PollPattern(patterns.quest.events.quest.skipMax, { DoClick: true, PredicatePattern: patterns.battle.prepare.disabled });
				macroService.PollPattern(patterns.tickets.use, { DoClick: true, PredicatePattern: patterns.tickets.prompt.ok });
				macroService.PollPattern(patterns.tickets.prompt.ok, { DoClick: true, ClickPattern: [patterns.skipAll.skipComplete, patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], PredicatePattern: patterns.titles.events });
			} else {
				logger.info('skipEventOrMainHardQuests: main hard quest');
				macroService.PollPattern(patterns.quest.main, { DoClick: true, PredicatePattern: patterns.titles.mainQuests });
				macroService.PollPattern(patterns.quest.main.part);
				const partResult = macroService.FindPattern(patterns.quest.main.part, { Limit: 10 });
				const targetPartPoint = partResult.Points.reduce((max, current) => (current.X > max.X ? current : max));
				macroService.PollPoint(targetPartPoint, { DoClick: true, PredicatePattern: patterns.quest.main.hard });
				macroService.PollPattern(patterns.quest.main.hard, { DoClick: true, PredicatePattern: patterns.quest.main.hard.skull });
				macroService.PollPattern(patterns.quest.main.skip, { DoClick: true, PredicatePattern: patterns.quest.main.skipAllAtOnce });

				let numStamina = macroService.GetText(patterns.quest.main.skipAllAtOnce.numStamina);
				while (numStamina >= 10) {
					macroService.PollPattern(patterns.quest.main.skipAllAtOnce.swipeLeft, { DoClick: true, PredicatePattern: patterns.quest.main.skipAllAtOnce.ok, IntervalDelayMs: 3_000 });
					macroService.PollPattern(patterns.quest.main.skipAllAtOnce.ok, { DoClick: true, ClickPattern: [patterns.skipAll.skipComplete, patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], PredicatePattern: patterns.tickets.prompt.ok });
					macroService.PollPattern(patterns.tickets.prompt.ok, { DoClick: true, PredicatePattern: patterns.quest.main.skipAllAtOnce });
					sleep(1_000);
					numStamina = macroService.GetText(patterns.quest.main.skipAllAtOnce.numStamina);
				}
			}

			return;
	}

	sleep(1_000);
}