// @position=8
const loopPatterns = [patterns.lobby.level, patterns.town.enter];
const daily = dailyManager.GetCurrentDaily();
const isLastRunWithinHour = (Date.now() - settings.doPartTimeJobAndRest.lastRun.Value.ToUnixTimeMilliseconds()) / 3_600_000 < 1;

if (isLastRunWithinHour && !settings.doPartTimeJobAndRest.forceRun.Value) {
	return 'Last run was within the hour. Use forceRun setting to override check';
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doPartTimeJobAndRest: click town tab');
			const notificationResult = macroService.PollPattern(patterns.lobby.town.notification, { TimeoutMs: 1_500 });
			if (notificationResult.IsSuccess) {
				macroService.ClickPattern(patterns.lobby.town);
				sleep(1_000);
			} else {	// nothing to do
				settings.doPartTimeJobAndRest.lastRun.Value = new Date().toISOString();
				return;
			}
			break;
		case 'town.enter':
			const partTimeJobNotification = macroService.PollPattern(patterns.town.partTimeJob.notification, { TimeoutMs: 2_000 });
			const soulRestNotification = macroService.PollPattern(patterns.town.rest.notification, { TimeoutMs: 2_000 });
			if (partTimeJobNotification.IsSuccess) {
				macroService.PollPattern(patterns.town.partTimeJob.notification, { DoClick: true, PredicatePattern: patterns.general.back });

				const recieveAllResult = macroService.FindPattern(patterns.town.partTimeJob.recieveAll);
				if (recieveAllResult.IsSuccess) {
					macroService.PollPattern(patterns.town.partTimeJob.recieveAll, { DoClick: true, PredicatePattern: patterns.town.partTimeJob.tapTheScreen });
					macroService.PollPattern(patterns.town.partTimeJob.tapTheScreen, { DoClick: true, PredicatePattern: patterns.town.partTimeJob.recieveAll.disabled });
				}

				const autoDeployResult = macroService.FindPattern(patterns.town.partTimeJob.autoDeploy);
				if (autoDeployResult.IsSuccess) {
					macroService.PollPattern(patterns.town.partTimeJob.autoDeploy, { DoClick: true, ClickPattern: patterns.town.partTimeJob.autoDeploy.confirm, PredicatePattern: patterns.town.partTimeJob.autoDeploy.disabled });
				}

				macroService.PollPattern(patterns.general.back, { DoClick: true, PredicatePattern: patterns.town.enter });
			}

			if (soulRestNotification.IsSuccess) {
				macroService.PollPattern(patterns.town.rest.notification, { DoClick: true, PredicatePattern: patterns.general.back });
				macroService.PollPattern(patterns.town.rest.restAll, { DoClick: true, ClickPattern: [patterns.town.rest.autoSelect, patterns.town.rest.restAll.confirm], PredicatePattern: patterns.town.rest.restAll.disabled });
				macroService.PollPattern(patterns.general.back, { DoClick: true, PredicatePattern: patterns.town.enter });
			}

			logger.info('doPartTimeJobAndRest: done');
			if (macroService.IsRunning) {
				daily.doPartTimeJobAndRest.count.Count++;
				settings.doPartTimeJobAndRest.lastRun.Value = new Date().toISOString();
			}
			return;
	}

	sleep(1_000);
}