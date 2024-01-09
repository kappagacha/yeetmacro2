// Skip 10-12 main or event HARD quests
const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.events];
const daily = dailyManager.GetDaily();
if (daily.doMainOrEventHardQuests.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const result = macroService.PollPattern(loopPatterns);
	switch (result.Path) {
		case 'titles.home':
			logger.info('doMainOrEventHardQuests: click tab quest');
			macroService.ClickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			logger.info('doMainOrEventHardQuests: click quest events');
			macroService.ClickPattern(patterns.quest.events);
			break;
		case 'titles.events':
			let staminaCost = 120;
			const questResult = macroService.PollPattern([patterns.quest.events.quest, patterns.quest.events.main]);
			if (questResult.Path === 'quest.events.quest') {
				logger.info('doMainOrEventHardQuests: event special quest');
				macroService.PollPattern(patterns.quest.events.quest, { DoClick: true, PredicatePattern: patterns.titles.quest });
				macroService.PollPattern(patterns.quest.events.quest.special, { DoClick: true, PredicatePattern: patterns.quest.events.quest.special.selected });
			} else if (questResult.Path === 'quest.events.main') {
				logger.info('doMainOrEventHardQuests: main hard quest');
				staminaCost = 30;
				macroService.PollPattern(patterns.quest.events.main, { DoClick: true, PredicatePattern: patterns.titles.quest });
				macroService.PollPattern(patterns.quest.main, { DoClick: true, PredicatePattern: patterns.titles.mainQuests });
				macroService.PollPattern(patterns.quest.main.part);
				const partResult = macroService.FindPattern(patterns.quest.main.part, { Limit: 10 });
				const targetPartPoint = partResult.Points.reduce((max, current) => (current.X > max.X ? current : max));
				macroService.PollPoint(targetPartPoint, { DoClick: true, PredicatePattern: patterns.quest.main.hard });

				macroService.PollPattern(patterns.quest.main.hard, { DoClick: true, PredicatePattern: patterns.quest.main.hard.skull });

				macroService.PollPattern(patterns.quest.main.complete);
				const completeResult = macroService.FindPattern(patterns.quest.main.complete, { Limit: 10 });
				const targetCompletePoint = completeResult.Points.reduce((max, current) => (current.Y * 100 + current.X > max.Y * 100 + max.X ? current : max));
				macroService.PollPoint(targetCompletePoint, { DoClick: true, PredicatePattern: patterns.skipAll.skipQuest });
			}
			
			macroService.PollPattern(patterns.skipAll.skipQuest, { DoClick: true, PredicatePattern: patterns.skipAll.title });
			
			const currentStaminaCost = macroService.GetText(patterns.quest.events.quest.skipAll.currentStamina);
			if (currentStaminaCost < staminaCost) {
				macroService.PollPattern(patterns.quest.events.quest.skipAll.addStamina, { DoClick: true, PredicatePattern: patterns.stamina.prompt.recoverStamina });
				macroService.PollPattern(patterns.stamina.meat, { DoClick: true, PredicatePattern: patterns.stamina.prompt.recoverStamina2 });
				let targetStamina = macroService.GetText(patterns.stamina.target);
				while (macroService.IsRunning && targetStamina < staminaCost) {
					macroService.ClickPattern(patterns.stamina.plusOne);
					sleep(500);
					targetStamina = macroService.GetText(patterns.stamina.target);
				}
				macroService.PollPattern(patterns.stamina.prompt.recover, { DoClick: true, ClickPattern: patterns.stamina.prompt.ok, PredicatePattern: patterns.skipAll.title, IntervalDelayMs: 1_000 });
			}
			macroService.PollPattern(patterns.quest.events.quest.skipAll.ok, { DoClick: true, PredicatePattern: patterns.skipAll.skipComplete });
			macroService.PollPattern(patterns.skipAll.skipComplete, {
				DoClick: true,
				ClickPattern: [patterns.skipAll.prompt.ok, patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp],
				PredicatePattern: patterns.skipAll.title
			});

			if (macroService.IsRunning) {
				daily.doMainOrEventHardQuests.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}