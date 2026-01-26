// @tags=favorites
// Sweep joint challenge
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.titles.jointChallenge];
const daily = dailyManager.GetCurrentDaily();
const jointChallengeLevel = settings.sweepJointChallenge.jointChallengeLevel.Value;

if (daily.sweepJointChallenge.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: patterns.adventure.doNotSeeFor3days });
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('sweepJointChallenge: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('sweepJointChallenge: click joint challenge');
			const jointChallengeResult = macroService.FindPattern([patterns.adventure.jointChallenge.locked, patterns.adventure.jointChallenge]);
			if (jointChallengeResult.Path === 'adventure.jointChallenge.locked') {
				if (macroService.IsRunning) daily.sweepJointChallenge.done.IsChecked = true;

				return;
			}

			macroService.ClickPattern(patterns.adventure.jointChallenge);
			sleep(500);
			break;
		case 'titles.jointChallenge':
			logger.info('sweepJointChallenge: claim rewards');
			let eventMissionNotification = macroService.PollPattern(patterns.jointChallenge.eventMission.notification, { TimeoutMs: 2_000 });
			if (eventMissionNotification.IsSuccess) {
				macroService.PollPattern(patterns.jointChallenge.eventMission.notification, { DoClick: true, PredicatePattern: patterns.event.move.close });

				let notificationResult = macroService.PollPattern(patterns.event.move.recieve, { TimeoutMs: 3_000 });
				while (notificationResult.IsSuccess) {
					macroService.PollPattern(patterns.event.move.recieve, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
					macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.event.move.close });
					notificationResult = macroService.PollPattern(patterns.event.move.recieve, { TimeoutMs: 3_000 });
				}

				macroService.PollPattern(patterns.event.move.close, { DoClick: true, PredicatePattern: patterns.titles.jointChallenge });
			}

			logger.info('sweepJointChallenge: sweep joint challenge');
			macroService.PollPattern(patterns.jointChallenge[jointChallengeLevel], { DoClick: true, PredicatePattern: patterns.battle.setup.auto });
			macroService.PollPattern(patterns.battle.setup.auto, { DoClick: true, PredicatePattern: patterns.battle.setup.sweep });
			macroService.PollPattern(patterns.battle.setup.sweep, { DoClick: true, PredicatePattern: patterns.battle.setup.sweep.ok });
			macroService.PollPattern(patterns.battle.setup.sweep.ok, { DoClick: true, InversePredicatePattern: patterns.battle.setup.sweep.ok });

			if (macroService.IsRunning) {
				daily.sweepJointChallenge.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}