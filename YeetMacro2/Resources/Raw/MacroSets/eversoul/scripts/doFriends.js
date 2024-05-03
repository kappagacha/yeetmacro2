// @position=6
// Claim and recieve hearts
// Hire 5 souls. Requires hireTarget pattern captures
const loopPatterns = [patterns.lobby.level, patterns.titles.friends];
const daily = dailyManager.GetCurrentDaily();
const resolution = macroService.GetCurrentResolution();
const hireTargetBounds = {
	X: 970,
	Y: 1,
	Width: 150,
	Height: 1060,
	OffsetCalcType: 'DockLeft'
};

const hireTarget1 = macroService.ClonePattern(settings.doFriends.hireTarget1.Value, { Path: 'settings.outings.hireTarget_1', ...hireTargetBounds });
const hireTarget2 = macroService.ClonePattern(settings.doFriends.hireTarget2.Value, { Path: 'settings.outings.hireTarget_2', ...hireTargetBounds });
const hireTarget3 = macroService.ClonePattern(settings.doFriends.hireTarget3.Value, { Path: 'settings.outings.hireTarget_3', ...hireTargetBounds });
const hireTarget4 = macroService.ClonePattern(settings.doFriends.hireTarget4.Value, { Path: 'settings.outings.hireTarget_4', ...hireTargetBounds });
const hireTarget5 = macroService.ClonePattern(settings.doFriends.hireTarget5.Value, { Path: 'settings.outings.hireTarget_5', ...hireTargetBounds });

const isLastRunWithinHour = (Date.now() - settings.doFriends.lastRun.Value.ToUnixTimeMilliseconds()) / 3_600_000 < 1;

if (isLastRunWithinHour && !settings.doFriends.forceRun.Value) {
	return 'Last run was within the hour. Use forceRun setting to override check';
}

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns);
	switch (loopResult.Path) {
		case 'lobby.level':
			logger.info('doFriends: click menu');
			macroService.PollPattern(patterns.lobby.menu, { DoClick: true, PredicatePattern: patterns.titles.menu });
			macroService.PollPattern(patterns.menu.friends, { DoClick: true, PredicatePattern: patterns.titles.friends });
			break;
		case 'titles.friends':
			logger.info('doFriends: send and receive hearts');
			macroService.PollPattern(patterns.friends.sendAllHearts, { DoClick: true, PredicatePattern: patterns.friends.sendAllHearts.disabled });
			macroService.PollPattern(patterns.friends.receiveAllHearts, { DoClick: true, PredicatePattern: patterns.friends.receiveAllHearts.disabled });

			if (settings.doFriends.isHiring.Value && !daily.doFriends.isHiring.done.IsChecked) {
				macroService.PollPattern(patterns.friends.hireSoul, { DoClick: true, PredicatePattern: patterns.friends.hireSoul.selected });

				let swipeCount = 0;
				let hireSoulDoneResult = macroService.FindPattern(patterns.friends.hireSoul.done);
				//let hireSoulDoneResult = { IsSuccess: false };
				let hireTargets = [hireTarget1, hireTarget2, hireTarget3, hireTarget4, hireTarget5];

				while (macroService.IsRunning && !hireSoulDoneResult.IsSuccess && swipeCount < 20) {
					const swipeResult = macroService.SwipePollPattern(hireTargets, { MaxSwipes: 1, Start: { X: 1500, Y: 700 }, End: { X: 1500, Y: 150 } });
					if (swipeResult.IsSuccess) {
						const hirePattern = macroService.ClonePattern(patterns.friends.hireSoul.hire, { CenterY: swipeResult.Point.Y, Padding: 25, Path: `friends.hireSoul.hire_y${swipeResult.Point.Y}` });
						const hirePatternResult = macroService.FindPattern(hirePattern);
						if (hirePatternResult.IsSuccess) {
							macroService.DoClick(hirePatternResult.Point);
							sleep(200);
							macroService.DoClick(hirePatternResult.Point);
							hireTargets = hireTargets.filter(ht => ht.Path !== swipeResult.Path);
							logger.info(swipeResult.Path + '___' + hireTargets.length);
							sleep(1_500);
						}
					}
					hireSoulDoneResult = macroService.FindPattern(patterns.friends.hireSoul.done);
					swipeCount++;
				}

				if (!hireSoulDoneResult.IsSuccess) {
					throw Error('Unable to find target souls.')
				}
			}
			
			logger.info('doFriends: done');
			if (macroService.IsRunning) {
				daily.doFriends.count.Count++;
				settings.doFriends.lastRun.Value = new Date().toISOString();
			}

			return;
	}

	sleep(1_000);
}