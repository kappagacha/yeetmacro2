// @position=7
// Send all likes. Do all expeditions. Looks at Help Received Yesterday then Like
// Prioritizes least helped
const loopPatterns = [patterns.lobby.level, patterns.town.level, patterns.titles.visitTown];
const daily = dailyManager.GetCurrentDaily();
if (daily.doExpeditions.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doExpeditions: click town');
			macroService.PollPattern(patterns.lobby.town, { DoClick: true, PredicatePattern: patterns.town.enter });
			sleep(500);
			macroService.PollPattern(patterns.town.enter, { DoClick: true, ClickPattern: patterns.general.tapTheScreen, PredicatePattern: patterns.town.level });
			break;
		case 'town.level':
			logger.info('doExpeditions: visit town');
			macroService.PollPattern(patterns.town.menu, { DoClick: true, PredicatePattern: patterns.town.menu.visitTown });
			macroService.PollPattern(patterns.town.menu.visitTown, { DoClick: true, PredicatePattern: patterns.titles.visitTown });
			break;
		case 'titles.visitTown':
			logger.info('doExpeditions: send likes');
			macroService.ClickPattern(patterns.visitTown.sendAllLikes);
			macroService.PollPattern(patterns.visitTown.guild, { DoClick: true, PredicatePattern: patterns.visitTown.guild.selected });
			macroService.ClickPattern(patterns.visitTown.sendAllLikes);

			logger.info('doExpeditions: do expeditions');
			macroService.PollPattern(patterns.visitTown.helpReceivedYesterday, { DoClick: true, PredicatePattern: patterns.visitTown.helpReceivedYesterday.selected });
			doExpeditionSweeps();

			if (!macroService.FindPattern(patterns.visitTown.done).IsSuccess) {
				macroService.PollPattern(patterns.visitTown.like, { DoClick: true, PredicatePattern: patterns.visitTown.like.selected });
				doExpeditionSweeps();
			}

			if (macroService.IsRunning) {
				daily.doExpeditions.done.IsChecked = true;
			}
			
			return;
	}

	sleep(1_000);
}

function doExpeditionSweeps() {
	const visitResult = macroService.FindPattern(patterns.visitTown.visit, { Limit: 10 });
	const helpReceivedArr = visitResult.Points
		.map(point => macroService.ClonePattern(patterns.visitTown.numHelpReceived, { CenterY: point.Y, Path: `visitTown.numHelpReceived_y${point.Y}` }))
		.map(pattern => ({ numHelpReceived: macroService.GetText(pattern), centerY: pattern.Pattern.Rect.Center.Y }));

	helpReceivedArr.sort((a, b) => a.numHelpReceived - b.numHelpReceived); // prioritize least helped

	for (const helpReceived of helpReceivedArr) {
		if (macroService.FindPattern(patterns.visitTown.done).IsSuccess) break;

		const visitPattern = macroService.ClonePattern(patterns.visitTown.visit, { X: 1730, CenterY: helpReceived.centerY, Width: 75, Height: 33, Padding: 5 });
		const sweepResult = macroService.PollPattern(visitPattern, { DoClick: true, PredicatePattern: [patterns.visitTown.sweep, patterns.visitTown.noSweep] });
		if (sweepResult.PredicatePath === 'visitTown.sweep') {
			macroService.PollPattern(patterns.visitTown.sweep, { DoClick: true, PredicatePattern: patterns.visitTown.sweep.confirm });
			macroService.PollPattern(patterns.visitTown.sweep.confirm, { DoClick: true, PredicatePattern: patterns.titles.visitTown });
		} else {
			macroService.PollPattern(patterns.visitTown.cancel, { DoClick: true, PredicatePattern: patterns.titles.visitTown });
		}
	}
}