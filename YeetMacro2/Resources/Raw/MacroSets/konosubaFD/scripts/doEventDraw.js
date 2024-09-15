// Draw Event
const loopPatterns = [patterns.titles.events, patterns.titles.eventDraw];

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'titles.events':
			logger.info('doEventDraw: click draw');
			macroService.ClickPattern(patterns.quest.events.draw);
			break;
		case 'titles.eventDraw':
			logger.info('doEventDraw: use x100');
			const usex100Result = macroService.FindPattern(patterns.quest.events.draw.usex100);
			if (!usex100Result.IsSuccess) {
				macroService.PollPattern(patterns.quest.events.draw.drawSettings, { DoClick: true, PredicatePattern: patterns.quest.events.draw.drawSettings.title });
				macroService.PollPattern(patterns.quest.events.draw.drawSettings.x100, { DoClick: true, ClickOffset: { X: -100 }, PredicatePattern: patterns.quest.events.draw.drawSettings.x100.checked });
				macroService.PollPattern(patterns.quest.events.draw.drawSettings.close, { DoClick: true, PredicatePattern: patterns.quest.events.draw.drawSettings });
			}

			macroService.PollPattern(patterns.quest.events.draw.usex100, { DoClick: true, ClickPattern: patterns.quest.events.draw.youGot, PredicatePattern: patterns.quest.events.draw.drawResult });
			const tryAgainResult = macroService.PollPattern(patterns.quest.events.draw.tryAgain, { DoClick: true, ClickPattern: patterns.quest.events.draw.youGot, PredicatePattern: [patterns.quest.events.draw.ok2, patterns.quest.events.draw.tryAgain.disabled] });
			if (tryAgainResult.P)
			macroService.PollPattern(patterns.quest.events.draw.ok2, { DoClick: true, ClickPattern: patterns.quest.events.draw.youGot, PredicatePattern: patterns.quest.events.draw.drawResult });
			macroService.PollPattern(patterns.quest.events.draw.ok, { DoClick: true, ClickPattern: patterns.quest.events.draw.ok2, PredicatePattern: patterns.quest.events.draw.usex100 });
			break;
	}

	sleep(1_000);
}