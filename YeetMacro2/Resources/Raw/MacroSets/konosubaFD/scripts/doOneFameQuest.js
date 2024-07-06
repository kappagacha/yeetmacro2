// @position=7
// Battles the first level fame quest once for daily task. Does not obtain bonuses
const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.fameQuest];
const daily = dailyManager.GetCurrentDaily();
if (daily.doOneFameQuest.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const result = macroService.PollPattern(loopPatterns);
	switch (result.Path) {
		case 'titles.home':
			logger.info('doOneFameQuest: click tab quest');
			macroService.ClickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			logger.info('doOneFameQuest: click fame quest');
			macroService.ClickPattern(patterns.quest.fame);
			break;
		case 'titles.fameQuest':
			logger.info('doOneFameQuest: click fame quest');
			macroService.PollPattern(patterns.quest.fame.leftArrow, { DoClick: true, PredicatePattern: patterns.quest.fame.rank1 });
			macroService.PollPattern(patterns.quest.fame.rank1, { DoClick: true, PredicatePattern: patterns.quest.fame.axel });
			macroService.PollPattern(patterns.quest.fame.axel, { DoClick: true, PredicatePattern: patterns.quest.fame.direBunny });
			const direBunnyResult = macroService.PollPattern(patterns.quest.fame.direBunny, { DoClick: true, PredicatePattern: [patterns.battle.prepare, patterns.battle.prepare.disabled] });

			if (direBunnyResult.PredicatePath === 'battle.prepare.disabled') {
				const staminaCost = 20;
				macroService.PollPattern(patterns.quest.fame.addStamina, { DoClick: true, PredicatePattern: patterns.stamina.prompt.recoverStamina });
				macroService.PollPattern(patterns.stamina.meat, { DoClick: true, PredicatePattern: patterns.stamina.prompt.recoverStamina2 });
				let targetStamina = macroService.GetText(patterns.stamina.target);
				while (macroService.IsRunning && targetStamina < staminaCost) {
					macroService.ClickPattern(patterns.stamina.plusOne);
					sleep(500);
					targetStamina = macroService.GetText(patterns.stamina.target);
				}
				macroService.PollPattern(patterns.stamina.prompt.recover, { DoClick: true, ClickPattern: patterns.stamina.prompt.ok, PredicatePattern: patterns.battle.prepare, IntervalDelayMs: 1_000 });
			}
			
			logger.info('doOneFameQuest: battle');
			//macroService.PollPattern(patterns.battle.prepare, { DoClick: true, ClickPattern: [patterns.battle.begin, patterns.quest.fame.doNotObtain, patterns.quest.fame.ok], PredicatePattern: patterns.battle.next });
			macroService.PollPattern(patterns.battle.prepare, { DoClick: true, PredicatePattern: patterns.battle.skip });
			macroService.PollPattern(patterns.battle.skip, { DoClick: true, PredicatePattern: patterns.battle.skip.skip });
			macroService.PollPattern(patterns.battle.skip.skip, { DoClick: true, PredicatePattern: patterns.battle.skip.ok });
			macroService.PollPattern(patterns.battle.skip.ok, { DoClick: true, ClickPattern: [patterns.quest.fame.doNotObtain, patterns.quest.fame.ok], PredicatePattern: patterns.battle.skip.ok2 });
			macroService.PollPattern(patterns.battle.skip.ok2, { DoClick: true, PredicatePattern: patterns.battle.skip.skip });

			if (macroService.IsRunning) {
				daily.doOneFameQuest.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}