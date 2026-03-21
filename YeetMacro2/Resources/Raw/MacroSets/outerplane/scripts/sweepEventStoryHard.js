// @tags=event
// Sweep event story hard
const loopPatterns = [patterns.lobby.level, patterns.titles.adventure, patterns.event.story.enter];
const daily = dailyManager.GetCurrentDaily();
const resolution = macroService.GetCurrentResolution();
const teamSlot = settings.sweepEventStoryHard.teamSlot.Value;

if (daily.sweepEventStoryHard.done.IsChecked) {
	return "Script already completed. Uncheck done to override daily flag.";
}

const targetEventPattern = macroService.ClonePattern(settings.sweepEventStoryHard.targetEventPattern.Value, {
	X: 80,
	Y: 215,
	Width: resolution.Width - 100,
	Height: 785,
	Path: 'settings.sweepEventStoryHard.targetEventPattern',
	OffsetCalcType: 'DockLeft'
});

const targetPartPattern = macroService.ClonePattern(settings.sweepEventStoryHard.targetPartPattern.Value, {
	X: 80,
	Y: 215,
	Width: resolution.Width - 100,
	Height: 785,
	Path: 'settings.sweepEventStoryHard.targetPartPattern',
	OffsetCalcType: 'DockLeft'
});

while (macroService.IsRunning) {
	const loopResult = macroService.PollPattern(loopPatterns, { ClickPattern: [targetEventPattern, patterns.adventure.doNotSeeFor3days] });
	switch (loopResult.Path) {
		case 'lobby.level':
			refillStamina(80);
			logger.info('sweepEventStoryHard: click adventure tab');
			macroService.ClickPattern(patterns.tabs.adventure);
			sleep(500);
			break;
		case 'titles.adventure':
			logger.info('sweepEventStoryHard: click event');
			macroService.ClickPattern(patterns.adventure.event);
			sleep(500);
			break;
		case 'event.story.enter':
			logger.info('sweepEventStoryHard: claim rewards');
			const done = handleEventShop();
			macroService.PollPattern(patterns.general.back, { DoClick: true, PrimaryClickPredicatePattern: patterns.titles.adventurerShop, PredicatePattern: patterns.event.story.enter });

			handleRewards();

			if (done) {
				settings.dailies.sweepEventStoryHard2.Value = false;
				return;
			}

			logger.info('sweepEventStoryHard: sweep event hard stages');
			macroService.PollPattern(patterns.event.story.enter, { DoClick: true, InversePredicatePattern: patterns.event.story.enter });
			const storyPartSwipeResult = macroService.PollPattern(targetPartPattern, { SwipePattern: patterns.general.swipeRight, TimeoutMs: 7_000 });

			if (!storyPartSwipeResult.IsSuccess) {
				throw Error('Unable to find pattern: settings.sweepEventStoryHard.targetPartPattern');
			}

			macroService.PollPattern(targetPartPattern, { DoClick: true, PredicatePattern: patterns.event.story.selectTeam });
			macroService.PollPattern(patterns.event.story.selectTeam, { DoClick: true, PredicatePattern: patterns.battle.enter });
			selectTeamAndBattle(teamSlot);

			if (macroService.IsRunning) {
				daily.sweepEventStoryHard.done.IsChecked = true;
			}
			return;
	}
	sleep(1_000);
}

// if return true, the this shop is complete
function handleEventShop() {
	let staminaResult = macroService.PollPattern(patterns.general.stamina);
	const currency1Bounds = {
		X: staminaResult.Point.X + 230,
		Y: staminaResult.Point.Y - 17,
		Height: 36,
		Width: 30
	};
	const currency1Pattern = macroService.CapturePatternWithBounds(currency1Bounds);

	macroService.PollPattern(patterns.event.story.eventShop, { DoClick: true, PredicatePattern: patterns.titles.adventurerShop });
	sleep(500);
	staminaResult = macroService.PollPattern(patterns.general.stamina);
	const shopCurrencyBounds = {
		X: staminaResult.Point.X + 220,
		Y: staminaResult.Point.Y - 17,
		Height: 32,
		Width: 30,
		Padding: 20,
		OffsetCalcType: 'None',
	};
	const currency1BoundedPattern = macroService.ClonePattern(currency1Pattern, shopCurrencyBounds);
	let currencyResult = macroService.FindPattern(currency1BoundedPattern);
	if (!currencyResult.IsSuccess) {
		const subTabShopResult = macroService.FindPattern(patterns.shop.subTabShop, { Limit: 5 });
		if (!subTabShopResult.IsSuccess) {
			throw new Error('Could not find shop(s)');
		}

		for (const p of subTabShopResult.Points) {
			macroService.DoClick(p);
			sleep(500);
			currencyResult = macroService.FindPattern(currency1BoundedPattern);
			if (currencyResult.IsSuccess) break;
		}

		if (!currencyResult.IsSuccess) {
			throw new Error('Could not find target currency');
		}
	}

	let purchaseResult = macroService.PollPattern(patterns.shop.purchase1, { TimeoutMs: 3_000 });
	while (purchaseResult.IsSuccess) {
		macroService.PollPattern(patterns.shop.purchase1, { DoClick: true, PredicatePattern: patterns.shop.purchase.ok });
		const maxResult = macroService.FindPattern(patterns.shop.purchase.max);
		if (maxResult.IsSuccess) {
			const maxResult = macroService.PollPattern(patterns.shop.purchase.max, { DoClick: true, PredicatePattern: patterns.shop.purchase.sliderMax, TimeoutMs: 2_000 });
			if (!maxResult.IsSuccess) {
				macroService.PollPattern(patterns.shop.purchase1.cancel, { DoClick: true, PredicatePattern: [patterns.shop.purchase1, patterns.shop.purchase1.disabled] });
				purchaseResult = { IsSuccess: false };
				continue;
			}
		}
		const okResult = macroService.PollPattern(patterns.shop.purchase.ok, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace, TimeoutMs: 3_500 });
		if (!okResult.IsSuccess) break;

		macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: [patterns.titles.adventurerShop, patterns.titles.shop, patterns.shop.premium.title] });
		purchaseResult = macroService.PollPattern(patterns.shop.purchase1, { TimeoutMs: 3_000 });
	}

	purchaseResult = macroService.PollPattern([patterns.shop.purchase1, patterns.shop.purchase1.disabled]);
	return purchaseResult.Path === 'shop.purchase1.disabled';
}

function handleRewards() {
	let moveNotification = macroService.PollPattern(patterns.event.move.notification, { TimeoutMs: 2_000 });
	if (moveNotification.IsSuccess) {
		macroService.PollPattern(patterns.event.move, { DoClick: true, PredicatePattern: patterns.event.move.close });

		let notificationResult = macroService.PollPattern(patterns.event.move.recieve, { TimeoutMs: 3_000 });
		while (notificationResult.IsSuccess) {
			macroService.PollPattern(patterns.event.move.recieve, { DoClick: true, PredicatePattern: patterns.general.tapEmptySpace });
			macroService.PollPattern(patterns.general.tapEmptySpace, { DoClick: true, PredicatePattern: patterns.event.move.close });
			notificationResult = macroService.PollPattern(patterns.event.move.recieve, { TimeoutMs: 3_000 });
		}

		macroService.PollPattern(patterns.event.move.close, { DoClick: true, PredicatePattern: patterns.event.story.enter });
	}
}