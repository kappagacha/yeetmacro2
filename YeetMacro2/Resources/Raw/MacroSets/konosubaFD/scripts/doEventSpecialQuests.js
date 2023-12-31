// Skip event special quests
const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.events, patterns.battle.report];
const daily = dailyManager.GetDaily();
if (daily.doEventSpecialQuests.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const result = macroService.PollPattern(loopPatterns);
	switch (result.Path) {
		case 'titles.home':
			logger.info('doEventSpecialQuests: click tab quest');
			macroService.ClickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			logger.info('doEventSpecialQuests: click quest events');
			macroService.ClickPattern(patterns.quest.events);
			break;
		case 'titles.events':
			logger.info('doEventSpecialQuests: click quest');
			macroService.PollPattern(patterns.quest.events.quest, { DoClick: true, PredicatePattern: patterns.titles.quest });
			macroService.PollPattern(patterns.quest.events.quest.special, { DoClick: true, PredicatePattern: patterns.quest.events.quest.special.selected });
			macroService.PollPattern(patterns.skipAll.skipQuest, { DoClick: true, PredicatePattern: patterns.skipAll.title });
			const staminaCost = 120;
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
				daily.doEventSpecialQuests.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}