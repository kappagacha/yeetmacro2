// Do all free quests. See options to select upgrade stone difficulty
const loopPatterns = [patterns.titles.home, patterns.titles.quest, patterns.titles.freeQuests];
const offset = macroService.CalcOffset(patterns.titles.home);
const daily = dailyManager.GetDaily();
if (daily.doFreeQuests.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'titles.home':
			logger.info('doFreeQuests: click tab quest');
			macroService.ClickPattern(patterns.tabs.quest);
			break;
		case 'titles.quest':
			logger.info('doFreeQuests: click quest free quests');
			macroService.ClickPattern(patterns.quest.freeQuests);
			break;
		case 'titles.freeQuests':
			logger.info('doFreeQuests: target upgrade stone');
			const upgradeStoneTargetLevel = settings.doFreeQuests.upgradeStone.targetLevel.Value || 'intermediate';
			if (upgradeStoneTargetLevel !== 'extreme') {
				macroService.PollPattern(patterns.freeQuests.upgradeStone, { DoClick: true, PredicatePattern: patterns.freeQuests.upgradeStone[upgradeStoneTargetLevel] });
				macroService.PollPattern(patterns.freeQuests.upgradeStone[upgradeStoneTargetLevel], { DoClick: true, PredicatePattern: patterns.tickets.add });
				// sample text capture: "25 x1" (it catches some of the words)
				let numTickets = (macroService.GetText(patterns.tickets.numTickets)).split('x')[1];
				while (macroService.IsRunning && numTickets < 2) {
					macroService.ClickPattern(patterns.tickets.add);
					sleep(500);
					numTickets = (macroService.GetText(patterns.tickets.numTickets)).split('x')[1];
				}
				macroService.PollPattern(patterns.tickets.use, { DoClick: true, ClickPattern: [patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], PredicatePattern: patterns.tickets.prompt.ok });
				macroService.PollPattern(patterns.tickets.prompt.ok, { DoClick: true, PredicatePattern: patterns.titles.freeQuests });
			}
			
			logger.info('doFreeQuests: skip all');
			macroService.PollPattern(patterns.freeQuests.eris, { DoClick: true, PredicatePattern: patterns.skipAll.skipQuest });
			macroService.PollPattern(patterns.skipAll.skipQuest, { DoClick: true, PredicatePattern: patterns.skipAll.button });
			const filterOffResult = macroService.FindPattern(patterns.skipAll.search.filter.off);
			if (filterOffResult.IsSuccess) {
				macroService.PollPattern(patterns.skipAll.search.filter.off, { DoClick: true, PredicatePattern: patterns.skipAll.search.filter });
				sleep(500);
				const checkResult = macroService.FindPattern(patterns.skipAll.search.filter.check, { Limit: 5 });
				for (let point of checkResult.Points) {
					logger.debug(JSON.stringify(point));
					if (point.X < offset.X + 300.0) continue;		// skip 4 stars
					macroService.DoClick(point);
					sleep(250);
				}
				macroService.PollPattern(patterns.skipAll.search.filter.close, { DoClick: true, PredicatePattern: patterns.skipAll.title });
				sleep(500);
			}

			let maxNumSkips = macroService.GetText(patterns.skipAll.maxNumSkips);
			while (macroService.IsRunning && maxNumSkips < 2) {
				macroService.ClickPattern(patterns.skipAll.addMaxSkips);
				sleep(500);
				maxNumSkips = macroService.GetText(patterns.skipAll.maxNumSkips);
			}
			macroService.PollPattern(patterns.skipAll.button, { DoClick: true, PredicatePattern: patterns.skipAll.prompt.ok });
			sleep(1_000);
			macroService.PollPattern(patterns.skipAll.prompt.ok, { DoClick: true, PredicatePattern: patterns.skipAll.skipComplete });
			macroService.PollPattern(patterns.skipAll.skipComplete, { DoClick: true, ClickPattern: [patterns.skipAll.prompt.ok, patterns.branchEvent.availableNow, patterns.branchEvent.playLater, patterns.prompt.playerRankUp], PredicatePattern: patterns.skipAll.title });

			if (macroService.IsRunning) {
				daily.doFreeQuests.done.IsChecked = true;
			}
			return;
	}

	sleep(1_000);
}